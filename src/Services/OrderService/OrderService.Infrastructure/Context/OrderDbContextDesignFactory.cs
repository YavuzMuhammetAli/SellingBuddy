using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderService.Infrastructure.Context
{
    //used by dotnet ef for migrations, mediator not needed there
    public class OrderDbContextDesignFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>()
                .UseSqlServer("Server=localhost,1435;Database=order;User Id=sa;Password=Pass@word1;TrustServerCertificate=True");

            return new OrderDbContext(optionsBuilder.Options, null);
        }
    }
}
