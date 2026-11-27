---
name: order-ddd-cqrs
description: >
  Use for any work inside src/Services/OrderService (Api, Application, Domain,
  Infrastructure). Explains the DDD layering and project references, SeedWork base
  types, the Order and Buyer aggregates, domain events dispatched through MediatR on
  SaveEntitiesAsync, CQRS commands/queries, EF Core configuration and where
  integration events are handled and published.
---

# OrderService — DDD + CQRS

## Layers and references

```
OrderService.Api            -> Application, Infrastructure, EventBus.Factory
OrderService.Infrastructure -> Application, Domain          (EF Core, repositories)
OrderService.Application    -> Domain, EventBus.Base        (MediatR handlers, integration events)
OrderService.Domain         -> MediatR.Contracts only       (no EF, no ASP.NET)
```

Never reference Infrastructure from Application or Domain. Application talks to
persistence only through repository interfaces it owns.

MediatR is pinned to **12.x** (last Apache-2.0 version) — don't upgrade to 13+.

## Folder layout

```
OrderService.Domain/
  SeedWork/            BaseEntity, Enumeration, ValueObject, IAggregateRoot, IRepository<T>, IUnitOfWork
  AggregateModels/
    OrderAggregate/    Order, OrderItem, Address (value object), OrderStatus (enumeration)
    BuyerAggregate/    Buyer, PaymentMethod, CardType (enumeration)
  Events/              OrderStartedDomainEvent, BuyerAndPaymentMethodVerifiedDomainEvent
  Exceptions/          OrderingDomainException

OrderService.Application/
  Features/Commands/CreateOrder/        CreateOrderCommand, CreateOrderCommandHandler
  Features/Queries/GetOrderDetailById/  GetOrderDetailsQuery, GetOrderDetailsQueryHandler
  Features/Queries/ViewModels/          OrderDetailViewModel
  DomainEventHandlers/                  OrderStartedDomainEventHandler, UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler
  IntegrationEvents/Events/             OrderCreatedIntegrationEvent, OrderStartedIntegrationEvent
  IntegrationEvents/EventHandlers/      OrderCreatedIntegrationEventHandler
  Interfaces/Repositories/              IOrderRepository, IBuyerRepository
  ServiceRegistration.cs                AddApplicationRegistration()

OrderService.Infrastructure/
  Context/                OrderDbContext (DbContext + IUnitOfWork), OrderDbContextDesignFactory
  EntityConfigurations/   one IEntityTypeConfiguration<T> per entity
  Repositories/           GenericRepository<T>, OrderRepository, BuyerRepository
  Extensions/             MediatorExtension (DispatchDomainEventsAsync)
  Migrations/
  ServiceRegistration.cs  AddPersistenceRegistration(configuration)
```

## SeedWork essentials

```csharp
public abstract class BaseEntity
{
    public virtual Guid Id { get; protected set; }
    public DateTime CreateDate { get; protected set; }

    private List<INotification> domainEvents;
    public IReadOnlyCollection<INotification> DomainEvents => domainEvents?.AsReadOnly();

    public void AddDomainEvent(INotification eventItem)
    {
        domainEvents ??= new List<INotification>();
        domainEvents.Add(eventItem);
    }

    public void ClearDomainEvents() => domainEvents?.Clear();
}

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}

public interface IRepository<T> where T : BaseEntity
{
    IUnitOfWork UnitOfWork { get; }
    ...
}
```

**Ids:** `BaseEntity`'s constructor assigns `Id = Guid.NewGuid()` and every entity
configuration uses `ValueGeneratedNever()`. With EF's default "generated Guid" a new
child (e.g. a `PaymentMethod` added to an already tracked `Buyer`) that already has an
id is treated as existing and saved with UPDATE instead of INSERT.

**Tracked aggregates:** don't call `repository.Update(entity)` on an aggregate you
loaded in the same scope — it's already tracked, and `Update` would mark new children as
Modified. Just change it and call `SaveEntitiesAsync`.

**Unsaved lookups:** `GenericRepository.GetById` uses `FindAsync`, which checks the change
tracker first. Domain event handlers that run inside `SaveEntitiesAsync` need it because
the order isn't in the database yet.

`Enumeration` (int `Id`, `Name`) replaces C# enums for `OrderStatus` and `CardType`.
They are seeded with `HasData` in their entity configuration. Entities keep the FK
(`OrderStatusId`), never assign a static `Enumeration` instance to a navigation —
EF would try to insert it.

## Aggregates

- **Order** (root): `OrderDate`, `Description`, `Address` (owned value object),
  `OrderStatusId`, `BuyerId?`, `PaymentMethodId?`, `OrderItems`. Created through its
  constructor, which raises `OrderStartedDomainEvent`. Items only via
  `AddOrderItem(...)` — `OrderItems` is a read-only collection over a private list.
- **Buyer** (root): `Name`, `PaymentMethods`. `VerifyOrAddPaymentMethod(...)` reuses
  a matching card or adds one, then raises `BuyerAndPaymentMethodVerifiedDomainEvent`.

State changes happen through methods on the aggregate, never by setting properties
from handlers (setters are `private`).

## Domain events flow

```
CreateOrderCommandHandler
  new Order(...)                  -> OrderStartedDomainEvent
  orderRepository.Add(order)
  UnitOfWork.SaveEntitiesAsync()  -> MediatorExtension dispatches domain events, then SaveChanges
      OrderStartedDomainEventHandler
        buyer.VerifyOrAddPaymentMethod(...)  -> BuyerAndPaymentMethodVerifiedDomainEvent
        buyerRepository.UnitOfWork.SaveEntitiesAsync()
            UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler
              order.SetBuyerId(...); order.SetPaymentMethodId(...)
  eventBus.Publish(new OrderStartedIntegrationEvent(userName, order.Id))
```

```csharp
public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
{
    //domain events dispatched before save
    await _mediator.DispatchDomainEventsAsync(this);

    await base.SaveChangesAsync(cancellationToken);
    return true;
}
```

## CQRS

- Commands change state and return `bool` / id: `IRequest<bool>` + `IRequestHandler<,>`.
- Queries never change state and return view models from `Features/Queries/ViewModels`.
- Mapping is done by hand in the handler (no AutoMapper).
- Controllers only call `_mediator.Send(...)`.

## Integration events

- Consumes `OrderCreatedIntegrationEvent` (from Basket) → handler maps it to
  `CreateOrderCommand` and sends it with `IMediator`.
- Publishes `OrderStartedIntegrationEvent` after the order is saved.
- Handler registration + subscription happen in `OrderService.Api/Program.cs`
  (see `eventbus-integration`).

## EF Core

- Default schema `ordering`. Table names plural English: `orders`, `orderItems`,
  `buyers`, `paymentMethods`, `orderStatus`, `cardTypes`.
- `Address` is `OwnsOne` on `Order`.
- Migrations live in Infrastructure; `OrderDbContextDesignFactory` makes Infrastructure
  usable as its own startup project:
  `dotnet ef migrations add <Name> -p src/Services/OrderService/OrderService.Infrastructure -s src/Services/OrderService/OrderService.Infrastructure -o Migrations`
  (if Smart App Control blocks the DLLs, run the same command in `mcr.microsoft.com/dotnet/sdk:10.0`
  on a copy of the repo without `bin/obj`, then copy `Migrations/` back).
- The API applies migrations at startup with a Polly retry (SQL Server in docker may still be booting).

## Checklist

- [ ] New behaviour added as an aggregate method, not as a public setter
- [ ] Domain project has no EF / ASP.NET reference
- [ ] Command/query in its own folder under `Features/`
- [ ] Domain event handled in `DomainEventHandlers/`
- [ ] Entity configuration added for new entities, migration created
- [ ] Integration event published only after `SaveEntitiesAsync` succeeded
