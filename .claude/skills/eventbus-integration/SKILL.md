---
name: eventbus-integration
description: >
  Use when a SellingBuddy service publishes or consumes an integration event, or when
  changing anything under src/BuildingBlocks/EventBus. Covers IntegrationEvent /
  IIntegrationEventHandler<T> naming, folder layout, EventBusConfig + EventBusFactory DI
  registration, subscribing after app.Build(), handler lifetime, and the event catalog.
---

# EventBus Integration

Async service-to-service communication goes through `IEventBus`
(`src/BuildingBlocks/EventBus/EventBus.Base/Abstraction/IEventBus.cs`):

```csharp
public interface IEventBus : IDisposable
{
    void Publish(IntegrationEvent @event);
    void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
    void UnSubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
}
```

Transports: `EventBusRabbitMQ` (default) and `EventBusServiceBus` (Azure), both
inherit `BaseEventBus`. Pick one with `EventBusConfig.EventBusType`; never `new` a
transport directly, always go through `EventBusFactory.Create`.

## How names are matched

`BaseEventBus.ProcessEventName` strips `EventNamePrefix` / `EventNameSuffix`
(default suffix `"IntegrationEvent"`), so `OrderCreatedIntegrationEvent` travels as
routing key / label `OrderCreated`.

- RabbitMQ: exchange = `DefaultTopicName` (`SellingBuddyEventBus`, direct),
  queue = `{SubscriberClientAppName}.{EventName}` e.g. `BasketService.OrderStarted`.
- Azure Service Bus: topic = `DefaultTopicName`, subscription = same `{App}.{Event}`,
  correlation rule on `Label`.

The consumer finds the CLR type by **class name**, so the publisher's and the
consumer's event classes must have the same name and compatible properties.

## Folder layout per service

```
IntegrationEvents/
  Events/
    OrderStartedIntegrationEvent.cs
  EventHandlers/
    OrderStartedIntegrationEventHandler.cs
```

## Event class

```csharp
using EventBus.Base.Events;

namespace BasketService.Api.IntegrationEvents.Events
{
    public class OrderStartedIntegrationEvent : IntegrationEvent
    {
        public string UserName { get; set; }
        public Guid OrderId { get; set; }

        public OrderStartedIntegrationEvent(string userName, Guid orderId)
        {
            UserName = userName;
            OrderId = orderId;
        }
    }
}
```

Rules: inherit `IntegrationEvent` (gives `Id`, `CreatedDate`), public settable
properties (Newtonsoft.Json), only primitive / DTO properties — no domain entities.
Never log an event that carries card or personal data with `{@IntegrationEvent}`
(e.g. `OrderCreatedIntegrationEvent`) — log the id and a few safe fields instead.

## Handler class

```csharp
using EventBus.Base.Abstraction;

namespace BasketService.Api.IntegrationEvents.EventHandlers
{
    public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
    {
        private readonly IBasketRepository _repository;
        private readonly ILogger<OrderStartedIntegrationEventHandler> _logger;

        public OrderStartedIntegrationEventHandler(IBasketRepository repository, ILogger<OrderStartedIntegrationEventHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(OrderStartedIntegrationEvent @event)
        {
            _logger.LogInformation("Handling integration event {IntegrationEventId} at BasketService - ({@IntegrationEvent})", @event.Id, @event);

            //basket cleared after order started
            await _repository.DeleteBasketAsync(@event.UserName);
        }
    }
}
```

## DI registration (Program.cs)

```csharp
using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using RabbitMQ.Client;

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

//handlers are resolved from a scope per message
builder.Services.AddTransient<OrderStartedIntegrationEventHandler>();

var app = builder.Build();

//subscriptions added after build
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();
```

- `SubscriberClientAppName` = the service name (it becomes the queue prefix).
- The project references `EventBus.Factory` only; Base/RabbitMQ come with it transitively.
- `appsettings.json`: `"RabbitMQ": { "Host": "localhost" }`, overridden in
  docker-compose with `RabbitMQ__Host=s_rabbitmq`.
- Azure: set `EventBusType = EventBusType.AzureServiceBus` and
  `EventBusConnectionString`; leave `Connection` null.

## Publishing

```csharp
var orderCreatedEvent = new OrderCreatedIntegrationEvent(userId, userName, ...);

//checkout event published to order service
_eventBus.Publish(orderCreatedEvent);
```

Publish **after** your own state is saved (e.g. after `SaveEntitiesAsync`).

## Event Catalog

| Event | Publisher | Consumers | Payload |
|---|---|---|---|
| `OrderCreatedIntegrationEvent` | BasketService (checkout) | OrderService | user, address, card info, basket items |
| `OrderStartedIntegrationEvent` | OrderService | BasketService, PaymentService | `UserName`, `OrderId` |
| `OrderPaymentSuccessIntegrationEvent` | PaymentService | NotificationService | `OrderId` |
| `OrderPaymentFailedIntegrationEvent` | PaymentService | NotificationService | `OrderId`, `ErrorMessage` |

Add a row here when you add a new event.

## Checklist

- [ ] Event name ends with `IntegrationEvent`, handler with `IntegrationEventHandler`
- [ ] Same event class name in publisher and every consumer
- [ ] Handler registered with `AddTransient<THandler>()`
- [ ] `Subscribe<T, TH>()` called after `builder.Build()`
- [ ] `SubscriberClientAppName` set to the service name
- [ ] Event catalog table above updated
- [ ] Handler logs the event with `_logger.LogInformation(...)`
