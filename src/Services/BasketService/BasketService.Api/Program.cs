using HealthChecks.Common;
using BasketService.Api.Core.Application.Repository;
using BasketService.Api.Core.Application.Services;
using BasketService.Api.Extensions;
using BasketService.Api.Infrastructure.Repository;
using BasketService.Api.IntegrationEvents.EventHandlers;
using BasketService.Api.IntegrationEvents.Events;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using RabbitMQ.Client;
using Serilog;

namespace BasketService.Api
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
            //health checks with dependencies added
            builder.Services.AddDefaultHealthChecks()
                .AddCheck<RedisHealthCheck>("redis", tags: new[] { "dependency" })
                .AddTcpCheck("rabbitmq", builder.Configuration["RabbitMQ:Host"] ?? "localhost", 5672);
            builder.Services.AddHttpContextAccessor();

            builder.Services.ConfigureAuth(builder.Configuration);

            //redis basket store added
            builder.Services.ConfigureRedis(builder.Configuration);
            builder.Services.AddTransient<IBasketRepository, RedisBasketRepository>();
            builder.Services.AddTransient<IWishlistRepository, RedisWishlistRepository>();
            builder.Services.AddTransient<IIdentityService, IdentityService>();

            //consul registration added
            builder.Services.ConfigureConsul(builder.Configuration);

            //eventbus registration added
            builder.Services.AddSingleton<IEventBus>(sp =>
            {
                var config = new EventBusConfig
                {
                    ConnectionRetryCount = 5,
                    EventNameSuffix = "IntegrationEvent",
                    SubscriberClientAppName = "BasketService",
                    EventBusType = EventBusType.RabbitMQ,
                    Connection = new ConnectionFactory
                    {
                        HostName = builder.Configuration["RabbitMQ:Host"] ?? "localhost"
                    }
                };

                return EventBusFactory.Create(config, sp);
            });

            builder.Services.AddTransient<OrderStartedIntegrationEventHandler>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
                app.MapOpenApi();

            app.UseSerilogRequestLogging();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapDefaultHealthChecks();

            app.RegisterWithConsul(app.Lifetime, builder.Configuration);

            //subscriptions added after build
            var eventBus = app.Services.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();

            app.Run();
        }
    }
}
