# RSS Feed Reader (MVP)

A minimal RSS/Atom feed reader built with **spec-driven development** using GitHub Spec Kit. The MVP demonstrates one capability: **add a feed subscription by URL and see the subscription list**.

## Architecture

| Component | Technology | URL |
|---|---|---|
| Backend | ASP.NET Core Web API (.NET 8) | <http://localhost:5151> |
| Frontend | Blazor WebAssembly (.NET 8) | <http://localhost:5213> |

Subscriptions are stored **in memory** behind an `ISubscriptionStore` abstraction, so they are cleared when the backend restarts. The abstraction allows a persistent implementation (EF Core + SQLite) to be added later without changing controllers or UI.

## Prerequisites

- .NET SDK 8.0 or later
- A modern browser with WebAssembly enabled

## Build

```powershell
dotnet build RSSFeedReader.slnx
```

## Run

Use two terminals.

**Terminal 1 - backend:**

```powershell
dotnet run --project backend/RSSFeedReader.Api --launch-profile http
```

**Terminal 2 - frontend:**

```powershell
dotnet run --project frontend/RSSFeedReader.UI --launch-profile http
```

Then open <http://localhost:5213>.

## Configuration

Ports must stay consistent across these four locations:

| Setting | File | Value |
|---|---|---|
| Backend port | `backend/RSSFeedReader.Api/Properties/launchSettings.json` | `http://localhost:5151` |
| Frontend port | `frontend/RSSFeedReader.UI/Properties/launchSettings.json` | `http://localhost:5213` |
| API base URL | `frontend/RSSFeedReader.UI/wwwroot/appsettings.json` | `http://localhost:5151/api/` |
| CORS origins | `backend/RSSFeedReader.Api/Program.cs` | `http://localhost:5213`, `https://localhost:7213` |

## API

| Method | Route | Result |
|---|---|---|
| `GET` | `/api/subscriptions` | `200` with the subscription list in insertion order |
| `POST` | `/api/subscriptions` | `201` with the created subscription, or `400` for a blank URL or a URL longer than 2,048 characters |

```powershell
curl http://localhost:5151/api/subscriptions
curl -X POST http://localhost:5151/api/subscriptions -H "Content-Type: application/json" -d '{"url":"https://devblogs.microsoft.com/dotnet/feed/"}'
```

## Spec-driven development artifacts

| Artifact | Path |
|---|---|
| Constitution | `.specify/memory/constitution.md` |
| Specification | `specs/001-mvp-rss-reader/spec.md` |
| Requirements checklist | `specs/001-mvp-rss-reader/checklists/requirements.md` |
| Technical plan | `specs/001-mvp-rss-reader/plan.md` |
| Research | `specs/001-mvp-rss-reader/research.md` |
| Data model | `specs/001-mvp-rss-reader/data-model.md` |
| API contract | `specs/001-mvp-rss-reader/contracts/subscriptions-api.md` |
| Quickstart | `specs/001-mvp-rss-reader/quickstart.md` |
| Tasks | `specs/001-mvp-rss-reader/tasks.md` |

Stakeholder inputs are in `StakeholderDocuments/`.

## MVP scope

**Included**: add a subscription by URL, list subscriptions, empty/loading/error states, blank and length validation on both client and server.

**Not included** (Extended-MVP and post-MVP): feed fetching and parsing, item display, refresh, persistence, subscription removal, de-duplication, background polling, read/unread tracking, folders, search, OPML, authentication, automated tests.
