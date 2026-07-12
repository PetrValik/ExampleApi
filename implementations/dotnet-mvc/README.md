# Example API — .NET 10 Classic Layered (MVC Controllers)

One implementation of the shared [Example API contract](../CONTRACT-FOR-IMPLEMENTERS.md), built as a
**traditional, layered "enterprise" ASP.NET Core** application. It is the deliberate architectural
counterpoint to the [`dotnet-vsa`](../dotnet-vsa) reference (which is organised by *feature*): here
the code is organised by **technical layer**.

## Stack

- **.NET 10**, ASP.NET Core **MVC controllers** (`[ApiController]`, attribute routing)
- **EF Core 10** + **Npgsql** over **PostgreSQL 16**
- **FluentValidation** for request validation
- **JWT** bearer auth (HS256) via `Microsoft.AspNetCore.Authentication.JwtBearer`

## Architecture — organised by layer, not feature

```
src/ExampleApi/
├── Controllers/        Thin HTTP layer — bind, delegate, shape the result
│   ├── HealthController, AuthController, ArticlesController
├── Services/           Business logic (pagination clamp, concurrency orchestration)
│   ├── IArticleService / ArticleService
│   └── ITokenService  / TokenService     (JWT issuance)
├── Repositories/       Persistence boundary over EF Core — the ONLY layer that sees DbContext
│   ├── IArticleRepository / ArticleRepository
├── Data/               AppDbContext + startup migrator (DbInitializer)
├── Models/             Article entity
├── Dtos/               Request/response DTOs (snake_case articles, camelCase wrappers)
├── Mapping/            Manual entity <-> DTO mapping (no AutoMapper dependency)
├── Validation/         FluentValidation validators + a global validation action filter
├── Common/
│   ├── Currency/       The 49 supported ISO-4217 codes
│   ├── Exceptions/     NotFoundException, ConflictException (domain -> HTTP)
│   └── Filters/        GlobalExceptionFilter (domain exception -> problem+json)
├── Configuration/      JwtSettings
├── Migrations/         EF Core InitialCreate
└── Program.cs          Composition root (DI wiring, auth, pipeline)
```

**Dependency direction:** `Controller → Service → Repository → DbContext`. Controllers never touch
`AppDbContext` directly; the service layer never sees EF Core types (concurrency conflicts are
translated to a bool at the repository boundary and re-thrown as a domain `ConflictException`).

## How error handling is normalised

- **400 (validation):** a global `FluentValidationActionFilter` runs the registered
  `IValidator<T>` for every bound action argument and, on failure, short-circuits with
  `400 application/problem+json` carrying an `errors` map (`{ "Name": ["Name is required."] }`).
  This replaces the framework's default validation envelope.
- **404 / 409 / 500:** a global `GlobalExceptionFilter` (an MVC `IExceptionFilter`) maps
  `NotFoundException → 404`, `ConflictException → 409`, everything else → `500`, each as
  `application/problem+json`.

## Optimistic concurrency (`row_version`)

`Article.RowVersion` is a `uint` mapped onto PostgreSQL's built-in **`xmin`** system column
(`.HasColumnName("xmin").HasColumnType("xid").IsRowVersion().IsConcurrencyToken()`), so it changes
automatically on every `UPDATE` at zero storage cost. On update the repository sets the tracked
entity's `RowVersion` *original value* to the client-supplied `row_version`, making EF Core emit a
`… WHERE xmin = @clientVersion` guard. A mismatch raises `DbUpdateConcurrencyException`, which the
repository reports as a failed save and the service turns into a `ConflictException` → **409**.
Lifecycle: create → PUT with the returned `row_version` (**200**) → PUT again with the now-stale
value (**409**).

## Contract mapping

| Contract | Here |
|----------|------|
| `GET /health` (anon) | `HealthController.Get` → `{ "status": "healthy" }` |
| `POST /auth/token` (anon) | `AuthController.GetToken` → `TokenService.Authenticate` (admin/admin → HS256 JWT, else 401) |
| `GET /api/articles` (JWT) | `ArticlesController.List` → filter `name` (partial, case-insensitive) / `category` (exact); `pageSize` clamped to 1..100 |
| `POST /api/articles` (JWT) | `ArticlesController.Create` → 201 + `Location` ending in the new `article_id` |
| `GET /api/articles/{id}` (JWT) | `ArticlesController.GetById` → 200 / 404 |
| `PUT /api/articles/{id}` (JWT) | `ArticlesController.Update` → 200 / 400 / 404 / 409 (stale `row_version`) |
| `DELETE /api/articles/{id}` (JWT) | `ArticlesController.Delete` → 204 / 404 |
| `POST /api/articles-concurrent` (JWT) | `ArticlesController.CreateBatch` → 201 array (parallel inserts, order preserved; empty/invalid/>100 → 400) |

Article payloads use **snake_case** (`article_id`, `row_version`) via `[JsonPropertyName]`; the
pagination wrapper and token response use **camelCase** (`pageSize`, `expiresAt`) via the default
naming policy — exactly as pinned by the contract. A free article (`price: 0, currency: null`) is
valid; `currency` is required and validated against the 49 supported codes only when `price > 0`.

## Run it

### Docker (serves port 8080)

```bash
docker compose up --build
```

This starts its own `postgres:16-alpine` (with a healthcheck), waits for it to be ready, applies the
EF Core migration at startup (with connect-retry), and serves the API on **http://localhost:8080**.

```bash
curl -s localhost:8080/health
# {"status":"healthy"}

TOKEN=$(curl -s localhost:8080/auth/token -H 'content-type: application/json' \
  -d '{"username":"admin","password":"admin"}' | jq -r .token)
curl -s localhost:8080/api/articles -H "Authorization: Bearer $TOKEN"
```

### Local development

```bash
# needs a local Postgres matching appsettings.Development.json (port 5432)
dotnet run --project src/ExampleApi   # http://localhost:5088
```

## Configuration

| Setting | Env var | Default |
|---------|---------|---------|
| DB connection | `ConnectionStrings__Default` | `Host=localhost;…;Database=exampleapi` |
| JWT issuer | `Jwt__Issuer` | `ExampleApi` |
| JWT audience | `Jwt__Audience` | `ExampleApiClient` |
| JWT secret (≥32 chars) | `Jwt__SecretKey` | set in compose |
| JWT lifetime (min) | `Jwt__ExpirationMinutes` | `60` |

## Build / verify

Docker is unavailable in the build environment, so parity is proven by compilation:

```bash
dotnet build -c Release   # 0 warnings, 0 errors
```

The 29-test black-box [`conformance/`](../../conformance) suite runs against the container once a
Docker daemon is available (`scripts/verify-impl.sh implementations/dotnet-mvc`).

> Demo credentials `admin` / `admin` are hardcoded on purpose for the showcase. Replace
> `TokenService` with a real identity provider before any production use.
