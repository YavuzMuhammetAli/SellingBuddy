using EventBus.Base;
using EventBus.UnitTest.Events;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EventBus.UnitTest
{
    public class BaseEventBusTests
    {
        private static TestEventBus CreateBus(IServiceProvider serviceProvider, string prefix = "")
        {
            return new TestEventBus(new EventBusConfig
            {
                SubscriberClientAppName = "EventBus.UnitTest",
                EventNamePrefix = prefix,
                EventNameSuffix = "IntegrationEvent"
            }, serviceProvider);
        }

        [Theory]
        [InlineData("OrderCreatedIntegrationEvent", "OrderCreated")]
        [InlineData("PaymentIntegrationEvent", "Payment")]
        [InlineData("OrderPaymentSuccessIntegrationEvent", "OrderPaymentSuccess")]
        [InlineData("OrderCreated", "OrderCreated")]
        public void ProcessEventName_RemovesSuffixAsWholeWord(string eventName, string expected)
        {
            var bus = CreateBus(new ServiceCollection().BuildServiceProvider());

            Assert.Equal(expected, bus.ProcessEventName(eventName));
        }

        [Fact]
        public void ProcessEventName_RemovesPrefixAsWholeWord()
        {
            var bus = CreateBus(new ServiceCollection().BuildServiceProvider(), prefix: "SB");

            Assert.Equal("Basket", bus.ProcessEventName("SBBasketIntegrationEvent"));
            Assert.Equal("Stock", bus.ProcessEventName("StockIntegrationEvent"));
        }

        [Fact]
        public void GetSubName_UsesAppNameAndProcessedEventName()
        {
            var bus = CreateBus(new ServiceCollection().BuildServiceProvider());

            Assert.Equal("EventBus.UnitTest.OrderCreated", bus.GetSubName("OrderCreatedIntegrationEvent"));
        }

        [Fact]
        public async Task ProcessEvent_InvokesSubscribedHandler()
        {
            var services = new ServiceCollection();
            services.AddSingleton<HandledEventSink>();
            services.AddTransient<OrderCreatedIntegrationEventHandler>();
            var provider = services.BuildServiceProvider();

            var bus = CreateBus(provider);
            bus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

            var message = JsonConvert.SerializeObject(new OrderCreatedIntegrationEvent(42));
            var processed = await bus.ProcessEvent("OrderCreated", message);

            Assert.True(processed);
            var handled = await provider.GetRequiredService<HandledEventSink>().Received.Task;
            Assert.Equal(42, handled.OrderId);
        }

        [Fact]
        public async Task ProcessEvent_ReturnsFalseWithoutSubscription()
        {
            var bus = CreateBus(new ServiceCollection().BuildServiceProvider());

            Assert.False(await bus.ProcessEvent("OrderCreated", "{}"));
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            var bus = CreateBus(new ServiceCollection().BuildServiceProvider());

            var exception = Record.Exception(bus.Dispose);

            Assert.Null(exception);
        }
    }
}
