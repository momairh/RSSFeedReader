# API Contract: Subscriptions

**Feature**: 001-mvp-rss-reader | **Base URL**: `http://localhost:5151/api`

All requests and responses use `application/json`.

---

## GET /api/subscriptions

Returns every subscription currently stored, in the order it was added (newest last).

**Request**: no body, no parameters.

**Responses**

| Status | Body | Meaning |
|---|---|---|
| 200 OK | `SubscriptionResponse[]` | Zero or more subscriptions; an empty array when none exist |

**Example response (200)**

```json
[
  {
    "id": "8f14e45f-ea4e-4b1c-9c39-3f6e0a4b1f2d",
    "url": "https://devblogs.microsoft.com/dotnet/feed/",
    "addedAt": "2026-09-04T15:42:11.123+00:00"
  },
  {
    "id": "b2c3d4e5-1234-4a5b-8c9d-0e1f2a3b4c5d",
    "url": "https://devblogs.microsoft.com/visualstudio/feed/",
    "addedAt": "2026-09-04T15:43:02.987+00:00"
  }
]
```

**Requirements covered**: FR-002, FR-008, FR-009

---

## POST /api/subscriptions

Adds a new subscription.

**Request body**

```json
{ "url": "https://devblogs.microsoft.com/dotnet/feed/" }
```

| Field | Type | Required | Constraints |
|---|---|---|---|
| `url` | string | Yes | Not null/empty/whitespace after trimming; 2,048 characters or fewer |

**Responses**

| Status | Body | Meaning |
|---|---|---|
| 201 Created | `SubscriptionResponse` | Subscription stored; `Location` header points to `GET /api/subscriptions` |
| 400 Bad Request | `ValidationError` | URL was blank or exceeded the length limit |

**Example response (201)**

```json
{
  "id": "8f14e45f-ea4e-4b1c-9c39-3f6e0a4b1f2d",
  "url": "https://devblogs.microsoft.com/dotnet/feed/",
  "addedAt": "2026-09-04T15:42:11.123+00:00"
}
```

**Example response (400) - blank URL**

```json
{ "error": "Feed URL is required." }
```

**Example response (400) - too long**

```json
{ "error": "Feed URL must be 2048 characters or fewer." }
```

**Behavior notes**

- The URL is trimmed before storage.
- Duplicate URLs are accepted; each add creates a distinct subscription.
- The URL is not validated as a well-formed URI or as a reachable RSS/Atom feed (FR-012).
- No feed content is fetched (FR-016).

**Requirements covered**: FR-001, FR-003, FR-011, FR-012, FR-015

---

## Schemas

### SubscriptionResponse

| Field | Type | Description |
|---|---|---|
| `id` | string (GUID) | Unique identifier |
| `url` | string | Feed URL as stored |
| `addedAt` | string (ISO 8601 date-time) | When the subscription was added |

### ValidationError

| Field | Type | Description |
|---|---|---|
| `error` | string | Human-readable validation message suitable for display to the user |

---

## Out-of-contract (deferred)

`DELETE /api/subscriptions/{id}`, `POST /api/subscriptions/{id}/refresh`, and `GET /api/items` are Extended-MVP or post-MVP and are intentionally absent from this contract.
