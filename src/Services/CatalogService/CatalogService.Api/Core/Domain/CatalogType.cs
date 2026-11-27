namespace CatalogService.Api.Core.Domain
{
    //category, can have one parent level for subcategories
    public class CatalogType
    {
        public int Id { get; set; }

        public string Slug { get; set; }

        public LocalizedText Name { get; set; } = new();

        public LocalizedText Description { get; set; } = new();

        public string ImageFileName { get; set; }

        public int DisplayOrder { get; set; }

        public int? ParentCatalogTypeId { get; set; }

        public CatalogType ParentCatalogType { get; set; }

        public List<CatalogType> Children { get; set; } = new();
    }
}
