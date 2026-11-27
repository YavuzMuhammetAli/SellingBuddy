namespace Web.ApiGateway.Models
{
    public class AddBasketItemRequest
    {
        public int CatalogItemId { get; set; }

        public int Quantity { get; set; } = 1;
    }
}
