using Microsoft.Extensions.Options;
using StatusDashboard.Core.Models;
using StatusDashboard.Core.Options;
using StatusDashboard.Infrastructure.Clients;

namespace StatusDashboard.Application
{
    //polls consul, health endpoints, rabbitmq and infra ports on a fixed interval
    public class StatusMonitorService : BackgroundService
    {
        private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(3);

        private readonly IServiceProvider _serviceProvider;
        private readonly StatusStore _store;
        private readonly MonitoringOptions _options;
        private readonly ILogger<StatusMonitorService> _logger;

        public StatusMonitorService(IServiceProvider serviceProvider, StatusStore store, IOptions<MonitoringOptions> options, ILogger<StatusMonitorService> logger)
        {
            _serviceProvider = serviceProvider;
            _store = store;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var snapshot = await CollectAsync(stoppingToken);
                    _store.Update(snapshot);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "Status collection failed");
                }

                try
                {
                    //wakes up early when the ui asks for a refresh
                    await _store.WaitForRefreshAsync(TimeSpan.FromSeconds(_store.RefreshSeconds), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task<DashboardSnapshot> CollectAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var consul = scope.ServiceProvider.GetRequiredService<ConsulStatusClient>();
            var health = scope.ServiceProvider.GetRequiredService<HealthEndpointClient>();
            var rabbit = scope.ServiceProvider.GetRequiredService<RabbitMqManagementClient>();

            var snapshot = new DashboardSnapshot { UpdatedAt = DateTime.UtcNow };

            //broker first, worker status depends on queue consumers
            var brokerTask = rabbit.GetOverviewAsync(cancellationToken);
            var queuesTask = rabbit.GetQueuesAsync(cancellationToken);
            var infraTask = CollectInfrastructureAsync(cancellationToken);

            snapshot.Services.AddRange(await CollectConsulServicesAsync(consul, health, snapshot, cancellationToken));
            snapshot.Services.AddRange(await Task.WhenAll(_options.StaticTargets.Select(t => CollectStaticAsync(t, health, cancellationToken))));

            snapshot.Broker = await brokerTask;
            snapshot.Queues = await queuesTask;
            snapshot.Services.AddRange(_options.WorkerTargets.Select(t => CollectWorker(t, snapshot)));
            snapshot.Infrastructure = (await infraTask).ToList();

            ApplyInfrastructureDetails(snapshot);

            snapshot.Services = snapshot.Services.OrderBy(s => s.Kind).ThenBy(s => s.Name).ToList();

            return snapshot;
        }

        private async Task<List<ServiceStatus>> CollectConsulServicesAsync(ConsulStatusClient consul, HealthEndpointClient health, DashboardSnapshot snapshot, CancellationToken cancellationToken)
        {
            try
            {
                var services = await consul.GetServicesAsync(cancellationToken);
                snapshot.ConsulReachable = true;

                try
                {
                    snapshot.ConsulLeader = await consul.GetLeaderAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Consul leader could not be read");
                }

                var tasks = services.Select(async service =>
                {
                    var instances = await consul.GetInstancesAsync(service.Key, cancellationToken);

                    await Task.WhenAll(instances.Select(async instance =>
                    {
                        var result = await health.GetAsync($"http://{instance.Address}:{instance.Port}/health", cancellationToken);

                        instance.Health = result.Report;
                        instance.LatencyMs = result.LatencyMs;
                        instance.Error = result.Error;

                        //worst of consul checks and our own call
                        instance.Status = instance.Checks.Select(c => c.OverallStatus)
                            .Append(result.Status)
                            .Worst();
                    }));

                    return new ServiceStatus
                    {
                        Name = service.Key,
                        Kind = ServiceKind.Consul,
                        Tags = service.Value,
                        Instances = instances,
                        Status = instances.Select(i => i.Status).Worst(whenEmpty: OverallStatus.Critical),
                        LatencyMs = Average(instances.Select(i => i.LatencyMs)),
                        Error = instances.Any() ? null : "No instance registered"
                    };
                });

                return (await Task.WhenAll(tasks)).ToList();
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                snapshot.ConsulReachable = false;
                snapshot.ConsulError = ex.Message;

                return new List<ServiceStatus>();
            }
        }

        private static async Task<ServiceStatus> CollectStaticAsync(StaticTarget target, HealthEndpointClient health, CancellationToken cancellationToken)
        {
            var result = await health.GetAsync(target.HealthUrl, cancellationToken);
            var uri = new Uri(target.HealthUrl);

            var instance = new ServiceInstanceStatus
            {
                Id = target.Name,
                Address = uri.Host,
                Port = uri.Port,
                Node = "-",
                Status = result.Status,
                LatencyMs = result.LatencyMs,
                Error = result.Error,
                Health = result.Report
            };

            return new ServiceStatus
            {
                Name = target.Name,
                Kind = ServiceKind.Static,
                Instances = new List<ServiceInstanceStatus> { instance },
                Status = instance.Status,
                LatencyMs = instance.LatencyMs,
                Error = result.Error
            };
        }

        private static ServiceStatus CollectWorker(WorkerTarget target, DashboardSnapshot snapshot)
        {
            var queues = snapshot.Queues.Where(q => q.Name.StartsWith(target.QueuePrefix, StringComparison.OrdinalIgnoreCase)).ToList();

            //no broker means we can't tell
            var status = !snapshot.Broker.Reachable
                ? OverallStatus.Unknown
                : queues.Any() && queues.All(q => q.Consumers > 0) ? OverallStatus.Passing : OverallStatus.Critical;

            return new ServiceStatus
            {
                Name = target.Name,
                Kind = ServiceKind.Worker,
                Tags = new[] { "worker" },
                Status = status,
                Instances = queues.Select(q => new ServiceInstanceStatus
                {
                    Id = q.Name,
                    Address = q.EventName,
                    Node = "rabbitmq",
                    Status = q.Status,
                    Error = q.Consumers > 0 ? null : "Queue has no consumer"
                }).ToList(),
                Error = status == OverallStatus.Critical ? "No consumer is listening on its queues" : null
            };
        }

        private async Task<InfrastructureStatus[]> CollectInfrastructureAsync(CancellationToken cancellationToken)
        {
            return await Task.WhenAll(_options.InfrastructureTargets.Select(async target =>
            {
                var probe = await TcpProbe.ProbeAsync(target.Host, target.Port, ProbeTimeout, cancellationToken);

                return new InfrastructureStatus
                {
                    Name = target.Name,
                    Kind = target.Kind,
                    Host = target.Host,
                    Port = target.Port,
                    Status = probe.Reachable ? OverallStatus.Passing : OverallStatus.Critical,
                    LatencyMs = probe.LatencyMs,
                    Error = probe.Error
                };
            }));
        }

        private static void ApplyInfrastructureDetails(DashboardSnapshot snapshot)
        {
            foreach (var infra in snapshot.Infrastructure)
            {
                infra.Detail = infra.Kind?.ToLowerInvariant() switch
                {
                    "consul" when snapshot.ConsulReachable => $"Leader {snapshot.ConsulLeader ?? "-"}",
                    "rabbitmq" when snapshot.Broker.Reachable => $"RabbitMQ {snapshot.Broker.RabbitMqVersion}, Erlang {snapshot.Broker.ErlangVersion}",
                    "rabbitmq" => "Management API not reachable",
                    _ => null
                };
            }
        }

        private static double? Average(IEnumerable<double?> values)
        {
            var list = values.Where(v => v.HasValue).Select(v => v.Value).ToList();
            return list.Any() ? Math.Round(list.Average(), 1) : null;
        }
    }
}
