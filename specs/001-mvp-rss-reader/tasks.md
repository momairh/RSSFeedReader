# Tasks: MVP RSS Feed Reader - Subscription Management

**Feature Branch**: `001-mvp-rss-reader` | **Date**: 2026-09-04

**Input**: [spec.md](./spec.md), [plan.md](./plan.md), [data-model.md](./data-model.md), [contracts/subscriptions-api.md](./contracts/subscriptions-api.md), [.specify/memory/constitution.md](../../.specify/memory/constitution.md)

**Legend**: `[P]` = can run in parallel with other `[P]` tasks in the same phase (different files, no dependency).

---

## Phase 1: Setup (project initialization)

- [x] **T001** Create the solution file `RSSFeedReader.slnx` at the repository root.
- [x] **T002** Create the ASP.NET Core Web API project at `backend/RSSFeedReader.Api` targeting .NET 8 (`dotnet new webapi -o backend/RSSFeedReader.Api -f net8.0 --use-controllers`).
- [x] **T003** Create the Blazor WebAssembly project at `frontend/RSSFeedReader.UI` targeting .NET 8 (`dotnet new blazorwasm -o frontend/RSSFeedReader.UI -f net8.0`).
- [x] **T004** Add both projects to `RSSFeedReader.slnx`.
- [x] **T005** Add a `.gitignore` covering `bin/`, `obj/`, and IDE artifacts.
- [x] **T006** Verify the baseline: `dotnet build RSSFeedReader.slnx` completes with 0 errors. **Gate** (Constitution V).

---

## Phase 2: Foundational (BLOCKING - must complete before any user story work)

- [x] **T007** Delete Blazor template demo pages: `frontend/RSSFeedReader.UI/Pages/Home.razor`, `Pages/Counter.razor`, `Pages/Weather.razor`.
  - Blocks: T008, and all of Phase 4.
- [x] **T008** Update `frontend/RSSFeedReader.UI/Layout/NavMenu.razor`: remove nav links to the deleted demo pages and add a single "Subscriptions" link pointing at `/`.
  - Depends on: T007.
- [x] **T009** **Verification gate**: confirm no `Home.razor`/`Counter.razor`/`Weather.razor` remain and that exactly ONE component declares `@page "/"`.
  - `Get-ChildItem frontend/RSSFeedReader.UI/Pages -Filter *.razor | Select-Object Name`
  - `Select-String -Path frontend/RSSFeedReader.UI/**/*.razor -Pattern '@page "/"'`
  - Depends on: T007, T008. **Do not start Phase 3 or 4 until this passes** (Constitution V, research R8).
- [x] **T010** [P] Configure the backend port in `backend/RSSFeedReader.Api/Properties/launchSettings.json` to `http://localhost:5151`.
- [x] **T011** [P] Configure the frontend port in `frontend/RSSFeedReader.UI/Properties/launchSettings.json` to `http://localhost:5213`.
- [x] **T012** Create `frontend/RSSFeedReader.UI/wwwroot/appsettings.json` with `{ "ApiBaseUrl": "http://localhost:5151/api/" }` (FR-014, Constitution IV).
  - Depends on: T010.
- [x] **T013** In `frontend/RSSFeedReader.UI/Program.cs`, read `ApiBaseUrl` from configuration with a documented fallback and register an `HttpClient` with that base address.
  - Depends on: T012.
- [x] **T014** In `backend/RSSFeedReader.Api/Program.cs`, add a named CORS policy allowing origin `http://localhost:5213` (any header, any method) and apply it in the middleware pipeline before controller mapping (research R6).
  - Depends on: T011.
- [x] **T015** Create `backend/RSSFeedReader.Api/Models/Subscription.cs` with `Id` (Guid), `Url` (string), `AddedAt` (DateTimeOffset) per data-model.md (FR-011).
- [x] **T016** Create `backend/RSSFeedReader.Api/Services/ISubscriptionStore.cs` declaring `IReadOnlyList<Subscription> GetAll()` and `Subscription Add(string url)` (Constitution II).
  - Depends on: T015.
- [x] **T017** Create `backend/RSSFeedReader.Api/Services/InMemorySubscriptionStore.cs`: `List<Subscription>` guarded by a lock, `GetAll()` returns a snapshot preserving insertion order, `Add()` trims the URL and appends (FR-009, FR-010, concurrency edge case).
  - Depends on: T016.
- [x] **T018** Register `ISubscriptionStore` → `InMemorySubscriptionStore` as a **singleton** in `backend/RSSFeedReader.Api/Program.cs`, and ensure `AddControllers()` / `MapControllers()` are wired (FR-010).
  - Depends on: T017.
- [x] **T019** Remove the `WeatherForecast` template artifacts from the API project (model and controller) so only feature code remains.
- [x] **T020** Build gate: `dotnet build RSSFeedReader.slnx` completes with 0 errors.
  - Depends on: T007-T019.

**Checkpoint**: Both applications start, the frontend loads without routing errors, and the storage abstraction is available for injection.

---

## Phase 3: User Story 1 - Add a feed subscription by URL (P1)

**Goal**: A user can submit a feed URL and have it accepted and stored.
**Independent test**: POST a URL through the UI and confirm it is accepted and appears in the list.

- [x] **T021** Create `backend/RSSFeedReader.Api/Models/AddSubscriptionRequest.cs` with a single `Url` string property (contract: POST body).
- [x] **T022** [P] Create `backend/RSSFeedReader.Api/Models/SubscriptionResponse.cs` with `Id`, `Url`, `AddedAt` and a mapping helper from `Subscription` (contract: response schema).
  - Depends on: T015.
- [x] **T023** Create `backend/RSSFeedReader.Api/Controllers/SubscriptionsController.cs` with route `api/subscriptions`, injecting `ISubscriptionStore`.
  - Depends on: T016, T021, T022.
- [x] **T024** Implement `POST /api/subscriptions`: validate that `Url` is not null/empty/whitespace → HTTP 400 `{ "error": "Feed URL is required." }` (FR-003).
  - Depends on: T023.
- [x] **T025** Extend `POST /api/subscriptions` validation: reject trimmed URLs longer than 2,048 characters → HTTP 400 `{ "error": "Feed URL must be 2048 characters or fewer." }` (FR-015).
  - Depends on: T024.
- [x] **T026** Implement the success path of `POST /api/subscriptions`: store via `ISubscriptionStore.Add`, return HTTP 201 with `SubscriptionResponse` and a `Location` header (FR-001, FR-011, FR-012).
  - Depends on: T024, T025.
- [x] **T027** [P] Create `frontend/RSSFeedReader.UI/Models/SubscriptionResponse.cs` mirroring the API response schema.
- [x] **T028** Create `frontend/RSSFeedReader.UI/Services/ISubscriptionApiClient.cs` declaring `Task<IReadOnlyList<SubscriptionResponse>> GetSubscriptionsAsync()` and `Task<SubscriptionResponse> AddSubscriptionAsync(string url)`.
  - Depends on: T027.
- [x] **T029** Create `frontend/RSSFeedReader.UI/Services/SubscriptionApiClient.cs` implementing the interface over the configured `HttpClient`, translating non-success responses into a descriptive exception message (FR-013, research R9).
  - Depends on: T013, T028.
- [x] **T030** Register `ISubscriptionApiClient` → `SubscriptionApiClient` in `frontend/RSSFeedReader.UI/Program.cs`.
  - Depends on: T029.
- [x] **T031** Create `frontend/RSSFeedReader.UI/Pages/Subscriptions.razor` with `@page "/"`, a page heading, and the injected `ISubscriptionApiClient`.
  - Depends on: T009, T030.
- [x] **T032** Add the add-subscription form to `Subscriptions.razor`: a text input bound to a field and an "Add Subscription" button (FR-001).
  - Depends on: T031.
- [x] **T033** Implement client-side blank validation in `Subscriptions.razor`: block submission and show an inline validation message for empty/whitespace-only input (FR-004, US1 scenario 4).
  - Depends on: T032.
- [x] **T034** Implement the add handler: call `AddSubscriptionAsync`, and on success clear the input and refresh the displayed list immediately (FR-005, FR-006, US1 scenarios 2, 3, 5).
  - Depends on: T032, T033.
- [x] **T035** Implement error handling in `Subscriptions.razor`: catch client exceptions and API failures, show a friendly alert, and leave the list unchanged (FR-013, US1 scenario 6).
  - Depends on: T034.
- [x] **T036** Disable the Add button while a request is in flight to prevent duplicate submissions.
  - Depends on: T034.
- [x] **T037** Build gate: `dotnet build RSSFeedReader.slnx` completes with 0 errors.
  - Depends on: T021-T036.
- [x] **T038** Verify US1 acceptance scenarios 1-6 manually in the browser per quickstart.md steps 1-4 and 7.
  - Depends on: T037.

**Checkpoint**: User Story 1 is independently testable and complete.

---

## Phase 4: User Story 2 - View the list of subscriptions (P1)

**Goal**: The subscription list is retrieved from the backend and displayed in insertion order.
**Independent test**: Add two subscriptions, reload the page, and confirm both are retrieved and displayed in order.

- [x] **T039** Implement `GET /api/subscriptions` in `SubscriptionsController`: return HTTP 200 with all subscriptions mapped to `SubscriptionResponse`, preserving insertion order (FR-002, FR-008, FR-009).
  - Depends on: T023.
- [x] **T040** Implement `GetSubscriptionsAsync` in `SubscriptionApiClient`, returning an empty list rather than null when no subscriptions exist.
  - Depends on: T029, T039.
- [x] **T041** Load subscriptions in `Subscriptions.razor` `OnInitializedAsync` and store them in component state (FR-008, US2 scenarios 1 and 4).
  - Depends on: T031, T040.
- [x] **T042** Render the subscription list in `Subscriptions.razor` (URL plus added timestamp), preserving order newest last (FR-002, FR-009).
  - Depends on: T041.
- [x] **T043** Add the empty-state message ("No subscriptions yet. Add a feed URL to get started.") shown only when the list is empty and loading has finished (FR-007, US1 scenario 1).
  - Depends on: T042.
- [x] **T044** Add a loading indicator shown while the initial retrieval is in flight, so an incorrect empty state is never displayed (US2 scenario 2).
  - Depends on: T041, T043.
- [x] **T045** Add retrieval error handling: if `GetSubscriptionsAsync` fails, show a friendly message instead of an empty state (FR-013).
  - Depends on: T041.
- [x] **T046** Confirm that the add handler refreshes the list from state without a full page reload (FR-005, US2 scenario 3).
  - Depends on: T034, T042.
- [x] **T047** Build gate: `dotnet build RSSFeedReader.slnx` completes with 0 errors.
  - Depends on: T039-T046.
- [x] **T048** Verify US2 acceptance scenarios 1-5 manually per quickstart.md steps 5, 6, and 8.
  - Depends on: T047.

**Checkpoint**: User Story 2 is complete; the MVP (add + list) is fully functional.

---

## Phase 5: Polish and verification

- [x] **T049** [P] Update the repository `README.md` with prerequisites, build commands, run commands for both applications, the port/CORS configuration table, and the MVP scope statement.
- [x] **T050** [P] Verify the API contract directly with the curl commands in quickstart.md (200 on GET, 201 on valid POST, 400 on blank POST).
  - Depends on: T037, T047.
- [x] **T051** Run both applications and walk through every acceptance scenario in spec.md; confirm zero unhandled errors in the browser console (SC-006, SC-007).
  - Depends on: T048.
- [x] **T052** Final build gate and commit: `dotnet build RSSFeedReader.slnx` with 0 errors, then commit the MVP implementation.
  - Depends on: T049-T051.

---

## Dependencies Summary

- Phase 1 (T001-T006) blocks everything.
- Phase 2 (T007-T020) is blocking; **T009 is a hard gate** before any UI feature work.
- Phase 3 (T021-T038) delivers US1 and depends on Phase 2.
- Phase 4 (T039-T048) delivers US2; T039/T040 depend on the controller and client created in Phase 3.
- Phase 5 (T049-T052) depends on both user stories being complete.

## Implementation Strategy

**MVP First (recommended)**

- **Phases**: Setup → Foundational → US1 → US2 → Polish
- **Tasks**: T001 - T052 (52 tasks)
- **Deliverable**: A user can add a feed subscription by URL and see the full subscription list; both applications run locally with no console errors.

Because User Story 1 and User Story 2 are both P1 and jointly define "MVP working" in ProjectGoals.md, the MVP range covers all phases through Polish.

**Incremental alternative**: stop after T038 for a demonstrable add-only slice, then complete T039-T048 to add the read path.

## Requirements Coverage

| Requirement | Tasks |
|---|---|
| FR-001 | T026, T032 |
| FR-002 | T039, T042 |
| FR-003 | T024 |
| FR-004 | T033 |
| FR-005 | T034, T046 |
| FR-006 | T034 |
| FR-007 | T043 |
| FR-008 | T039, T041 |
| FR-009 | T017, T039, T042 |
| FR-010 | T017, T018 |
| FR-011 | T015, T026 |
| FR-012 | T026 |
| FR-013 | T029, T035, T045 |
| FR-014 | T010, T011, T012, T014 |
| FR-015 | T025 |
| FR-016 | (no fetching implemented anywhere - verified in T051) |


