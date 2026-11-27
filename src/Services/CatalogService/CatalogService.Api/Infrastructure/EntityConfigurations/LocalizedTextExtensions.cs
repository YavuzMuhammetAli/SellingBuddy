using CatalogService.Api.Core.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace CatalogService.Api.Infrastructure.EntityConfigurations
{
    public static class LocalizedTextExtensions
    {
        //maps a localized text to {Name}_En and {Name}_Tr columns
        public static EntityTypeBuilder<T> HasLocalizedText<T>(this EntityTypeBuilder<T> builder, Expression<Func<T, LocalizedText>> property, int? maxLength = null) where T : class
        {
            builder.ComplexProperty(property, text =>
            {
                var en = text.Property(t => t.En).IsRequired();
                var tr = text.Property(t => t.Tr).IsRequired();

                if (maxLength.HasValue)
                {
                    en.HasMaxLength(maxLength.Value);
                    tr.HasMaxLength(maxLength.Value);
                }
            });

            return builder;
        }
    }
}
