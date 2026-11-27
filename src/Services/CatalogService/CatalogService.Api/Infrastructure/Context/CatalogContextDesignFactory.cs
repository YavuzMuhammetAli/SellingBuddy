using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatalogService.Api.Infrastructure.Context
{
    //used by dotnet ef for migrations
    public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
    {
        public CatalogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
                .UseSqlServer("Server=localhost,1435;Database=catalog;User Id=sa;Password=Pass@word1;TrustServerCertificate=True");

            return new CatalogContext(optionsBuilder.Options);
        }
    }
}
