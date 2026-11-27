using Consul;
using StatusDashboard.Core.Models;

namespace StatusDashboard.Infrastructure.Clients
{
    public class ConsulStatusClient
    {
        private readonly IConsulClient _consulClient;

        public ConsulStatusClient(IConsulClient consulClient)
        {
            _consulClient = consulClient;
        }

        //service name -> tags, consul itself left out
        public async Task<Dictionary<string, string[]>> GetServicesAsync(CancellationToken cancellationToken)
        {
            var result = await _consulClient.Catalog.Services(cancellationToken);

            return result.Response
                .Where(s => !string.Equals(s.Key, "consul", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(s => s.Key, s => s.Value ?? Array.Empty<string>());
        }

        public async Task<List<ServiceInstanceStatus>> GetInstancesAsync(string serviceName, CancellationToken cancellationToken)
        {
            //passing only false, failing instances must be visible too
            var result = await _consulClient.Health.Service(serviceName, null, false, cancellationToken);

            return result.Response.Select(entry => new ServiceInstanceStatus
            {
                Id = entry.Service.ID,
                Address = string.IsNullOrEmpty(entry.Service.Address) ? entry.Node.Address : entry.Service.Address,
                Port = entry.Service.Port,
                Node = entry.Node.Name,
                Checks = entry.Checks
                    .Where(c => !string.IsNullOrEmpty(c.ServiceID))
                    .Select(c => new ConsulCheck
                    {
                        CheckId = c.CheckID,
                        Name = c.Name,
                        Type = c.Type,
                        Status = c.Status?.Status,
                        Output = c.Output
                    }).ToList()
            }).ToList();
        }

        public async Task<string> GetLeaderAsync(CancellationToken cancellationToken)
        {
            return await _consulClient.Status.Leader(cancellationToken);
        }
    }
}
