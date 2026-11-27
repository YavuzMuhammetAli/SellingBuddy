namespace CatalogService.Api.Core.Application
{
    //query string of GET api/catalog/items, all filters optional
    public class CatalogItemQuery
    {
        //category slug, parent categories include their children
        public string Category { get; set; }

        //comma separated brand ids
        public string Brands { get; set; }

        //comma separated seller slugs
        public string Sellers { get; set; }

        public string Q { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public double? MinRating { get; set; }

        public bool InStock { get; set; }

        public bool OnSale { get; set; }

        //campaign slug
        public string Campaign { get; set; }

        //recommended, bestsellers, newest, price_asc, price_desc, rating, discount
        public string Sort { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; } = 12;

        //legacy ids=1,2,3 lookup
        public string Ids { get; set; }

        public IReadOnlyList<int> BrandIds => ParseInts(Brands);

        public IReadOnlyList<string> SellerSlugs => ParseStrings(Sellers);

        public IReadOnlyList<int> ItemIds => ParseInts(Ids);

        private static List<int> ParseInts(string value) => string.IsNullOrWhiteSpace(value)
            ? new List<int>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(v => int.TryParse(v.Trim(), out var id) ? id : (int?)null)
                .Where(v => v.HasValue)
                .Select(v => v.Value)
                .ToList();

        private static List<string> ParseStrings(string value) => string.IsNullOrWhiteSpace(value)
            ? new List<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim().ToLowerInvariant()).ToList();
    }
}
