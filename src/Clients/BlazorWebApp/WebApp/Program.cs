using HealthChecks.Common;
using Microsoft.AspNetCore.Components.Authorization;
using WebApp.Application.Services;
using WebApp.Application.Services.Interfaces;
using WebApp.Components;
using WebApp.Infrastructure;
using WebApp.Infrastructure.Auth;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddDefaultHealthChecks();

            //gateway http client added, webapp only talks to the gateway
            builder.Services.AddHttpClient<GatewayClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"]);
            });

            //scoped services, one per user circuit
            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<ICatalogService, CatalogService>();
            builder.Services.AddScoped<IBasketService, BasketService>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            //auth state provider added
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddAuthorizationCore();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
                app.UseExceptionHandler("/Error");

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

            app.UseAntiforgery();

            app.MapDefaultHealthChecks();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
