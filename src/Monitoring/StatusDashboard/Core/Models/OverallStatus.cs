namespace StatusDashboard.Core.Models
{
    //order matters, higher value is worse
    public enum OverallStatus
    {
        Unknown = 0,
        Passing = 1,
        Warning = 2,
        Critical = 3
    }

    public static class OverallStatusExtensions
    {
        public static OverallStatus Worst(this IEnumerable<OverallStatus> statuses, OverallStatus whenEmpty = OverallStatus.Unknown)
        {
            var list = statuses.ToList();

            if (!list.Any())
                return whenEmpty;

            //unknown only wins when nothing else is known
            var known = list.Where(s => s != OverallStatus.Unknown).ToList();

            return known.Any() ? known.Max() : OverallStatus.Unknown;
        }

        //asp.net health report values
        public static OverallStatus FromHealthStatus(string status) => status?.ToLowerInvariant() switch
        {
            "healthy" => OverallStatus.Passing,
            "degraded" => OverallStatus.Warning,
            "unhealthy" => OverallStatus.Critical,
            _ => OverallStatus.Unknown
        };

        //consul check values
        public static OverallStatus FromConsulStatus(string status) => status?.ToLowerInvariant() switch
        {
            "passing" => OverallStatus.Passing,
            "warning" => OverallStatus.Warning,
            "critical" => OverallStatus.Critical,
            _ => OverallStatus.Unknown
        };

        public static string ToText(this OverallStatus status) => status switch
        {
            OverallStatus.Passing => "Healthy",
            OverallStatus.Warning => "Degraded",
            OverallStatus.Critical => "Critical",
            _ => "Unknown"
        };
    }
}
