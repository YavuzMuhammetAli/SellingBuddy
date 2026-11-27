namespace CatalogService.Api.Core.Domain
{
    public class CatalogItem
    {
        public int Id { get; set; }

        public string Slug { get; set; }

        public LocalizedText Name { get; set; } = new();

        public LocalizedText Description { get; set; } = new();

        //best offer values, kept on the item for sorting and filtering
        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        public int AvailableStock { get; set; }

        public string PictureFileName { get; set; }

        public bool OnReorder { get; set; }

        public double Rating { get; set; }

        public int ReviewCount { get; set; }

        //BestSeller, FreeShipping or FastDelivery
        public string Badge { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CatalogTypeId { get; set; }

        public CatalogType CatalogType { get; set; }

        public int CatalogBrandId { get; set; }

        public CatalogBrand CatalogBrand { get; set; }

        public List<CatalogOffer> Offers { get; set; } = new();

        public List<CatalogItemSpec> Specs { get; set; } = new();

        public List<CatalogReview> Reviews { get; set; } = new();

        public List<CatalogCampaign> Campaigns { get; set; } = new();

        public int DiscountPercent => OldPrice.HasValue && OldPrice.Value > Price && OldPrice.Value > 0
            ? (int)Math.Round((OldPrice.Value - Price) / OldPrice.Value * 100)
            : 0;

        //best offer becomes the item price
        public void RefreshFromOffers()
        {
            if (!Offers.Any())
                return;

            var best = Offers.OrderBy(o => o.Price + o.ShippingFee).First();

            Price = best.Price;
            OldPrice = best.OldPrice;
            AvailableStock = Offers.Sum(o => o.Stock);
        }
    }
}
