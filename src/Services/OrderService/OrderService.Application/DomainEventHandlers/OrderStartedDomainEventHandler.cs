using MediatR;
using OrderService.Application.Interfaces.Repositories;
using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Domain.Events;

namespace OrderService.Application.DomainEventHandlers
{
    public class OrderStartedDomainEventHandler : INotificationHandler<OrderStartedDomainEvent>
    {
        private readonly IBuyerRepository _buyerRepository;

        public OrderStartedDomainEventHandler(IBuyerRepository buyerRepository)
        {
            _buyerRepository = buyerRepository;
        }

        public async Task Handle(OrderStartedDomainEvent orderStartedEvent, CancellationToken cancellationToken)
        {
            var cardTypeId = orderStartedEvent.CardTypeId != 0 ? orderStartedEvent.CardTypeId : 1;

            var buyer = await _buyerRepository.GetSingleAsync(i => i.Name == orderStartedEvent.UserName, i => i.PaymentMethods);

            //buyer created on first order
            var buyerOriginallyExisted = buyer != null;

            if (!buyerOriginallyExisted)
                buyer = new Buyer(orderStartedEvent.UserName);

            buyer.VerifyOrAddPaymentMethod(cardTypeId,
                $"Payment Method on {DateTime.UtcNow}",
                orderStartedEvent.CardNumber,
                orderStartedEvent.CardSecurityNumber,
                orderStartedEvent.CardHolderName,
                orderStartedEvent.CardExpiration,
                orderStartedEvent.Order.Id);

            //existing buyer is already tracked, new payment method is picked up on save
            if (!buyerOriginallyExisted)
                await _buyerRepository.AddAsync(buyer);

            await _buyerRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }
}
