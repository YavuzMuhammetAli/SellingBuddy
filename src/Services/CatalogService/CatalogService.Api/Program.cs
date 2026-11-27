using CatalogService.Api.Extensions;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CatalogService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //logging class added
            builder.Host.UseSerilog((context, loggerConfig) =>
                loggerConfig.ReadFrom.Configuration(context.Configuration));

            //async suffix kept for CreatedAtAction routes
            builder.Services.AddControllers(options => options.SuppressAsyncSuffixInActionNames = false);
            builder.Services.AddOpenApi();
            builder.Services.AddHealthChecks();

            builder.Services.Configure<CatalogSettings>(builder.Configuration.GetSection("CatalogSettings"));

            //sql server context added
            builder.Services.AddDbContext<CatalogContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDb"), sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                }));

            //consul registration added
            builder.Services.ConfigureConsul(builder.Configuration);

            var app = builder.Build();

            //database migrated and seeded on startup
            app.MigrateDbContext<CatalogContext>((context, services) =>
            {
                var logger = services.GetRequiredService<ILogger<CatalogContextSeed>>();
                return new CatalogContextSeed().SeedAsync(context, logger);
            });

            if (app.Environment.IsDevelopment())
                app.MapOpenApi();

            app.UseSerilogRequestLogging();

            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            app.RegisterWithConsul(app.Lifetime, builder.Configuration);

            app.Run();
        }
    }
}
