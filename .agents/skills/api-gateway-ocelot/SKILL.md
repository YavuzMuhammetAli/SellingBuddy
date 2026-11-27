---
name: api-gateway-ocelot
description: >
  Use when changing src/ApiGateways/WebApiGateway/Web.ApiGateway: adding an Ocelot
  route, Consul service discovery, gateway aggregator endpoints (e.g. add item to
  basket), typed HttpClients that forward the Authorization header, or CORS for the
  Blazor WebApp.
---

# Web.ApiGateway — Ocelot + Consul

The WebApp only knows the gateway (`http://localhost:5131`). The gateway resolves
downstream hosts from Consul by service name — never hard-code a service host/port
in a route.

## Program.cs essentials

```csharp
//env vars re-added after ocelot.json so docker can override GlobalConfiguration
builder.Configuration
    .AddJsonFile("Configurations/ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

//ocelot with consul added
builder.Services.AddOcelot(builder.Configuration).AddConsul();

//cors for blazor webapp
builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", policy =>
    policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!)
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()));

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<HttpClientDelegatingHandler>();
// typed clients for aggregators (see below)

var app = builder.Build();

app.UseCors("CorsPolicy");

//explicit UseEndpoints, otherwise ocelot (terminal) runs before the implicit endpoint middleware
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints => endpoints.MapControllers());

//ocelot must be the last middleware
await app.UseOcelot();
await app.RunAsync();
```

Because `UseOcelot()` is awaited, `Main` is `public static async Task Main`.

## ocelot.json route template

```json
{
  "Routes": [
    {
      "UseServiceDiscovery": true,
      "ServiceName": "CatalogService",
      "DownstreamPathTemplate": "/api/catalog/{everything}",
      "DownstreamScheme": "http",
      "UpstreamPathTemplate": "/catalog/{everything}",
      "UpstreamHttpMethod": [ "Get", "Post", "Delete", "Put" ],
      "UpstreamScheme": "http"
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:5131",
    "ServiceDiscoveryProvider": {
      "Scheme": "http",
      "Host": "localhost",
      "Port": 8500,
      "Type": "Consul"
    }
  }
}
```

Upstream → downstream map:

| Upstream | Service | Downstream |
|---|---|---|
| `/catalog/{everything}` | CatalogService | `/api/catalog/{everything}` |
| `/auth` | IdentityService | `/api/auth` |
| `/basket` | BasketService | `/api/basket` (current user's basket) |
| `/basket/{everything}` | BasketService | `/api/basket/{everything}` |
| `/order` | OrderService | `/api/order` (current user's orders) |
| `/order/{everything}` | OrderService | `/api/order/{everything}` |

`{everything}` never matches an empty segment, so root paths need their own route.

Docker overrides: `GlobalConfiguration__ServiceDiscoveryProvider__Host=c_consul`.

`ServiceName` must equal `ConsulConfig:ServiceName` of the target service.

## Aggregators

A route that needs data from more than one service is a normal controller in the
gateway under `Controllers/`, using typed HttpClients under `Services/`:

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BasketController : ControllerBase
{
    [HttpPost("items")]
    public async Task<ActionResult> AddBasketItemAsync([FromBody] AddBasketItemRequest request)
    {
        //item read from catalog first
        var item = await _catalogService.GetCatalogItemAsync(request.CatalogItemId);
        var basket = await _basketService.GetBasketAsync(request.BasketId);
        ...
        await _basketService.UpdateBasketAsync(basket);
        return Ok();
    }
}
```

Aggregator paths start with `api/` so they never collide with Ocelot upstream paths.

Typed clients call **the gateway itself** (`urls:gateway`, `http://localhost:5131/`
locally, `http://localhost:8080/` in docker), so Ocelot + Consul resolve the downstream
service. The incoming auth header is forwarded:

```csharp
public class HttpClientDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        //incoming token forwarded to downstream
        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authorizationHeader))
            request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        return base.SendAsync(request, cancellationToken);
    }
}
```

## Checklist

- [ ] New service → route in `Configurations/ocelot.json` with `UseServiceDiscovery` + `ServiceName`
- [ ] No hard-coded downstream host/port in routes
- [ ] Aggregator controllers under `api/...`, registered before `UseOcelot()`
- [ ] Typed HttpClient + `HttpClientDelegatingHandler` for calls that need the user token
- [ ] WebApp origin listed in `AllowedOrigins`
