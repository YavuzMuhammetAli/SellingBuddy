---
name: status-dashboard
description: >
  Use when changing src/Monitoring/StatusDashboard (TabBlazor based service status UI on
  port 5300) or the health check contract it reads. Covers TabBlazor setup and traps,
  layout hosts, the icon pattern, StatusStore / StatusMonitorService data flow, how to
  add a page, a monitored target or an infrastructure probe, and the /health JSON shape.
---

# Status Dashboard (TabBlazor)

The Consul UI (http://localhost:8500) can't be themed, so service health is shown by our
own Blazor Server app: **http://localhost:5300** (`c_statusdashboard` in docker). It reads:

- Consul catalog + health (`/v1/catalog/services`, `/v1/health/service/{name}`, leader)
- every instance's `/health` JSON (address:port from Consul, or `Monitoring:StaticTargets`)
- RabbitMQ management API (`/api/overview`, `/api/queues`, basic auth)
- TCP probes for `Monitoring:InfrastructureTargets` (SQL Server, Redis, RabbitMQ, Consul)

It is meant to run **in docker** (`ASPNETCORE_ENVIRONMENT=Docker` → `appsettings.Docker.json`
with `c_*` host names). Run locally, Consul instance addresses (`c_*`) can't be resolved and
those instances show as Unknown.

## TabBlazor rules

- Package `TabBlazor` **1.7.86-beta**, pinned (prerelease; `5.2.0-alpha` is an old deprecated net5 line).
- `App.razor` head: Tabler `@tabler/core@1.7.0` css from unpkg, `_content/TabBlazor/css/tabblazor.min.css`,
  `app.css`, `StatusDashboard.styles.css` (needed for Tabs/DataView/Dropdown styles);
  scripts `_content/TabBlazor/js/tabblazor.js`, `js/theme.js`. In razor write `@@tabler`.
- `MainLayout` must host `<ModalContainer />`, `<ToastContainer />`, `<OffcanvasContainer />`.
- `ToastService` lives in `TabBlazor.Services` (in `_Imports`).
- **No icons ship.** Add Tabler icon SVG paths to `Components/Shared/AppIcons.cs`
  (`new TablerIcon("<path .../>")`), use `<Icon IconType="AppIcons.X" />`.
- Colors only via `TablerColor` (`Success`, `Danger`, `Yellow`, `Secondary`, `Primary`, `Azure`, `Purple`).
  Status → color/icon mapping lives in `StatusStyle`; don't hard-code it in pages.
- Exact parameters: `~/.nuget/packages/tabblazor/1.7.86-beta/componentapi.json` and `AGENTS.md`.
- Lists use `DataView` (+ `DataColumn` with `Template`); small fixed tables use plain
  `table card-table` markup.

## Data flow

```
StatusMonitorService (BackgroundService, every Monitoring:RefreshSeconds)
   ├─ ConsulStatusClient / HealthEndpointClient / RabbitMqManagementClient / TcpProbe
   └─ StatusStore.Update(snapshot)   singleton: Current snapshot, history ring buffer, status changes
         ├─ Changed        → every LiveComponentBase re-renders (InvokeAsync(StateHasChanged))
         └─ StatusChanged  → MainLayout shows a toast (critical / recovered)
```

- Status rules: instance = worst of Consul checks and its own `/health` call; service = worst
  instance (no instance → Critical); worker (no HTTP, e.g. NotificationService) = Passing when
  all its queues (`QueuePrefix`) have consumers; Unknown never beats a known status.
- Pages and live widgets inherit `LiveComponentBase` (`@inherits LiveComponentBase`) and read
  `Snapshot` / `Store.GetHistory(StatusStore.ServiceKey(name))`. Keep values as computed
  properties so each re-render uses the latest snapshot.
- "Refresh now" → `StatusStore.RequestRefresh()` wakes the monitor early.

## Pages

| Route | File | Content |
|---|---|---|
| `/` | `Overview.razor` | KPI cards, service cards with sparkline, infrastructure table, status change timeline |
| `/services` | `Services.razor` | DataView of all services |
| `/services/{Name}` | `ServiceDetail.razor` | Tabs (`?tab=`): instances, consul checks, dependencies, response time, queues |
| `/eventbus` | `EventBusPage.razor` | broker KPIs, queues (orphan warning), event flow from `Core/EventCatalog.cs` |
| `/infrastructure` | `InfrastructurePage.razor` | probe cards, Consul and RabbitMQ details |

Page components that clash with a namespace get a `Page` suffix (`InfrastructurePage`).

## Adding things

- **New service in Consul** → nothing to do, it shows up automatically (give it the standard `/health`).
- **New HTTP app outside Consul** → `Monitoring:StaticTargets` (both appsettings files).
- **New queue-only worker** → `Monitoring:WorkerTargets` with its `SubscriberClientAppName` + `.`.
- **New infra component** → `Monitoring:InfrastructureTargets` (+ icon in `StatusStyle.InfrastructureIcon`).
- **New event** → add it to `Core/EventCatalog.cs` and the `eventbus-integration` skill table.
- **New page** → `Components/Pages`, `@inherits LiveComponentBase`, `DashboardHeader` + `RefreshButton`,
  menu item in `Components/Layout/MainNavigation.razor`.

## /health contract (HealthChecks.Common)

```json
{ "status": "Healthy|Degraded|Unhealthy", "totalDurationMs": 12.3,
  "entries": [ { "name": "redis", "status": "Healthy", "durationMs": 1.2, "description": "...", "tags": ["dependency"] } ] }
```

Unhealthy answers HTTP 503 with the same body. `/health/live` only runs the `self` check.

## Checklist

- [ ] English only, `//short lowercase` comments
- [ ] Colors/icons through `StatusStyle` / `AppIcons`
- [ ] Live widget inherits `LiveComponentBase`
- [ ] New config keys added to `appsettings.json` **and** `appsettings.Docker.json`
- [ ] `docker compose up -d --build statusdashboard` and check http://localhost:5300
