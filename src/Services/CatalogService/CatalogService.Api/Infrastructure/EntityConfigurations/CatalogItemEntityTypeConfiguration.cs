using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Api.Infrastructure.EntityConfigurations
{
    public class CatalogItemEntityTypeConfiguration : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable("Catalog", CatalogContext.DEFAULT_SCHEMA);

            builder.Property(ci => ci.Id)
                .UseHiLo("catalog_hilo")
                .IsRequired();

            builder.Property(ci => ci.Slug).HasMaxLength(150).IsRequired();
            builder.HasIndex(ci => ci.Slug).IsUnique();

            builder.HasLocalizedText(ci => ci.Name, 150);
            builder.HasLocalizedText(ci => ci.Description, 2000);

            builder.Property(ci => ci.Price).HasPrecision(18, 2).IsRequired();
            builder.Property(ci => ci.OldPrice).HasPrecision(18, 2);
            builder.Property(ci => ci.PictureFileName).IsRequired(false);
            builder.Property(ci => ci.Badge).HasMaxLength(30);

            builder.Ignore(ci => ci.DiscountPercent);

            builder.HasOne(ci => ci.CatalogBrand)
                .WithMany()
                .HasForeignKey(ci => ci.CatalogBrandId);

            builder.HasOne(ci => ci.CatalogType)
                .WithMany()
                .HasForeignKey(ci => ci.CatalogTypeId);

            builder.HasMany(ci => ci.Offers)
                .WithOne(o => o.CatalogItem)
                .HasForeignKey(o => o.CatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ci => ci.Specs)
                .WithOne()
                .HasForeignKey(s => s.CatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ci => ci.Reviews)
                .WithOne()
                .HasForeignKey(r => r.CatalogItemId)
                .OnDelete(DeleteBehavior.Cascade);

            //campaign items are a plain join table
            builder.HasMany(ci => ci.Campaigns)
                .WithMany(c => c.Items)
                .UsingEntity(j => j.ToTable("CatalogCampaignItem", CatalogContext.DEFAULT_SCHEMA));
        }
    }
}
