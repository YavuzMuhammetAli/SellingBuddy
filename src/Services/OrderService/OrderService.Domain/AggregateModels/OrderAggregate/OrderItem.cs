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

        //used by ef core
        protected OrderItem()
        {
        }

        public OrderItem(int productId, string productName, decimal unitPrice, string pictureUrl, int units = 1)
        {
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
