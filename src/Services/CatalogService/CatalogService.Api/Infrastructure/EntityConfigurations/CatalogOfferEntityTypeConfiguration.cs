using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Api.Infrastructure.EntityConfigurations
{
    public class CatalogOfferEntityTypeConfiguration : IEntityTypeConfiguration<CatalogOffer>
    {
        public void Configure(EntityTypeBuilder<CatalogOffer> builder)
        {
            builder.ToTable("CatalogOffer", CatalogContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .UseHiLo("catalog_offer_hilo")
                .IsRequired();

            builder.Property(o => o.Price).HasPrecision(18, 2);
            builder.Property(o => o.OldPrice).HasPrecision(18, 2);
            builder.Property(o => o.ShippingFee).HasPrecision(18, 2);

            builder.HasLocalizedText(o => o.Condition, 100);

            //one offer per seller and product
            builder.HasIndex(o => new { o.CatalogItemId, o.CatalogSellerId }).IsUnique();
        }
    }
}
