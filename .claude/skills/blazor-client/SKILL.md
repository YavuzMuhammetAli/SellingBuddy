---
name: blazor-client
description: >
  Use when changing src/Clients/BlazorWebApp/WebApp, the multi-vendor storefront on port 5198.
  Blazor Web App with global InteractiveServer (prerender off), hand-written design system
  (tokens.css + storefront.css, no Bootstrap), TR/EN resx localization, GatewayClient + scoped
  services, StorefrontState, CustomAuthStateProvider, AuthorizeView instead of [Authorize],
  page/route map and component conventions.
---

# Blazor WebApp (storefront)

Blazor Web App (.NET 10). The design comes from the Google Stitch project
"SellingBudy Multi-Vendor Storefront Design". The whole app is interactive server with
**prerender off**, set once in `Components/App.razor`. Pages never declare `@rendermode`.

Why prerender is off: the session (JWT + user info) lives in protected browser storage, which
needs JS interop. Without prerender the first render already runs in the circuit.

The WebApp talks **only** to the gateway (`ApiGateway:BaseUrl`, `http://localhost:5131/`
locally, `http://c_webapigateway:8080/` in docker, trailing slash kept). Product images are
absolute URLs served by Catalog through the gateway (`catalog/pics/{file}`).

## Folder layout

```
WebApp/
  Application/Services/Interfaces/IServices.cs  IIdentityService, ICatalogService, IWishlistService, IBasketService, IOrderService
  Application/Services/                         implementations (WishlistService lives in BasketService.cs)
  Application/StorefrontState.cs                scoped ui state: cart count, wishlist ids, categories, toasts, session
  Domain/Models/                                CatalogModels, BasketModels, OrderModels (OrderStage + OrderStatusMap), AuthModels (UserSession)
  Infrastructure/GatewayClient.cs               typed HttpClient, Accept-Language + bearer token
  Infrastructure/Auth/CustomAuthStateProvider.cs
  Infrastructure/Fmt.cs                         price / rating / date / order number formatting per culture
  Resources/SharedResource.resx                 english text
  Resources/SharedResource.tr.resx              turkish text (default culture)
  wwwroot/css/fonts.css tokens.css storefront.css
  wwwroot/fonts/                                self hosted Manrope, Instrument Sans, Red Hat Mono
  wwwroot/img/                                  hero and auth images
  Components/Layout/   MainLayout, CheckoutLayout, AuthLayout, SiteHeader, SearchBar, LanguageToggle, SiteFooter, BottomNav, ToastHost
  Components/Shared/   AppIcon (+ AppIconPaths.cs), ProductCard, ProductListing, FilterPanelBody, SortSelect, Pagination,
                       PriceDisplay, RatingDisplay, ProductBadge, WishlistButton, QuantityStepper, Breadcrumb, EmptyState,
                       Notice, SkeletonGrid, Field, OrderStatusBadge, OrderTimeline, AccountNav, AuthShell, RedirectToLogin
  Components/Pages/    one file per route, see the table below
```

Page components that would clash with a model name get a `Page` suffix (`CartPage`, `ProductPage`).

## Routes

| Route | Page | Layout |
|---|---|---|
| `/` | Home | Main |
| `/categories`, `/categories/{slug}` | CategoriesPage, CategoryPage | Main |
| `/search?q=&sort=` | SearchPage | Main |
| `/products/{slug}` | ProductPage (offers per seller, add a specific offer) | Main |
| `/sellers/{slug}`, `/campaigns/{slug}` | SellerPage, CampaignPage | Main |
| `/cart`, `/wishlist` | CartPage (grouped by seller), WishlistPage | Main |
| `/checkout` | CheckoutPage (success view shows the order number) | Checkout |
| `/account`, `/account/orders`, `/account/orders/{id}` | AccountPage, OrdersPage, OrderDetailPage | Main |
| `/login`, `/register`, `/forgot-password`, `/reset-password` | auth pages | Auth |

Register / forgot / reset only validate and show a "not available yet" notice: Identity has no
user store. Account menu items without a backend (addresses, payment methods, personal info,
security), coupons, newsletter and store follow are disabled or show a "coming soon" state.
**Never fake a success** for something the backend doesn't do.

`ProductListing` is shared by category, search, seller and campaign pages: it owns filters,
chips, facets, sort, grid/list, pagination (desktop) and load more + filter sheet (mobile).
The query lives in the URL (`CatalogQuery.ToQueryString()`), so filters survive reloads.

## Design system

- Tokens are CSS variables in `tokens.css` (`--c-primary` navy `#243B53`, `--c-accent`
  champagne `#C49A63`, surfaces, outline, radii `--r-*`, spacing `--s-*`, shadows).
  Never hard-code a color in a component, use a token.
- `storefront.css` holds everything else: typography classes (`t-display`, `t-headline-*`,
  `t-title-*`, `t-body-*`, `t-label-*`, `t-mono`), buttons (`btn btn-primary|btn-outline|btn-sm`),
  inputs, badges, pills, cards, product card (`pcard`), listing, pdp, cart, checkout, account,
  auth. Breakpoints are 1100px and 768px; `hide-mobile` / `only-mobile` helpers.
- Icons: `<AppIcon Name="shopping_bag" />` renders inline Material Symbols SVG paths from
  `AppIconPaths.cs` (`Filled="true"` uses the `_fill` path). To add an icon, add its path there.
- No Bootstrap, no Tailwind, no icon fonts, no CDN.

## Localization

- UI text comes from `IStringLocalizer<SharedResource>`, injected as `L` in `_Imports.razor`:
  `@L["Cart.Summary"]`, `@string.Format(L["Cart.Subtitle"], count, sellers)`.
- Keys are `Area.Name` (`Nav.Home`, `Product.AddToCart`, `Validation.Required`). Every key
  must exist in **both** `SharedResource.resx` (en) and `SharedResource.tr.resx` (tr).
  Turkish text lives only in `*.tr.resx` (and Catalog seed data), never in `.razor`/`.cs`.
- Culture comes only from the `.AspNetCore.Culture` cookie; default `tr-TR`, also `en-US`.
  `LanguageToggle` navigates to `/culture/set?culture=&redirectUri=` with `forceLoad: true`
  so the circuit restarts in the new culture.
- `GatewayClient` sends `Accept-Language`; Catalog returns names and descriptions in that
  language. Basket and orders keep the name from add time, so cart and checkout re-resolve
  names through `ICatalogService.GetItemsByIdsAsync`.
- Format numbers through `Fmt` (`Fmt.Price` gives `12.499 TL` / `12,499 TL`,
  `Fmt.OrderNumber` gives `SB-yyyy-xxxxx`).

## State and services

- Everything user related is **scoped** (one per circuit). Never singleton.
- `StorefrontState.EnsureLoadedAsync()` loads categories, cart count and wishlist once per
  circuit. Layout parts subscribe to `State.Changed` and unsubscribe in `Dispose`.
- `AddToCartAsync` / `ToggleWishlistAsync` return `false` for guests; the caller redirects
  to `login?returnUrl=...`. Show feedback with `State.ShowToast(text, isError)`.
- Calls go through a service. Anonymous endpoints use `_gateway.Anonymous`, user endpoints
  `await _gateway.AuthorizedAsync()`.
- Catch `HttpRequestException` in pages and show `Notice` / toast text from resources.

## Auth

- `IdentityService.LoginAsync(userOrEmail, password, rememberMe)` posts to gateway `/auth`
  and calls `CustomAuthStateProvider.SignInAsync(session, rememberMe)`. Remember me stores the
  `UserSession` in `ProtectedLocalStorage`, otherwise in `ProtectedSessionStorage`.
- Protected pages wrap markup in `<AuthorizeView>` with
  `<NotAuthorized><RedirectToLogin /></NotAuthorized>`. **Don't use `@attribute [Authorize]`**:
  the server has no auth scheme (the token is only in the browser).
- Login only follows local `returnUrl` values.
- Orders are created asynchronously after checkout, so `OrderDetailPage` retries a 404 a few
  times before showing "not found".

## Checklist

- [ ] Calls go through a service in `Application/Services`, never `HttpClient` in a page
- [ ] New UI text added to both resx files, no literal text in markup
- [ ] Colors / spacing from tokens, layout checked at 390px and 1280px
- [ ] Protected page uses `<AuthorizeView>` + `RedirectToLogin`, no `[Authorize]`
- [ ] Loading, empty and error states handled (`SkeletonGrid`, `EmptyState`, `Notice`)
- [ ] Gateway route or aggregator exists for every endpoint the page calls
- [ ] Missing backend features shown as "coming soon", not as fake success
