# SellingBuddy

.NET 10 microservice e-commerce sample: Catalog, Identity, Basket, Order (DDD + CQRS),
Payment, Notification services, Ocelot + Consul gateway, RabbitMQ EventBus, Blazor WebApp.

## Commands

```bash
docker compose up -d sqlserver redis rabbitmq consul    # infra only
dotnet tool restore                                      # local dotnet-ef
dotnet build SellingBuddy.slnx
dotnet test src/BuildingBlocks/EventBus/EventBus.UnitTest
dotnet run --project src/Services/CatalogService/CatalogService.Api
docker compose up -d --build                             # everything in containers
```

Infra UIs: RabbitMQ http://localhost:15672 (guest/guest), Consul http://localhost:8500.
Host ports: SQL Server 1435, Redis 6389 (non-default to avoid clashes with other local SQL Server / Redis instances). Full list in `ServiceUrls.txt`.

If Windows Smart App Control is enabled, it can block freshly built DLLs (`0x800711C7`).
If `dotnet run`/`dotnet ef` fail with that, use docker (`docker compose up -d --build`) —
see `sellingbuddy-architecture` skill.

Tests inside docker (same network as RabbitMQ, not affected by Smart App Control; run from the repo root, `pwd -W` is for Git Bash, use `$(pwd)` on Linux/macOS):

```bash
MSYS_NO_PATHCONV=1 docker run --rm --network sellingbuddy_net_backendservices -e RABBITMQ_HOST=c_rabbitmq   -v "$(pwd -W):/repo:ro" mcr.microsoft.com/dotnet/sdk:10.0 bash -c   "mkdir /work && cd /repo && tar --exclude=bin --exclude=obj --exclude=.git -cf - . | (cd /work && tar xf -) && cd /work && dotnet test src/BuildingBlocks/EventBus/EventBus.UnitTest"
```

## Skills — read the matching one before coding

| Skill | When |
|---|---|
| `sellingbuddy-architecture` | always first: map, ports, event flow, naming + comment rules |
| `eventbus-integration` | publishing/consuming integration events, EventBus code |
| `microservice-api` | service Program.cs, Consul, Serilog, JWT, Dockerfile, new service |
| `order-ddd-cqrs` | anything in OrderService |
| `api-gateway-ocelot` | gateway routes and aggregators |
| `blazor-client` | WebApp pages and services |

## Hard rules

- **English only** in code: names, routes, tables, config keys, log messages, UI text. No Turkish words or characters.
- **Comments** are short, lowercase, `//` with no space, saying what was added or why:
  `//logging class added`, `//consul registration added`. Only on meaningful blocks. Remove template noise comments.
- Services never reference each other's projects. Sync calls go through the gateway; async calls go through the EventBus.
- Keep the `public class Program { Main }` style and block-scoped namespaces.
- Conventional commits (`feat(scope): ...`, `fix(scope): ...`, `chore: ...`). Commit only when asked.
