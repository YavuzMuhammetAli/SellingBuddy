using Web.ApiGateway.Models;
using Web.ApiGateway.Services.Interfaces;

namespace Web.ApiGateway.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient _httpClient;

        public CatalogService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        //goes through ocelot route so consul resolves the service
        public async Task<CatalogItem> GetCatalogItemAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<CatalogItem>($"catalog/items/{id}");
        }
    }
}
