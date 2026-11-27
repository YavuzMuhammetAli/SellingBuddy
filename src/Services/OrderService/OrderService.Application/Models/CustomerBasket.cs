namespace OrderService.Application.Models
{
    //copy of basket service model, comes inside OrderCreatedIntegrationEvent
    public class CustomerBasket
    {
        public string BuyerId { get; set; }

        public List<BasketItem> Items { get; set; } = new List<BasketItem>();
    }
}
