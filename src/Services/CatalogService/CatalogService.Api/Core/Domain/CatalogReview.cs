namespace CatalogService.Api.Core.Domain
{
    public class CatalogReview
    {
        public int Id { get; set; }

        public int CatalogItemId { get; set; }

        public string AuthorName { get; set; }

        //1 to 5
        public int Rating { get; set; }

        public LocalizedText Comment { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
