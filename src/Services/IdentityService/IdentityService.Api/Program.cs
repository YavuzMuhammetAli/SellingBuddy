using HealthChecks.Common;
using IdentityService.Api.Application.Models;
using IdentityService.Api.Application.Services;
using IdentityService.Api.Extensions;
using Serilog;

namespace IdentityService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //logging class added
            builder.Host.UseSerilog((context, loggerConfig) =>
                loggerConfig.ReadFrom.Configuration(context.Configuration));

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddDefaultHealthChecks();

            //token settings and service added
            builder.Services.Configure<AuthConfig>(builder.Configuration.GetSection("AuthConfig"));
            builder.Services.AddScoped<IIdentityService, Application.Services.IdentityService>();

            //consul registration added
            builder.Services.ConfigureConsul(builder.Configuration);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
                app.MapOpenApi();

            app.UseSerilogRequestLogging();

            app.MapControllers();
            app.MapDefaultHealthChecks();

            app.RegisterWithConsul(app.Lifetime, builder.Configuration);

            app.Run();
        }
    }
}
