using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using OrderService.Api.Extensions;
using OrderService.Application;
using OrderService.Application.IntegrationEvents.EventHandlers;
using OrderService.Application.IntegrationEvents.Events;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Context;
using RabbitMQ.Client;
using Serilog;

namespace OrderService.Api
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
            builder.Services.AddHealthChecks();

            builder.Services.ConfigureAuth(builder.Configuration);

            //application and persistence layers added
            builder.Services.AddApplicationRegistration();
            builder.Services.AddPersistenceRegistration(builder.Configuration);

            //consul registration added
            builder.Services.ConfigureConsul(builder.Configuration);

            //eventbus registration added
            builder.Services.AddSingleton<IEventBus>(sp =>
            {
                var config = new EventBusConfig
                {
                    ConnectionRetryCount = 5,
                    EventNameSuffix = "IntegrationEvent",
                    SubscriberClientAppName = "OrderService",
                    EventBusType = EventBusType.RabbitMQ,
                    Connection = new ConnectionFactory
                    {
                        HostName = builder.Configuration["RabbitMQ:Host"] ?? "localhost"
                    }
                };

                return EventBusFactory.Create(config, sp);
            });

            var app = builder.Build();

            //database migrated on startup, lookup data comes with migration
            app.MigrateDbContext<OrderDbContext>((context, services) => Task.CompletedTask);

            if (app.Environment.IsDevelopment())
                app.MapOpenApi();

            app.UseSerilogRequestLogging();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            app.RegisterWithConsul(app.Lifetime, builder.Configuration);

            //subscriptions added after build
            var eventBus = app.Services.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

            app.Run();
        }
    }
}
