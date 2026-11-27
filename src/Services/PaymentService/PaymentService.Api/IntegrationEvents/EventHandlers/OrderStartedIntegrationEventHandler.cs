using EventBus.Base.Abstraction;
using EventBus.Base.Events;
using PaymentService.Api.IntegrationEvents.Events;

namespace PaymentService.Api.IntegrationEvents.EventHandlers
{
    public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
    {
        private readonly IConfiguration _configuration;
        private readonly IEventBus _eventBus;
        private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;

        public OrderStartedIntegrationEventHandler(IConfiguration configuration, IEventBus eventBus, ILogger<OrderStartedIntegrationEventHandler> logger)
        {
            _configuration = configuration;
            _eventBus = eventBus;
            _logger = logger;
        }

        public Task Handle(OrderStartedIntegrationEvent @event)
        {
            _logger.LogInformation("Handling integration event {IntegrationEventId} at PaymentService - ({@IntegrationEvent})", @event.Id, @event);

            //fake payment, result comes from config
            var paymentSuccessFlag = _configuration.GetValue<bool>("PaymentSuccess");

            IntegrationEvent paymentEvent = paymentSuccessFlag
                ? new OrderPaymentSuccessIntegrationEvent(@event.OrderId)
                : new OrderPaymentFailedIntegrationEvent(@event.OrderId, "This is a fake error message");

            _logger.LogInformation("Payment for order {OrderId} finished, success: {PaymentSuccess}", @event.OrderId, paymentSuccessFlag);

            _eventBus.Publish(paymentEvent);

            return Task.CompletedTask;
        }
    }
}
