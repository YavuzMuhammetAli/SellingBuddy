using CatalogService.Api.Core.Application.ViewModels;
using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace CatalogService.Api.Core.Application
{
    //read side of the catalog, texts resolved for the request culture
    public class CatalogQueryService
    {
        private readonly CatalogContext _context;
        private readonly CatalogSettings _settings;

        public CatalogQueryService(CatalogContext context, IOptionsSnapshot<CatalogSettings> settings)
        {
            _context = context;
            _settings = settings.Value;
        }

        private static bool IsTurkish => string.Equals(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "tr", StringComparison.OrdinalIgnoreCase);

        private static string Pick(string en, string tr) => IsTurkish && !string.IsNullOrEmpty(tr) ? tr : (string.IsNullOrEmpty(en) ? tr : en);

        private string ItemPictureUri(int id) => _settings.PicBaseUrl.Replace("[0]", id.ToString());

        private string FileUri(string fileName) => string.IsNullOrEmpty(fileName) ? null : $"{_settings.FileBaseUrl.TrimEnd('/')}/{fileName}";

        #region items

        public async Task<PaginatedItemsViewModel<ProductSummaryViewModel>> GetItemsAsync(CatalogItemQuery query, CancellationToken cancellationToken)
        {
            var filtered = await ApplyFiltersAsync(_context.CatalogItems.AsNoTracking(), query, cancellationToken);

            var total = await filtered.LongCountAsync(cancellationToken);
            var pageSize = Math.Clamp(query.PageSize, 1, 60);
            var pageIndex = Math.Max(0, query.PageIndex);

            var rows = await Project(ApplySort(filtered, query.Sort))
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedItemsViewModel<ProductSummaryViewModel>(pageIndex, pageSize, total, rows.Select(ToSummary).ToList());
        }

        public async Task<List<ProductSummaryViewModel>> GetItemsByIdsAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken)
        {
            var rows = await Project(_context.CatalogItems.AsNoTracking().Where(i => ids.Contains(i.Id))).ToListAsync(cancellationToken);
            return rows.Select(ToSummary).ToList();
        }

        public async Task<FacetsViewModel> GetFacetsAsync(CatalogItemQuery query, CancellationToken cancellationToken)
        {
            //facets use only the scope (category, search, campaign), so other filters keep their counts
            var scopeQuery = new CatalogItemQuery { Category = query.Category, Q = query.Q, Campaign = query.Campaign };
            var scope = await ApplyFiltersAsync(_context.CatalogItems.AsNoTracking(), scopeQuery, cancellationToken);

            var categoryCounts = await scope.GroupBy(i => i.CatalogTypeId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var brandCounts = await scope.GroupBy(i => i.CatalogBrandId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var sellerCounts = await _context.CatalogOffers.AsNoTracking()
                .Where(o => scope.Any(i => i.Id == o.CatalogItemId))
                .GroupBy(o => new { o.CatalogSeller.Slug, o.CatalogSeller.Name })
                .Select(g => new { g.Key.Slug, g.Key.Name, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var typeIds = categoryCounts.Select(c => c.Key).ToList();
            var types = await _context.CatalogTypes.AsNoTracking().Where(t => typeIds.Contains(t.Id)).ToListAsync(cancellationToken);

            var brandIds = brandCounts.Select(b => b.Key).ToList();
            var brands = await _context.CatalogBrands.AsNoTracking().Where(b => brandIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id, b => b.Brand, cancellationToken);

            return new FacetsViewModel
            {
                Total = await scope.LongCountAsync(cancellationToken),
                MinPrice = await scope.MinAsync(i => (decimal?)i.Price, cancellationToken) ?? 0,
                MaxPrice = await scope.MaxAsync(i => (decimal?)i.Price, cancellationToken) ?? 0,
                Categories = categoryCounts
                    .Join(types, c => c.Key, t => t.Id, (c, t) => new FacetValueViewModel { Key = t.Slug, Name = t.Name.Resolve(), Count = c.Count })
                    .OrderByDescending(f => f.Count).ToList(),
                Brands = brandCounts
                    .Select(b => new FacetValueViewModel { Key = b.Key.ToString(), Name = brands.GetValueOrDefault(b.Key), Count = b.Count })
                    .OrderBy(f => f.Name).ToList(),
                Sellers = sellerCounts
                    .Select(s => new FacetValueViewModel { Key = s.Slug, Name = s.Name, Count = s.Count })
                    .OrderBy(f => f.Name).ToList()
            };
        }

        public async Task<ProductDetailViewModel> GetDetailAsync(int? id, string slug, CancellationToken cancellationToken)
        {
            var item = await _context.CatalogItems.AsNoTracking()
                .Include(i => i.CatalogBrand)
                .Include(i => i.CatalogType).ThenInclude(t => t.ParentCatalogType)
                .Include(i => i.Offers).ThenInclude(o => o.CatalogSeller)
                .Include(i => i.Specs)
                .Include(i => i.Reviews)
                .AsSplitQuery()
                .FirstOrDefaultAsync(i => id.HasValue ? i.Id == id.Value : i.Slug == slug, cancellationToken);

            if (item == null)
                return null;

            var offers = item.Offers.OrderBy(o => o.Price + o.ShippingFee).ToList();
            var best = offers.FirstOrDefault();

            var detail = new ProductDetailViewModel
            {
                Id = item.Id,
                Slug = item.Slug,
                Name = item.Name.Resolve(),
                Description = item.Description.Resolve(),
                BrandName = item.CatalogBrand?.Brand,
                CategorySlug = item.CatalogType?.Slug,
                CategoryName = item.CatalogType?.Name.Resolve(),
                Price = item.Price,
                OldPrice = item.OldPrice,
                DiscountPercent = item.DiscountPercent,
                Rating = item.Rating,
                ReviewCount = item.ReviewCount,
                Badge = item.Badge,
                PictureUri = ItemPictureUri(item.Id),
                AvailableStock = item.AvailableStock,
                OfferId = best?.Id,
                SellerId = best?.CatalogSellerId,
                SellerSlug = best?.CatalogSeller.Slug,
                SellerName = best?.CatalogSeller.Name,
                FreeShipping = best?.ShippingFee == 0,
                OfferCount = offers.Count,
                Specs = item.Specs.OrderBy(s => s.DisplayOrder)
                    .Select(s => new ProductSpecViewModel { Label = s.Label.Resolve(), Value = s.Value.Resolve() }).ToList(),
                Offers = offers.Select((o, index) => new OfferViewModel
                {
                    Id = o.Id,
                    SellerId = o.CatalogSellerId,
                    SellerSlug = o.CatalogSeller.Slug,
                    SellerName = o.CatalogSeller.Name,
                    SellerInitials = o.CatalogSeller.Initials,
                    SellerVerified = o.CatalogSeller.IsVerified,
                    SellerRating = o.CatalogSeller.Rating,
                    SellerSalesCount = o.CatalogSeller.SalesCount,
                    Price = o.Price,
                    OldPrice = o.OldPrice,
                    Stock = o.Stock,
                    ShippingFee = o.ShippingFee,
                    DeliveryDaysMin = o.DeliveryDaysMin,
                    DeliveryDaysMax = o.DeliveryDaysMax,
                    Condition = o.Condition.Resolve(),
                    IsBestOffer = index == 0
                }).ToList(),
                RatingDistribution = BuildDistribution(item),
                Reviews = item.Reviews.OrderByDescending(r => r.CreatedAt).Take(5)
                    .Select(r => new ReviewViewModel { AuthorName = r.AuthorName, Rating = r.Rating, Comment = r.Comment.Resolve(), CreatedAt = r.CreatedAt }).ToList()
            };

            //parent category first, then the item category
            if (item.CatalogType?.ParentCatalogType != null)
                detail.Breadcrumb.Add(new BreadcrumbItemViewModel { Slug = item.CatalogType.ParentCatalogType.Slug, Name = item.CatalogType.ParentCatalogType.Name.Resolve() });

            if (item.CatalogType != null)
                detail.Breadcrumb.Add(new BreadcrumbItemViewModel { Slug = item.CatalogType.Slug, Name = item.CatalogType.Name.Resolve() });

            return detail;
        }

        public async Task<List<ReviewViewModel>> GetReviewsAsync(int itemId, CancellationToken cancellationToken)
        {
            var reviews = await _context.CatalogReviews.AsNoTracking()
                .Where(r => r.CatalogItemId == itemId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

            return reviews.Select(r => new ReviewViewModel { AuthorName = r.AuthorName, Rating = r.Rating, Comment = r.Comment.Resolve(), CreatedAt = r.CreatedAt }).ToList();
        }

        //seeded reviews give the shape, total review count gives the size
        private static Dictionary<int, int> BuildDistribution(CatalogItem item)
        {
            var result = Enumerable.Range(1, 5).ToDictionary(star => star, _ => 0);

            if (!item.Reviews.Any() || item.ReviewCount == 0)
                return result;

            foreach (var group in item.Reviews.GroupBy(r => Math.Clamp(r.Rating, 1, 5)))
                result[group.Key] = (int)Math.Round((double)group.Count() / item.Reviews.Count * item.ReviewCount);

            return result;
        }

        private async Task<IQueryable<CatalogItem>> ApplyFiltersAsync(IQueryable<CatalogItem> items, CatalogItemQuery query, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                var slug = query.Category.Trim().ToLowerInvariant();

                var typeIds = await _context.CatalogTypes.AsNoTracking()
                    .Where(t => t.Slug == slug || (t.ParentCatalogType != null && t.ParentCatalogType.Slug == slug))
                    .Select(t => t.Id)
                    .ToListAsync(cancellationToken);

                items = items.Where(i => typeIds.Contains(i.CatalogTypeId));
            }

            if (query.ItemIds.Any())
            {
                var ids = query.ItemIds;
                items = items.Where(i => ids.Contains(i.Id));
            }

            if (query.BrandIds.Any())
            {
                var brandIds = query.BrandIds;
                items = items.Where(i => brandIds.Contains(i.CatalogBrandId));
            }

            if (query.SellerSlugs.Any())
            {
                var sellerSlugs = query.SellerSlugs;
                items = items.Where(i => i.Offers.Any(o => sellerSlugs.Contains(o.CatalogSeller.Slug)));
            }

            if (!string.IsNullOrWhiteSpace(query.Q))
            {
                var term = query.Q.Trim();
                items = items.Where(i => i.Name.En.Contains(term) || i.Name.Tr.Contains(term)
                    || i.CatalogBrand.Brand.Contains(term)
                    || i.CatalogType.Name.En.Contains(term) || i.CatalogType.Name.Tr.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(query.Campaign))
            {
                var campaign = query.Campaign.Trim().ToLowerInvariant();
                items = items.Where(i => i.Campaigns.Any(c => c.Slug == campaign));
            }

            if (query.MinPrice.HasValue)
                items = items.Where(i => i.Price >= query.MinPrice.Value);

            if (query.MaxPrice.HasValue)
                items = items.Where(i => i.Price <= query.MaxPrice.Value);

            if (query.MinRating.HasValue)
                items = items.Where(i => i.Rating >= query.MinRating.Value);

            if (query.InStock)
                items = items.Where(i => i.AvailableStock > 0);

            if (query.OnSale)
                items = items.Where(i => i.OldPrice.HasValue && i.OldPrice > i.Price);

            return items;
        }

        private static IQueryable<CatalogItem> ApplySort(IQueryable<CatalogItem> items, string sort) => sort?.ToLowerInvariant() switch
        {
            "bestsellers" => items.OrderByDescending(i => i.ReviewCount),
            "newest" => items.OrderByDescending(i => i.CreatedAt),
            "price_asc" => items.OrderBy(i => i.Price),
            "price_desc" => items.OrderByDescending(i => i.Price),
            "rating" => items.OrderByDescending(i => i.Rating).ThenByDescending(i => i.ReviewCount),
            "discount" => items.OrderByDescending(i => i.OldPrice.HasValue && i.OldPrice > 0 ? (i.OldPrice.Value - i.Price) / i.OldPrice.Value : 0),
            //badged and well rated items first
            _ => items.OrderByDescending(i => i.Badge != null).ThenByDescending(i => i.Rating).ThenByDescending(i => i.ReviewCount)
        };

        private static IQueryable<SummaryRow> Project(IQueryable<CatalogItem> items) => items.Select(i => new SummaryRow
        {
            Id = i.Id,
            Slug = i.Slug,
            NameEn = i.Name.En,
            NameTr = i.Name.Tr,
            BrandName = i.CatalogBrand.Brand,
            CategorySlug = i.CatalogType.Slug,
            CategoryEn = i.CatalogType.Name.En,
            CategoryTr = i.CatalogType.Name.Tr,
            Price = i.Price,
            OldPrice = i.OldPrice,
            Rating = i.Rating,
            ReviewCount = i.ReviewCount,
            Badge = i.Badge,
            AvailableStock = i.AvailableStock,
            OfferCount = i.Offers.Count(),
            BestOffer = i.Offers.OrderBy(o => o.Price + o.ShippingFee)
                .Select(o => new BestOfferRow
                {
                    OfferId = o.Id,
                    SellerId = o.CatalogSellerId,
                    SellerSlug = o.CatalogSeller.Slug,
                    SellerName = o.CatalogSeller.Name,
                    ShippingFee = o.ShippingFee
                })
                .FirstOrDefault()
        });

        private ProductSummaryViewModel ToSummary(SummaryRow row) => new()
        {
            Id = row.Id,
            Slug = row.Slug,
            Name = Pick(row.NameEn, row.NameTr),
            BrandName = row.BrandName,
            CategorySlug = row.CategorySlug,
            CategoryName = Pick(row.CategoryEn, row.CategoryTr),
            Price = row.Price,
            OldPrice = row.OldPrice,
            DiscountPercent = row.OldPrice.HasValue && row.OldPrice > row.Price && row.OldPrice > 0
                ? (int)Math.Round((row.OldPrice.Value - row.Price) / row.OldPrice.Value * 100)
                : 0,
            Rating = row.Rating,
            ReviewCount = row.ReviewCount,
            Badge = row.Badge,
            PictureUri = ItemPictureUri(row.Id),
            AvailableStock = row.AvailableStock,
            OfferId = row.BestOffer?.OfferId,
            SellerId = row.BestOffer?.SellerId,
            SellerSlug = row.BestOffer?.SellerSlug,
            SellerName = row.BestOffer?.SellerName,
            FreeShipping = row.BestOffer?.ShippingFee == 0,
            OfferCount = row.OfferCount
        };

        private class SummaryRow
        {
            public int Id { get; set; }
            public string Slug { get; set; }
            public string NameEn { get; set; }
            public string NameTr { get; set; }
            public string BrandName { get; set; }
            public string CategorySlug { get; set; }
            public string CategoryEn { get; set; }
            public string CategoryTr { get; set; }
            public decimal Price { get; set; }
            public decimal? OldPrice { get; set; }
            public double Rating { get; set; }
            public int ReviewCount { get; set; }
            public string Badge { get; set; }
            public int AvailableStock { get; set; }
            public int OfferCount { get; set; }
            public BestOfferRow BestOffer { get; set; }
        }

        private class BestOfferRow
        {
            public int OfferId { get; set; }
            public int SellerId { get; set; }
            public string SellerSlug { get; set; }
            public string SellerName { get; set; }
            public decimal ShippingFee { get; set; }
        }

        #endregion

        #region directory

        public async Task<List<CategoryViewModel>> GetCategoriesAsync(CancellationToken cancellationToken)
        {
            var types = await _context.CatalogTypes.AsNoTracking().OrderBy(t => t.DisplayOrder).ToListAsync(cancellationToken);
            var counts = await _context.CatalogItems.AsNoTracking().GroupBy(i => i.CatalogTypeId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Key, g => g.Count, cancellationToken);

            return types.Where(t => t.ParentCatalogTypeId == null).Select(t => ToCategory(t, types, counts)).ToList();
        }

        public async Task<CategoryViewModel> GetCategoryAsync(string slug, CancellationToken cancellationToken)
        {
            var categories = await GetCategoriesAsync(cancellationToken);

            return categories.SelectMany(c => c.Children.Prepend(c))
                .FirstOrDefault(c => string.Equals(c.Slug, slug, StringComparison.OrdinalIgnoreCase));
        }

        private CategoryViewModel ToCategory(CatalogType type, List<CatalogType> all, Dictionary<int, int> counts)
        {
            var children = all.Where(t => t.ParentCatalogTypeId == type.Id).OrderBy(t => t.DisplayOrder).ToList();

            return new CategoryViewModel
            {
                Id = type.Id,
                Slug = type.Slug,
                Name = type.Name.Resolve(),
                Description = type.Description.Resolve(),
                ImageUri = FileUri(type.ImageFileName),
                ParentSlug = all.FirstOrDefault(t => t.Id == type.ParentCatalogTypeId)?.Slug,
                ProductCount = counts.GetValueOrDefault(type.Id) + children.Sum(c => counts.GetValueOrDefault(c.Id)),
                Children = children.Select(c => ToCategory(c, all, counts)).ToList()
            };
        }

        public async Task<List<SellerViewModel>> GetSellersAsync(CancellationToken cancellationToken)
        {
            var sellers = await _context.CatalogSellers.AsNoTracking()
                .OrderByDescending(s => s.Rating).ThenByDescending(s => s.SalesCount)
                .Select(s => new { Seller = s, ProductCount = s.Offers.Count() })
                .ToListAsync(cancellationToken);

            return sellers.Select(s => ToSeller(s.Seller, s.ProductCount)).ToList();
        }

        public async Task<SellerViewModel> GetSellerAsync(string slug, CancellationToken cancellationToken)
        {
            var seller = await _context.CatalogSellers.AsNoTracking().FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);

            if (seller == null)
                return null;

            var model = ToSeller(seller, await _context.CatalogOffers.CountAsync(o => o.CatalogSellerId == seller.Id, cancellationToken));
            model.Categories = await CountTopCategoriesAsync(_context.CatalogItems.Where(i => i.Offers.Any(o => o.CatalogSellerId == seller.Id)), cancellationToken);

            return model;
        }

        private SellerViewModel ToSeller(CatalogSeller seller, int productCount) => new()
        {
            Id = seller.Id,
            Slug = seller.Slug,
            Name = seller.Name,
            Initials = seller.Initials,
            Tagline = seller.Tagline.Resolve(),
            City = seller.City,
            Rating = seller.Rating,
            ReviewCount = seller.ReviewCount,
            SalesCount = seller.SalesCount,
            IsVerified = seller.IsVerified,
            OnTimeShippingRate = seller.OnTimeShippingRate,
            BannerUri = FileUri(seller.BannerFileName),
            ProductCount = productCount
        };

        public async Task<List<CampaignViewModel>> GetCampaignsAsync(CancellationToken cancellationToken)
        {
            var campaigns = await _context.CatalogCampaigns.AsNoTracking()
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new { Campaign = c, ProductCount = c.Items.Count() })
                .ToListAsync(cancellationToken);

            return campaigns.Select(c => ToCampaign(c.Campaign, c.ProductCount)).ToList();
        }

        public async Task<CampaignViewModel> GetCampaignAsync(string slug, CancellationToken cancellationToken)
        {
            var campaign = await _context.CatalogCampaigns.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

            if (campaign == null)
                return null;

            var items = _context.CatalogItems.Where(i => i.Campaigns.Any(c => c.Id == campaign.Id));

            var model = ToCampaign(campaign, await items.CountAsync(cancellationToken));
            model.Categories = await CountTopCategoriesAsync(items, cancellationToken);

            return model;
        }

        private CampaignViewModel ToCampaign(CatalogCampaign campaign, int productCount) => new()
        {
            Id = campaign.Id,
            Slug = campaign.Slug,
            Title = campaign.Title.Resolve(),
            Description = campaign.Description.Resolve(),
            BadgeText = campaign.BadgeText.Resolve(),
            MaxDiscountPercent = campaign.MaxDiscountPercent,
            EndsAt = campaign.EndsAt,
            HeroUri = FileUri(campaign.HeroFileName),
            ProductCount = productCount
        };

        //items grouped by top level category, used for pill filters
        private async Task<List<FacetValueViewModel>> CountTopCategoriesAsync(IQueryable<CatalogItem> items, CancellationToken cancellationToken)
        {
            var counts = await items.AsNoTracking()
                .GroupBy(i => i.CatalogType.ParentCatalogTypeId ?? i.CatalogTypeId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var ids = counts.Select(c => c.Key).ToList();
            var types = await _context.CatalogTypes.AsNoTracking().Where(t => ids.Contains(t.Id)).ToListAsync(cancellationToken);

            return counts.Join(types, c => c.Key, t => t.Id, (c, t) => new FacetValueViewModel { Key = t.Slug, Name = t.Name.Resolve(), Count = c.Count })
                .OrderByDescending(f => f.Count)
                .ToList();
        }

        public async Task<List<BrandViewModel>> GetBrandsAsync(CancellationToken cancellationToken)
        {
            return await _context.CatalogBrands.AsNoTracking()
                .OrderBy(b => b.Brand)
                .Select(b => new BrandViewModel { Id = b.Id, Name = b.Brand })
                .ToListAsync(cancellationToken);
        }

        #endregion
    }
}
