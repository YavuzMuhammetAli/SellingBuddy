namespace WebApp.Domain.Models
{
    public class CatalogItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string PictureUri { get; set; }

        public int AvailableStock { get; set; }

        public CatalogBrand CatalogBrand { get; set; }

        public CatalogType CatalogType { get; set; }
    }

    public class CatalogBrand
    {
        public int Id { get; set; }

        public string Brand { get; set; }
    }

    public class CatalogType
    {
        public int Id { get; set; }

        public string Type { get; set; }
    }

    public class PaginatedItemsViewModel<TEntity> where TEntity : class
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public long Count { get; set; }

        public IEnumerable<TEntity> Data { get; set; } = Enumerable.Empty<TEntity>();
    }
}
