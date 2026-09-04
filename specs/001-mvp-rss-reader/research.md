# Phase 0 Research: MVP RSS Feed Reader

**Feature**: 001-mvp-rss-reader | **Date**: 2026-09-04

## Purpose

Resolve the technical unknowns for the MVP subscription-management slice and record the decisions that the implementation plan depends on.

## R1: Backend framework and API shape

**Decision**: ASP.NET Core Web API (.NET 8) with a controller-based `SubscriptionsController` exposing `GET /api/subscriptions` and `POST /api/subscriptions`.

**Rationale**: TechStack.md mandates ASP.NET Core Web API. Controllers give a conventional, discoverable structure and model binding/validation attributes out of the box, and they extend naturally when Extended-MVP adds refresh and items endpoints.

**Alternatives considered**: Minimal APIs - fewer lines for two endpoints, but less structure for the endpoints added later. Rejected for consistency with the anticipated growth path.

## R2: Frontend framework

**Decision**: Blazor WebAssembly (.NET 8), single routed page `Subscriptions.razor` at `@page "/"`.

**Rationale**: Required by TechStack.md; C# end-to-end avoids a second language and toolchain; WebAssembly hosting keeps the frontend a static SPA that talks to the API over HTTP, reinforcing the separation of concerns in Constitution II.

**Alternatives considered**: Blazor Server (fewer CORS concerns but couples UI to the server process and contradicts the stated architecture). Rejected.

## R3: MVP storage strategy

**Decision**: Singleton `InMemorySubscriptionStore` implementing `ISubscriptionStore`, backed by `List<Subscription>` with a private lock object guarding mutations and enumeration.

**Rationale**: ProjectGoals.md and AppFeatures.md explicitly call for in-memory storage and accept data loss on restart (FR-010). A singleton preserves state across requests within the process lifetime. The lock addresses the concurrent-add edge case without introducing a database or additional dependencies.

**Alternatives considered**:
- `ConcurrentBag<T>` - thread-safe but does not preserve insertion order, violating FR-009. Rejected.
- EF Core + SQLite - satisfies persistence but is explicitly post-MVP scope and adds migration complexity. Rejected.
- `List<T>` without a lock - simplest, but risks corruption under concurrent adds. Rejected.

## R4: Identifier strategy

**Decision**: `Guid` generated in the store when a subscription is created.

**Rationale**: Satisfies FR-011 (unique identifier) with no coordination or sequence; portable to a future database implementation without changing the contract.

**Alternatives considered**: Incrementing integer - readable, but requires managing a counter and would change semantics when persistence is introduced. Rejected.

## R5: Validation boundaries

**Decision**: Validate on both boundaries. The API rejects null/empty/whitespace-only URLs and URLs longer than 2,048 characters with HTTP 400 and a descriptive message. The UI performs the same blank check before dispatching a request and shows an inline validation message.

**Rationale**: Constitution III requires server-side enforcement so the rule holds when the API is called directly, while client-side checks give instant feedback (FR-003, FR-004, FR-015). Feed-format validation is explicitly excluded by the stakeholders (FR-012).

**Alternatives considered**: Client-only validation - fails Constitution III. Rejected. Server-only validation - adds an avoidable round trip for an obvious error. Rejected.

## R6: Cross-origin configuration

**Decision**: A named CORS policy in the backend `Program.cs` that lists the frontend origins explicitly (`http://localhost:5213` and the https profile URL), allowing any header and any method, without credentials.

**Rationale**: The Blazor WASM app runs on a different origin than the API, so browser requests fail without CORS. TechStack.md calls out port/CORS coordination as a common failure point. Explicit origins satisfy Constitution IV.

**Alternatives considered**: `AllowAnyOrigin()` - simpler, but a poor default and disallowed with credentials. Rejected.

## R7: Frontend configuration of the API base URL

**Decision**: Store `ApiBaseUrl` in `frontend/RSSFeedReader.UI/wwwroot/appsettings.json` and read it in `Program.cs` with a fallback of `http://localhost:5151/api/`, then register a typed `SubscriptionApiClient` over an `HttpClient` configured with that base address.

**Rationale**: Constitution IV forbids hard-coded environment values. A typed client keeps HTTP concerns out of the Razor component and makes the call sites readable.

**Alternatives considered**: Hard-coded `HttpClient` base address in `Program.cs` - violates Constitution IV. Rejected.

## R8: Blazor template cleanup

**Decision**: Delete `Pages/Home.razor`, `Pages/Counter.razor`, and `Pages/Weather.razor`, remove their `NavMenu.razor` links, and verify that only `Subscriptions.razor` declares `@page "/"` before implementing any feature code.

**Rationale**: TechStack.md flags this as critical: two components declaring the root route cause an `InvalidOperationException: The following routes are ambiguous` that only appears at runtime, after feature work is done. Doing the cleanup as a blocking foundational task avoids costly late debugging (Constitution V).

**Alternatives considered**: Give the subscriptions page a non-root route such as `/subscriptions` and keep `Home.razor` - keeps template pages that add no value and leaves the landing page empty. Rejected.

## R9: Error presentation

**Decision**: The API returns a descriptive message for validation failures; the UI catches `HttpRequestException` and non-success responses and shows a single dismissible alert with plain-language text (for example "Could not reach the RSS Feed Reader service. Make sure the backend is running.").

**Rationale**: FR-013 requires clear, non-technical errors; Constitution III forbids unhandled exceptions and HTTP 500s for predictable failure modes.

## R10: Testing approach for the MVP

**Decision**: Manual verification against the spec's acceptance scenarios, plus a build gate (`dotnet build` with zero errors) at each phase boundary. No automated test projects in the MVP.

**Rationale**: ProjectGoals.md prioritizes rapid delivery of a proof of concept, and TechStack.md lists xUnit testing under post-MVP enhancements. The architecture (interfaces + typed client) leaves the code testable when tests are added.

**Alternatives considered**: Adding xUnit projects now - improves confidence but exceeds MVP scope defined in Constitution I. Rejected, recorded as deferred scope.

## Open Questions

None. All Technical Context fields are resolved; no `NEEDS CLARIFICATION` markers remain.
