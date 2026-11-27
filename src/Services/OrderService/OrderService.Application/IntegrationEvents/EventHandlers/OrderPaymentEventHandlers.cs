using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using OrderService.Application.IntegrationEvents.Events;
using OrderService.Application.Interfaces.Repositories;

namespace OrderService.Application.IntegrationEvents.EventHandlers
{
    //payment result moves the order out of submitted
    public class OrderPaymentSuccessIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentSuccessIntegrationEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderPaymentSuccessIntegrationEventHandler> _logger;

        public OrderPaymentSuccessIntegrationEventHandler(IOrderRepository orderRepository, ILogger<OrderPaymentSuccessIntegrationEventHandler> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task Handle(OrderPaymentSuccessIntegrationEvent @event)
        {
            var order = await _orderRepository.GetById(@event.OrderId);

            if (order == null)
            {
                _logger.LogWarning("Payment success for unknown order {OrderId}", @event.OrderId);
                return;
            }

            order.SetPaidStatus();
            await _orderRepository.UnitOfWork.SaveEntitiesAsync();

            _logger.LogInformation("Order {OrderId} marked as paid", @event.OrderId);
        }
    }

    public class OrderPaymentFailedIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentFailedIntegrationEvent>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderPaymentFailedIntegrationEventHandler> _logger;

        public OrderPaymentFailedIntegrationEventHandler(IOrderRepository orderRepository, ILogger<OrderPaymentFailedIntegrationEventHandler> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task Handle(OrderPaymentFailedIntegrationEvent @event)
        {
            var order = await _orderRepository.GetById(@event.OrderId);

            if (order == null)
            {
                _logger.LogWarning("Payment failure for unknown order {OrderId}", @event.OrderId);
                return;
            }

            order.SetCancelledStatus();
            await _orderRepository.UnitOfWork.SaveEntitiesAsync();

            _logger.LogInformation("Order {OrderId} cancelled after failed payment: {ErrorMessage}", @event.OrderId, @event.ErrorMessage);
        }
    }
}
