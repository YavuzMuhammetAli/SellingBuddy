using EventBus.Base.Abstraction;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Features.Commands.CreateOrder;
using OrderService.Application.IntegrationEvents.Events;

namespace OrderService.Application.IntegrationEvents.EventHandlers
{
    public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;

        public OrderCreatedIntegrationEventHandler(IMediator mediator, ILogger<OrderCreatedIntegrationEventHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(OrderCreatedIntegrationEvent @event)
        {
            //event carries card data, so only safe fields are logged
            _logger.LogInformation("Handling integration event {IntegrationEventId} at OrderService for {UserName} with {ItemCount} items",
                @event.Id, @event.UserName, @event.Basket?.Items?.Count ?? 0);

            //basket checkout turned into create order command
            var createOrderCommand = new CreateOrderCommand(@event.Basket.Items, @event.UserName, @event.City, @event.Street,
                @event.State, @event.Country, @event.ZipCode, @event.CardNumber, @event.CardHolderName,
                @event.CardExpiration, @event.CardSecurityNumber, @event.CardTypeId)
                .WithCheckoutDetails(@event.OrderId, @event.ContactName, @event.Phone, @event.ShippingMethod, @event.ShippingFee);

            await _mediator.Send(createOrderCommand);
        }
    }
}
