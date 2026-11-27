namespace BasketService.Api.Core.Application.Repository
{
    public interface IWishlistRepository
    {
        Task<List<int>> GetProductIdsAsync(string userName);

        Task AddAsync(string userName, int productId);

        Task RemoveAsync(string userName, int productId);
    }
}
