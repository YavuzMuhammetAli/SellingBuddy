namespace WebApp.Domain.Models
{
    public class OrderSummary
    {
        public string OrderNumber { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public string City { get; set; }
        public string Street { get; set; }
    }

    public class OrderDetail
    {
        public string OrderNumber { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string ShippingMethod { get; set; }
        public decimal ShippingFee { get; set; }
        public string CardLast4 { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
        public decimal ItemsTotal { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductSlug { get; set; }
        public string ProductName { get; set; }
        public int Units { get; set; }
        public decimal UnitPrice { get; set; }
        public string PictureUrl { get; set; }
        public int? SellerId { get; set; }
        public string SellerName { get; set; }
    }

    //design statuses, mapped from the order service values
    public enum OrderStage
    {
        Preparing,
        Shipped,
        Delivered,
        Cancelled
    }

    public static class OrderStatusMap
    {
        public static OrderStage ToStage(string status) => status?.ToLowerInvariant() switch
        {
            "shipped" => OrderStage.Shipped,
            "cancelled" => OrderStage.Cancelled,
            _ => OrderStage.Preparing
        };

        public static bool IsPaid(string status) => status?.ToLowerInvariant() is "paid" or "shipped";
    }
}
