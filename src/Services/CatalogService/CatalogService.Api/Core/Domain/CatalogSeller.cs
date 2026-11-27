namespace CatalogService.Api.Core.Domain
{
    public class CatalogSeller
    {
        public int Id { get; set; }

        public string Slug { get; set; }

        public string Name { get; set; }

        public string Initials { get; set; }

        public LocalizedText Tagline { get; set; } = new();

        public string City { get; set; }

        public double Rating { get; set; }

        public int ReviewCount { get; set; }

        public int SalesCount { get; set; }

        public bool IsVerified { get; set; }

        //percent of orders shipped on time
        public int OnTimeShippingRate { get; set; }

        public string BannerFileName { get; set; }

        public List<CatalogOffer> Offers { get; set; } = new();
    }
}
