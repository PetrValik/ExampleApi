# Example API — Python Flask (sync WSGI) implementation

One implementation of the shared [Example API contract](../CONTRACT-FOR-IMPLEMENTERS.md).
This is the **classic synchronous WSGI** counterpart to the async FastAPI approach:
Flask + Flask-SQLAlchemy + flask-jwt-extended, served by **gunicorn** on port 8080,
backed by **PostgreSQL**.

## Stack

| Concern          | Choice                                                        |
|------------------|--------------------------------------------------------------|
| Web framework    | Flask 3 (sync WSGI), one **Blueprint per resource**          |
| ORM              | Flask-SQLAlchemy 3 / SQLAlchemy 2 (`psycopg` v3 driver)      |
| Auth             | flask-jwt-extended (HS256 JWT, `name` claim, issuer/audience) |
| Validation       | manual validators -> shared `application/problem+json`        |
| Server           | gunicorn (2 sync workers) on `0.0.0.0:8080`                   |
| Database         | PostgreSQL 16 (own `postgres:16-alpine` in compose)          |

## Architecture

```
app/
  __init__.py        # app factory (create_app) + init_db (startup schema + retry)
  extensions.py      # db (SQLAlchemy) and jwt (JWTManager) singletons
  config              # (inline in __init__) env-driven config
  models.py          # Article model; integer `version` -> row_version
  currency.py        # the 49 supported ISO-4217 codes
  validation.py      # manual create/update body validation
  problems.py        # RFC 7807 problem+json helpers (400/401/404/409)
  blueprints/
    health.py        # GET /health
    auth.py          # POST /auth/token
    articles.py      # /api/articles CRUD + list + /api/articles-concurrent
wsgi.py              # gunicorn entrypoint: app = create_app(); init_db(app)
```

Every error path funnels through `problems.py`, so validation failures are always
**HTTP 400 `application/problem+json`** with an `errors` object (field -> messages),
never Flask's default HTML/JSON envelope. JWT failures are normalised to 401
problem+json via the `@jwt.*_loader` hooks.

## How to run

### Docker (production profile — serves 8080)

```bash
docker compose up --build
```

Brings up PostgreSQL (with a `pg_isready` healthcheck) and the API. The API waits
for the DB (`depends_on: condition: service_healthy`) and, on boot, creates the
schema with a connect-retry loop (`init_db`). Then:

```bash
curl localhost:8080/health
TOKEN=$(curl -s localhost:8080/auth/token -H 'content-type: application/json' \
  -d '{"username":"admin","password":"admin"}' | python -c 'import sys,json;print(json.load(sys.stdin)["token"])')
curl localhost:8080/api/articles -H "Authorization: Bearer $TOKEN"
```

### Local (dev)

```bash
python -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
export DATABASE_URL=postgresql+psycopg://postgres:postgres@localhost:5432/exampleapi
python wsgi.py            # dev server on :8080  (or: gunicorn --bind 0.0.0.0:8080 wsgi:app)
```

### Configuration (env vars)

| Var                     | Default                                         |
|-------------------------|-------------------------------------------------|
| `DATABASE_URL`          | `postgresql+psycopg://postgres:postgres@localhost:5432/exampleapi` |
| `JWT_SECRET_KEY`        | a >=32-char demo key (override in production)    |
| `JWT_ISSUER`            | `ExampleApi`                                    |
| `JWT_AUDIENCE`          | `ExampleApiClient`                              |
| `JWT_EXPIRATION_MINUTES`| `60`                                            |

`postgres://` / `postgresql://` URLs are auto-rewritten to the `psycopg` v3 driver.

## Contract mapping

| Contract requirement                              | Where |
|---------------------------------------------------|-------|
| `GET /health` -> `{"status":"healthy"}`           | `blueprints/health.py` |
| `POST /auth/token`, admin/admin -> HS256 JWT, else 401 | `blueprints/auth.py` |
| Article CRUD, list, batch (all JWT-protected)     | `blueprints/articles.py` |
| snake_case article body, camelCase paged wrapper  | `models.Article.to_response`, `list_articles` |
| Validation -> 400 problem+json with `errors` map  | `validation.py` + `problems.validation_problem` |
| 49 supported currencies; required only when price>0 | `currency.py`, `validation.py` |
| `Location` header ends in new `article_id`        | `create_article` |
| pageSize clamped to 100 (not rejected)            | `list_articles` |
| Batch: empty -> 400, any invalid -> 400, max 100  | `batch_create_articles` |
| Optimistic concurrency (see below)                | `models.py` + `update_article` |

### Optimistic concurrency (`row_version`)

`Article.version` is a plain integer column that starts at **1** and is
incremented on **every** successful update; it is surfaced on the wire as
`row_version`. On `PUT`, the supplied `row_version` is compared to the stored
value — a mismatch (stale token) returns **409 Conflict**; a match updates the
row and bumps the version. This is the portable integer-version approach the
contract endorses (the .NET reference uses PostgreSQL `xmin`; both satisfy the
conformance suite: create -> PUT with returned `row_version` = 200 -> PUT again
with the now-stale value = 409). `row_version` is required (>= 1) on update and
absent from create.

## Build / verification

Docker is unavailable in the build environment, so compilation is proven by:

```bash
python -m py_compile <every module>
# then boot the whole app (factory + init_db + a smoke request) against SQLite:
DATABASE_URL=sqlite:///<tmp>.db python -c "import wsgi; c=wsgi.app.test_client(); assert c.get('/health').status_code==200"
```

Both pass. The conformance suite (`../../conformance`) runs against the live
container once a Docker daemon is available (`scripts/verify-impl.sh`).
