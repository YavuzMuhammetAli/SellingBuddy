using WebApp.Application.Services.Interfaces;
using WebApp.Domain.Models;
using WebApp.Infrastructure;

namespace WebApp.Application.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly GatewayClient _gateway;

        public CatalogService(GatewayClient gateway)
        {
            _gateway = gateway;
        }

        public async Task<PaginatedItemsViewModel<CatalogItem>> GetCatalogItemsAsync(int pageIndex = 0, int pageSize = 9)
        {
            var response = await _gateway.Anonymous.GetFromJsonAsync<PaginatedItemsViewModel<CatalogItem>>(
                $"catalog/items?pageIndex={pageIndex}&pageSize={pageSize}");

            return response ?? new PaginatedItemsViewModel<CatalogItem>();
        }

        public Task<CatalogItem> GetCatalogItemAsync(int id)
        {
            return _gateway.Anonymous.GetFromJsonAsync<CatalogItem>($"catalog/items/{id}");
        }
    }
}
