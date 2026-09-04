# Quickstart: MVP RSS Feed Reader

**Feature**: 001-mvp-rss-reader | **Date**: 2026-09-04

## Prerequisites

- .NET SDK 8.0 or later (`dotnet --list-sdks`)
- A modern browser with WebAssembly enabled
- Two terminal windows (or a split terminal)

## Solution layout

```text
RSSFeedReader/
├── RSSFeedReader.sln
├── backend/RSSFeedReader.Api/     # ASP.NET Core Web API - http://localhost:5151
└── frontend/RSSFeedReader.UI/     # Blazor WebAssembly    - http://localhost:5213
```

## Build

From the repository root:

```powershell
dotnet build RSSFeedReader.sln
```

Expected result: build succeeds with 0 errors.

## Run

**Terminal 1 - backend:**

```powershell
dotnet run --project backend/RSSFeedReader.Api
```

Wait for `Now listening on: http://localhost:5151`.

**Terminal 2 - frontend:**

```powershell
dotnet run --project frontend/RSSFeedReader.UI
```

Wait for `Now listening on: http://localhost:5213`, then open <http://localhost:5213> in a browser.

## Configuration checkpoints

Ports must agree across three places (a common source of failures):

| Setting | File | Value |
|---|---|---|
| Backend port | `backend/RSSFeedReader.Api/Properties/launchSettings.json` | `http://localhost:5151` |
| Frontend port | `frontend/RSSFeedReader.UI/Properties/launchSettings.json` | `http://localhost:5213` |
| API base URL | `frontend/RSSFeedReader.UI/wwwroot/appsettings.json` | `http://localhost:5151/api/` |
| CORS origins | `backend/RSSFeedReader.Api/Program.cs` | includes `http://localhost:5213` |

## Verify the API directly

```powershell
# List subscriptions (expect [] on a fresh start)
curl http://localhost:5151/api/subscriptions

# Add a subscription (expect 201 with the created resource)
curl -X POST http://localhost:5151/api/subscriptions -H "Content-Type: application/json" -d '{"url":"https://devblogs.microsoft.com/dotnet/feed/"}'

# Blank URL is rejected (expect 400)
curl -X POST http://localhost:5151/api/subscriptions -H "Content-Type: application/json" -d '{"url":"   "}'
```

## Manual test walkthrough (MVP acceptance)

1. Open <http://localhost:5213>. The page shows "No subscriptions yet." (US1 scenario 1)
2. Paste `https://devblogs.microsoft.com/dotnet/feed/` and select **Add Subscription**. The URL appears in the list and the empty state disappears. (US1 scenarios 2 and 5)
3. Confirm the input field is now empty. (US1 scenario 3)
4. Select **Add Subscription** with an empty field. A validation message appears and no entry is added. (US1 scenario 4)
5. Add `https://devblogs.microsoft.com/visualstudio/feed/`. Both subscriptions are listed, the first one added appearing first. (US2 scenario 1)
6. Reload the browser page. Both subscriptions are still listed, retrieved from the backend. (US2 scenario 4)
7. Stop the backend (Ctrl+C) and try to add a subscription. A friendly error message appears and the list is unchanged. (US1 scenario 6)
8. Restart the backend and reload the page. The list is empty, confirming in-memory storage. (US2 scenario 5)
9. Open browser DevTools (F12). The console shows no unhandled errors. (SC-006)

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| "The following routes are ambiguous" | Template demo pages still present | Delete `Home.razor`, `Counter.razor`, `Weather.razor`; only `Subscriptions.razor` may use `@page "/"` |
| List never loads, console shows CORS error | Frontend origin missing from the CORS policy | Add `http://localhost:5213` to `WithOrigins(...)` in the backend `Program.cs` |
| "Could not reach the service" banner | Backend not running or wrong `ApiBaseUrl` | Start the backend; confirm `wwwroot/appsettings.json` matches the backend port |
| Subscriptions vanish after a restart | Expected MVP behavior | In-memory storage only (FR-010) |

## What is not included

No feed fetching, parsing, item display, refresh, deletion, or persistence. Those are Extended-MVP and post-MVP scope.
