namespace Web.ApiGateway.Models
{
    public class CatalogItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string PictureUri { get; set; }

        public string Slug { get; set; }

        public string BrandName { get; set; }

        public List<CatalogOffer> Offers { get; set; } = new();
    }

    public class CatalogOffer
    {
        public int Id { get; set; }

        public int SellerId { get; set; }

        public string SellerSlug { get; set; }

        public string SellerName { get; set; }

        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        public int Stock { get; set; }

        public decimal ShippingFee { get; set; }

        public int DeliveryDaysMax { get; set; }

        public bool IsBestOffer { get; set; }
    }
}
