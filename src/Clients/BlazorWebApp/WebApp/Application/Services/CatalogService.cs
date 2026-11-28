using System.Net;
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

        public async Task<PagedResult<ProductSummary>> GetItemsAsync(CatalogQuery query, CancellationToken cancellationToken = default)
        {
            return await _gateway.Anonymous.GetFromJsonAsync<PagedResult<ProductSummary>>($"catalog/items?{query.ToQueryString()}", cancellationToken)
                ?? new PagedResult<ProductSummary>();
        }

        public async Task<Facets> GetFacetsAsync(CatalogQuery query, CancellationToken cancellationToken = default)
        {
            return await _gateway.Anonymous.GetFromJsonAsync<Facets>($"catalog/items/facets?{query.ToQueryString()}", cancellationToken) ?? new Facets();
        }

        public async Task<List<ProductSummary>> GetItemsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
        {
            var list = ids.Distinct().ToList();

            if (!list.Any())
                return new List<ProductSummary>();

            return await _gateway.Anonymous.GetFromJsonAsync<List<ProductSummary>>($"catalog/items/bulk?ids={string.Join(',', list)}", cancellationToken)
                ?? new List<ProductSummary>();
        }

        public Task<ProductDetail> GetProductAsync(string slug, CancellationToken cancellationToken = default) =>
            GetOrNullAsync<ProductDetail>($"catalog/items/slug/{Uri.EscapeDataString(slug)}", cancellationToken);

        public async Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
            await _gateway.Anonymous.GetFromJsonAsync<List<Category>>("catalog/categories", cancellationToken) ?? new List<Category>();

        public Task<Category> GetCategoryAsync(string slug, CancellationToken cancellationToken = default) =>
            GetOrNullAsync<Category>($"catalog/categories/{Uri.EscapeDataString(slug)}", cancellationToken);

        public async Task<List<Seller>> GetSellersAsync(CancellationToken cancellationToken = default) =>
            await _gateway.Anonymous.GetFromJsonAsync<List<Seller>>("catalog/sellers", cancellationToken) ?? new List<Seller>();

        public Task<Seller> GetSellerAsync(string slug, CancellationToken cancellationToken = default) =>
            GetOrNullAsync<Seller>($"catalog/sellers/{Uri.EscapeDataString(slug)}", cancellationToken);

        public async Task<List<Campaign>> GetCampaignsAsync(CancellationToken cancellationToken = default) =>
            await _gateway.Anonymous.GetFromJsonAsync<List<Campaign>>("catalog/campaigns", cancellationToken) ?? new List<Campaign>();

        public Task<Campaign> GetCampaignAsync(string slug, CancellationToken cancellationToken = default) =>
            GetOrNullAsync<Campaign>($"catalog/campaigns/{Uri.EscapeDataString(slug)}", cancellationToken);

        //404 means not found, not an error
        private async Task<T> GetOrNullAsync<T>(string url, CancellationToken cancellationToken) where T : class
        {
            using var response = await _gateway.Anonymous.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        }
    }
}
