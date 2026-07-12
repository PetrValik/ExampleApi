# Example API — Python + FastAPI + SQLAlchemy

One implementation of the shared [Example API contract](../CONTRACT-FOR-IMPLEMENTERS.md)
(authoritative spec: [`contract/openapi.yaml`](../../contract/openapi.yaml)). It is a black-box
behavioural twin of the canonical [`dotnet-vsa`](../dotnet-vsa) reference — same routes, same JSON
shapes, same status codes — so the [`conformance/`](../../conformance) suite passes against it.

## Stack

| Concern | Choice |
|---|---|
| Web framework | **FastAPI** (async) on **uvicorn**, port **8080** |
| ORM / DB | **SQLAlchemy 2.x** (async, `asyncpg`) → **PostgreSQL 16** |
| Schemas | **Pydantic v2** |
| Auth | **PyJWT** (HS256) |
| Packaging | Dockerfile + docker-compose (its own `postgres:16-alpine`) |

## Architecture

```
app/
  main.py         FastAPI app: routers, problem+json exception handlers, DB bootstrap (lifespan)
  config.py       Settings from env (DB URL, JWT secret/issuer/audience/expiry, demo creds)
  database.py     Async engine + session factory + declarative Base (lazy — import never connects)
  models.py       SQLAlchemy Article model (integer `version` = optimistic-concurrency token)
  schemas.py      Pydantic request/response models (snake_case article, camelCase paging/token)
  validation.py   Explicit business-rule validation → field-keyed error maps
  currencies.py   The 49 supported ISO-4217 codes
  security.py     JWT create/verify + the Bearer dependency (missing/invalid → 401, never 403)
  problem.py      RFC 7807 problem+json helpers, domain exceptions, the 422→400 override
  routers/
    health.py     GET /health              (anonymous)
    auth.py       POST /auth/token         (anonymous)
    articles.py   /api/articles**          (JWT-guarded router)
```

- **Layered, resource-per-router.** Routers hold thin endpoints; validation, security and
  problem-shaping live in dedicated modules.
- **Tables are created at startup** by the FastAPI `lifespan` hook (`Base.metadata.create_all`),
  with exponential-backoff retry so the API tolerates the DB still coming up. Compose additionally
  gates the API on the Postgres healthcheck (`depends_on: condition: service_healthy`).

## How to run

```bash
# from this folder — brings up API (8080) + its own PostgreSQL
docker compose up --build
```

`GET http://localhost:8080/health` → `{"status":"healthy"}`.
Get a token with `POST /auth/token` `{"username":"admin","password":"admin"}`, then call
`/api/articles**` with `Authorization: Bearer <token>`.

Configuration (env, defaults in `docker-compose.yml`): `DATABASE_URL`, `JWT_SECRET` (≥32 chars),
`JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_EXPIRATION_MINUTES`.

## Contract mapping (the details that matter)

- **Field casing is exact.** Article bodies are `snake_case` (`article_id`, `row_version`); the
  pagination wrapper and token response are `camelCase` (`pageSize`, `expiresAt`). Responses are
  built as explicit dicts, so no serializer can "fix" the intentional inconsistency.
- **All validation → HTTP 400 `application/problem+json`.** FastAPI/Pydantic's default 422 envelope
  is overridden: the `RequestValidationError` handler *and* the explicit validators both emit
  `{type,title,status,errors}` where `errors` maps a field name → messages (e.g. `errors.name`).
  Business rules enforced explicitly: `name` 1–64, `description` 1–2048, `category` ≤64, `price`
  0…9999999999999999.99, `currency` required + supported-ISO only when `price > 0`, update
  `row_version` required and ≥1.
- **Optimistic concurrency.** `Article.version` is an integer starting at 1 and **incremented on
  every update**; it is returned as `row_version`. A PUT whose `row_version` ≠ the current value →
  **409**. (Portable equivalent of the reference's Postgres `xmin`; satisfies create → PUT with the
  returned value (200) → PUT again with the now-stale value (409).)
- **Auth.** `admin`/`admin` → HS256 JWT with a `name` claim, issuer, audience and expiry from
  config; wrong/empty creds → 401. Protected endpoints require the bearer; missing/invalid → 401.
- **Behaviour.** Create → 201 + `Location` ending in the new `article_id`. List filters `name`
  (partial, case-insensitive `ILIKE`, wildcards escaped) and `category` (exact); `page` default 1,
  `pageSize` default 10 **clamped** to 100 (never rejected). Batch `POST /api/articles-concurrent`:
  array in → array out (same order); empty → 400, any invalid item → 400, >100 → 400. Free article
  (`price:0, currency:null`) → 201.

## Build / verify

Docker is unavailable in the build environment, so parity is proven by compiling + importing the
app and driving it in-process:

```bash
python -m venv .venv && ./.venv/bin/pip install -r requirements.txt
./.venv/bin/python -m py_compile app/*.py app/routers/*.py     # compiles clean
./.venv/bin/python -c "from app.main import app"               # imports clean (no DB needed)
```

The full CRUD / concurrency / batch / pagination behaviour was additionally exercised in-process
via Starlette's `TestClient` (with the DB dependency pointed at SQLite) and matches every
`conformance/` expectation. The live green run happens once a Docker daemon is available:
`../../scripts/verify-impl.sh implementations/python-fastapi`.
