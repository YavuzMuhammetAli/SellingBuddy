using System.Diagnostics;
using System.Net.Sockets;

namespace StatusDashboard.Infrastructure.Clients
{
    public static class TcpProbe
    {
        public record ProbeResult(bool Reachable, double? LatencyMs, string Error);

        //connect only, no protocol handshake
        public static async Task<ProbeResult> ProbeAsync(string host, int port, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                using var client = new TcpClient();
                using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutSource.CancelAfter(timeout);

                await client.ConnectAsync(host, port, timeoutSource.Token);

                return new ProbeResult(true, Math.Round(stopwatch.Elapsed.TotalMilliseconds, 1), null);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new ProbeResult(false, null, "Timed out");
            }
            catch (SocketException ex)
            {
                return new ProbeResult(false, null, ex.Message);
            }
        }
    }
}
