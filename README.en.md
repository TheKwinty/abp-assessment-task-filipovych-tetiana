[Українська](README.md) | **[English](README.en.md)**

# Conference Rooms API

## Overview

Conference Rooms API is an ASP.NET Core Web API for conference Hall management, availability search, booking, rental pricing, and booking analytics. It models the supplied assessment rules while keeping persistence, HTTP concerns, and business logic in focused projects.

## Implemented Features

- Hall CRUD with capacity, hourly rate, and service offerings.
- Availability search by future time window and required capacity.
- Booking confirmation with immutable Hall, service-price, and total-price snapshots.
- Half-hour booking starts, whole-hour durations, operating-hour validation, and overlap prevention.
- Tariff-boundary pricing with one-time service charges.
- Booking analytics grouped by current Hall identity and name.
- Development-only Swagger/OpenAPI documentation.
- Safe ProblemDetails responses with trace identifiers.
- Configurable CORS, rate limiting, request-body protection, HTTPS redirection, and HSTS.
- Portable unit and API test suites.

## Tech Stack

- .NET 10
- ASP.NET Core Web API with controllers
- Entity Framework Core 10
- Microsoft SQL Server
- xUnit
- Swashbuckle / Swagger

## Solution Structure

| Project | Responsibility |
| --- | --- |
| `ConferenceRooms.Api` | HTTP contracts, controllers, application services, configuration, and API pipeline. |
| `ConferenceRooms.Core` | Domain entities, booking-time rules, and rental pricing. |
| `ConferenceRooms.Infrastructure` | EF Core SQL Server context, mappings, migrations, and seed data. |
| `ConferenceRooms.UnitTests` | Portable Core and business-rule tests. |
| `ConferenceRooms.ApiTests` | Portable HTTP pipeline, validation, and Swagger tests. |

Dependencies point inward: API and Infrastructure depend on Core, while Core does not depend on ASP.NET Core or EF Core. The API composes the application and references Infrastructure for persistence.

## Prerequisites

- .NET 10 SDK
- Microsoft SQL Server reachable as `localhost`
- Optional: `dotnet-ef` 10.x for running EF CLI commands manually

## Database Setup

The committed local-development connection uses:

- Server: `localhost`
- Database: `ConferenceRoomsDb`
- Windows Integrated Authentication

It contains no password and is not intended as production configuration. Override it through `ConnectionStrings__DefaultConnection` in the target environment.

Apply the existing migration from PowerShell:

```powershell
dotnet ef database update `
  --project src/ConferenceRooms.Infrastructure `
  --startup-project src/ConferenceRooms.Api
```

Windows-friendly one-line alternative:

```powershell
dotnet ef database update --project src/ConferenceRooms.Infrastructure --startup-project src/ConferenceRooms.Api
```

## Running the API

```powershell
dotnet run --project src/ConferenceRooms.Api --launch-profile https
```

Development URLs:

- `https://localhost:7284`
- `http://localhost:5228`

## Swagger

Open `https://localhost:7284/swagger` after starting the HTTPS Development profile. Swagger UI and the OpenAPI JSON document are enabled in Development only.

## Seed Data

| Hall | Capacity | Base hourly rate |
| --- | ---: | ---: |
| Hall A | 50 | 2000 UAH |
| Hall B | 100 | 3500 UAH |
| Hall C | 30 | 1500 UAH |

Every seeded Hall offers:

- Projector: 500 UAH
- Wi-Fi: 300 UAH
- Sound system (`Sound`): 700 UAH

[`src/ConferenceRooms.Api/ConferenceRooms.Api.http`](src/ConferenceRooms.Api/ConferenceRooms.Api.http) contains practical request examples and stable seed IDs.

## API Endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/api/halls` | List all Halls and their service offerings. |
| GET | `/api/halls/{id}` | Get one Hall. |
| GET | `/api/halls/available` | Search available Halls by start, duration, and capacity. |
| POST | `/api/halls` | Create a Hall. |
| PUT | `/api/halls/{id}` | Replace Hall details and service offerings. |
| DELETE | `/api/halls/{id}` | Delete a Hall with no historical bookings. |
| POST | `/api/bookings` | Validate, price, and confirm a booking. |
| GET | `/api/bookings/{id}` | Get a booking confirmation. |
| GET | `/api/reports/bookings-summary` | Aggregate booking counts and revenue for a scheduled-start period. |

## Pricing Rules

| Time segment | Hall-rate multiplier |
| --- | ---: |
| 06:00–09:00 | 0.90 (-10%) |
| 09:00–12:00 | 1.00 (base price) |
| 12:00–14:00 | 1.15 (+15%) |
| 14:00–18:00 | 1.00 (base price) |
| 18:00–23:00 | 0.80 (-20%) |

Tariff segments are half-open and do not stack. A booking that crosses a tariff boundary is prorated by the duration inside each segment. Selected services are charged once per booking.

For Hall A from 10:30 to 12:30 with no services:

- 10:30–12:00: `1.5 × 2000 × 1.00 = 3000`
- 12:00–12:30: `0.5 × 2000 × 1.15 = 1150`
- Total: `4150 UAH`

## Booking Rules

- A booking must start in the future.
- Start time must be exactly on `:00` or `:30`, including zero seconds and sub-second ticks.
- `durationHours` is a positive integer number of hours.
- The full booking must remain within 06:00–23:00 on one calendar day.
- Attendee count cannot exceed Hall capacity.
- Service IDs must belong to the selected Hall and cannot be duplicated.
- Overlap uses half-open intervals: `[start, end)`.
- Touching intervals are allowed; one booking may start exactly when another ends.
- A confirmed Booking is immutable and stores its total price, Hall-name, and selected-service snapshots.
- A Hall referenced by a historical Booking cannot be deleted.

## Availability Rules

Availability searches are future-only and use the same start, duration, operating-hour, and calendar-day rules as bookings. Results are filtered by required capacity and exclude Halls with a half-open interval overlap. A subsequent `POST /api/bookings` remains authoritative because availability can change concurrently.

## Reports / Analytics

`GET /api/reports/bookings-summary?from=...&to=...` accepts required `DateTimeOffset` boundaries where `from < to`.

A Booking belongs to the report when `Booking.StartAt` is in `[from, to)`: a Booking starting exactly at `from` is included, while one starting exactly at `to` is excluded. Revenue is summed from immutable `Booking.TotalPrice` snapshots rather than recalculated from current rates. Hall rows are grouped by stable Hall ID and the current Hall name, ordered by name and then ID. Periods with no Bookings return zero totals and an empty Hall list.

## Concurrency

Booking creation uses a SQL Server Serializable transaction around Hall/service validation, overlap checking, and insertion. This prevents two concurrent requests from both successfully reserving the same Hall and time window. SQL Server deadlock victim error 1205 is retried with a bounded delay for a maximum of three total transaction attempts. The implementation does not claim or require distributed locking.

## Security / Hardening

- Safe ProblemDetails responses include `traceId` and do not expose stack traces or database details.
- HTTPS redirection is enabled; HSTS is enabled outside Development.
- CORS uses an exact-origin allow-list and trusts no origins by default.
- A fixed-window limiter permits 120 requests per 60 seconds per remote IP by default, with no queue.
- Kestrel rejects request bodies larger than 64 KiB by default.
- Build warnings are treated as errors.
- No passwords, API tokens, private keys, or certificates are committed.

Authentication and authorization were intentionally not invented because the assessment does not define users, roles, ownership, or an identity model.

## Tests

```powershell
dotnet build ConferenceRooms.sln
dotnet test ConferenceRooms.sln
```

Verified test inventory:

- `ConferenceRooms.UnitTests`: 108 portable Core and business-rule tests.
- `ConferenceRooms.ApiTests`: 20 portable HTTP pipeline, validation, and Swagger tests that do not require or mutate the local SQL Server.
- Total: 128 tests (0 failed, 0 skipped).

Real SQL smoke verification is used separately for persistence, booking concurrency behavior, and report aggregation.

## Design Decisions / Trade-offs

- The focused three-project architecture is retained instead of adding speculative layers.
- No generic repository was added because EF Core `DbContext` already provides the required persistence abstraction for this assessment.
- `DateTimeOffset` preserves the supplied offset for booking and report boundaries.
- Booking snapshots make historical prices reliable even after current Hall or service prices change.
- Reports aggregate in SQL rather than loading matching Booking entities into memory.
- Booking cancellation and status lifecycles were not added because they were not requested.
- No authentication model was invented without business requirements.
- Swagger is restricted to Development.

## Configuration

| Environment variable | Purpose | Committed default |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | Local Windows-integrated SQL Server |
| `Cors__AllowedOrigins__0` | First exact trusted browser origin | No trusted origins |
| `RateLimiting__PermitLimit` | Requests permitted in a fixed window | `120` |
| `RateLimiting__WindowSeconds` | Fixed-window duration | `60` |
| `RequestLimits__MaxRequestBodySizeBytes` | Global Kestrel body limit | `65536` |

## HTTP Examples

Visual Studio and JetBrains HTTP Client examples are available in [`src/ConferenceRooms.Api/ConferenceRooms.Api.http`](src/ConferenceRooms.Api/ConferenceRooms.Api.http). They cover Hall CRUD, availability, booking, validation, overlap, pricing, and report boundaries.
