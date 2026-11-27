using WebApp.Domain.Models;

namespace WebApp.Application.Services.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> LoginAsync(string userName, string password);

        Task LogoutAsync();
    }

    public interface ICatalogService
    {
        Task<PaginatedItemsViewModel<CatalogItem>> GetCatalogItemsAsync(int pageIndex = 0, int pageSize = 9);

        Task<CatalogItem> GetCatalogItemAsync(int id);
    }

    public interface IBasketService
    {
        Task<Basket> GetBasketAsync();

        Task AddItemToBasketAsync(int catalogItemId, int quantity = 1);

        Task<Basket> UpdateBasketAsync(Basket basket);

        Task<bool> CheckoutAsync(BasketCheckout basketCheckout);
    }

    public interface IOrderService
    {
        Task<List<OrderSummary>> GetMyOrdersAsync();

        Task<OrderDetail> GetOrderDetailAsync(string orderNumber);
    }
}
