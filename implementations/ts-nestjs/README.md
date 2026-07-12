# Example API — NestJS + TypeORM

A NestJS + TypeORM implementation of the shared [Example API contract](../CONTRACT-FOR-IMPLEMENTERS.md).
It serves the exact same HTTP behaviour as every other implementation in this repo, so the
[`conformance/`](../../conformance) suite passes against it unchanged.

## Stack

- **Runtime:** Node.js 20 / TypeScript
- **Framework:** [NestJS 10](https://nestjs.com) (opinionated DI + decorator modules/controllers/providers)
- **ORM:** [TypeORM 0.3](https://typeorm.io) over **PostgreSQL 16**
- **Validation:** `class-validator` + `class-transformer` via a global `ValidationPipe`
- **Auth:** `@nestjs/jwt` (HS256) + a custom `JwtAuthGuard`
- **Config:** `@nestjs/config` (environment variables)

## Architecture

Feature modules wired through Nest's DI container — the deliberate contrast to the plain-Express
implementation:

```
src/
  main.ts                       # bootstrap: global ValidationPipe + problem+json exception filter
  app.module.ts                 # root module: ConfigModule + TypeOrmModule.forRootAsync
  common/
    currencies.ts               # the 49 supported ISO-4217 codes
    validation-errors.ts        # class-validator error tree -> { field: [messages] }
    exceptions/validation.exception.ts
    filters/all-exceptions.filter.ts   # everything -> application/problem+json (RFC 7807)
  articles/
    article.entity.ts           # TypeORM entity; integer `version` -> row_version
    article.mapper.ts           # entity -> snake_case wire shape + paged wrapper
    dto/create-article.dto.ts   # class-validator rules (name/description/category/price/currency)
    dto/update-article.dto.ts   # create rules + required row_version >= 1
    articles.service.ts         # persistence, filtering/pagination, optimistic concurrency, batch
    articles.controller.ts      # @UseGuards(JwtAuthGuard) on /api/articles**
    articles.module.ts
  auth/
    auth.service.ts             # admin/admin demo check -> signed HS256 JWT
    jwt-auth.guard.ts           # verifies Bearer token (secret/issuer/audience)
    auth.controller.ts          # POST /auth/token
    auth.module.ts              # JwtModule.registerAsync (global)
  health/                       # GET /health (anonymous)
```

### How the contract is satisfied

| Contract requirement | Where |
|----------------------|-------|
| Port 8080, PostgreSQL backend | `main.ts` (`listen(8080)`), `app.module.ts` (`TypeOrmModule` postgres) |
| Schema created at startup, retry while DB warms up | `synchronize: true` + `retryAttempts`/`retryDelay` in `app.module.ts` |
| Mixed casing (`article_id`/`row_version` snake, `pageSize`/`expiresAt` camel) | `article.mapper.ts`, `token-response.dto.ts` |
| Validation failures -> **400 `application/problem+json`** with an `errors` map | global `ValidationPipe` `exceptionFactory` + `AllExceptionsFilter` (never Nest's default envelope) |
| `currency` required only when `price > 0` | `@ValidateIf(o => o.price > 0)` in `create-article.dto.ts` |
| 49 supported currencies | `common/currencies.ts` (`@IsIn`) |
| `pageSize` clamped to 100 (not rejected) | `ArticlesService.list` (`Math.min(..., 100)`) |
| `name` partial/case-insensitive, `category` exact filters | `ArticlesService.list` (`ILIKE` with escaped wildcards / `=`) |
| Optimistic concurrency: stale `row_version` -> **409** | integer `version` column + conditional `UPDATE ... WHERE version = :row_version` |
| Create -> 201 + `Location` header ending in the new id | `ArticlesController.create` |
| Batch: empty -> 400, any invalid -> 400, max 100 | `ArticlesService.batchCreate` |
| Free article (`price: 0, currency: null`) -> 201 | `create-article.dto.ts` / service |
| `admin`/`admin` -> HS256 JWT, wrong creds -> 401 | `auth.service.ts`; protected routes -> `JwtAuthGuard` |

### Optimistic concurrency (`row_version`)

`row_version` is backed by a plain integer `version` column that starts at `1`. Updates run a single
conditional statement:

```sql
UPDATE articles
   SET ..., version = version + 1
 WHERE article_id = :id AND version = :row_version
```

If the client's `row_version` no longer matches the stored value, zero rows are affected and the
service throws a `409 Conflict`. This is the portable integer approach suggested by the contract
(the canonical .NET reference uses Postgres `xmin`; both satisfy the create → PUT(200) → PUT-stale(409)
conformance flow).

## Run

```bash
docker compose up --build
```

This starts its own `postgres:16-alpine` (with a healthcheck), waits for it, creates the schema, and
serves the API on <http://localhost:8080>.

```bash
# Get a token
curl -s localhost:8080/auth/token -H 'Content-Type: application/json' \
  -d '{"username":"admin","password":"admin"}'

# Use it
curl -s localhost:8080/api/articles -H "Authorization: Bearer <token>"
```

## Local development

```bash
npm install
# point DB_* at a local Postgres, then:
npm run start:dev
```

## Build / typecheck

```bash
npm install
npm run typecheck   # tsc --noEmit
npm run build       # nest build -> dist/
```
