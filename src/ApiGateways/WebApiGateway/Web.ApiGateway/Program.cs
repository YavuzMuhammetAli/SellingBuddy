using HealthChecks.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Serilog;
using System.Text;
using Web.ApiGateway.Infrastructure;
using Web.ApiGateway.Services;
using Web.ApiGateway.Services.Interfaces;

namespace Web.ApiGateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //ocelot routes added, env vars re-added so docker can override them
            builder.Configuration
                .AddJsonFile("Configurations/ocelot.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            //logging class added
            builder.Host.UseSerilog((context, loggerConfig) =>
                loggerConfig.ReadFrom.Configuration(context.Configuration));

            //ocelot with consul added
            builder.Services.AddOcelot(builder.Configuration).AddConsul<ServiceAddressConsulServiceBuilder>();

            //cors for blazor webapp
            builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", policy =>
                policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()));

            //jwt auth for aggregator endpoints
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthConfig:Secret"]))
                    };
                });

            //gateway health with consul reachability
            builder.Services.AddDefaultHealthChecks()
                .AddTcpCheck("consul",
                    builder.Configuration["GlobalConfiguration:ServiceDiscoveryProvider:Host"] ?? "localhost",
                    builder.Configuration.GetValue("GlobalConfiguration:ServiceDiscoveryProvider:Port", 8500));

            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<HttpClientDelegatingHandler>();

            //typed clients call the gateway itself, ocelot routes them
            var gatewayBaseUrl = builder.Configuration["urls:gateway"];

            builder.Services.AddHttpClient<ICatalogService, CatalogService>(c => c.BaseAddress = new Uri(gatewayBaseUrl))
                .AddHttpMessageHandler<HttpClientDelegatingHandler>();

            builder.Services.AddHttpClient<IBasketService, BasketService>(c => c.BaseAddress = new Uri(gatewayBaseUrl))
                .AddHttpMessageHandler<HttpClientDelegatingHandler>();

            var app = builder.Build();

            app.UseSerilogRequestLogging();

            app.UseCors("CorsPolicy");

            //aggregator endpoints run before ocelot, unmatched requests fall through
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultHealthChecks();
            });

            //ocelot must be the last middleware
            await app.UseOcelot();

            await app.RunAsync();
        }
    }
}
