namespace StatusDashboard.Core.Options
{
    public class MonitoringOptions
    {
        public int RefreshSeconds { get; set; } = 10;

        //samples kept per target for sparklines
        public int HistoryLength { get; set; } = 30;

        public List<StaticTarget> StaticTargets { get; set; } = new();

        public List<WorkerTarget> WorkerTargets { get; set; } = new();

        public List<InfrastructureTarget> InfrastructureTargets { get; set; } = new();
    }

    public class StaticTarget
    {
        public string Name { get; set; }

        public string HealthUrl { get; set; }
    }

    public class WorkerTarget
    {
        public string Name { get; set; }

        public string QueuePrefix { get; set; }
    }

    public class InfrastructureTarget
    {
        public string Name { get; set; }

        public string Kind { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }
    }

    public class RabbitMqOptions
    {
        public string ManagementUrl { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
