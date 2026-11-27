using CatalogService.Api.Core.Domain;

namespace CatalogService.Api.Infrastructure.Setup
{
    //shape of catalog-seed.json, texts are bilingual
    public class CatalogSeedModel
    {
        public List<string> Brands { get; set; } = new();

        public List<SellerSeed> Sellers { get; set; } = new();

        public List<CategorySeed> Categories { get; set; } = new();

        public List<ProductSeed> Products { get; set; } = new();

        public List<CampaignSeed> Campaigns { get; set; } = new();
    }

    public class SellerSeed
    {
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Initials { get; set; }
        public LocalizedText Tagline { get; set; }
        public string City { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public int SalesCount { get; set; }
        public bool Verified { get; set; }
        public int OnTimeShippingRate { get; set; }
        public string Banner { get; set; }
    }

    public class CategorySeed
    {
        public string Slug { get; set; }
        public LocalizedText Name { get; set; }
        public LocalizedText Description { get; set; }
        public string Image { get; set; }
        public List<CategorySeed> Children { get; set; } = new();
    }

    public class ProductSeed
    {
        public string Slug { get; set; }
        public LocalizedText Name { get; set; }
        public LocalizedText Description { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string Picture { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string Badge { get; set; }
        public int CreatedDaysAgo { get; set; }
        public List<OfferSeed> Offers { get; set; } = new();
        public List<SpecSeed> Specs { get; set; } = new();
        public List<ReviewSeed> Reviews { get; set; } = new();
    }

    public class OfferSeed
    {
        public string Seller { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int Stock { get; set; }
        public decimal Shipping { get; set; }
        public int DaysMin { get; set; }
        public int DaysMax { get; set; }
        public LocalizedText Condition { get; set; }
    }

    public class SpecSeed
    {
        public LocalizedText Label { get; set; }
        public LocalizedText Value { get; set; }
    }

    public class ReviewSeed
    {
        public string Author { get; set; }
        public int Rating { get; set; }
        public LocalizedText Comment { get; set; }
        public int DaysAgo { get; set; }
    }

    public class CampaignSeed
    {
        public string Slug { get; set; }
        public LocalizedText Title { get; set; }
        public LocalizedText Description { get; set; }
        public LocalizedText Badge { get; set; }
        public int MaxDiscountPercent { get; set; }
        public int EndsInDays { get; set; }
        public string Hero { get; set; }
        public List<string> Products { get; set; } = new();
    }
}
