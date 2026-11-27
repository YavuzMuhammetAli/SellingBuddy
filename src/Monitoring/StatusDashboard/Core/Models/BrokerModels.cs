namespace StatusDashboard.Core.Models
{
    public class BrokerOverview
    {
        public bool Reachable { get; set; }

        public string Error { get; set; }

        public string ClusterName { get; set; }

        public string RabbitMqVersion { get; set; }

        public string ErlangVersion { get; set; }

        public int Connections { get; set; }

        public int Channels { get; set; }

        public int Consumers { get; set; }

        public int Queues { get; set; }

        public long MessagesReady { get; set; }

        public long MessagesUnacknowledged { get; set; }

        public double PublishRate { get; set; }

        public double DeliverRate { get; set; }
    }

    public class QueueInfo
    {
        public string Name { get; set; }

        //queue name is {SubscriberClientAppName}.{EventName}
        public string OwnerService { get; set; }

        public string EventName { get; set; }

        public long MessagesReady { get; set; }

        public long MessagesUnacknowledged { get; set; }

        public int Consumers { get; set; }

        public string State { get; set; }

        public double PublishRate { get; set; }

        public double DeliverRate { get; set; }

        public OverallStatus Status => Consumers > 0 ? OverallStatus.Passing : OverallStatus.Critical;
    }
}
