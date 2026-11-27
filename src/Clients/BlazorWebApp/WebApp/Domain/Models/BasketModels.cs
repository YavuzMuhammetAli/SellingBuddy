using System.ComponentModel.DataAnnotations;

namespace WebApp.Domain.Models
{
    public class Basket
    {
        public string BuyerId { get; set; }

        public List<BasketItem> Items { get; set; } = new List<BasketItem>();

        public decimal Total() => Math.Round(Items.Sum(x => x.UnitPrice * x.Quantity), 2);
    }

    public class BasketItem
    {
        public string Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal OldUnitPrice { get; set; }

        public int Quantity { get; set; }

        public string PictureUrl { get; set; }
    }

    public class BasketCheckout
    {
        [Required]
        public string City { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string ZipCode { get; set; }

        [Required, CreditCard]
        public string CardNumber { get; set; }

        [Required]
        public string CardHolderName { get; set; }

        //card must not be expired, order service checks it
        public DateTime CardExpiration { get; set; }

        [Required, StringLength(4, MinimumLength = 3)]
        public string CardSecurityNumber { get; set; }

        public int CardTypeId { get; set; } = 2;

        public string Buyer { get; set; }
    }
}
