using CatalogService.Api.Core.Domain;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogService.Api.Infrastructure.EntityConfigurations
{
    public class CatalogTypeEntityTypeConfiguration : IEntityTypeConfiguration<CatalogType>
    {
        public void Configure(EntityTypeBuilder<CatalogType> builder)
        {
            builder.ToTable("CatalogType", CatalogContext.DEFAULT_SCHEMA);

            builder.HasKey(ct => ct.Id);

            builder.Property(ct => ct.Id)
                .UseHiLo("catalog_type_hilo")
                .IsRequired();

            builder.Property(ct => ct.Slug).HasMaxLength(100).IsRequired();
            builder.HasIndex(ct => ct.Slug).IsUnique();

            builder.HasLocalizedText(ct => ct.Name, 100);
            builder.HasLocalizedText(ct => ct.Description, 500);

            builder.Property(ct => ct.ImageFileName).HasMaxLength(200);

            //subcategories point to their parent
            builder.HasOne(ct => ct.ParentCatalogType)
                .WithMany(ct => ct.Children)
                .HasForeignKey(ct => ct.ParentCatalogTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
