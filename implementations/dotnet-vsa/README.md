# Example API

> A production-style RESTful API for article/product management, built to showcase modern .NET development — Vertical Slice Architecture, Minimal APIs, PostgreSQL with EF Core, JWT authentication, and a thorough automated test suite.

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)

> **This is the reference implementation** of the [Example API multi-approach
> showcase](../../README.md) (`dotnet-vsa`). It defines the behaviour that
> [`contract/openapi.yaml`](../../contract/openapi.yaml) documents and that every other
> implementation must match via the [conformance suite](../../conformance). Run
> `./scripts/verify-impl.sh implementations/dotnet-vsa` from the repo root to prove it.

## Overview

**Example API** is a demonstration back-end service for managing shop articles (products). It is intentionally small in domain scope but built the way a real production service would be: features are sliced vertically, requests are validated at the edge, errors map to RFC 7807 problem-details responses, the database uses optimistic concurrency, and every slice is covered by both unit and integration tests.

The project exists as a portfolio piece to demonstrate practical, current .NET back-end skills rather than to ship a specific product.

## Highlights

- **Vertical Slice Architecture** — each feature (create, read, update, delete, batch, auth) is a self-contained folder with its own endpoint, handler, request/response DTOs, and validators.
- **Minimal APIs** — endpoints are auto-discovered and registered through a small `IEndpoint` abstraction, keeping `Program.cs` lean.
- **PostgreSQL + EF Core 10** — real relational persistence with EF Core migrations and connection configuration via app settings / environment variables.
- **Optimistic concurrency** — update conflicts are detected using PostgreSQL's native `xmin` system column mapped to a `RowVersion` property, returning `409 Conflict` on stale writes.
- **JWT Bearer authentication** — a token endpoint issues signed JWTs; write endpoints require authorization, wired through into the OpenAPI/Scalar security scheme.
- **FluentValidation** — declarative request validation runs before handlers via a reusable validation filter, producing `400` validation-problem responses.
- **Global exception handling** — a single middleware maps domain exceptions (`NotFoundException`, `ConflictException`) and unexpected errors to consistent `ProblemDetails` payloads.
- **Interactive docs** — OpenAPI document plus a [Scalar](https://scalar.com/) UI with request examples and a "try it out" experience.
- **Containerised** — multi-stage `Dockerfile` and a `docker-compose.yml` that brings up the API together with PostgreSQL.
- **150+ automated tests** — xUnit + FluentAssertions, with integration tests running against a real PostgreSQL instance spun up on the fly via Testcontainers.

## Tech stack

| Category            | Technology |
|---------------------|------------|
| Framework           | .NET 10 / ASP.NET Core Minimal APIs |
| Language            | Modern C# (records, required members, collection expressions, nullable reference types) |
| Database            | PostgreSQL 16 |
| Data access         | Entity Framework Core 10 (Npgsql provider) |
| Authentication      | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) |
| Validation          | FluentValidation 11 |
| API documentation   | OpenAPI (`Microsoft.AspNetCore.OpenApi`) + Scalar UI |
| Testing             | xUnit, FluentAssertions, EF Core InMemory (unit), Testcontainers (integration), Coverlet |
| Containerisation    | Docker + Docker Compose |
| Architecture        | Vertical Slice Architecture, REPR (Request–Endpoint–Response) |

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A running **PostgreSQL** instance (or use the provided Docker Compose setup)
- Docker (optional, for containerised run and for integration tests)

### Option A — Run with Docker Compose (API + PostgreSQL)

This is the quickest way to get a fully working stack:

```bash
docker compose up --build
```

The API becomes available at `http://localhost:8080`. Compose provisions a `postgres:16-alpine` container, waits for it to be healthy, and wires the connection string and JWT settings through environment variables.

### Option B — Run locally with `dotnet run`

1. Start PostgreSQL and make sure the connection string in `src/ExampleApi/appsettings.json` matches your instance (defaults to `Host=localhost;Port=5432;Database=exampleapi;Username=postgres;Password=postgres`).
2. Run the API:

   ```bash
   cd src/ExampleApi
   dotnet run
   ```

3. Open the interactive docs at **http://localhost:5088/scalar/v1**.

On startup the application initialises the database (applies migrations / seeds data) automatically.

### Authenticating

Write operations require a JWT. Obtain one from the token endpoint using the demo credentials:

```bash
curl -X POST http://localhost:5088/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin"}'
```

Then send the returned token as `Authorization: Bearer <token>` on protected requests. In the Scalar UI you can paste the token into the **Bearer** auth field.

> The token endpoint uses hardcoded demo credentials (`admin` / `admin`) on purpose — it is a stand-in for a real identity provider and is clearly marked as such in the code.

## API endpoints

### Authentication

| Method | Endpoint       | Description                        | Auth |
|--------|----------------|------------------------------------|------|
| POST   | `/auth/token`  | Issue a JWT for the demo user      | none |

### Articles

| Method | Endpoint                     | Description                             | Status codes             | Auth |
|--------|------------------------------|-----------------------------------------|--------------------------|------|
| GET    | `/api/articles`              | List articles with filters + pagination | 200, 401                 | JWT  |
| GET    | `/api/articles/{id}`         | Get a single article by id              | 200, 401, 404            | JWT  |
| POST   | `/api/articles`              | Create an article                       | 201, 400, 401            | JWT  |
| PUT    | `/api/articles/{id}`         | Update an article                       | 200, 400, 401, 404, 409  | JWT  |
| DELETE | `/api/articles/{id}`         | Delete an article                       | 204, 401, 404            | JWT  |
| POST   | `/api/articles-concurrent`   | Batch-create articles in parallel       | 201, 400, 401            | JWT  |

> **All `/api/articles/**` operations require a JWT** (including the `GET`s). Obtain a token
> from `POST /auth/token` first.

### Health

| Method | Endpoint  | Description        |
|--------|-----------|--------------------|
| GET    | `/health` | API health status  |

**Query parameters for `GET /api/articles`:**

- `name` — partial, case-insensitive name match
- `category` — exact category match
- `page` — page number (default `1`)
- `pageSize` — items per page (default `10`, max `100`)

Paginated responses carry metadata (`totalCount`, `page`, `pageSize`, `hasNext`, `hasPrevious`).

## Project structure

```
ExampleApi/
├── src/
│   └── ExampleApi/
│       ├── Features/                     # Vertical slices
│       │   ├── Auth/                      # POST /auth/token (JWT issuance)
│       │   ├── Articles/
│       │   │   ├── Shared/                # Article entity, DTOs, mappings, shared validator
│       │   │   ├── CreateArticle/         # POST /api/articles
│       │   │   ├── GetArticle/            # GET  /api/articles/{id}
│       │   │   ├── GetArticles/           # GET  /api/articles (filter + paging)
│       │   │   ├── UpdateArticle/         # PUT  /api/articles/{id}
│       │   │   ├── DeleteArticle/         # DELETE /api/articles/{id}
│       │   │   └── BatchCreateArticles/   # POST /api/articles-concurrent
│       │   └── Health/                    # GET /health
│       ├── Common/
│       │   ├── Endpoints/                 # IEndpoint abstraction + auto-registration
│       │   ├── Validation/                # Reusable validation filter
│       │   ├── Exceptions/                # NotFoundException, ConflictException
│       │   ├── Currency/                  # ISO 4217 currency validation
│       │   └── Pagination/                # PagedResponse<T>
│       ├── Infrastructure/Database/       # AppDbContext, migrations, initializer
│       ├── Configuration/                 # Service + middleware extension methods, JwtSettings
│       └── Program.cs                     # Slim entry point wiring it all together
├── test/
│   ├── ExampleApi.UnitTests/              # Handlers, validators, mappings, utilities
│   └── ExampleApi.IntegrationTests/       # Full HTTP cycle over a real PostgreSQL container
├── Dockerfile                            # Multi-stage build
├── docker-compose.yml                    # API + PostgreSQL
└── ExampleApi.slnx                       # Solution
```

Each feature slice follows the same shape:

```
Feature/
├── IHandler.cs        # Handler interface (for DI + testability)
├── Handler.cs         # Business logic
├── Endpoint.cs        # HTTP mapping (implements IEndpoint)
├── Request.cs         # Request DTO (when feature-specific)
├── Validator.cs       # FluentValidation rules (when feature-specific)
└── README.md          # Short feature note
```

## Architecture notes

- **REPR pattern** — every endpoint is a thin adapter: it validates the request, delegates to a handler, and shapes the response. Handlers hold the business logic behind an interface so they can be unit-tested without HTTP.
- **Endpoint auto-registration** — all `IEndpoint` implementations are discovered and mapped via `MapEndpoints()`, so adding a feature never means editing a central router.
- **Composition in `Program.cs`** — service registration and the middleware pipeline are expressed as small, well-named extension methods (`AddDatabaseContext`, `AddJwtAuthentication`, `AddFeatures`, `UseGlobalExceptionHandler`, …), keeping the entry point readable.
- **Optimistic concurrency** — the `Article.RowVersion` property maps to PostgreSQL's `xmin` system column; concurrent updates raise a `ConflictException` that surfaces as `409 Conflict`, so clients can refetch and retry.

## Tests

The suite mixes fast unit tests with realistic integration tests:

| Type              | What it covers                                              | Backing store                     |
|-------------------|-------------------------------------------------------------|-----------------------------------|
| Unit tests        | Handlers, validators, mapping extensions, common utilities  | EF Core InMemory                  |
| Integration tests | Full HTTP request/response cycle across every endpoint      | Real PostgreSQL via Testcontainers |

Integration tests boot the app with `WebApplicationFactory<Program>` and start a throwaway `postgres:16-alpine` container per test run, applying EF Core migrations against it — so query translation, migrations, and concurrency behaviour are all exercised against the real database engine, not a substitute.

```bash
# Run everything
dotnet test

# Unit tests only
dotnet test test/ExampleApi.UnitTests

# Integration tests only (requires Docker to be running)
dotnet test test/ExampleApi.IntegrationTests
```

> Integration tests need a working Docker daemon because Testcontainers provisions PostgreSQL on demand.

## Database migrations

```bash
# Add a migration
dotnet ef migrations add <Name> --project src/ExampleApi/ExampleApi.csproj

# Apply migrations
dotnet ef database update --project src/ExampleApi/ExampleApi.csproj
```

## Status & notes

This is a personal demonstration project and is complete for its intended scope. A few things are deliberately simplified and would be hardened before any real deployment:

- The `/auth/token` endpoint uses fixed demo credentials in place of a real identity provider.
- JWT secrets and connection strings ship with placeholder development values — replace them via environment variables / secrets in any real environment.

## License

Released under the [MIT License](LICENSE).
