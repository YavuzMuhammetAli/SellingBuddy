using OrderService.Domain.Exceptions;
using OrderService.Domain.SeedWork;

namespace OrderService.Domain.AggregateModels.OrderAggregate
{
    public class OrderItem : BaseEntity
    {
        public int ProductId { get; private set; }

        public string ProductName { get; private set; }

        public string PictureUrl { get; private set; }

        public decimal UnitPrice { get; private set; }

        public int Units { get; private set; }

        public Guid OrderId { get; private set; }

        //seller of the offer the item was bought from
        public int? SellerId { get; private set; }

        public string SellerName { get; private set; }

        public string ProductSlug { get; private set; }

        //used by ef core
        protected OrderItem()
        {
        }

        public OrderItem(int productId, string productName, decimal unitPrice, string pictureUrl, int units = 1,
            int? sellerId = null, string sellerName = null, string productSlug = null)
        {
            SellerId = sellerId;
            SellerName = sellerName;
            ProductSlug = productSlug;
            if (units <= 0)
                throw new OrderingDomainException("Invalid number of units");

            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            PictureUrl = pictureUrl;
            Units = units;
        }

        public void AddUnits(int units)
        {
            if (units < 0)
                throw new OrderingDomainException("Invalid units");

            Units += units;
        }
    }
}
