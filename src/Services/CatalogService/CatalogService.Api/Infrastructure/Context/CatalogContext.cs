using CatalogService.Api.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContext : DbContext
    {
        public const string DEFAULT_SCHEMA = "catalog";

        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
        {
        }

        public DbSet<CatalogItem> CatalogItems { get; set; }

        public DbSet<CatalogBrand> CatalogBrands { get; set; }

        public DbSet<CatalogType> CatalogTypes { get; set; }

        public DbSet<CatalogSeller> CatalogSellers { get; set; }

        public DbSet<CatalogOffer> CatalogOffers { get; set; }

        public DbSet<CatalogItemSpec> CatalogItemSpecs { get; set; }

        public DbSet<CatalogReview> CatalogReviews { get; set; }

        public DbSet<CatalogCampaign> CatalogCampaigns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogContext).Assembly);
        }
    }
}
