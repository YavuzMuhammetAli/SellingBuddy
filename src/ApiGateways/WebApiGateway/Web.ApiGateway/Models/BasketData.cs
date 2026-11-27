namespace Web.ApiGateway.Models
{
    public class BasketData
    {
        public string BuyerId { get; set; }

        public List<BasketDataItem> Items { get; set; } = new List<BasketDataItem>();

        public BasketData()
        {
        }

        public BasketData(string buyerId)
        {
            BuyerId = buyerId;
        }
    }

    public class BasketDataItem
    {
        public string Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal OldUnitPrice { get; set; }

        public int Quantity { get; set; }

        public string PictureUrl { get; set; }

        public string ProductSlug { get; set; }

        public string BrandName { get; set; }

        public int? OfferId { get; set; }

        public int? SellerId { get; set; }

        public string SellerSlug { get; set; }

        public string SellerName { get; set; }

        public decimal ShippingFee { get; set; }

        public int DeliveryDaysMax { get; set; }
    }
}
