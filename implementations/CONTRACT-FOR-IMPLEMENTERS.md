# Brief for implementers

Every implementation under `implementations/<name>/` must serve the **exact same HTTP behaviour**
so the [conformance suite](../conformance) passes against it and the comparison stays fair.
The authoritative spec is [`contract/openapi.yaml`](../contract/openapi.yaml); this file is the
quick, unambiguous checklist. When in doubt, match the reference implementation
[`dotnet-vsa`](dotnet-vsa) — it is behaviourally canonical.

Only the **architecture/runtime** changes between implementations. The wire behaviour below does not.

## Runtime contract (identical for every implementation)

- Serve HTTP on **port 8080** inside the container.
- Back it with **PostgreSQL** (each implementation ships its own `docker-compose.yml` with its own
  `postgres:16-alpine`; they never share a database, so schema/migrations are your choice).
- `docker compose up --build` in the implementation folder must bring up API + DB and serve 8080.
  Wait for DB readiness (compose healthcheck + retry on connect), like the reference.

### Endpoints

| Method | Path | Auth | Success | Errors |
|--------|------|------|---------|--------|
| GET | `/health` | none | 200 `{"status":"healthy"}` | — |
| POST | `/auth/token` | none | 200 token | 401 |
| GET | `/api/articles` | **JWT** | 200 paged | 401 |
| POST | `/api/articles` | **JWT** | 201 article + `Location` | 400, 401 |
| GET | `/api/articles/{id}` | **JWT** | 200 article | 401, 404 |
| PUT | `/api/articles/{id}` | **JWT** | 200 article | 400, 401, 404, 409 |
| DELETE | `/api/articles/{id}` | **JWT** | 204 | 401, 404 |
| POST | `/api/articles-concurrent` | **JWT** | 201 article[] | 400, 401 |

`{id}` is an integer.

### JSON shapes (field names are EXACT — note the mixed casing, it is intentional)

**Article response** (`snake_case`):
```json
{ "article_id": 1, "name": "…", "description": "…", "category": "…|null",
  "price": 49.99, "currency": "USD|null", "row_version": 123 }
```
**Create/Update request** (`snake_case`):
```json
{ "name": "…", "description": "…", "category": "…|null", "price": 49.99,
  "currency": "USD|null", "row_version": 123 }
```
`row_version` is **only** in the *update* request (required there), never in the create request.

**Paged list** (`camelCase` wrapper around snake_case items):
```json
{ "items": [ <article> ], "page": 1, "pageSize": 10, "totalCount": 42,
  "totalPages": 5, "hasPrevious": false, "hasNext": true }
```
**Token request / response**:
```json
// POST /auth/token  →
{ "username": "admin", "password": "admin" }
// 200 →
{ "token": "<jwt>", "expiresAt": "2026-07-12T10:00:00Z" }
```

### Auth

- Demo credentials **`admin` / `admin`** (hardcoded, documented as demo). Any other → **401**.
- Issue an **HS256 JWT** with a name claim, an issuer, an audience and an expiry, signed with a
  secret from config/env (≥32 chars). Default lifetime 60 min.
- Protected endpoints require `Authorization: Bearer <jwt>`; missing/invalid → **401**.

### Validation (all failures → HTTP 400)

- `name`: required, 1–64 chars.
- `description`: required, 1–2048 chars.
- `category`: optional; if present, ≤64 chars.
- `price`: ≥ 0 and ≤ 9999999999999999.99.
- `currency`: required **only when `price > 0`**, must be a 3-letter supported ISO-4217 code
  (list below); ignored/optional when `price == 0`.
- Update only: `row_version` required and ≥ 1.

**Error body — must be `application/problem+json` (RFC 7807).** For validation (400) include an
`errors` object mapping field name → messages, e.g.
`{"type":"…","title":"…","status":400,"errors":{"name":["Name is required."]}}`. The key for the
name field must contain `name` (case-insensitive). For 404/409 return a problem+json with
`type/title/status/detail`. **Do not** use a framework's default 422/validation envelope — normalise
to 400 problem+json. (FastAPI/DRF/Express/Nest all default to something else; override it.)

Supported currencies (49): USD EUR JPY GBP CHF CAD AUD NZD SEK NOK DKK CZK PLN HUF RON BGN ISK CNY
HKD SGD KRW INR THB MYR IDR PHP TWD VND BRL MXN ARS CLP COP PEN AED SAR ILS QAR KWD BHD ZAR TRY RUB
UAH EGP NGN KES MAD PKR.

### Behaviour details

- **Create** → 201 with a `Location` header ending in the new `article_id`, body = the article.
- **Optimistic concurrency:** `row_version` is a token that **changes on every update**. Simplest
  portable approach: an integer `version` column starting at some value, incremented on each update
  and returned as `row_version`; a PUT whose `row_version` ≠ the current value → **409**. (Postgres
  `xmin` is also fine if your stack exposes it — the reference uses xmin. Either satisfies the test:
  create → PUT with the returned row_version (200) → PUT again with the now-stale value (409).)
- **List:** filter `name` (partial, case-insensitive) and `category` (exact). `page` default 1;
  `pageSize` default 10, **clamped** to max 100 (a larger value returns pageSize=100, not an error).
- **Batch** `POST /api/articles-concurrent`: body is an array of create bodies → 201 with the array
  of created articles (same order). Empty array → 400; any item invalid → 400; max 100 items.
- **Free article:** `price: 0, currency: null` is valid → 201.

## Deliverables per implementation

1. Working source under `implementations/<name>/` in the specified architecture/stack.
2. `Dockerfile` + `docker-compose.yml` (API on 8080 + its own postgres:16-alpine, healthcheck +
   startup migration/retry).
3. `README.md` — what stack/architecture, how to run, how it maps to the contract.
4. **Build/typecheck must pass** (Docker is unavailable in this environment, so prove it compiles:
   `dotnet build`, `tsc --noEmit` / build, `python -m py_compile` + framework check, etc.). Report
   the exact command and its result.
5. Do **not** modify anything outside your `implementations/<name>/` folder (not the contract, the
   conformance suite, the root README, or another implementation).

## Acceptance

An implementation is "done" when it (a) builds/typechecks, and (b) will pass `conformance/` once
Docker is available (`scripts/verify-impl.sh implementations/<name>`). Conform by construction; the
live green run happens when a Docker daemon is up.
