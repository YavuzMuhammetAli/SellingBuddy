namespace OrderService.Application.Features.Queries.ViewModels
{
    public class OrderSummaryViewModel
    {
        public string OrderNumber { get; init; }

        public DateTime Date { get; init; }

        public string Status { get; init; }

        public decimal Total { get; init; }

        public int ItemCount { get; init; }

        public List<OrderItemViewModel> Items { get; init; } = new();

        //short address for account overview
        public string City { get; init; }

        public string Street { get; init; }
    }
}
