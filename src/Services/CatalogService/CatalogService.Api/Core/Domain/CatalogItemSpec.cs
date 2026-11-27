namespace CatalogService.Api.Core.Domain
{
    public class CatalogItemSpec
    {
        public int Id { get; set; }

        public int CatalogItemId { get; set; }

        public LocalizedText Label { get; set; } = new();

        public LocalizedText Value { get; set; } = new();

        public int DisplayOrder { get; set; }
    }
}
