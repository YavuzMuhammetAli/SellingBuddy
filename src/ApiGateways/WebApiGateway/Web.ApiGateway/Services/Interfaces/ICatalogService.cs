using Web.ApiGateway.Models;

namespace Web.ApiGateway.Services.Interfaces
{
    public interface ICatalogService
    {
        Task<CatalogItem> GetCatalogItemAsync(int id);
    }
}
