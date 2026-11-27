using Web.ApiGateway.Models;
using Web.ApiGateway.Services.Interfaces;

namespace Web.ApiGateway.Services
{
    public class BasketService : IBasketService
    {
        private readonly HttpClient _httpClient;

        public BasketService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BasketData> GetBasketAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<BasketData>("basket");

            return response ?? new BasketData();
        }

        public async Task<BasketData> UpdateBasketAsync(BasketData currentBasket)
        {
            var response = await _httpClient.PostAsJsonAsync("basket/update", currentBasket);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<BasketData>();
        }
    }
}
