namespace OrderService.Application.Features.Queries.ViewModels
{
    public class OrderDetailViewModel
    {
        public string OrderNumber { get; init; }

        public DateTime Date { get; init; }

        //submitted, awaitingvalidation, stockconfirmed, paid, shipped, cancelled
        public string Status { get; init; }

        public string Description { get; init; }

        public string ContactName { get; init; }

        public string Phone { get; init; }

        public string Street { get; init; }

        public string City { get; init; }

        public string State { get; init; }

        public string ZipCode { get; init; }

        public string Country { get; init; }

        public string ShippingMethod { get; init; }

        public decimal ShippingFee { get; init; }

        public string CardLast4 { get; set; }

        public List<OrderItemViewModel> OrderItems { get; set; }

        public decimal ItemsTotal { get; set; }

        public decimal Total { get; set; }
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; init; }

        public string ProductSlug { get; init; }

        public string ProductName { get; init; }

        public int Units { get; init; }

        public decimal UnitPrice { get; init; }

        public string PictureUrl { get; init; }

        public int? SellerId { get; init; }

        public string SellerName { get; init; }
    }
}
