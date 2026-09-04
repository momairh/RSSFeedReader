# Implementation Plan: MVP RSS Feed Reader - Subscription Management

**Branch**: `001-mvp-rss-reader` | **Date**: 2026-09-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-mvp-rss-reader/spec.md`

## Summary

Deliver the MVP RSS Feed Reader: a user can add a feed subscription by URL and see the list of subscriptions. The solution is an ASP.NET Core Web API backend (`http://localhost:5151`) that stores subscriptions in a thread-safe in-memory collection behind an `ISubscriptionStore` abstraction, plus a Blazor WebAssembly frontend (`http://localhost:5213`) with a single Subscriptions page that adds and lists subscriptions through a typed HTTP client. No feed fetching, parsing, or persistence is implemented; those are deferred to Extended-MVP and post-MVP phases.

## Technical Context

**Language/Version**: C# 12 / .NET 8 (LTS; also builds on .NET 9/10 SDKs installed locally)

**Primary Dependencies**: ASP.NET Core Web API, Blazor WebAssembly (`Microsoft.AspNetCore.Components.WebAssembly`). No third-party NuGet packages.

**Storage**: In-memory `List<Subscription>` guarded by a lock, behind `ISubscriptionStore`, registered as a singleton. Data is lost on backend restart (FR-010).

**Testing**: Manual verification against the acceptance scenarios in spec.md using the running applications. Automated test projects are deferred to post-MVP per the stakeholder "rapid development" directive.

**Target Platform**: Cross-platform (Windows, macOS, Linux); browser-hosted WebAssembly frontend, Kestrel-hosted backend.

**Project Type**: Web application - separate backend API and frontend SPA.

**Performance Goals**: Subscription appears in the list within 1 second of a successful add (SC-002); list renders within 2 seconds for up to 100 subscriptions (SC-005).

**Constraints**: MVP-only scope; in-memory storage; no feed operations; ports and CORS origins must stay consistent across `launchSettings.json`, `wwwroot/appsettings.json`, and `Program.cs`.

**Scale/Scope**: Single local user, one feature, two API endpoints, one UI page.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Gate | Status |
|---|---|---|
| I. MVP-First Delivery | Only "add subscription" and "list subscriptions" implemented; in-memory storage only | PASS - no fetching, parsing, persistence, or delete in scope |
| II. Layered Separation of Concerns | Backend API + Blazor WASM frontend, HTTP/JSON only; storage behind `ISubscriptionStore` | PASS - UI holds no storage logic; store is DI-registered |
| III. Explicit, Verifiable Input Handling | API returns 400 on blank/over-length URL; UI blocks blank submission | PASS - validated on both boundaries (FR-003, FR-004, FR-015) |
| IV. Configuration Over Hard-Coding | Frontend reads `ApiBaseUrl` from `wwwroot/appsettings.json`; CORS lists explicit origins | PASS - no hard-coded API URL, no wildcard-with-credentials |
| V. Runnable, Verifiable State | Template demo pages deleted in Foundational phase; exactly one `@page "/"` | PASS - verification task included before feature work |

**Post-design re-check**: PASS. No principle deviations; Complexity Tracking section is empty.

## Project Structure

### Documentation (this feature)

```text
specs/001-mvp-rss-reader/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── subscriptions-api.md
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
RSSFeedReader.slnx
backend/
└── RSSFeedReader.Api/
    ├── Models/
    │   ├── Subscription.cs                 # Domain entity
    │   ├── AddSubscriptionRequest.cs       # POST request contract
    │   └── SubscriptionResponse.cs         # API response contract
    ├── Services/
    │   ├── ISubscriptionStore.cs           # Storage abstraction
    │   └── InMemorySubscriptionStore.cs    # MVP implementation (thread-safe)
    ├── Controllers/
    │   └── SubscriptionsController.cs      # GET/POST /api/subscriptions
    ├── Properties/launchSettings.json      # http://localhost:5151
    ├── Program.cs                          # DI, CORS, controllers
    └── appsettings.json
frontend/
└── RSSFeedReader.UI/
    ├── Models/
    │   └── SubscriptionResponse.cs         # Client-side DTO
    ├── Services/
    │   ├── ISubscriptionApiClient.cs
    │   └── SubscriptionApiClient.cs        # Typed HttpClient wrapper
    ├── Pages/
    │   └── Subscriptions.razor             # @page "/" - the only routed root page
    ├── Layout/
    │   ├── MainLayout.razor
    │   └── NavMenu.razor                   # demo links removed
    ├── wwwroot/appsettings.json            # { "ApiBaseUrl": "http://localhost:5151/api/" }
    ├── Properties/launchSettings.json      # http://localhost:5213
    └── Program.cs                          # config-driven HttpClient + DI
```

**Structure Decision**: Web application layout with `backend/` and `frontend/` folders, as directed by TechStack.md. A single solution file at the repository root allows building both projects with one command while keeping deployment units independent.

## Implementation Approach

### Phase 1: Setup

Create the solution, the Web API project, and the Blazor WebAssembly project; wire them into the solution; confirm a clean baseline build.

### Phase 2: Foundational (blocking)

1. Delete Blazor template demo pages (`Home.razor`, `Counter.razor`, `Weather.razor`) and their `NavMenu.razor` links, then verify exactly one component declares `@page "/"`. This is a hard gate (Constitution V, TechStack.md) because ambiguous-route exceptions only surface at runtime.
2. Configure ports: backend `http://localhost:5151`, frontend `http://localhost:5213`.
3. Configure `wwwroot/appsettings.json` with `ApiBaseUrl` and read it in `Program.cs`.
4. Configure the backend CORS policy for the frontend origin.
5. Register `ISubscriptionStore` -> `InMemorySubscriptionStore` as a singleton, and the typed API client in the frontend.

### Phase 3: User Story 1 - Add a subscription (P1)

Backend: `Subscription` entity, `AddSubscriptionRequest`, `POST /api/subscriptions` with blank/over-length validation returning HTTP 400 plus a descriptive message, HTTP 201 with the created resource on success.
Frontend: input field plus "Add Subscription" button, client-side blank check, error banner, input cleared and list refreshed on success.

### Phase 4: User Story 2 - View subscriptions (P1)

Backend: `GET /api/subscriptions` returning the list in insertion order (newest last).
Frontend: load the list in `OnInitializedAsync`, show a loading indicator while in flight, an empty-state message when the list is empty, and the ordered list otherwise; surface a friendly error message when retrieval fails.

### Phase 5: Polish and verification

Build both projects with zero errors, run both applications, walk through every acceptance scenario in the browser, confirm no console errors, and document the run steps in the README.

## Key Technical Decisions

| Decision | Choice | Rationale |
|---|---|---|
| API style | Controller-based Web API | Familiar structure, easy to extend for Extended-MVP endpoints |
| Storage | `List<Subscription>` + `lock`, singleton | Simplest approach satisfying FR-010 while remaining safe for concurrent adds |
| Storage abstraction | `ISubscriptionStore` | Allows a future EF Core/SQLite implementation without touching controllers or UI (Constitution II) |
| Identifier | `Guid` | Unique without coordination; no database sequence needed (FR-011) |
| Ordering | Insertion order preserved by `List<T>` | Directly satisfies FR-009 with no sort logic |
| Validation | Blank check and 2,048-character limit, both server and client | FR-003, FR-004, FR-015; server-side check holds even for direct API calls |
| Error handling | `ProblemDetails`-style message from API; friendly banner in UI | FR-013 without exposing stack traces |
| HTTP client | Typed `SubscriptionApiClient` behind `ISubscriptionApiClient` | Keeps the Razor page free of HTTP details, testable later |

## Deferred Scope (not implemented in this plan)

Feed fetching/parsing (`System.ServiceModel.Syndication`), manual refresh, item display, persistence (EF Core + SQLite), subscription removal, de-duplication, background polling, read/unread tracking, folders, search, OPML, authentication, automated test projects.

## Complexity Tracking

*No constitution deviations. This section is intentionally empty.*

