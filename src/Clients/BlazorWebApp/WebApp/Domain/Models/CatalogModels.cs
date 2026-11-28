using System.Globalization;

namespace WebApp.Domain.Models
{
    public class ProductSummary
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public string CategorySlug { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int DiscountPercent { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string Badge { get; set; }
        public string PictureUri { get; set; }
        public int AvailableStock { get; set; }
        public int? OfferId { get; set; }
        public int? SellerId { get; set; }
        public string SellerSlug { get; set; }
        public string SellerName { get; set; }
        public bool FreeShipping { get; set; }
        public int OfferCount { get; set; }
    }

    public class ProductDetail : ProductSummary
    {
        public string Description { get; set; }
        public List<BreadcrumbItem> Breadcrumb { get; set; } = new();
        public List<ProductSpec> Specs { get; set; } = new();
        public List<Offer> Offers { get; set; } = new();
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }

    public class BreadcrumbItem
    {
        public string Slug { get; set; }
        public string Name { get; set; }
    }

    public class ProductSpec
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public class Offer
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public string SellerSlug { get; set; }
        public string SellerName { get; set; }
        public string SellerInitials { get; set; }
        public bool SellerVerified { get; set; }
        public double SellerRating { get; set; }
        public int SellerSalesCount { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int Stock { get; set; }
        public decimal ShippingFee { get; set; }
        public bool FreeShipping { get; set; }
        public int DeliveryDaysMin { get; set; }
        public int DeliveryDaysMax { get; set; }
        public string Condition { get; set; }
        public bool IsBestOffer { get; set; }
    }

    public class Review
    {
        public string AuthorName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string LanguageCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUri { get; set; }
        public int ProductCount { get; set; }
        public string ParentSlug { get; set; }
        public List<Category> Children { get; set; } = new();
    }

    public class Seller
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
        public List<FacetValue> Categories { get; set; } = new();
    }

    public class Campaign
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
        public List<FacetValue> Categories { get; set; } = new();
    }

    public class Facets
    {
        public long Total { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public List<FacetValue> Categories { get; set; } = new();
        public List<FacetValue> Brands { get; set; } = new();
        public List<FacetValue> Sellers { get; set; } = new();
    }

    public class FacetValue
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }

    public class PagedResult<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public long Count { get; set; }
        public List<T> Data { get; set; } = new();

        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Count / (double)PageSize);
    }

    //filters of the catalog items endpoint
    public class CatalogQuery
    {
        public string Category { get; set; }
        public List<int> Brands { get; set; } = new();
        public List<string> Sellers { get; set; } = new();
        public string Q { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public double? MinRating { get; set; }
        public bool InStock { get; set; }
        public bool OnSale { get; set; }
        public string Campaign { get; set; }
        public string Sort { get; set; } = "recommended";
        public int PageIndex { get; set; }
        public int PageSize { get; set; } = 12;

        public int ActiveFilterCount => Brands.Count + Sellers.Count + (MinPrice.HasValue ? 1 : 0) + (MaxPrice.HasValue ? 1 : 0)
            + (MinRating.HasValue ? 1 : 0) + (InStock ? 1 : 0) + (OnSale ? 1 : 0);

        public string ToQueryString()
        {
            var parts = new List<string>();

            void Add(string key, string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    parts.Add($"{key}={Uri.EscapeDataString(value)}");
            }

            Add("category", Category);
            Add("brands", string.Join(',', Brands));
            Add("sellers", string.Join(',', Sellers));
            Add("q", Q);
            Add("minPrice", MinPrice?.ToString(CultureInfo.InvariantCulture));
            Add("maxPrice", MaxPrice?.ToString(CultureInfo.InvariantCulture));
            Add("minRating", MinRating?.ToString(CultureInfo.InvariantCulture));
            Add("inStock", InStock ? "true" : null);
            Add("onSale", OnSale ? "true" : null);
            Add("campaign", Campaign);
            Add("sort", Sort);
            Add("pageIndex", PageIndex.ToString(CultureInfo.InvariantCulture));
            Add("pageSize", PageSize.ToString(CultureInfo.InvariantCulture));

            return string.Join('&', parts);
        }

        public CatalogQuery Clone() => new()
        {
            Category = Category, Brands = Brands.ToList(), Sellers = Sellers.ToList(), Q = Q, MinPrice = MinPrice, MaxPrice = MaxPrice,
            MinRating = MinRating, InStock = InStock, OnSale = OnSale, Campaign = Campaign, Sort = Sort, PageIndex = PageIndex, PageSize = PageSize
        };
    }
}
