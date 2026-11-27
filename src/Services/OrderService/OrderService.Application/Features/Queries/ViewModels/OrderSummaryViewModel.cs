namespace OrderService.Application.Features.Queries.ViewModels
{
    public class OrderSummaryViewModel
    {
        public string OrderNumber { get; init; }

        public DateTime Date { get; init; }

        public string Status { get; init; }

        public decimal Total { get; init; }
    }
}
