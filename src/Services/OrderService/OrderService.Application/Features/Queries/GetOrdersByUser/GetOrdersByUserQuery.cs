using MediatR;
using OrderService.Application.Features.Queries.ViewModels;

namespace OrderService.Application.Features.Queries.GetOrdersByUser
{
    public class GetOrdersByUserQuery : IRequest<List<OrderSummaryViewModel>>
    {
        public string UserName { get; set; }

        public GetOrdersByUserQuery(string userName)
        {
            UserName = userName;
        }
    }
}
