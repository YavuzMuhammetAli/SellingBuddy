using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Common
{
    public static class HealthCheckExtensions
    {
        public const string LiveTag = "live";

        //self check always added, used by /health/live
        public static IHealthChecksBuilder AddDefaultHealthChecks(this IServiceCollection services)
        {
            return services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("Service is running"), new[] { LiveTag });
        }

        public static IHealthChecksBuilder AddTcpCheck(this IHealthChecksBuilder builder, string name, string host, int port)
        {
            return builder.AddCheck(name, new TcpHealthCheck(host, port), HealthStatus.Unhealthy, new[] { "dependency" });
        }

        public static IEndpointRouteBuilder MapDefaultHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            //full report with every dependency
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
            });

            //only tells that the process answers
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(LiveTag),
                ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync
            });

            return endpoints;
        }
    }
}
