using EventBus.Base.SubManager;
using EventBus.UnitTest.Events;

namespace EventBus.UnitTest
{
    public class SubscriptionManagerTests
    {
        private static InMemoryEventBusSubscriptionManager CreateManager() =>
            new InMemoryEventBusSubscriptionManager(name => name.Replace("IntegrationEvent", string.Empty));

        [Fact]
        public void NewManager_IsEmpty()
        {
            Assert.True(CreateManager().IsEmpty);
        }

        [Fact]
        public void AddSubscription_RegistersHandlerAndEventType()
        {
            var manager = CreateManager();

            manager.AddSubscription<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

            Assert.False(manager.IsEmpty);
            Assert.True(manager.HasSubscriptionsForEvent<OrderCreatedIntegrationEvent>());
            Assert.Equal(typeof(OrderCreatedIntegrationEvent), manager.GetEventTypeByName(nameof(OrderCreatedIntegrationEvent)));
            Assert.Single(manager.GetHandlersForEvent("OrderCreated"));
        }

        [Fact]
        public void AddSubscription_SameHandlerTwice_Throws()
        {
            var manager = CreateManager();
            manager.AddSubscription<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

            Assert.Throws<ArgumentException>(() =>
                manager.AddSubscription<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>());
        }

        [Fact]
        public void RemoveSubscription_ClearsEventAndRaisesRemoved()
        {
            var manager = CreateManager();
            string? removedEvent = null;
            manager.OnEventRemoved += (_, name) => removedEvent = name;
            manager.AddSubscription<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

            manager.RemoveSubscription<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

            Assert.True(manager.IsEmpty);
            Assert.Equal("OrderCreated", removedEvent);
            Assert.Null(manager.GetEventTypeByName(nameof(OrderCreatedIntegrationEvent)));
        }

        [Fact]
        public void Clear_RemovesEverything()
        {
            var manager = CreateManager();
            manager.AddSubscription<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

            manager.Clear();

            Assert.True(manager.IsEmpty);
            Assert.Null(manager.GetEventTypeByName(nameof(OrderCreatedIntegrationEvent)));
        }
    }
}
