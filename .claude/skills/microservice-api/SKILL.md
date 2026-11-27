---
name: microservice-api
description: >
  Use when creating or wiring a SellingBuddy service API (Catalog, Identity, Basket,
  Order.Api, Payment) or adding a new service. Standard Program.cs shape, Serilog,
  Consul registration extension, health check, JWT auth, appsettings layout,
  Dockerfile template and the new-service checklist. Event wiring lives in
  eventbus-integration, DDD layering in order-ddd-cqrs.
---

# Microservice API Conventions

## Program.cs shape

Projects keep the template's `public class Program { Main }` style and block-scoped
namespaces (`namespace X { ... }`) — don't convert to top-level statements or
file-scoped namespaces. Order inside `Main`:

```csharp
using CatalogService.Api.Extensions;
using Serilog;

namespace CatalogService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //logging class added
            builder.Host.UseSerilog((context, loggerConfig) =>
                loggerConfig.ReadFrom.Configuration(context.Configuration));

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddHealthChecks();

            //consul registration added
            builder.Services.ConfigureConsul(builder.Configuration);

            // ... service specific: db context, redis, auth, eventbus

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
                app.MapOpenApi();

            app.UseSerilogRequestLogging();

            app.UseAuthentication();   // only when the service uses JWT
            app.UseAuthorization();

            app.MapControllers();
            app.MapHealthChecks("/health");

            app.RegisterWithConsul(app.Lifetime, builder.Configuration);

            app.Run();
        }
    }
}
```

No `UseHttpsRedirection` — services run on plain HTTP behind the gateway and in docker.

## Consul registration (`Extensions/ConsulRegistration.cs`)

Each service has its own copy (no shared project for it):

```csharp
using Consul;

namespace CatalogService.Api.Extensions
{
    public static class ConsulRegistration
    {
        public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(configuration["ConsulConfig:Address"]!);
            }));

            return services;
        }

        public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime, IConfiguration configuration)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("ConsulRegistration");

            var serviceName = configuration["ConsulConfig:ServiceName"]!;
            var serviceUri = new Uri(configuration["ConsulConfig:ServiceAddress"]!);

            var registration = new AgentServiceRegistration
            {
                ID = $"{serviceName}-{serviceUri.Host}-{serviceUri.Port}",
                Name = serviceName,
                Address = serviceUri.Host,
                Port = serviceUri.Port,
                Tags = new[] { serviceName }
            };

            //old registration removed before register
            lifetime.ApplicationStarted.Register(() =>
            {
                logger.LogInformation("Registering {ServiceName} with Consul", serviceName);
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                consulClient.Agent.ServiceRegister(registration).Wait();
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("Deregistering {ServiceName} from Consul", serviceName);
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            });

            return app;
        }
    }
}
```

No HTTP check is registered on purpose: Consul runs in docker and can't reach
`localhost` services on the host, and Ocelot only routes to passing services.

## appsettings.json layout

```json
{
  "Serilog": {
    "MinimumLevel": { "Default": "Information", "Override": { "Microsoft.AspNetCore": "Warning" } },
    "WriteTo": [ { "Name": "Console" } ]
  },
  "ConsulConfig": {
    "Address": "http://localhost:8500",
    "ServiceName": "CatalogService",
    "ServiceAddress": "http://localhost:5140"
  },
  "RabbitMQ": { "Host": "localhost" },
  "ConnectionStrings": { "CatalogDb": "Server=localhost,1435;Database=catalog;User Id=sa;Password=Pass@word1;TrustServerCertificate=True" },
  "AuthConfig": { "Secret": "sellingbuddy-shared-jwt-secret-key-min-32-chars" },
  "AllowedHosts": "*"
}
```

Docker overrides use `__` and container names:
`ConsulConfig__Address=http://c_consul:8500`, `ConsulConfig__ServiceAddress=http://c_catalogservice:8080`,
`ConnectionStrings__CatalogDb=Server=c_sqlserver,1433;...`, `RabbitMQ__Host=c_rabbitmq`.
Keep `ServiceAddress` port equal to the http port in the skill's port table
(`sellingbuddy-architecture`).

## JWT auth (Basket, Order, Gateway)

Identity issues the token; consumers validate with the shared secret:

```csharp
//jwt auth added
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthConfig:Secret"]!))
        };
    });
```

Controllers that need a user: `[Authorize]`. The user name comes from
`ClaimTypes.Name` via a small `IIdentityService` in the service.

## Database migration on startup

Services with a database call `app.MigrateDbContext<TContext>(seeder)` right after
`builder.Build()` (`Extensions/HostExtension.cs`) — it runs `Database.Migrate()` and the
seeder inside a Polly retry on `SqlException`, because SQL Server in docker may still be booting.

## Controller style

```csharp
[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    [HttpGet("items/{id:int}")]
    [ProducesResponseType(typeof(CatalogItem), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetItemByIdAsync(int id) { ... }
}
```

Routes are lowercase-able English nouns (`api/catalog/items`, `api/basket`, `api/order`).
If a controller uses `CreatedAtAction(nameof(XxxAsync))`, register controllers with
`AddControllers(o => o.SuppressAsyncSuffixInActionNames = false)` — otherwise the
`Async` suffix is stripped and the route lookup fails at runtime.

## Common packages

| Need | Package |
|---|---|
| Logging | `Serilog.AspNetCore` |
| Consul | `Consul` |
| JWT | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| EF Core | `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design` |
| Redis | `StackExchange.Redis` |
| Retry | `Polly` |
| EventBus | ProjectReference to `src/BuildingBlocks/EventBus/EventBus.Factory` |

## Dockerfile template (build context = repo root)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
#build context is the repo root, project references building blocks
COPY . .
RUN dotnet publish "src/Services/CatalogService/CatalogService.Api/CatalogService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.Api.dll"]
```

## New service checklist

- [ ] Project under `src/Services/<Name>Service/<Name>Service.Api` and added to `SellingBuddy.slnx`, `<Nullable>disable</Nullable>`
- [ ] Fixed port in `launchSettings.json`, added to the port table in `sellingbuddy-architecture` and `ServiceUrls.txt`
- [ ] Serilog, `/health`, `ConsulRegistration` wired as above
- [ ] `ConsulConfig` section in appsettings
- [ ] Gateway route added (`api-gateway-ocelot`)
- [ ] Dockerfile + `docker-compose.yml` entry
- [ ] Template leftovers removed (WeatherForecast, `// Add services to the container.`)
