using System.Globalization;

namespace CatalogService.Api.Core.Domain
{
    //stored as two columns, en is the fallback language
    public class LocalizedText
    {
        public string En { get; set; } = string.Empty;

        public string Tr { get; set; } = string.Empty;

        public LocalizedText()
        {
        }

        public LocalizedText(string en, string tr)
        {
            En = en ?? string.Empty;
            Tr = tr ?? string.Empty;
        }

        public string Resolve(CultureInfo culture)
        {
            var isTurkish = string.Equals(culture?.TwoLetterISOLanguageName, "tr", StringComparison.OrdinalIgnoreCase);

            if (isTurkish && !string.IsNullOrEmpty(Tr))
                return Tr;

            return string.IsNullOrEmpty(En) ? Tr : En;
        }

        public string Resolve() => Resolve(CultureInfo.CurrentUICulture);
    }
}
