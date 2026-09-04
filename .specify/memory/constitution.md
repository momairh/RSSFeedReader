# RSSFeedReader Constitution

## Core Principles

### I. MVP-First Delivery (NON-NEGOTIABLE)

Scope MUST be limited to the smallest slice that delivers demonstrable user value. For the MVP that slice is exactly two capabilities: add a feed subscription by URL, and display the list of subscriptions.

- Features listed as Extended-MVP or post-MVP (feed fetching, parsing, persistence, deletion, background polling, read/unread tracking, folders, OPML) MUST NOT be implemented in the MVP.
- Any work item that does not directly enable "add a subscription" or "list subscriptions" MUST be deferred and recorded in the plan's deferred-scope section.
- Simplicity wins: in-memory storage (`List<T>`) MUST be used for MVP state; no database, ORM, or migration tooling is added.

### II. Layered Separation of Concerns

The application MUST be split into an ASP.NET Core Web API backend and a Blazor WebAssembly frontend, communicating only over HTTP/JSON.

- The backend owns subscription storage and API contracts; it MUST NOT contain UI concerns.
- The frontend owns presentation and user interaction; it MUST NOT contain storage logic and MUST access data exclusively through the API client abstraction.
- Storage MUST sit behind an interface (`ISubscriptionStore`) registered via dependency injection, so an in-memory implementation can be replaced by a persistent one without changing controllers or UI.

### III. Explicit, Verifiable Input Handling

Every API endpoint and UI form MUST validate input before processing.

- The "Add Subscription" API endpoint MUST reject requests with a null, empty, or whitespace-only URL and return HTTP 400 with a descriptive message.
- The frontend MUST disable or block submission for empty/whitespace-only input, so the user receives immediate feedback without a round trip.
- Deep RSS/Atom URL format validation is explicitly out of scope for the MVP; the app accepts any non-blank string as a subscription URL.
- Validation failures MUST NOT throw unhandled exceptions or return HTTP 500.

### IV. Configuration Over Hard-Coding

Environment-specific values MUST be read from configuration rather than embedded in code.

- The frontend MUST read the API base URL from `wwwroot/appsettings.json` (`ApiBaseUrl`) with a documented fallback default.
- Backend and frontend ports MUST be declared in each project's `Properties/launchSettings.json` and MUST stay consistent with the frontend `ApiBaseUrl` and the backend CORS origins.
- The backend CORS policy MUST explicitly list the frontend origins; wildcard origins combined with credentials MUST NOT be used.

### V. Runnable, Verifiable State at Every Step

The solution MUST build and run cleanly at the end of every implementation phase.

- `dotnet build` MUST complete with zero errors before a phase is considered done.
- Blazor template demo pages (`Home.razor`, `Counter.razor`, `Weather.razor`) MUST be removed during the foundational phase, and exactly ONE component may declare `@page "/"`, to prevent ambiguous-route runtime exceptions.
- Each user story MUST be independently testable through the running UI, and its acceptance scenarios MUST be expressed in Given-When-Then form.

## Technology Standards

- **Backend**: ASP.NET Core Web API (.NET 8 or later), minimal or controller-based endpoints, `http://localhost:5151`.
- **Frontend**: Blazor WebAssembly (.NET 8 or later), `http://localhost:5213`.
- **Language**: C# with nullable reference types and implicit usings enabled.
- **Storage (MVP)**: thread-safe in-memory collection behind `ISubscriptionStore`; data loss on restart is accepted and documented.
- **Cross-platform**: The solution MUST build and run on Windows, macOS, and Linux; no OS-specific APIs or paths.
- **Dependencies**: No third-party NuGet packages for the MVP. Extended-MVP may add `System.ServiceModel.Syndication` only.
- **API contract**: JSON over HTTP under the `/api/` prefix; resource-oriented routes (`GET /api/subscriptions`, `POST /api/subscriptions`).

## Development Workflow

- **Spec-driven order**: constitution → spec → plan → tasks → implement. Code MUST NOT be written before the corresponding task exists in `tasks.md`.
- **Traceability**: every functional requirement in `spec.md` MUST map to at least one task, and every task MUST reference the requirement or user story it satisfies.
- **Branching**: feature work happens on a numbered feature branch (for example `001-mvp-rss-reader`); artifacts live under `specs/<branch-name>/`.
- **Commits**: commit at phase boundaries with messages describing the completed phase; the repository MUST always be left in a building state.
- **Verification gate**: before marking a user story complete, run the backend and frontend, exercise each acceptance scenario in the browser, and confirm no console errors.
- **Quality**: builds MUST be warning-aware (no new warnings introduced by feature code); public service and controller members MUST have clear, intention-revealing names.

## Governance

This constitution supersedes ad-hoc preferences for the RSSFeedReader project. All specs, plans, tasks, and code reviews MUST verify compliance with the principles above.

- Amendments MUST be recorded in this file with a version bump and a short rationale.
- Versioning follows MAJOR.MINOR.PATCH: MAJOR for removing or redefining a principle, MINOR for adding a principle or section, PATCH for clarifications.
- Any deviation from a principle MUST be justified in `plan.md` under Complexity Tracking, including the simpler alternative that was rejected and why.
- Scope expansion beyond the MVP requires an explicit amendment or a new feature specification; it MUST NOT be introduced during implementation.

**Version**: 1.0.0 | **Ratified**: 2026-09-04 | **Last Amended**: 2026-09-04
