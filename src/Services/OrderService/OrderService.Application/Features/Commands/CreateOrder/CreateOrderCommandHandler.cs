using EventBus.Base.Abstraction;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.IntegrationEvents.Events;
using OrderService.Application.Interfaces.Repositories;
using OrderService.Domain.AggregateModels.OrderAggregate;

namespace OrderService.Application.Features.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, bool>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(IOrderRepository orderRepository, IEventBus eventBus, ILogger<CreateOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task<bool> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var address = new Address(request.Street, request.City, request.State, request.Country, request.ZipCode);

            var order = new Order(request.UserName, address, request.CardTypeId, request.CardNumber, request.CardSecurityNumber,
                request.CardHolderName, request.CardExpiration, null);

            foreach (var item in request.OrderItems)
                order.AddOrderItem(item.ProductId, item.ProductName, item.UnitPrice, item.PictureUrl, item.Units);

            await _orderRepository.AddAsync(order);

            //domain events handled inside save
            await _orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} created for {UserName}", order.Id, request.UserName);

            //published after order is saved
            var orderStartedIntegrationEvent = new OrderStartedIntegrationEvent(request.UserName, order.Id);
            _eventBus.Publish(orderStartedIntegrationEvent);

            return true;
        }
    }
}
