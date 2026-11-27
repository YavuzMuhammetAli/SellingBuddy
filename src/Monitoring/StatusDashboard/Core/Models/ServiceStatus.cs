namespace StatusDashboard.Core.Models
{
    public enum ServiceKind
    {
        //registered in consul
        Consul,
        //fixed health url, not in consul
        Static,
        //no http, alive when its queues have consumers
        Worker
    }

    public class ServiceStatus
    {
        public string Name { get; set; }

        public ServiceKind Kind { get; set; }

        public OverallStatus Status { get; set; }

        public string[] Tags { get; set; } = Array.Empty<string>();

        public List<ServiceInstanceStatus> Instances { get; set; } = new();

        public double? LatencyMs { get; set; }

        public string Error { get; set; }

        public int PassingInstances => Instances.Count(i => i.Status == OverallStatus.Passing);
    }

    public class ServiceInstanceStatus
    {
        public string Id { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public string Node { get; set; }

        public OverallStatus Status { get; set; }

        public double? LatencyMs { get; set; }

        public string Error { get; set; }

        public List<ConsulCheck> Checks { get; set; } = new();

        public HealthReport Health { get; set; }

        public string Endpoint => Port > 0 ? $"{Address}:{Port}" : Address;
    }

    public class ConsulCheck
    {
        public string CheckId { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Status { get; set; }

        public string Output { get; set; }

        public OverallStatus OverallStatus => OverallStatusExtensions.FromConsulStatus(Status);
    }
}
