---
name: blazor-client
description: >
  Use when changing src/Clients/BlazorWebApp/WebApp. Blazor Web App with global
  InteractiveServer render mode (prerender off): GatewayClient + scoped services,
  login with CustomAuthStateProvider over ProtectedLocalStorage, AuthorizeView instead
  of [Authorize], catalog/basket/checkout/orders pages and component conventions.
---

# Blazor WebApp

Blazor Web App (.NET 10). The whole app is interactive server, **prerender off**, set
once in `Components/App.razor`:

```razor
<HeadOutlet @rendermode="InteractiveServerNoPrerender" />
<Routes @rendermode="InteractiveServerNoPrerender" />

@code {
    private static readonly IComponentRenderMode InteractiveServerNoPrerender = new InteractiveServerRenderMode(prerender: false);
}
```

Why: the JWT lives in `ProtectedLocalStorage`, which needs JS interop. Without prerender
the first render already runs in the circuit, so the auth state is readable immediately.
Pages therefore **don't** declare `@rendermode` themselves.

The WebApp talks **only** to the gateway (`ApiGateway:BaseUrl`, `http://localhost:5131/`
locally, `http://c_webapigateway:8080/` in docker). Keep the trailing slash — services use
relative paths (`catalog/items`, `basket`, `api/basket/items`).

## Folder layout

```
WebApp/
  Application/Services/Interfaces/IServices.cs   IIdentityService, ICatalogService, IBasketService, IOrderService
  Application/Services/                          implementations
  Domain/Models/                                 CatalogModels, BasketModels, OrderModels, AuthModels
  Infrastructure/GatewayClient.cs                typed HttpClient, adds bearer token per call
  Infrastructure/Auth/CustomAuthStateProvider.cs
  Components/Layout/                             MainLayout (LoginDisplay in top row), NavMenu
  Components/Pages/                              Catalog (/), Login, BasketPage, Checkout, Orders, OrderDetailPage
  Components/Shared/                             ProductCard, LoginDisplay, RedirectToLogin
```

Page components that would clash with a model name get a `Page` suffix
(`BasketPage` vs `Basket` model).

## Service registration

```csharp
//gateway http client added, webapp only talks to the gateway
builder.Services.AddHttpClient<GatewayClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"]);
});

//scoped services, one per user circuit
builder.Services.AddScoped<ICatalogService, CatalogService>();
...

//auth state provider added
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();
```

Everything user related is **scoped** (one per circuit). Never singleton.

## Calling the gateway

```csharp
public async Task<Basket> GetBasketAsync()
{
    var client = await _gateway.AuthorizedAsync();   // sets Authorization: Bearer <token>
    return await client.GetFromJsonAsync<Basket>("basket") ?? new Basket();
}
```

Anonymous endpoints (catalog, auth) use `_gateway.Anonymous`.

## Auth

- `IdentityService.LoginAsync` posts to gateway `/auth`, then
  `CustomAuthStateProvider.SignInAsync(userName, token)` stores both in
  `ProtectedLocalStorage` and raises `NotifyAuthenticationStateChanged`.
- `GetAuthenticationStateAsync` caches the state per circuit and wraps storage access in
  try/catch.
- Protected pages wrap their markup in `<AuthorizeView>` with
  `<NotAuthorized><RedirectToLogin /></NotAuthorized>`. **Don't use `@attribute [Authorize]`**:
  it becomes endpoint metadata and the server has no auth scheme (the token is only in
  the browser), so the request fails before the circuit starts.
- Inside `<AuthorizeView>`, an `EditForm` needs `Context="formContext"` (both use `context`).

## Checklist

- [ ] Calls go through a service in `Application/Services`, never `HttpClient` in a page
- [ ] Service registered as scoped
- [ ] Protected page uses `<AuthorizeView>` + `RedirectToLogin`, no `[Authorize]`
- [ ] Data that needs the user is loaded after checking the cascading `AuthenticationState`
- [ ] Link added to `Components/Layout/NavMenu.razor`
- [ ] Gateway route/aggregator exists for every endpoint the page calls
- [ ] All visible text English
