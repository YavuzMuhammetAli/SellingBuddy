using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Domain.Events;
using OrderService.Domain.SeedWork;

namespace OrderService.Domain.AggregateModels.OrderAggregate
{
    public class Order : BaseEntity, IAggregateRoot
    {
        public DateTime OrderDate { get; private set; }

        public string Description { get; private set; }

        public Guid? BuyerId { get; private set; }

        public Buyer Buyer { get; private set; }

        public Address Address { get; private set; }

        public int OrderStatusId { get; private set; }

        public OrderStatus OrderStatus { get; private set; }

        public Guid? PaymentMethodId { get; private set; }

        public string ShippingMethod { get; private set; }

        public decimal ShippingFee { get; private set; }

        private readonly List<OrderItem> orderItems;

        public IReadOnlyCollection<OrderItem> OrderItems => orderItems;

        //used by ef core
        protected Order()
        {
            orderItems = new List<OrderItem>();
        }

        public Order(string userName, Address address, int cardTypeId, string cardNumber, string cardSecurityNumber,
            string cardHolderName, DateTime cardExpiration, Guid? paymentMethodId, Guid? buyerId = null,
            Guid? orderId = null, string shippingMethod = null, decimal shippingFee = 0) : this()
        {
            //id comes from the basket checkout so the client knows it already
            if (orderId.HasValue && orderId.Value != Guid.Empty)
                Id = orderId.Value;

            ShippingMethod = shippingMethod;
            ShippingFee = shippingFee;
            BuyerId = buyerId;
            PaymentMethodId = paymentMethodId;
            OrderStatusId = OrderStatus.Submitted.Id;
            OrderDate = DateTime.UtcNow;
            Address = address;

            //buyer and payment method checked by domain event handler
            AddOrderStartedDomainEvent(userName, cardTypeId, cardNumber, cardSecurityNumber, cardHolderName, cardExpiration);
        }

        public void AddOrderItem(int productId, string productName, decimal unitPrice, string pictureUrl, int units = 1,
            int? sellerId = null, string sellerName = null, string productSlug = null)
        {
            var existingOrderForProduct = orderItems.SingleOrDefault(o => o.ProductId == productId && o.SellerId == sellerId);

            if (existingOrderForProduct != null)
            {
                existingOrderForProduct.AddUnits(units);
                return;
            }

            orderItems.Add(new OrderItem(productId, productName, unitPrice, pictureUrl, units, sellerId, sellerName, productSlug));
        }

        public void SetBuyerId(Guid buyerId)
        {
            BuyerId = buyerId;
        }

        public void SetPaymentMethodId(Guid paymentMethodId)
        {
            PaymentMethodId = paymentMethodId;
        }

        public void SetPaidStatus()
        {
            if (OrderStatusId == OrderStatus.Submitted.Id || OrderStatusId == OrderStatus.StockConfirmed.Id)
            {
                OrderStatusId = OrderStatus.Paid.Id;
                Description = "The payment was performed";
            }
        }

        public void SetCancelledStatus()
        {
            if (OrderStatusId == OrderStatus.Paid.Id || OrderStatusId == OrderStatus.Shipped.Id)
                throw new Exceptions.OrderingDomainException($"It is not possible to change the order status from {OrderStatusId} to {OrderStatus.Cancelled.Name}.");

            OrderStatusId = OrderStatus.Cancelled.Id;
            Description = "The order was cancelled";
        }

        public decimal GetTotal()
        {
            return orderItems.Sum(o => o.Units * o.UnitPrice);
        }

        public decimal GetGrandTotal()
        {
            return GetTotal() + ShippingFee;
        }

        private void AddOrderStartedDomainEvent(string userName, int cardTypeId, string cardNumber, string cardSecurityNumber,
            string cardHolderName, DateTime cardExpiration)
        {
            var orderStartedDomainEvent = new OrderStartedDomainEvent(this, userName, cardTypeId, cardNumber,
                cardSecurityNumber, cardHolderName, cardExpiration);

            AddDomainEvent(orderStartedDomainEvent);
        }
    }
}
