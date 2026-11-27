using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.AggregateModels.BuyerAggregate;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.EntityConfigurations
{
    public class PaymentMethodEntityConfiguration : IEntityTypeConfiguration<PaymentMethod>
    {
        public void Configure(EntityTypeBuilder<PaymentMethod> builder)
        {
            builder.ToTable("paymentMethods", OrderDbContext.DEFAULT_SCHEMA);

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Ignore(p => p.DomainEvents);

            builder.Property(p => p.Alias).HasMaxLength(200).IsRequired();
            builder.Property(p => p.CardHolderName).HasMaxLength(200).IsRequired();
            builder.Property(p => p.CardNumber).HasMaxLength(25).IsRequired();
            builder.Property(p => p.SecurityNumber).HasMaxLength(5).IsRequired();
            builder.Property(p => p.Expiration).IsRequired();

            builder.HasOne(p => p.CardType)
                .WithMany()
                .HasForeignKey(p => p.CardTypeId);
        }
    }
}
