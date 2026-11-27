using MediatR;
using OrderService.Application.Models;

namespace OrderService.Application.Features.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<bool>
    {
        private readonly List<OrderItemDto> orderItems;

        public Guid OrderId { get; private set; }

        public string ContactName { get; private set; }

        public string Phone { get; private set; }

        public string ShippingMethod { get; private set; }

        public decimal ShippingFee { get; private set; }

        public string UserName { get; private set; }

        public string City { get; private set; }

        public string Street { get; private set; }

        public string State { get; private set; }

        public string Country { get; private set; }

        public string ZipCode { get; private set; }

        public string CardNumber { get; private set; }

        public string CardHolderName { get; private set; }

        public DateTime CardExpiration { get; private set; }

        public string CardSecurityNumber { get; private set; }

        public int CardTypeId { get; private set; }

        public IEnumerable<OrderItemDto> OrderItems => orderItems;

        public CreateOrderCommand(List<BasketItem> basketItems, string userName, string city, string street,
            string state, string country, string zipCode, string cardNumber, string cardHolderName,
            DateTime cardExpiration, string cardSecurityNumber, int cardTypeId)
        {
            //basket items mapped to order item dtos
            orderItems = basketItems.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                PictureUrl = item.PictureUrl,
                UnitPrice = item.UnitPrice,
                Units = item.Quantity,
                SellerId = item.SellerId,
                SellerName = item.SellerName,
                ProductSlug = item.ProductSlug
            }).ToList();

            UserName = userName;
            City = city;
            Street = street;
            State = state;
            Country = country;
            ZipCode = zipCode;
            CardNumber = cardNumber;
            CardHolderName = cardHolderName;
            CardExpiration = cardExpiration;
            CardSecurityNumber = cardSecurityNumber;
            CardTypeId = cardTypeId;
        }

        public CreateOrderCommand WithCheckoutDetails(Guid orderId, string contactName, string phone, string shippingMethod, decimal shippingFee)
        {
            OrderId = orderId;
            ContactName = contactName;
            Phone = phone;
            ShippingMethod = shippingMethod;
            ShippingFee = shippingFee;
            return this;
        }
    }

    public class OrderItemDto
    {
        public int ProductId { get; init; }

        public string ProductName { get; init; }

        public decimal UnitPrice { get; init; }

        public int Units { get; init; }

        public string PictureUrl { get; init; }

        public int? SellerId { get; init; }

        public string SellerName { get; init; }

        public string ProductSlug { get; init; }
    }
}
