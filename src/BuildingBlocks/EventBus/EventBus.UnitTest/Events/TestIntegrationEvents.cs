using EventBus.Base.Abstraction;
using EventBus.Base.Events;

namespace EventBus.UnitTest.Events
{
    public class OrderCreatedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; set; }

        public OrderCreatedIntegrationEvent(int orderId)
        {
            OrderId = orderId;
        }
    }

    //collects handled events so tests can wait on them
    public class HandledEventSink
    {
        public TaskCompletionSource<OrderCreatedIntegrationEvent> Received { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public class OrderCreatedIntegrationEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        private readonly HandledEventSink _sink;

        public OrderCreatedIntegrationEventHandler(HandledEventSink sink)
        {
            _sink = sink;
        }

        public Task Handle(OrderCreatedIntegrationEvent @event)
        {
            _sink.Received.TrySetResult(@event);
            return Task.CompletedTask;
        }
    }
}
