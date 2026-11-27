using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Sockets;

namespace HealthChecks.Common
{
    //plain tcp connect, enough to see if a broker or agent is reachable
    public class TcpHealthCheck : IHealthCheck
    {
        private readonly string _host;
        private readonly int _port;
        private readonly TimeSpan _timeout;

        public TcpHealthCheck(string host, int port, TimeSpan? timeout = null)
        {
            _host = host;
            _port = port;
            _timeout = timeout ?? TimeSpan.FromSeconds(3);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = new TcpClient();
                using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutSource.CancelAfter(_timeout);

                await client.ConnectAsync(_host, _port, timeoutSource.Token);

                return HealthCheckResult.Healthy($"{_host}:{_port} is reachable");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, $"{_host}:{_port} is not reachable", ex);
            }
        }
    }
}
