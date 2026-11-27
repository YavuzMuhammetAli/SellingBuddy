using StatusDashboard.Core.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.Sockets;

namespace StatusDashboard.Infrastructure.Clients
{
    public class HealthEndpointResult
    {
        public HealthReport Report { get; init; }

        public double? LatencyMs { get; init; }

        public string Error { get; init; }

        //host name could not be resolved, e.g. docker names when running locally
        public bool Unresolvable { get; init; }

        public OverallStatus Status => Report != null
            ? OverallStatusExtensions.FromHealthStatus(Report.Status)
            : Unresolvable ? OverallStatus.Unknown : OverallStatus.Critical;
    }

    public class HealthEndpointClient
    {
        private readonly HttpClient _httpClient;

        public HealthEndpointClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HealthEndpointResult> GetAsync(string url, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                //unhealthy answers 503 but still sends the json body
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                stopwatch.Stop();

                var report = await response.Content.ReadFromJsonAsync<HealthReport>(cancellationToken);

                return new HealthEndpointResult
                {
                    Report = report,
                    LatencyMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 1),
                    Error = response.IsSuccessStatusCode ? null : $"HTTP {(int)response.StatusCode}"
                };
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException { SocketErrorCode: SocketError.HostNotFound or SocketError.TryAgain })
            {
                return new HealthEndpointResult { Error = "Host name can't be resolved from the dashboard", Unresolvable = true };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                return new HealthEndpointResult { Error = ex is TaskCanceledException ? "Timed out" : ex.Message };
            }
        }
    }
}
