namespace CatalogService.Api.Core.Application.ViewModels
{
    //card data, also read by the gateway basket aggregator (Id, Name, Price, PictureUri)
    public class ProductSummaryViewModel
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

    public class ProductDetailViewModel : ProductSummaryViewModel
    {
        public string Description { get; set; }

        public List<BreadcrumbItemViewModel> Breadcrumb { get; set; } = new();

        public List<ProductSpecViewModel> Specs { get; set; } = new();

        public List<OfferViewModel> Offers { get; set; } = new();

        //star -> count, keys 1 to 5
        public Dictionary<int, int> RatingDistribution { get; set; } = new();

        public List<ReviewViewModel> Reviews { get; set; } = new();
    }

    public class BreadcrumbItemViewModel
    {
        public string Slug { get; set; }

        public string Name { get; set; }
    }

    public class ProductSpecViewModel
    {
        public string Label { get; set; }

        public string Value { get; set; }
    }

    public class OfferViewModel
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

        public bool FreeShipping => ShippingFee == 0;

        public int DeliveryDaysMin { get; set; }

        public int DeliveryDaysMax { get; set; }

        public string Condition { get; set; }

        public bool IsBestOffer { get; set; }
    }

    public class ReviewViewModel
    {
        public string AuthorName { get; set; }

        public int Rating { get; set; }

        public string Comment { get; set; }

        public string LanguageCode { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
