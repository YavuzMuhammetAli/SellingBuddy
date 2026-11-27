namespace CatalogService.Api.Core.Domain
{
    //one seller selling one product, a product can have many offers
    public class CatalogOffer
    {
        public int Id { get; set; }

        public int CatalogItemId { get; set; }

        public CatalogItem CatalogItem { get; set; }

        public int CatalogSellerId { get; set; }

        public CatalogSeller CatalogSeller { get; set; }

        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        public int Stock { get; set; }

        public decimal ShippingFee { get; set; }

        public int DeliveryDaysMin { get; set; }

        public int DeliveryDaysMax { get; set; }

        public LocalizedText Condition { get; set; } = new();
    }
}
