using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.EntityConfigurations
{
    public class BuyerEntityConfiguration : IEntityTypeConfiguration<Buyer>
    {
        public void Configure(EntityTypeBuilder<Buyer> builder)
        {
            builder.ToTable("buyers", OrderDbContext.DEFAULT_SCHEMA);

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).ValueGeneratedNever();

            builder.Ignore(b => b.DomainEvents);

            builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
            builder.HasIndex(b => b.Name).IsUnique();

            builder.HasMany(b => b.PaymentMethods)
                .WithOne()
                .HasForeignKey(p => p.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(b => b.PaymentMethods).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
