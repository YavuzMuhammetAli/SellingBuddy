using MediatR;
using OrderService.Application.Features.Queries.ViewModels;
using OrderService.Application.Interfaces.Repositories;

namespace OrderService.Application.Features.Queries.GetOrdersByUser
{
    public class GetOrdersByUserQueryHandler : IRequestHandler<GetOrdersByUserQuery, List<OrderSummaryViewModel>>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrdersByUserQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<OrderSummaryViewModel>> Handle(GetOrdersByUserQuery request, CancellationToken cancellationToken)
        {
            var orders = await _orderRepository.Get(
                o => o.Buyer.Name == request.UserName,
                q => q.OrderByDescending(o => o.OrderDate),
                o => o.OrderItems, o => o.OrderStatus);

            return orders.Select(o => new OrderSummaryViewModel
            {
                OrderNumber = o.Id.ToString(),
                Date = o.OrderDate,
                Status = o.OrderStatus?.Name,
                Total = o.GetTotal()
            }).ToList();
        }
    }
}
