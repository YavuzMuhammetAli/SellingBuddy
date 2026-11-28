using HealthChecks.Common;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using WebApp.Application;
using WebApp.Application.Services;
using WebApp.Application.Services.Interfaces;
using WebApp.Components;
using WebApp.Infrastructure;
using WebApp.Infrastructure.Auth;

namespace WebApp
{
    public class Program
    {
        public static readonly string[] SupportedCultures = { "tr-TR", "en-US" };

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddDefaultHealthChecks();

            //ui texts from resx files, turkish is the default
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            //gateway http client added, webapp only talks to the gateway
            builder.Services.AddHttpClient<GatewayClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"]);
            });

            //scoped services, one per user circuit
            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<ICatalogService, CatalogService>();
            builder.Services.AddScoped<IBasketService, BasketService>();
            builder.Services.AddScoped<IWishlistService, WishlistService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<StorefrontState>();

            //auth state provider added
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddAuthorizationCore();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
                app.UseExceptionHandler("/Error");

            //cookie wins, browser language is ignored so turkish stays the default
            var localization = new RequestLocalizationOptions()
                .SetDefaultCulture(SupportedCultures[0])
                .AddSupportedCultures(SupportedCultures)
                .AddSupportedUICultures(SupportedCultures);
            localization.RequestCultureProviders = new List<IRequestCultureProvider> { new CookieRequestCultureProvider() };
            app.UseRequestLocalization(localization);

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

            app.UseAntiforgery();

            app.MapDefaultHealthChecks();

            //language switch writes the culture cookie and goes back
            app.MapGet("/culture/set", (string culture, string redirectUri, HttpContext context) =>
            {
                if (SupportedCultures.Contains(culture))
                {
                    context.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)),
                        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true, SameSite = SameSiteMode.Lax });
                }

                var target = string.IsNullOrEmpty(redirectUri) || !redirectUri.StartsWith('/') || redirectUri.StartsWith("//") ? "/" : redirectUri;
                return Results.LocalRedirect(target);
            });

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
