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

        //gateway aggregator reads price and seller from the catalog offer
        public async Task AddItemAsync(int catalogItemId, int? offerId, int quantity = 1)
        {
            var client = await _gateway.AuthorizedAsync();

            var response = await client.PostAsJsonAsync("api/basket/items", new { CatalogItemId = catalogItemId, OfferId = offerId, Quantity = quantity });
            response.EnsureSuccessStatusCode();
        }

        public async Task<Basket> UpdateBasketAsync(Basket basket)
        {
            var client = await _gateway.AuthorizedAsync();

            var response = await client.PostAsJsonAsync("basket/update", basket);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Basket>() ?? basket;
        }

        public async Task<Guid?> CheckoutAsync(BasketCheckoutRequest request)
        {
            var client = await _gateway.AuthorizedAsync();

            var response = await client.PostAsJsonAsync("basket/checkout", request);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<CheckoutResult>();
            return result?.OrderId;
        }
    }

    public class WishlistService : IWishlistService
    {
        private readonly GatewayClient _gateway;

        public WishlistService(GatewayClient gateway)
        {
            _gateway = gateway;
        }

        public async Task<List<int>> GetIdsAsync()
        {
            var client = await _gateway.AuthorizedAsync();

            return await client.GetFromJsonAsync<List<int>>("wishlist") ?? new List<int>();
        }

        public async Task AddAsync(int productId)
        {
            var client = await _gateway.AuthorizedAsync();

            var response = await client.PostAsync($"wishlist/{productId}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveAsync(int productId)
        {
            var client = await _gateway.AuthorizedAsync();

            var response = await client.DeleteAsync($"wishlist/{productId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
