using System.Globalization;

namespace WebApp.Infrastructure
{
    //culture aware display formats from the design system
    public static class Fmt
    {
        private static CultureInfo Culture => CultureInfo.CurrentCulture;

        private static bool IsTurkish => Culture.TwoLetterISOLanguageName == "tr";

        //12.499 TL in turkish, 12,499 TL in english
        public static string Price(decimal value) =>
            $"{value.ToString(value % 1 == 0 ? "N0" : "N2", Culture)} TL";

        //4,9 in turkish, 4.9 in english
        public static string Rating(double value) => value.ToString("0.0", Culture);

        public static string Count(long value) => value.ToString("N0", Culture);

        //-%18 in turkish, -18% in english
        public static string Discount(int percent) => IsTurkish ? $"-%{percent}" : $"-{percent}%";

        public static string Percent(int percent) => IsTurkish ? $"%{percent}" : $"{percent}%";

        public static string Date(DateTime value) => value.ToLocalTime().ToString("d MMMM yyyy", Culture);

        public static string ShortDateTime(DateTime value) => value.ToLocalTime().ToString("d MMM, HH:mm", Culture);

        //readable order number from the order guid, e.g. SB-2026-84920
        public static string OrderNumber(string orderId, DateTime date)
        {
            if (!Guid.TryParse(orderId, out var guid))
                return orderId;

            var number = BitConverter.ToUInt32(guid.ToByteArray(), 0) % 100000;
            return $"SB-{date:yyyy}-{number:D5}";
        }
    }
}
