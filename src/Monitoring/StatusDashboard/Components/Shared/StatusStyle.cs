using StatusDashboard.Core.Models;
using TabBlazor;

namespace StatusDashboard.Components.Shared
{
    //one place for status colors and icons
    public static class StatusStyle
    {
        public static TablerColor Color(OverallStatus status) => status switch
        {
            OverallStatus.Passing => TablerColor.Success,
            OverallStatus.Warning => TablerColor.Yellow,
            OverallStatus.Critical => TablerColor.Danger,
            _ => TablerColor.Secondary
        };

        public static IIconType Icon(OverallStatus status) => status switch
        {
            OverallStatus.Passing => AppIcons.CircleCheck,
            OverallStatus.Warning => AppIcons.Alert,
            OverallStatus.Critical => AppIcons.CircleX,
            _ => AppIcons.Help
        };

        public static IIconType KindIcon(ServiceKind kind) => kind switch
        {
            ServiceKind.Worker => AppIcons.Mail,
            ServiceKind.Static => AppIcons.Exchange,
            _ => AppIcons.Server
        };

        public static IIconType InfrastructureIcon(string kind) => kind?.ToLowerInvariant() switch
        {
            "sqlserver" or "redis" => AppIcons.Database,
            "rabbitmq" => AppIcons.Exchange,
            "consul" => AppIcons.Stack,
            _ => AppIcons.Box
        };

        public static string Latency(double? latencyMs) => latencyMs.HasValue ? $"{latencyMs.Value:0.#} ms" : "-";

        public static string Ago(DateTime utcTime)
        {
            if (utcTime == DateTime.MinValue)
                return "never";

            var span = DateTime.UtcNow - utcTime;

            return span.TotalSeconds < 60 ? $"{Math.Max(0, (int)span.TotalSeconds)}s ago"
                : span.TotalMinutes < 60 ? $"{(int)span.TotalMinutes}m ago"
                : $"{(int)span.TotalHours}h ago";
        }
    }
}
