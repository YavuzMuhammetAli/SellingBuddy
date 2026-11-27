namespace StatusDashboard.Core.Models
{
    public class DashboardSnapshot
    {
        public DateTime UpdatedAt { get; set; }

        public bool ConsulReachable { get; set; }

        public string ConsulError { get; set; }

        public string ConsulLeader { get; set; }

        public List<ServiceStatus> Services { get; set; } = new();

        public BrokerOverview Broker { get; set; } = new();

        public List<QueueInfo> Queues { get; set; } = new();

        public List<InfrastructureStatus> Infrastructure { get; set; } = new();

        public static DashboardSnapshot Empty => new() { UpdatedAt = DateTime.MinValue };

        public bool HasData => UpdatedAt != DateTime.MinValue;

        public int CountServices(OverallStatus status) => Services.Count(s => s.Status == status);
    }

    public class InfrastructureStatus
    {
        public string Name { get; set; }

        public string Kind { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public OverallStatus Status { get; set; }

        public double? LatencyMs { get; set; }

        public string Detail { get; set; }

        public string Error { get; set; }
    }

    public class StatusSample
    {
        public DateTime Time { get; set; }

        public OverallStatus Status { get; set; }

        public double? LatencyMs { get; set; }
    }

    public class StatusChange
    {
        public DateTime Time { get; set; }

        public string Target { get; set; }

        public OverallStatus From { get; set; }

        public OverallStatus To { get; set; }

        public string Message => To == OverallStatus.Passing && From != OverallStatus.Unknown
            ? $"{Target} recovered"
            : $"{Target} is {To.ToText().ToLowerInvariant()}";
    }
}
