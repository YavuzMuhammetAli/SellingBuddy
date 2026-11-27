using OrderService.Domain.AggregateModels.OrderAggregate;
using OrderService.Domain.SeedWork;

namespace OrderService.Application.Interfaces.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
    }
}
