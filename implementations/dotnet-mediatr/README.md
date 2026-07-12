# Example API — .NET 10, Vertical Slice + MediatR + Result

One implementation of the shared [Example API contract](../CONTRACT-FOR-IMPLEMENTERS.md) in
the **"textbook modern .NET"** flavour: Vertical Slice Architecture where every slice is a
MediatR request/handler pair, validated by a FluentValidation pipeline behaviour, returning a
typed `Result<T>` that a thin Minimal-API endpoint maps to HTTP.

## Stack

- **.NET 10** Minimal APIs (`Microsoft.NET.Sdk.Web`)
- **MediatR 12.5** — commands/queries + a validation pipeline behaviour
- **FluentValidation 11** — per-slice validators
- **EF Core 10 + Npgsql** over **PostgreSQL 16**
- **HS256 JWT** bearer auth (`Microsoft.AspNetCore.Authentication.JwtBearer`)

## Architecture

Each feature lives in its own folder under `Features/<Area>/<Slice>/` and contains only what
that slice needs:

```
Features/Articles/CreateArticle/
  CreateArticleCommand.cs     record : IRequest<Result<ArticleResponse>>
  CreateArticleValidator.cs   AbstractValidator<CreateArticleCommand>
  CreateArticleHandler.cs     internal sealed IRequestHandler, primary ctor
  CreateArticleEndpoint.cs    IEndpoint — dispatches via ISender, maps Result → HTTP
```

The cross-cutting pieces live in `Common/`:

- **`Results/Result.cs`, `Error.cs`** — a `Result` / `Result<T>` type with a typed `Error`
  (`Validation`, `NotFound`, `Conflict`, `Failure`). Handlers return failures; they never throw
  for expected business outcomes.
- **`Behaviors/ValidationBehavior.cs`** — an `IPipelineBehavior<TRequest, TResponse>` (constrained
  `where TResponse : Result`) that runs every registered validator and, on failure,
  **short-circuits the pipeline into a `Result` failure carrying a `ValidationError`** — the
  handler never runs.
- **`Results/ResultExtensions.cs`** — maps a failed `Result` onto an RFC 7807
  `application/problem+json` response. **Validation failures become HTTP 400** with an `errors`
  map (via `Results.ValidationProblem`), not 422; not-found → 404; conflict → 409.
- **`Endpoints/`** — the `IEndpoint` abstraction and reflection-based discovery/mapping.

`Program.cs` is the composition root: `AddProblemDetails`, DB context, JWT, MediatR
(`RegisterServicesFromAssemblyContaining<Program>` + `AddOpenBehavior(typeof(ValidationBehavior<,>))`),
validators, and endpoint discovery.

## Contract mapping

| Contract requirement | Where |
|---|---|
| Routes + status codes | `Features/**/**Endpoint.cs` |
| snake_case article payloads / camelCase wrappers | `[JsonPropertyName]` on the DTOs in `Shared/Dtos`, `PagedResponse`, `GetTokenResponse` |
| Validation → **400 problem+json** with `errors` | `ValidationBehavior` → `ValidationError` → `ResultExtensions.ToProblem` → `Results.ValidationProblem` |
| Currency required only when `price > 0`; 49-code allow-list | `ArticleRequestValidator` + `Common/Currency/CurrencyCodes.cs` |
| `pageSize` clamped to 100 (not rejected) | `GetArticlesHandler` (`Math.Clamp(…, 1, 100)`) |
| Optimistic concurrency (`row_version`) | PostgreSQL `xmin` mapped as a concurrency token in `AppDbContext`; `UpdateArticleHandler` sets the client value as `OriginalValue` and maps `DbUpdateConcurrencyException` → 409 |
| Batch: empty → 400, any invalid → 400, max 100 | `BatchCreateArticlesValidator` |
| Free article (`price:0, currency:null`) | currency rule guarded by `.When(price > 0)` |
| Demo `admin`/`admin` → HS256 JWT; wrong creds → 401 | `GetTokenHandler` + `AddJwtAuthentication` |

### Optimistic concurrency (`row_version`)

`row_version` is backed by PostgreSQL's `xmin` system column — a per-row transaction id that
Postgres bumps on every UPDATE. It is mapped in `AppDbContext` as
`.HasColumnName("xmin").HasColumnType("xid").IsRowVersion().IsConcurrencyToken()`, so it costs no
storage and is not part of `CREATE TABLE`. On update the handler sets the client-supplied value as
the property's `OriginalValue`; EF Core puts it in the `WHERE` clause, so a stale value updates
0 rows and raises `DbUpdateConcurrencyException`, which the handler turns into a **409**. The value
is read back after each write (via `RETURNING xmin`) and returned as the new `row_version`.

## Run it

```bash
docker compose up --build      # API on http://localhost:8080, its own postgres:16-alpine
```

The API waits for the database (compose healthcheck + a retry loop in `DbInitializer`) and creates
the schema at startup (`EnsureCreatedAsync`).

```bash
# Smoke test
curl -s localhost:8080/health
TOKEN=$(curl -s localhost:8080/auth/token -H 'content-type: application/json' \
  -d '{"username":"admin","password":"admin"}' | jq -r .token)
curl -s localhost:8080/api/articles -H "Authorization: Bearer $TOKEN"
```

### Local (without Docker)

Point `ConnectionStrings__Default` at a Postgres instance, then:

```bash
dotnet run --project src/ExampleApi      # http://localhost:5088
```

## Build / verify

Docker is unavailable in the build environment, so correctness is proven by compilation:

```bash
dotnet build       # from src/ExampleApi — 0 warnings, 0 errors
```

Conformance (`../../conformance`) is satisfied by construction and runs live once a Docker daemon
is available (`scripts/verify-impl.sh implementations/dotnet-mediatr`).

## Config

| Setting | Env var | Default |
|---|---|---|
| Connection string | `ConnectionStrings__Default` | `Host=localhost;…;Database=exampleapi` |
| JWT issuer / audience | `Jwt__Issuer` / `Jwt__Audience` | `ExampleApi` / `ExampleApiClient` |
| JWT secret (≥ 32 chars) | `Jwt__SecretKey` | dev placeholder |
| Token lifetime (min) | `Jwt__ExpirationMinutes` | `60` |
