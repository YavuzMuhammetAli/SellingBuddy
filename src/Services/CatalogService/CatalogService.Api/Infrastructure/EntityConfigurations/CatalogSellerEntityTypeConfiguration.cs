using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Api.Infrastructure.EntityConfigurations
{
    public class CatalogSellerEntityTypeConfiguration : IEntityTypeConfiguration<CatalogSeller>
    {
        public void Configure(EntityTypeBuilder<CatalogSeller> builder)
        {
            builder.ToTable("CatalogSeller", CatalogContext.DEFAULT_SCHEMA);

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .UseHiLo("catalog_seller_hilo")
                .IsRequired();

            builder.Property(s => s.Slug).HasMaxLength(100).IsRequired();
            builder.HasIndex(s => s.Slug).IsUnique();

            builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
            builder.Property(s => s.Initials).HasMaxLength(4).IsRequired();
            builder.Property(s => s.City).HasMaxLength(100);
            builder.Property(s => s.BannerFileName).HasMaxLength(200);

            builder.HasLocalizedText(s => s.Tagline, 300);

            builder.HasMany(s => s.Offers)
                .WithOne(o => o.CatalogSeller)
                .HasForeignKey(o => o.CatalogSellerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
