using EventBus.Base.Events;

namespace PaymentService.Api.IntegrationEvents.Events
{
    public class OrderStartedIntegrationEvent : IntegrationEvent
    {
        public string UserName { get; set; }

        public Guid OrderId { get; set; }

        public OrderStartedIntegrationEvent(string userName, Guid orderId)
        {
            UserName = userName;
            OrderId = orderId;
        }
    }
}
