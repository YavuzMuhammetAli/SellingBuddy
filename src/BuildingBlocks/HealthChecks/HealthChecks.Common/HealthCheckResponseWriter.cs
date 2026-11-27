using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace HealthChecks.Common
{
    public static class HealthCheckResponseWriter
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

        //json body read by the status dashboard
        public static Task WriteJsonAsync(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var response = new
            {
                status = report.Status.ToString(),
                totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
                entries = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    durationMs = Math.Round(e.Value.Duration.TotalMilliseconds, 2),
                    description = e.Value.Description ?? e.Value.Exception?.Message,
                    tags = e.Value.Tags
                })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }
}
