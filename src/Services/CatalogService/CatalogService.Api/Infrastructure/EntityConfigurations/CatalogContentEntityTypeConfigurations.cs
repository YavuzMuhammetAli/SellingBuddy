using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Api.Infrastructure.EntityConfigurations
{
    public class CatalogItemSpecEntityTypeConfiguration : IEntityTypeConfiguration<CatalogItemSpec>
    {
        public void Configure(EntityTypeBuilder<CatalogItemSpec> builder)
        {
            builder.ToTable("CatalogItemSpec", CatalogContext.DEFAULT_SCHEMA);

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).UseHiLo("catalog_spec_hilo");

            builder.HasLocalizedText(s => s.Label, 100);
            builder.HasLocalizedText(s => s.Value, 300);
        }
    }

    public class CatalogReviewEntityTypeConfiguration : IEntityTypeConfiguration<CatalogReview>
    {
        public void Configure(EntityTypeBuilder<CatalogReview> builder)
        {
            builder.ToTable("CatalogReview", CatalogContext.DEFAULT_SCHEMA);

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).UseHiLo("catalog_review_hilo");

            builder.Property(r => r.AuthorName).HasMaxLength(100).IsRequired();

            builder.Property(r => r.Comment).HasMaxLength(2000).IsRequired();
            builder.Property(r => r.LanguageCode).HasMaxLength(8).IsRequired();
        }
    }

    public class CatalogCampaignEntityTypeConfiguration : IEntityTypeConfiguration<CatalogCampaign>
    {
        public void Configure(EntityTypeBuilder<CatalogCampaign> builder)
        {
            builder.ToTable("CatalogCampaign", CatalogContext.DEFAULT_SCHEMA);

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).UseHiLo("catalog_campaign_hilo");

            builder.Property(c => c.Slug).HasMaxLength(100).IsRequired();
            builder.HasIndex(c => c.Slug).IsUnique();

            builder.Property(c => c.HeroFileName).HasMaxLength(200);

            builder.HasLocalizedText(c => c.Title, 150);
            builder.HasLocalizedText(c => c.Description, 500);
            builder.HasLocalizedText(c => c.BadgeText, 100);
        }
    }
}
