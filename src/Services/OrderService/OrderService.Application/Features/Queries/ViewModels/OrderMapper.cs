using OrderService.Domain.AggregateModels.OrderAggregate;

namespace OrderService.Application.Features.Queries.ViewModels
{
    //manual mapping, no automapper
    public static class OrderMapper
    {
        public static OrderItemViewModel ToItem(OrderItem item) => new()
        {
            ProductId = item.ProductId,
            ProductSlug = item.ProductSlug,
            ProductName = item.ProductName,
            Units = item.Units,
            UnitPrice = item.UnitPrice,
            PictureUrl = item.PictureUrl,
            SellerId = item.SellerId,
            SellerName = item.SellerName
        };
    }
}
