using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.AggregateModels.OrderAggregate;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.EntityConfigurations
{
    public class OrderItemEntityConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("orderItems", OrderDbContext.DEFAULT_SCHEMA);

            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedNever();

            builder.Ignore(i => i.DomainEvents);

            builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
            builder.Property(i => i.PictureUrl).HasMaxLength(500);
            builder.Property(i => i.UnitPrice).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.Units).IsRequired();
            builder.Property(i => i.SellerName).HasMaxLength(100);
            builder.Property(i => i.ProductSlug).HasMaxLength(150);
        }
    }
}
