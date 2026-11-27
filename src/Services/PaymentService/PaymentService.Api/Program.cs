using HealthChecks.Common;
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using PaymentService.Api.Extensions;
using PaymentService.Api.IntegrationEvents.EventHandlers;
using PaymentService.Api.IntegrationEvents.Events;
using RabbitMQ.Client;
using Serilog;

namespace PaymentService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //logging class added
            builder.Host.UseSerilog((context, loggerConfig) =>
                loggerConfig.ReadFrom.Configuration(context.Configuration));

            builder.Services.AddOpenApi();
            //health checks with dependencies added
            builder.Services.AddDefaultHealthChecks()
                .AddTcpCheck("rabbitmq", builder.Configuration["RabbitMQ:Host"] ?? "localhost", 5672);

            //consul registration added
            builder.Services.ConfigureConsul(builder.Configuration);

            //eventbus registration added
            builder.Services.AddSingleton<IEventBus>(sp =>
            {
                var config = new EventBusConfig
                {
                    ConnectionRetryCount = 5,
                    EventNameSuffix = "IntegrationEvent",
                    SubscriberClientAppName = "PaymentService",
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

            app.MapDefaultHealthChecks();

            app.RegisterWithConsul(app.Lifetime, builder.Configuration);

            //subscriptions added after build
            var eventBus = app.Services.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();

            app.Run();
        }
    }
}
