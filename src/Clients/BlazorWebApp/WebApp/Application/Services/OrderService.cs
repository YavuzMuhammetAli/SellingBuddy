using WebApp.Application.Services.Interfaces;
using WebApp.Domain.Models;
using WebApp.Infrastructure;

namespace WebApp.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly GatewayClient _gateway;

        public OrderService(GatewayClient gateway)
        {
            _gateway = gateway;
        }

        public async Task<List<OrderSummary>> GetMyOrdersAsync()
        {
            var client = await _gateway.AuthorizedAsync();

            return await client.GetFromJsonAsync<List<OrderSummary>>("order") ?? new List<OrderSummary>();
        }

        public async Task<OrderDetail> GetOrderDetailAsync(string orderNumber)
        {
            var client = await _gateway.AuthorizedAsync();

            return await client.GetFromJsonAsync<OrderDetail>($"order/{orderNumber}");
        }
    }
}
