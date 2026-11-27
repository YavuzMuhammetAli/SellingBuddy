using MediatR;
using OrderService.Application.Features.Queries.ViewModels;
using OrderService.Application.Interfaces.Repositories;

namespace OrderService.Application.Features.Queries.GetOrderDetailById
{
    public class GetOrderDetailsQueryHandler : IRequestHandler<GetOrderDetailsQuery, OrderDetailViewModel>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderDetailsQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDetailViewModel> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId, o => o.OrderItems, o => o.OrderStatus);

            if (order == null)
                return null;

            //manual mapping, no automapper
            return new OrderDetailViewModel
            {
                OrderNumber = order.Id.ToString(),
                Date = order.OrderDate,
                Status = order.OrderStatus?.Name,
                Description = order.Description,
                Street = order.Address.Street,
                City = order.Address.City,
                ZipCode = order.Address.ZipCode,
                Country = order.Address.Country,
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.ProductName,
                    Units = oi.Units,
                    UnitPrice = oi.UnitPrice,
                    PictureUrl = oi.PictureUrl
                }).ToList(),
                Total = order.GetTotal()
            };
        }
    }
}
