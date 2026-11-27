using EventBus.Base.Events;
using OrderService.Application.Models;

namespace OrderService.Application.IntegrationEvents.Events
{
    public class OrderCreatedIntegrationEvent : IntegrationEvent
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public int OrderNumber { get; set; }

        //decided by the basket service at checkout
        public Guid OrderId { get; set; }

        public string ContactName { get; set; }

        public string Phone { get; set; }

        public string ShippingMethod { get; set; }

        public decimal ShippingFee { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string ZipCode { get; set; }

        public string CardNumber { get; set; }

        public string CardHolderName { get; set; }

        public DateTime CardExpiration { get; set; }

        public string CardSecurityNumber { get; set; }

        public int CardTypeId { get; set; }

        public string Buyer { get; set; }

        public CustomerBasket Basket { get; set; }
    }
}
