using System.Net;
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

        public async Task<OrderDetail> GetOrderAsync(string orderNumber)
        {
            var client = await _gateway.AuthorizedAsync();

            using var response = await client.GetAsync($"order/{Uri.EscapeDataString(orderNumber)}");

            //order is created async after checkout, it can be missing for a moment
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderDetail>();
        }
    }
}
