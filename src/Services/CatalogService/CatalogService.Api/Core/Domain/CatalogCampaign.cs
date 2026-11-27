namespace CatalogService.Api.Core.Domain
{
    public class CatalogCampaign
    {
        public int Id { get; set; }

        public string Slug { get; set; }

        public LocalizedText Title { get; set; } = new();

        public LocalizedText Description { get; set; } = new();

        public LocalizedText BadgeText { get; set; } = new();

        public int MaxDiscountPercent { get; set; }

        public DateTime EndsAt { get; set; }

        public string HeroFileName { get; set; }

        public int DisplayOrder { get; set; }

        public List<CatalogItem> Items { get; set; } = new();
    }
}
