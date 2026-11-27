namespace CatalogService.Api.Core.Application.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        public string Slug { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageUri { get; set; }

        public int ProductCount { get; set; }

        public string ParentSlug { get; set; }

        public List<CategoryViewModel> Children { get; set; } = new();
    }

    public class SellerViewModel
    {
        public int Id { get; set; }

        public string Slug { get; set; }

        public string Name { get; set; }

        public string Initials { get; set; }

        public string Tagline { get; set; }

        public string City { get; set; }

        public double Rating { get; set; }

        public int ReviewCount { get; set; }

        public int SalesCount { get; set; }

        public bool IsVerified { get; set; }

        public int OnTimeShippingRate { get; set; }

        public string BannerUri { get; set; }

        public int ProductCount { get; set; }

        public List<FacetValueViewModel> Categories { get; set; } = new();
    }

    public class CampaignViewModel
    {
        public int Id { get; set; }

        public string Slug { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string BadgeText { get; set; }

        public int MaxDiscountPercent { get; set; }

        public DateTime EndsAt { get; set; }

        public string HeroUri { get; set; }

        public int ProductCount { get; set; }

        public List<FacetValueViewModel> Categories { get; set; } = new();
    }

    public class BrandViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    //counts for filter sidebars and chips
    public class FacetsViewModel
    {
        public long Total { get; set; }

        public decimal MinPrice { get; set; }

        public decimal MaxPrice { get; set; }

        public List<FacetValueViewModel> Categories { get; set; } = new();

        public List<FacetValueViewModel> Brands { get; set; } = new();

        public List<FacetValueViewModel> Sellers { get; set; } = new();
    }

    public class FacetValueViewModel
    {
        //slug for categories and sellers, id for brands
        public string Key { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }
    }
}
