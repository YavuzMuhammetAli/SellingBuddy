using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure.Setup;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextSeed
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task SeedAsync(CatalogContext context, string contentRootPath, ILogger<CatalogContextSeed> logger)
        {
            //seed runs only on an empty catalog
            if (await context.CatalogItems.AnyAsync())
                return;

            var path = Path.Combine(contentRootPath, "Infrastructure", "Setup", "catalog-seed.json");

            if (!File.Exists(path))
            {
                logger.LogWarning("Catalog seed file not found at {Path}", path);
                return;
            }

            var seed = JsonSerializer.Deserialize<CatalogSeedModel>(await File.ReadAllTextAsync(path), JsonOptions);
            var now = DateTime.UtcNow;

            var brands = seed.Brands.Distinct().ToDictionary(b => b, b => new CatalogBrand { Brand = b });

            var sellers = seed.Sellers.ToDictionary(s => s.Slug, s => new CatalogSeller
            {
                Slug = s.Slug,
                Name = s.Name,
                Initials = s.Initials,
                Tagline = s.Tagline ?? new LocalizedText(),
                City = s.City,
                Rating = s.Rating,
                ReviewCount = s.ReviewCount,
                SalesCount = s.SalesCount,
                IsVerified = s.Verified,
                OnTimeShippingRate = s.OnTimeShippingRate,
                BannerFileName = s.Banner
            });

            //parents and children flattened, lookup by slug
            var categories = new Dictionary<string, CatalogType>();
            var order = 0;

            foreach (var parentSeed in seed.Categories)
            {
                var parent = ToType(parentSeed, order++);
                categories[parent.Slug] = parent;

                foreach (var childSeed in parentSeed.Children)
                {
                    var child = ToType(childSeed, order++);
                    child.ParentCatalogType = parent;
                    categories[child.Slug] = child;
                }
            }

            var items = new Dictionary<string, CatalogItem>();

            foreach (var product in seed.Products)
            {
                var item = new CatalogItem
                {
                    Slug = product.Slug,
                    Name = product.Name,
                    Description = product.Description ?? new LocalizedText(),
                    CatalogBrand = brands[product.Brand],
                    CatalogType = categories[product.Category],
                    PictureFileName = product.Picture,
                    Rating = product.Rating,
                    ReviewCount = product.ReviewCount,
                    Badge = product.Badge,
                    CreatedAt = now.AddDays(-product.CreatedDaysAgo),
                    Offers = product.Offers.Select(o => new CatalogOffer
                    {
                        CatalogSeller = sellers[o.Seller],
                        Price = o.Price,
                        OldPrice = o.OldPrice,
                        Stock = o.Stock,
                        ShippingFee = o.Shipping,
                        DeliveryDaysMin = o.DaysMin,
                        DeliveryDaysMax = o.DaysMax,
                        Condition = o.Condition ?? new LocalizedText("New", "Yeni")
                    }).ToList(),
                    Specs = product.Specs.Select((s, index) => new CatalogItemSpec { Label = s.Label, Value = s.Value, DisplayOrder = index }).ToList(),
                    Reviews = product.Reviews.Select(r => new CatalogReview
                    {
                        AuthorName = r.Author,
                        Rating = r.Rating,
                        Comment = r.Comment ?? new LocalizedText(),
                        CreatedAt = now.AddDays(-r.DaysAgo)
                    }).ToList()
                };

                item.RefreshFromOffers();
                items[item.Slug] = item;
            }

            var campaigns = seed.Campaigns.Select((c, index) => new CatalogCampaign
            {
                Slug = c.Slug,
                Title = c.Title,
                Description = c.Description ?? new LocalizedText(),
                BadgeText = c.Badge ?? new LocalizedText(),
                MaxDiscountPercent = c.MaxDiscountPercent,
                EndsAt = now.Date.AddDays(c.EndsInDays),
                HeroFileName = c.Hero,
                DisplayOrder = index,
                Items = c.Products.Where(items.ContainsKey).Select(slug => items[slug]).ToList()
            }).ToList();

            await context.CatalogTypes.AddRangeAsync(categories.Values);
            await context.CatalogItems.AddRangeAsync(items.Values);
            await context.CatalogCampaigns.AddRangeAsync(campaigns);
            await context.SaveChangesAsync();

            logger.LogInformation("Catalog seeded with {ItemCount} items, {SellerCount} sellers and {CampaignCount} campaigns",
                items.Count, sellers.Count, campaigns.Count);
        }

        private static CatalogType ToType(CategorySeed seed, int order) => new()
        {
            Slug = seed.Slug,
            Name = seed.Name,
            Description = seed.Description ?? new LocalizedText(),
            ImageFileName = seed.Image,
            DisplayOrder = order
        };
    }
}
