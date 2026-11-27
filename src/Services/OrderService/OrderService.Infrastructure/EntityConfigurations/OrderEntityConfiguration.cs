using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Domain.AggregateModels.OrderAggregate;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.EntityConfigurations
{
    public class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders", OrderDbContext.DEFAULT_SCHEMA);

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedNever();

            builder.Ignore(o => o.DomainEvents);

            //address stored in order table
            builder.OwnsOne(o => o.Address, a =>
            {
                a.WithOwner();
                a.Property(x => x.Street).HasMaxLength(200);
                a.Property(x => x.City).HasMaxLength(100);
                a.Property(x => x.State).HasMaxLength(100);
                a.Property(x => x.Country).HasMaxLength(100);
                a.Property(x => x.ZipCode).HasMaxLength(20);
            });

            builder.Property(o => o.OrderDate).IsRequired();
            builder.Property(o => o.Description).HasMaxLength(500);

            builder.HasOne(o => o.OrderStatus)
                .WithMany()
                .HasForeignKey(o => o.OrderStatusId);

            //restrict to avoid multiple cascade paths on sql server
            builder.HasOne(o => o.Buyer)
                .WithMany()
                .HasForeignKey(o => o.BuyerId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PaymentMethod>()
                .WithMany()
                .HasForeignKey(o => o.PaymentMethodId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.OrderItems)
                .WithOne()
                .HasForeignKey(i => i.OrderId);

            builder.Navigation(o => o.OrderItems).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
