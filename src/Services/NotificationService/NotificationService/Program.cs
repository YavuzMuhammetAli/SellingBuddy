using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.IntegrationEvents.EventHandlers;
using NotificationService.IntegrationEvents.Events;
using RabbitMQ.Client;

namespace NotificationService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //generic host keeps the console app alive
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddTransient<OrderPaymentSuccessIntegrationEventHandler>();
            builder.Services.AddTransient<OrderPaymentFailedIntegrationEventHandler>();

            //eventbus registration added
            builder.Services.AddSingleton<IEventBus>(sp =>
            {
                var config = new EventBusConfig
                {
                    ConnectionRetryCount = 5,
                    EventNameSuffix = "IntegrationEvent",
                    SubscriberClientAppName = "NotificationService",
                    EventBusType = EventBusType.RabbitMQ,
                    Connection = new ConnectionFactory
                    {
                        HostName = builder.Configuration["RabbitMQ:Host"] ?? "localhost"
                    }
                };

                return EventBusFactory.Create(config, sp);
            });

            var host = builder.Build();

            //subscriptions added after build
            var eventBus = host.Services.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderPaymentSuccessIntegrationEvent, OrderPaymentSuccessIntegrationEventHandler>();
            eventBus.Subscribe<OrderPaymentFailedIntegrationEvent, OrderPaymentFailedIntegrationEventHandler>();

            Console.WriteLine("NotificationService is listening for payment events...");

            host.Run();
        }
    }
}
