using Consul;
using HealthChecks.Common;
using Microsoft.Extensions.Options;
using StatusDashboard.Application;
using StatusDashboard.Components;
using StatusDashboard.Core.Options;
using StatusDashboard.Infrastructure.Clients;
using System.Net.Http.Headers;
using System.Text;
using TabBlazor;

namespace StatusDashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            //tabblazor components and services added
            builder.Services.AddTabBlazor();

            builder.Services.Configure<MonitoringOptions>(builder.Configuration.GetSection("Monitoring"));
            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMQ"));

            //consul client for the service catalog
            builder.Services.AddSingleton<IConsulClient>(_ => new ConsulClient(config =>
            {
                config.Address = new Uri(builder.Configuration["Consul:Address"]);
            }));
            builder.Services.AddTransient<ConsulStatusClient>();

            builder.Services.AddHttpClient<HealthEndpointClient>(client => client.Timeout = TimeSpan.FromSeconds(5));

            //management api uses basic auth
            builder.Services.AddHttpClient<RabbitMqManagementClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.UserName}:{options.Password}"));

                client.BaseAddress = new Uri(options.ManagementUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            });

            //one store for all users, filled by the background monitor
            builder.Services.AddSingleton<StatusStore>();
            builder.Services.AddHostedService<StatusMonitorService>();

            builder.Services.AddDefaultHealthChecks();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
                app.UseExceptionHandler("/Error", createScopeForErrors: true);

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
