using HealthChecks.Common;
using CatalogService.Api.Core.Application;
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
            //health checks with dependencies added
            builder.Services.AddDefaultHealthChecks()
                .AddDbContextCheck<CatalogContext>("sqlserver", tags: new[] { "dependency" });

            builder.Services.Configure<CatalogSettings>(builder.Configuration.GetSection("CatalogSettings"));
            builder.Services.AddScoped<CatalogQueryService>();

            //texts are returned in the accept-language of the request
            builder.Services.AddLocalization();

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
                return new CatalogContextSeed().SeedAsync(context, builder.Environment.ContentRootPath, logger);
            });

            if (app.Environment.IsDevelopment())
                app.MapOpenApi();

            app.UseSerilogRequestLogging();

            app.UseRequestLocalization(options =>
            {
                var cultures = new[] { "en", "tr" };
                options.SetDefaultCulture("en").AddSupportedCultures(cultures).AddSupportedUICultures(cultures);
            });

            app.UseAuthorization();

            app.MapControllers();
            app.MapDefaultHealthChecks();

            app.RegisterWithConsul(app.Lifetime, builder.Configuration);

            app.Run();
        }
    }
}
