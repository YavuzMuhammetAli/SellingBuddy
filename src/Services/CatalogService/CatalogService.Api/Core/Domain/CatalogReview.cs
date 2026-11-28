namespace CatalogService.Api.Core.Domain
{
    public class CatalogReview
    {
        public int Id { get; set; }

        public int CatalogItemId { get; set; }

        public string AuthorName { get; set; }

        //1 to 5
        public int Rating { get; set; }

        //reviews keep the language they were written in, they are never translated
        public string Comment { get; set; }

        public string LanguageCode { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
