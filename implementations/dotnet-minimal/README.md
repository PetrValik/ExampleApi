# Example API — .NET 10 Minimal API (minimalist extreme)

The **axis-A "ceremony" low-water mark**: the same behaviour as every other
implementation in this repo, squeezed into a **single `Program.cs`**. Raw
Minimal-API `MapGet`/`MapPost`/… lambdas with **inline EF Core queries**,
**inline validation** and **inline `problem+json`** — no services, handlers,
repositories, interfaces, MediatR or FluentValidation.

## Stack

- **.NET 10** Minimal API (`WebApplication`, reflection-based JSON)
- **EF Core 10 + Npgsql** over **PostgreSQL 16**
- **HS256 JWT** bearer auth (`Microsoft.AspNetCore.Authentication.JwtBearer`)

## Architecture

Everything lives in [`Program.cs`](Program.cs):

- Top-level statements wire config → services → schema → endpoints.
- Each route is a lambda that queries `AppDb` (the one `DbContext`) directly.
- Validation is one local function `Validate(...)` returning an
  `errors` map; endpoints turn it into `Results.ValidationProblem(...)`.
- The entity, `DbContext` and DTOs are declared at the bottom of the same file
  (C# requires them after the top-level statements).

That's the whole point of this variant: show how little code the contract needs.

## Contract mapping

| Contract requirement | Where / how |
|----------------------|-------------|
| `GET /health` → `{"status":"healthy"}` | `MapGet("/health")` |
| `POST /auth/token` → HS256 JWT, 401 on bad creds | `MapPost("/auth/token")`, demo `admin`/`admin`, `JwtSecurityToken` with name claim + issuer + audience + expiry |
| Protected `/api/articles/**` | `.RequireAuthorization()` on every article route; missing/invalid bearer → 401 |
| Article JSON `snake_case` (`article_id`, `row_version`) | `[JsonPropertyName]` on `ArticleRead`/`ArticleWrite` |
| Paged wrapper `camelCase` (`pageSize`, `totalCount`…) | anonymous object, default camelCase policy |
| Token response `camelCase` (`token`, `expiresAt`) | `TokenResponse` record |
| Validation → **400 `application/problem+json`** with `errors` map | `Validate(...)` + `Results.ValidationProblem(errors)` (never a 422 envelope) |
| 49 supported currencies; required only when `price > 0` | `currencies` set + inline rule |
| `pageSize` clamped to 100 (not rejected) | `Math.Clamp(pageSize ?? 10, 1, 100)` |
| Optimistic concurrency → 409 on stale `row_version` | PostgreSQL `xmin` mapped as EF concurrency token; stale value → `DbUpdateConcurrencyException` → `Results.Problem(409)` |
| Create → 201 + `Location` ending in new id | `Results.Created($"/api/articles/{id}", …)` |
| Batch `POST /api/articles-concurrent` (empty→400, item invalid→400, max 100) | `MapPost("/api/articles-concurrent")` |
| Free article (`price:0, currency:null`) valid | currency rule only fires when `price > 0` |
| 404 / 409 as `problem+json` with `type/title/status/detail` | `Results.Problem(...)` |

### `row_version` / optimistic concurrency

`row_version` is backed by PostgreSQL's built-in **`xmin`** system column — the
transaction id stamped on every row, which changes automatically on every
`UPDATE`. It is mapped as an EF Core concurrency token (`uint RowVersion` →
column `xmin`, type `xid`, `IsRowVersion()`/`IsConcurrencyToken()`). On update
the client's `row_version` is set as the tracked entity's *original* value, so
EF emits `UPDATE … WHERE article_id = @id AND xmin = @original`; a stale value
matches 0 rows → `DbUpdateConcurrencyException` → **409**. `xmin` is never
created by us (it is a system column); the startup `CREATE TABLE` omits it and
EF only reads it.

## Run

```bash
docker compose up --build      # API on http://localhost:8080 + its own postgres:16-alpine
```

The API waits for the DB (compose healthcheck) and retries the startup
`CREATE TABLE IF NOT EXISTS` until Postgres accepts connections.

Local (needs a Postgres on `localhost:5432`):

```bash
dotnet run                     # http://localhost:5088 by default
```

## Verify (compiles)

Docker is unavailable in the build environment, so correctness is proven by a
clean build:

```bash
dotnet build -c Release        # Build succeeded. 0 Warning(s) 0 Error(s)
```

## Config / env

`Jwt__SecretKey` (≥32 chars), `Jwt__Issuer`, `Jwt__Audience`,
`Jwt__ExpirationMinutes` and `ConnectionStrings__Default` are read from
config/env (see `docker-compose.yml`).
