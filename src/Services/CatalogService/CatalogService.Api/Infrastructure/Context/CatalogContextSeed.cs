using CatalogService.Api.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextSeed
    {
        public async Task SeedAsync(CatalogContext context, ILogger<CatalogContextSeed> logger)
        {
            //seed runs only on an empty catalog
            if (!await context.CatalogBrands.AnyAsync())
            {
                await context.CatalogBrands.AddRangeAsync(GetPreconfiguredCatalogBrands());
                await context.SaveChangesAsync();
            }

            if (!await context.CatalogTypes.AnyAsync())
            {
                await context.CatalogTypes.AddRangeAsync(GetPreconfiguredCatalogTypes());
                await context.SaveChangesAsync();
            }

            if (!await context.CatalogItems.AnyAsync())
            {
                var brands = await context.CatalogBrands.ToDictionaryAsync(b => b.Brand, b => b.Id);
                var types = await context.CatalogTypes.ToDictionaryAsync(t => t.Type, t => t.Id);

                await context.CatalogItems.AddRangeAsync(GetPreconfiguredItems(brands, types));
                await context.SaveChangesAsync();

                logger.LogInformation("Catalog database seeded");
            }
        }

        private static IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrands() => new List<CatalogBrand>
        {
            new CatalogBrand { Brand = "Azure" },
            new CatalogBrand { Brand = ".NET" },
            new CatalogBrand { Brand = "Visual Studio" },
            new CatalogBrand { Brand = "SQL Server" },
            new CatalogBrand { Brand = "Other" }
        };

        private static IEnumerable<CatalogType> GetPreconfiguredCatalogTypes() => new List<CatalogType>
        {
            new CatalogType { Type = "Mug" },
            new CatalogType { Type = "T-Shirt" },
            new CatalogType { Type = "Sheet" },
            new CatalogType { Type = "USB Memory Stick" }
        };

        private static IEnumerable<CatalogItem> GetPreconfiguredItems(Dictionary<string, int> brands, Dictionary<string, int> types)
        {
            CatalogItem Item(string name, string description, decimal price, string type, string brand, int picture, int stock) => new CatalogItem
            {
                Name = name,
                Description = description,
                Price = price,
                CatalogTypeId = types[type],
                CatalogBrandId = brands[brand],
                PictureFileName = $"{picture}.svg",
                AvailableStock = stock
            };

            return new List<CatalogItem>
            {
                Item(".NET Bot Black Hoodie", "Black hoodie with the .NET bot", 19.5m, "T-Shirt", ".NET", 1, 100),
                Item(".NET Black & White Mug", "Two tone .NET mug", 8.5m, "Mug", ".NET", 2, 89),
                Item("Prism White T-Shirt", "White t-shirt with prism print", 12m, "T-Shirt", "Other", 3, 56),
                Item(".NET Foundation T-Shirt", "Official .NET Foundation t-shirt", 12m, "T-Shirt", ".NET", 4, 120),
                Item("Roslyn Red Sheet", "Red sticker sheet for Roslyn fans", 8.5m, "Sheet", "Visual Studio", 5, 55),
                Item(".NET Blue Hoodie", "Blue hoodie with the .NET logo", 12m, "T-Shirt", ".NET", 6, 17),
                Item("Roslyn Red T-Shirt", "Red t-shirt with Roslyn print", 12m, "T-Shirt", "Visual Studio", 7, 8),
                Item("Kudu Purple Hoodie", "Purple hoodie for Azure fans", 8.5m, "T-Shirt", "Azure", 8, 34),
                Item("Cup<T> White Mug", "Generic white mug", 12m, "Mug", "Other", 9, 76),
                Item(".NET Foundation Sheet", "Sticker sheet with .NET Foundation logos", 12m, "Sheet", ".NET", 10, 11),
                Item("SQL Server USB Stick", "16 GB usb memory stick", 15m, "USB Memory Stick", "SQL Server", 11, 3),
                Item("Azure Blue USB Stick", "32 GB usb memory stick", 18m, "USB Memory Stick", "Azure", 12, 0)
            };
        }
    }
}
