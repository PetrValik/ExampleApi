# Example API — Node/TypeScript + Express + Prisma

One implementation of the shared [Example API contract](../CONTRACT-FOR-IMPLEMENTERS.md).
It is behaviourally identical to every other implementation in this repo (same routes,
same JSON shapes, same status codes) — only the stack and architecture differ.

## Stack

- **Runtime:** Node.js 20, TypeScript (strict), CommonJS.
- **Web framework:** Express 4 (routers per resource).
- **Validation:** [zod](https://zod.dev) schemas, normalised to RFC 7807 `application/problem+json`.
- **Auth:** [jsonwebtoken](https://github.com/auth0/node-jsonwebtoken) — HS256 bearer tokens.
- **Persistence:** PostgreSQL 16 via [Prisma](https://www.prisma.io) ORM.

## Architecture

```
src/
  index.ts               process entry — connect DB, listen on :8080, graceful shutdown
  app.ts                 Express assembly: json → public routes → guarded routes → error handlers
  config.ts              env-driven config (port, JWT secret/issuer/audience/expiry) with dev defaults
  currencies.ts          the 49 supported ISO 4217 codes + membership check
  problem.ts             RFC 7807 helpers + typed errors (ValidationProblem/NotFound/Conflict)
  mappers.ts             Prisma entity -> snake_case article wire shape
  prisma.ts              shared PrismaClient
  validation/
    schemas.ts           zod schemas (create / update / batch) + parseOrThrow (throws 400 problem+json)
  middleware/
    auth.ts              JWT bearer guard for /api/articles/**
    errorHandler.ts      central error handler -> problem+json (+ malformed-JSON handler)
    asyncHandler.ts      forwards async route rejections to Express's error pipeline
  routes/
    health.ts            GET /health           (anonymous)
    auth.ts              POST /auth/token       (anonymous)
    articles.ts          the JWT-guarded article CRUD + list + batch
prisma/schema.prisma     Article model (price = double precision, version = int concurrency token)
Dockerfile               multi-stage build; runtime entrypoint syncs schema then runs the API
docker-compose.yml       api (:8080) + postgres:16-alpine with healthcheck
docker-entrypoint.sh     retries `prisma db push` until the DB is ready, then `node dist/index.js`
```

## Contract mapping

| Contract requirement | Where / how |
|----------------------|-------------|
| `GET /health` → 200 `{"status":"healthy"}` | `routes/health.ts` (anonymous) |
| `POST /auth/token` admin/admin → HS256 JWT; else 401 | `routes/auth.ts` — `name` claim, issuer, audience, `expiresIn` from config |
| Bearer required on `/api/articles/**`; missing/invalid → 401 | `middleware/auth.ts`, applied via `articlesRouter.use(requireAuth)` |
| Article response `snake_case` (`article_id`, `row_version`) | `mappers.ts` `toArticleResponse` |
| Paged wrapper `camelCase` (`pageSize`, `hasNext`, …) | `routes/articles.ts` GET handler |
| Token response `camelCase` (`expiresAt`) | `routes/auth.ts` |
| All validation failures → **400** `application/problem+json` with `errors` map | `validation/schemas.ts` + `middleware/errorHandler.ts` (never a 422 envelope) |
| `currency` required only when `price > 0`; 49-code allow-list | `refineCurrency` in `validation/schemas.ts`, `currencies.ts` |
| `row_version` required + ≥ 1 on update only | `updateArticleRequestSchema` |
| Create → 201 + `Location` ending in new id | POST `/api/articles` uses `res.location(...)` |
| Optimistic concurrency; stale `row_version` → **409** | integer `version` column, conditional `updateMany` (see below) |
| List filter `name` (partial, case-insensitive) + `category` (exact) | Prisma `contains`+`mode:'insensitive'` / exact match |
| `pageSize` clamped to 100 (not rejected); `page` ≥ 1 | `Math.min(Math.max(…,1),100)` |
| Batch: array of creates → 201 array; empty → 400; any invalid → 400; ≤ 100 | POST `/api/articles-concurrent`, `batchCreateSchema` |
| Free article `price:0, currency:null` → 201 | currency rule is skipped when `price == 0` |
| 404 / 409 → problem+json with `type/title/status/detail` | `middleware/errorHandler.ts` |

### Optimistic concurrency (`row_version`)

The contract allows a simple portable integer version column (the .NET reference uses
Postgres `xmin`). Here the `Article.version` column starts at `1` and is incremented on
every update; its current value is returned on the wire as `row_version`.

An update runs a **conditional** `updateMany` with `where: { articleId, version: row_version }`
and `data: { …fields, version: { increment: 1 } }`:

- The row is first fetched by id — if absent → **404**.
- If it exists but the client's `row_version` no longer matches, the conditional update
  affects **zero rows** → **409 Conflict**.
- On success the version advances, so replaying the now-stale `row_version` yields 409.

This satisfies the conformance flow: create → PUT with the returned `row_version` (200) →
PUT again with the stale value (409).

## Run

### Docker (one command)

```bash
docker compose up --build
```

Brings up `postgres:16-alpine` (with a healthcheck) and the API. The API container waits
for the DB to be healthy, then `prisma db push` creates the `articles` table (retried until
the DB accepts connections) before the server starts listening on **http://localhost:8080**.

```bash
curl http://localhost:8080/health
# {"status":"healthy"}
```

### Local (without Docker)

Requires a reachable PostgreSQL. Copy `.env.example` to `.env` and adjust `DATABASE_URL`, then:

```bash
npm install
npx prisma generate
npx prisma db push      # create/sync the schema
npm run build && npm start
# or: npm run dev
```

## Configuration

All via environment variables (defaults in `src/config.ts`; supplied by compose in Docker):

| Variable | Default | Purpose |
|----------|---------|---------|
| `PORT` | `8080` | HTTP listen port |
| `DATABASE_URL` | — (from `.env`) | Prisma PostgreSQL connection string |
| `JWT_SECRET` | dev placeholder (≥ 32 chars) | HS256 signing key |
| `JWT_ISSUER` | `ExampleApi` | token `iss` |
| `JWT_AUDIENCE` | `ExampleApiClient` | token `aud` |
| `JWT_EXPIRATION_MINUTES` | `60` | token lifetime |

## Verify it compiles

Docker is not available in the build environment, so the proof is a clean type-check:

```bash
npm install
npx prisma generate      # generate the typed Prisma client
npx tsc --noEmit         # passes with no errors
```
