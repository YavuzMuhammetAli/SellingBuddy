namespace StatusDashboard.Core.Models
{
    //same shape as HealthCheckResponseWriter in HealthChecks.Common
    public class HealthReport
    {
        public string Status { get; set; }

        public double TotalDurationMs { get; set; }

        public List<HealthReportEntry> Entries { get; set; } = new();
    }

    public class HealthReportEntry
    {
        public string Name { get; set; }

        public string Status { get; set; }

        public double DurationMs { get; set; }

        public string Description { get; set; }

        public List<string> Tags { get; set; } = new();

        public OverallStatus OverallStatus => OverallStatusExtensions.FromHealthStatus(Status);
    }
}
