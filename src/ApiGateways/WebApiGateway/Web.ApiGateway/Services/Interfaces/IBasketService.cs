using Web.ApiGateway.Models;

namespace Web.ApiGateway.Services.Interfaces
{
    public interface IBasketService
    {
        Task<BasketData> GetBasketAsync();

        Task<BasketData> UpdateBasketAsync(BasketData currentBasket);
    }
}
