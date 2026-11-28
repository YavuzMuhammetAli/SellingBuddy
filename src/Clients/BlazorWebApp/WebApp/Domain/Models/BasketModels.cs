namespace WebApp.Domain.Models
{
    public class Basket
    {
        public string BuyerId { get; set; }

        public List<BasketItem> Items { get; set; } = new();

        public decimal ItemsTotal => Items.Sum(i => i.UnitPrice * i.Quantity);

        //old price used as list price, the difference is the discount
        public decimal ListTotal => Items.Sum(i => (i.OldUnitPrice > i.UnitPrice ? i.OldUnitPrice : i.UnitPrice) * i.Quantity);

        public decimal DiscountTotal => ListTotal - ItemsTotal;

        //one shipping fee per seller group
        public decimal ShippingTotal => Items.GroupBy(i => i.SellerId).Sum(g => g.Max(i => i.ShippingFee));

        public decimal GrandTotal => ItemsTotal + ShippingTotal;

        public int Count => Items.Sum(i => i.Quantity);

        public int SellerCount => Items.Select(i => i.SellerId).Distinct().Count();
    }

    public class BasketItem
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

    //form state of the checkout page
    public class CheckoutForm
    {
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; } = "Turkey";
        public string City { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string ShippingMethod { get; set; } = "standard";
        public string CardNumber { get; set; }
        public string CardExpiry { get; set; }
        public string CardSecurityNumber { get; set; }
        public bool BillingSameAsShipping { get; set; } = true;
    }

    //body of POST basket/checkout
    public class BasketCheckoutRequest
    {
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string ShippingMethod { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public DateTime CardExpiration { get; set; }
        public string CardSecurityNumber { get; set; }
        public int CardTypeId { get; set; }
        public string Buyer { get; set; }
    }

    public class CheckoutResult
    {
        public Guid OrderId { get; set; }
    }
}
