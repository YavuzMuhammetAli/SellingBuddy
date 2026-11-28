using WebApp.Domain.Models;

namespace WebApp.Application.Services.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> LoginAsync(string userName, string password, bool rememberMe);

        Task LogoutAsync();
    }

    public interface ICatalogService
    {
        Task<PagedResult<ProductSummary>> GetItemsAsync(CatalogQuery query, CancellationToken cancellationToken = default);

        Task<Facets> GetFacetsAsync(CatalogQuery query, CancellationToken cancellationToken = default);

        Task<List<ProductSummary>> GetItemsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

        Task<ProductDetail> GetProductAsync(string slug, CancellationToken cancellationToken = default);

        Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);

        Task<Category> GetCategoryAsync(string slug, CancellationToken cancellationToken = default);

        Task<List<Seller>> GetSellersAsync(CancellationToken cancellationToken = default);

        Task<Seller> GetSellerAsync(string slug, CancellationToken cancellationToken = default);

        Task<List<Campaign>> GetCampaignsAsync(CancellationToken cancellationToken = default);

        Task<Campaign> GetCampaignAsync(string slug, CancellationToken cancellationToken = default);
    }

    public interface IWishlistService
    {
        Task<List<int>> GetIdsAsync();

        Task AddAsync(int productId);

        Task RemoveAsync(int productId);
    }

    public interface IBasketService
    {
        Task<Basket> GetBasketAsync();

        Task AddItemAsync(int catalogItemId, int? offerId, int quantity = 1);

        Task<Basket> UpdateBasketAsync(Basket basket);

        Task<Guid?> CheckoutAsync(BasketCheckoutRequest request);
    }

    public interface IOrderService
    {
        Task<List<OrderSummary>> GetMyOrdersAsync();

        Task<OrderDetail> GetOrderAsync(string orderNumber);
    }
}
