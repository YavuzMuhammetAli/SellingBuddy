using WebApp.Application.Services.Interfaces;
using WebApp.Domain.Models;
using WebApp.Infrastructure;

namespace WebApp.Application.Services
{
    public class BasketService : IBasketService
    {
        private readonly GatewayClient _gateway;

        public BasketService(GatewayClient gateway)
        {
            _gateway = gateway;
        }

        public async Task<Basket> GetBasketAsync()
        {
            var client = await _gateway.AuthorizedAsync();

            return await client.GetFromJsonAsync<Basket>("basket") ?? new Basket();
        }

        //gateway aggregator reads the catalog item and updates the basket
        public async Task AddItemToBasketAsync(int catalogItemId, int quantity = 1)
        {
            var client = await _gateway.AuthorizedAsync();

            var response = await client.PostAsJsonAsync("api/basket/items", new { CatalogItemId = catalogItemId, Quantity = quantity });
            response.EnsureSuccessStatusCode();
        }

        public async Task<Basket> UpdateBasketAsync(Basket basket)
        {
            var client = await _gateway.AuthorizedAsync();

            var response = await client.PostAsJsonAsync("basket/update", basket);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Basket>();
        }

        public async Task<bool> CheckoutAsync(BasketCheckout basketCheckout)
        {
            var client = await _gateway.AuthorizedAsync();

            var response = await client.PostAsJsonAsync("basket/checkout", basketCheckout);

            return response.IsSuccessStatusCode;
        }
    }
}
