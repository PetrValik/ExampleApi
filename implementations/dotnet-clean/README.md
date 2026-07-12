# Example API — .NET 10 Clean Architecture

One implementation of the shared **Example API** contract, built as a four-project
**Clean Architecture** solution. It satisfies the same wire contract as every other
implementation in this repo (see [`../CONTRACT-FOR-IMPLEMENTERS.md`](../CONTRACT-FOR-IMPLEMENTERS.md)
and [`../../contract/openapi.yaml`](../../contract/openapi.yaml)) and is exercised by the
black-box [`conformance/`](../../conformance) suite.

## Stack

- **.NET 10** minimal APIs (composition root in `WebApi`)
- **EF Core 10 + Npgsql** over **PostgreSQL 16**
- **FluentValidation** for request validation → RFC 7807 `problem+json`
- **HS256 JWT** bearer auth (`System.IdentityModel.Tokens.Jwt`)

## Architecture — the dependency rule points inward

```
WebApi  ──►  Application  ──►  Domain
   │             ▲
   └──►  Infrastructure ──┘   (implements Application's ports)
```

| Project | Responsibility | Depends on |
|---------|----------------|------------|
| **Domain** | The `Article` aggregate (encapsulated state, `Create`/`Update` behaviour, version counter), the `Money` and `CurrencyCode` value objects. No framework references at all. | — (pure BCL) |
| **Application** | Use-case handlers (`CreateArticle`, `GetArticle`, `GetArticles`, `UpdateArticle`, `DeleteArticle`, `BatchCreateArticles`, `GetToken`), port interfaces (`IArticleRepository`, `IUnitOfWork`, `ITokenService`), wire DTOs, FluentValidation validators, application exceptions. | Domain |
| **Infrastructure** | EF Core `AppDbContext` + `ArticleRepository`/`UnitOfWork`, the JWT `JwtTokenService`, bearer-auth wiring, `DbInitializer`. Implements the Application ports. | Application |
| **WebApi** | The only entry point / composition root: minimal-API endpoints, DI wiring, `ProblemDetails` normalisation. | Application, Infrastructure |

The dependency rule is enforced structurally: `Domain` references nothing, `Application`
references only `Domain`, `Infrastructure` references only `Application`, and `WebApi`
is the sole host.

## How to run

### Docker (production profile — serves port 8080)

```bash
docker compose up --build
```

This starts its own `postgres:16-alpine` (with a healthcheck), waits for the DB to be
ready, creates the schema on startup, and serves the API on **http://localhost:8080**.

```bash
# Smoke test
curl -s localhost:8080/health
TOKEN=$(curl -s localhost:8080/auth/token -H 'content-type: application/json' \
  -d '{"username":"admin","password":"admin"}' | jq -r .token)
curl -s localhost:8080/api/articles -H "Authorization: Bearer $TOKEN"
```

### Local (development profile — serves port 5088)

Needs a local PostgreSQL matching `appsettings.Development.json`.

```bash
dotnet run --project src/WebApi
```

### Build / verify it compiles

```bash
dotnet build ExampleApi.Clean.slnx -c Release
```

## Contract mapping

| Contract | Where it lives |
|----------|----------------|
| `GET /health` → `{"status":"healthy"}` | `Endpoints/HealthEndpoints.cs` |
| `POST /auth/token` (admin/admin → HS256 JWT, else 401) | `Endpoints/AuthEndpoints.cs` → `GetTokenHandler` → `JwtTokenService` |
| `GET/POST/PUT/DELETE /api/articles[...]` (JWT-protected) | `Endpoints/ArticleEndpoints.cs` → use-case handlers |
| `POST /api/articles-concurrent` (batch) | `BatchCreateArticlesHandler` (single transactional commit, input order preserved) |
| snake_case article fields (`article_id`, `row_version`) | `[JsonPropertyName]` on the `Application/Articles/Dtos` records |
| camelCase pagination wrapper (`pageSize`, `hasNext` …) | `Application/Articles/Dtos/PagedResponse.cs` |
| All validation failures → **400** `problem+json` with an `errors` map | `EndpointValidation.ValidateAsync` + `Results.ValidationProblem` |
| 404 / 409 → `problem+json` | `Endpoints/ExceptionHandling.cs` maps `NotFoundException`/`ConflictException` |
| `pageSize` clamped to 1..100 (not rejected) | `GetArticlesHandler` (`Math.Clamp`) |
| Currency required + validated only when `price > 0` | `CreateArticleRequestValidator` / `UpdateArticleRequestValidator` |
| 49 supported ISO-4217 codes | `Domain/ValueObjects/CurrencyCode.cs` |

## Optimistic concurrency (`row_version`)

`row_version` is a portable **integer version counter** on the `Article` aggregate:

- `Article.Create(...)` starts it at **1**.
- `Article.Update(...)` **increments** it on every successful update.
- On `PUT`, `UpdateArticleHandler` compares the caller-supplied `row_version` against the
  current value; a mismatch throws `ConflictException` → **409**.

So the canonical flow holds: `create` (row_version 1) → `PUT` with 1 (**200**, now 2) →
`PUT` again with the stale 1 (**409**). `UnitOfWork` additionally translates any EF Core
`DbUpdateConcurrencyException` into a 409 as a backstop against true races.

## Notes

- The schema is created at startup with `EnsureCreated` (no migration assets needed); the
  initializer retries while PostgreSQL finishes starting.
- `Infrastructure` opts into the ASP.NET Core shared framework (`FrameworkReference`) so
  all framework-coupled adapters (auth, config, logging) stay in the one layer permitted
  to know about frameworks — the Domain and Application layers remain framework-free.
