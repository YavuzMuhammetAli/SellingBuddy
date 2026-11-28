namespace BasketService.Api.Core.Domain.Models
{
    //fees are decided here, the client only sends the method
    public static class ShippingOptions
    {
        public const string Standard = "standard";

        public const string SameDay = "same-day";

        //standard shipping is paid per seller, the offer fee of each seller group
        public static decimal FeeFor(string method, CustomerBasket basket) => method switch
        {
            SameDay => 89m,
            _ => basket.Items.GroupBy(i => i.SellerId).Sum(g => g.Max(i => i.ShippingFee))
        };

        public static string Normalize(string method) => method == SameDay ? SameDay : Standard;
    }
}
