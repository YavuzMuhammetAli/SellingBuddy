using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using EventBus.UnitTest.Events;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Net.Sockets;

namespace EventBus.UnitTest
{
    //needs rabbitmq from docker-compose, skipped when broker is down
    public class RabbitMQEventBusTests
    {
        //host can be changed when tests run inside docker
        private static readonly string Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

        private static bool IsBrokerReachable()
        {
            try
            {
                using var client = new TcpClient();
                return client.ConnectAsync(Host, 5672).Wait(TimeSpan.FromSeconds(1)) && client.Connected;
            }
            catch
            {
                return false;
            }
        }

        [Fact]
        public async Task PublishedEvent_IsConsumedBySubscriber()
        {
            Assert.SkipUnless(IsBrokerReachable(), $"RabbitMQ is not running on {Host}:5672");

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<HandledEventSink>();
            services.AddTransient<OrderCreatedIntegrationEventHandler>();

            services.AddSingleton<IEventBus>(sp => EventBusFactory.Create(new EventBusConfig
            {
                ConnectionRetryCount = 1,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "SellingBuddyTopicName",
                EventBusType = EventBusType.RabbitMQ,
                EventNameSuffix = "IntegrationEvent",
                Connection = new ConnectionFactory { HostName = Host }
            }, sp));

            var provider = services.BuildServiceProvider();
            using var eventBus = provider.GetRequiredService<IEventBus>();

            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            eventBus.Publish(new OrderCreatedIntegrationEvent(7));

            try
            {
                var handled = await provider.GetRequiredService<HandledEventSink>().Received.Task
                    .WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);

                Assert.Equal(7, handled.OrderId);
            }
            finally
            {
                await DeleteTestQueueAsync();
            }
        }

        //queue is durable, removed so it doesn't show up as orphan in the status dashboard
        private static async Task DeleteTestQueueAsync()
        {
            var factory = new ConnectionFactory { HostName = Host };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeleteAsync("EventBus.UnitTest.OrderCreated", ifUnused: false, ifEmpty: false);
        }
    }
}
