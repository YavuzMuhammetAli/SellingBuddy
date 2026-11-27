using BasketService.Api.Core.Application.Repository;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base.Abstraction;

namespace BasketService.Api.IntegrationEvents.EventHandlers
{
    public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
    {
        private readonly IBasketRepository _repository;
        private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;

        public OrderStartedIntegrationEventHandler(IBasketRepository repository, ILogger<OrderStartedIntegrationEventHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(OrderStartedIntegrationEvent @event)
        {
            _logger.LogInformation("Handling integration event {IntegrationEventId} at BasketService - ({@IntegrationEvent})", @event.Id, @event);

            //basket cleared after order started
            await _repository.DeleteBasketAsync(@event.UserName);
        }
    }
}
