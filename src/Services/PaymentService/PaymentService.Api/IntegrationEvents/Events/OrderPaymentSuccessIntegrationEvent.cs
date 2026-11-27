using EventBus.Base.Events;

namespace PaymentService.Api.IntegrationEvents.Events
{
    public class OrderPaymentSuccessIntegrationEvent : IntegrationEvent
    {
        public Guid OrderId { get; set; }

        public OrderPaymentSuccessIntegrationEvent(Guid orderId)
        {
            OrderId = orderId;
        }
    }
}
