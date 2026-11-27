namespace BasketService.Api.Core.Domain.Models
{
    //fees are decided here, the client only sends the method
    public static class ShippingOptions
    {
        public const string Standard = "standard";

        public const string SameDay = "same-day";

        public static decimal FeeFor(string method) => method switch
        {
            SameDay => 89m,
            _ => 0m
        };

        public static string Normalize(string method) => method == SameDay ? SameDay : Standard;
    }
}
