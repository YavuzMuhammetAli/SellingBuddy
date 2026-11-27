using MediatR;
using OrderService.Application.Features.Queries.ViewModels;
using OrderService.Application.Interfaces.Repositories;

namespace OrderService.Application.Features.Queries.GetOrderDetailById
{
    public class GetOrderDetailsQueryHandler : IRequestHandler<GetOrderDetailsQuery, OrderDetailViewModel>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IBuyerRepository _buyerRepository;

        public GetOrderDetailsQueryHandler(IOrderRepository orderRepository, IBuyerRepository buyerRepository)
        {
            _orderRepository = orderRepository;
            _buyerRepository = buyerRepository;
        }

        public async Task<OrderDetailViewModel> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, o => o.OrderItems, o => o.OrderStatus);

            if (order == null)
                return null;

            var model = new OrderDetailViewModel
            {
                OrderNumber = order.Id.ToString(),
                Date = order.OrderDate,
                Status = order.OrderStatus?.Name,
                Description = order.Description,
                ContactName = order.Address.ContactName,
                Phone = order.Address.Phone,
                Street = order.Address.Street,
                City = order.Address.City,
                State = order.Address.State,
                ZipCode = order.Address.ZipCode,
                Country = order.Address.Country,
                ShippingMethod = order.ShippingMethod,
                ShippingFee = order.ShippingFee,
                OrderItems = order.OrderItems.Select(OrderMapper.ToItem).ToList(),
                ItemsTotal = order.GetTotal(),
                Total = order.GetGrandTotal()
            };

            //only the last digits of the card leave the service
            if (order.BuyerId.HasValue && order.PaymentMethodId.HasValue)
            {
                var buyer = await _buyerRepository.GetSingleAsync(b => b.Id == order.BuyerId.Value, b => b.PaymentMethods);
                var cardNumber = buyer?.PaymentMethods.FirstOrDefault(p => p.Id == order.PaymentMethodId.Value)?.CardNumber;

                if (!string.IsNullOrEmpty(cardNumber) && cardNumber.Length >= 4)
                    model.CardLast4 = cardNumber[^4..];
            }

            return model;
        }
    }
}
