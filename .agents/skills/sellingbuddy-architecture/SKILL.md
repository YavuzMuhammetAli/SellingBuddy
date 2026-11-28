---
name: sellingbuddy-architecture
description: >
  Read FIRST before touching any SellingBuddy code. Solution map, service/port table,
  event flow between services, folder layout, naming rules (English only), the project
  comment style ("//logging class added") and commit style. Other skills
  (eventbus-integration, microservice-api, order-ddd-cqrs, api-gateway-ocelot,
  blazor-client) go deeper — this one tells you where things live.
---

# SellingBuddy Architecture

.NET 10 microservice e-commerce sample. Every service owns its own data store and
talks to other services **only** through the gateway (sync HTTP) or the EventBus
(async integration events). No service references another service's project.

## Solution Map

```
SellingBuddy.slnx
docker-compose.yml                      # infra: sqlserver, redis, rabbitmq, consul (+ services)
src/
  ApiGateways/WebApiGateway/Web.ApiGateway      # Ocelot + Consul, aggregators
  BuildingBlocks/HealthChecks/HealthChecks.Common   # /health json writer, tcp check, MapDefaultHealthChecks
  BuildingBlocks/EventBus/
    EventBus.Base            # IEventBus, IntegrationEvent, BaseEventBus, subscription manager
    EventBus.RabbitMQ        # default transport
    EventBus.AzureServiceBus # alternative transport
    EventBus.Factory         # EventBusFactory.Create(config, sp)
    EventBus.UnitTest        # xUnit tests
  Clients/BlazorWebApp/WebApp                   # Blazor Web App storefront (InteractiveServer, TR/EN)
  Monitoring/StatusDashboard                    # TabBlazor service status dashboard
  Services/
    CatalogService/CatalogService.Api           # EF Core + SQL Server
    IdentityService/IdentityService.Api         # JWT issuer
    BasketService/BasketService.Api             # Redis
    OrderService/OrderService.{Api,Application,Domain,Infrastructure}  # DDD + CQRS
    PaymentService/PaymentService.Api           # fake payment, event driven
    NotificationService/NotificationService     # console app, event driven
```

## Services and Ports

Ports come from each project's `Properties/launchSettings.json` (http profile).
Keep them stable — the gateway, Consul registration and the WebApp depend on them.

| Service | HTTP | HTTPS | Store | Consul name |
|---|---|---|---|---|
| Web.ApiGateway | 5131 | 7140 | – | – |
| CatalogService.Api | 5140 | 7260 | SQL Server `catalog` | `CatalogService` |
| IdentityService.Api | 5095 | 7085 | – (in memory users) | `IdentityService` |
| BasketService.Api | 5166 | 7011 | Redis | `BasketService` |
| OrderService.Api | 5226 | 7029 | SQL Server `order` | `OrderService` |
| PaymentService.Api | 5278 | 7138 | – | `PaymentService` |
| NotificationService | – (console) | – | – | – |
| WebApp (Blazor) | 5198 | 7125 | – | – |
| StatusDashboard | 5300 | – | – (in memory) | – |

Infra (from `docker-compose.yml`, host ports): SQL Server `1435` (container `c_sqlserver:1433`),
Redis `6389` (container `c_redis:6379`), RabbitMQ `5672` / UI `15672` (guest/guest),
Consul `8500` (raw UI; the themed view is the StatusDashboard on `5300`). SQL/Redis use non-default host ports to avoid clashes with other local
SQL Server / Redis instances. Inside docker every service listens on `8080` and is reached by
its container name (`c_catalogservice`, `c_basketservice`, ...).

## Multi-Vendor Model

- **Catalog** owns sellers, offers (one per seller per product, with price, stock,
  shipping fee, delivery days), categories with children, specs, reviews and campaigns.
  `CatalogItem.Price` / `OldPrice` / `AvailableStock` mirror the best offer.
  Content is bilingual (`LocalizedText`), resolved from `Accept-Language` (tr / en).
- **Basket** items keep the chosen `OfferId` and seller; standard shipping is the highest
  fee per seller summed, same day is a flat fee (`ShippingOptions.FeeFor`). The wishlist is a
  Redis set `wishlist:{user}` (`api/wishlist`).
- **Order** items keep the seller; status goes `submitted` → `paid` / `cancelled` from
  payment events. Delivery tracking (shipped / delivered) has no backend yet.
- **Identity** has two demo users only (login by email or user name), no user store.

## Event Flow (checkout)

```
WebApp ──HTTP──> Gateway ──> BasketService.Checkout
BasketService  ──OrderCreatedIntegrationEvent──>        OrderService   (creates order)
OrderService   ──OrderStartedIntegrationEvent──>        BasketService  (clears basket)
                                                 └────> PaymentService (charges)
PaymentService ──OrderPaymentSuccessIntegrationEvent──> NotificationService, OrderService (paid)
               └─OrderPaymentFailedIntegrationEvent──>  NotificationService, OrderService (cancelled)
```

Each consumer defines **its own copy** of the event class (same name, same
properties). Events are matched by name, so never share an "events" project.

## Naming Rules — English Only

- Class, method, property, variable, route, table, column, config key, log message:
  **English only**. Never Turkish words, never Turkish characters (`ç ğ ı ö ş ü`).
- The only Turkish allowed: WebApp translations in `Resources/*.tr.resx` and the `Tr`
  values of `CatalogService.Api/Infrastructure/Setup/catalog-seed.json` (bilingual
  catalog content, `LocalizedText` → `X_En` / `X_Tr` columns, picked by `Accept-Language`).
- PascalCase types/methods, camelCase locals, `_camelCase` private fields.
- Suffixes: `XxxIntegrationEvent`, `XxxIntegrationEventHandler`, `XxxCommand`,
  `XxxCommandHandler`, `XxxQuery`, `XxxDomainEvent`, `XxxRepository`, `XxxController`.
- Folder names: `Controllers`, `Core/Domain`, `Core/Application`, `Infrastructure`,
  `Extensions`, `IntegrationEvents/Events`, `IntegrationEvents/EventHandlers`.

## Comment Style

Comments look like a normal developer wrote them while working — short, lowercase,
`//` with **no space**, saying what was added or why. Only on meaningful blocks,
not on every line, never long XML-doc essays.

```csharp
//logging class added
builder.Host.UseSerilog();

//consul registration added
builder.Services.ConfigureConsul(builder.Configuration);

//retry policy for sql connection
var policy = Policy.Handle<SqlException>().WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(5));

//basket cleared after order started
await _repository.DeleteBasketAsync(@event.UserId.ToString());
```

Don't:
- `// Add services to the container.` (template noise — delete those)
- `// This method retrieves the basket for the given user id and returns it` (narration)
- Turkish comments of any kind.

## Commit Style

Follow the existing history (Conventional Commits):
`feat(catalog): add catalog item endpoints`, `fix(eventbus): ...`, `chore: ...`.
One commit per finished step. Commit only when the user asks.

## Project Settings

- `Directory.Build.props` (repo root) sets Version/Product/Company for every assembly.
  Keep it: Windows Smart App Control, when enabled, tends to block unsigned DLLs without
  assembly metadata.
- Even with that, Smart App Control may block freshly built DLLs
  (`0x800711C7`, "An Application Control policy has blocked this file"). When it does, run things in docker
  (`docker compose up -d --build`) or run `dotnet ef` inside the SDK container — Linux
  containers are not affected.
- New projects use `<Nullable>disable</Nullable>` like the existing services.
- `dotnet-ef` is a local tool (`dotnet-tools.json`): `dotnet tool restore`.

## Before Writing Code

- [ ] Read the matching skill (`eventbus-integration`, `microservice-api`, `order-ddd-cqrs`, `api-gateway-ocelot`, `blazor-client`)
- [ ] Check the service's existing `Extensions/` folder — Consul, Serilog, auth helpers may already exist
- [ ] Keep the ports in the table above
- [ ] New project → add it to `SellingBuddy.slnx` under the right solution folder
- [ ] `dotnet build SellingBuddy.slnx` passes with no errors
