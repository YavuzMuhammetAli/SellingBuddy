namespace StatusDashboard.Core
{
    //same list as the eventbus-integration skill
    public static class EventCatalog
    {
        public record EventFlow(string EventName, string Publisher, string[] Consumers, string Payload);

        public static IReadOnlyList<EventFlow> Flows { get; } = new List<EventFlow>
        {
            new("OrderCreatedIntegrationEvent", "BasketService", new[] { "OrderService" }, "user, address, card, basket items"),
            new("OrderStartedIntegrationEvent", "OrderService", new[] { "BasketService", "PaymentService" }, "UserName, OrderId"),
            new("OrderPaymentSuccessIntegrationEvent", "PaymentService", new[] { "NotificationService" }, "OrderId"),
            new("OrderPaymentFailedIntegrationEvent", "PaymentService", new[] { "NotificationService" }, "OrderId, ErrorMessage")
        };
    }
}
