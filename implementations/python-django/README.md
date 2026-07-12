# Example API — Python / Django + DRF

One implementation of the shared **Example API** contract, built with
**Django + Django REST Framework** and backed by **PostgreSQL**. It serves the
exact same HTTP behaviour as every other implementation in this repo, so the
[`conformance/`](../../conformance) suite passes against it unchanged.

## Stack

| Concern | Choice |
|---|---|
| Web framework | Django 5.1 |
| API layer | Django REST Framework 3.15 |
| Auth | HS256 JWT via **PyJWT** (the JWT engine that backs `djangorestframework-simplejwt`) |
| Database | PostgreSQL 16 (`psycopg2-binary`) |
| Server | gunicorn on port **8080** |

## Architecture

A single Django app, `articles`, holds the whole feature surface:

```
config/                 Django project (settings, urls, wsgi/asgi)
  settings.py           env-driven config: DB, JWT secret/issuer/audience/expiry
  urls.py               routes → views
articles/
  models.py             Article model + row_version (integer version column)
  serializers.py        DRF validators (field rules + currency/price rule)
  views.py              APIView endpoints (health, token, CRUD, batch)
  auth.py               JWT bearer authentication + token issuance
  exceptions.py         RFC 7807 problem+json exception handler + 409 exception
  currencies.py         the 49 supported ISO-4217 codes
  migrations/0001_…     schema (table `articles`)
manage.py, requirements.txt, Dockerfile, docker-compose.yml, entrypoint.sh
```

Plain `APIView` classes (not a router/ViewSet) are used so status codes, the
`Location` header, pagination clamping and batch semantics map to the contract
exactly.

### Validation → problem+json

DRF's default error envelope is normalised by a custom `EXCEPTION_HANDLER`
(`articles.exceptions.problem_exception_handler`) to
**`application/problem+json`** (RFC 7807):

* **400 validation** → `{type, title, status, errors: {field: [messages]}}`
  (the offending field name is always a key in `errors`).
* **404 / 409** → `{type, title, status, detail}`.
* **401** keeps DRF's response (status only + `WWW-Authenticate`).

### Auth

`POST /auth/token` checks the hardcoded demo credentials **`admin` / `admin`**
(any other → 401) and mints an HS256 JWT with a `name` claim, issuer, audience
and expiry — all from env/config (`JWT_SECRET` ≥ 32 chars, default lifetime
60 min). Because the demo has no user table, tokens are signed and verified
directly with PyJWT rather than SimpleJWT's user-coupled views. All
`/api/articles/**` endpoints require `Authorization: Bearer <jwt>`; missing or
invalid → 401. `GET /health` and `POST /auth/token` are anonymous.

### Optimistic concurrency (`row_version`)

The `Article` model carries an integer `version` column that starts at **1** and
is surfaced to clients as `row_version`. `PUT` performs a **conditional update**
(`filter(pk=id, version=row_version).update(..., version=F('version')+1)`):

* row matched → version incremented, updated article returned (200).
* row exists but `version` mismatched → **409 Conflict**.

So: create (`row_version=1`) → PUT with `1` (200, now `2`) → PUT again with the
stale `1` (409).

## Contract mapping

| Method | Path | Auth | View |
|---|---|---|---|
| GET | `/health` | none | `HealthView` |
| POST | `/auth/token` | none | `TokenView` |
| GET | `/api/articles` | JWT | `ArticleListCreateView.get` (filter `name` partial/CI, `category` exact; `pageSize` clamped 1..100) |
| POST | `/api/articles` | JWT | `ArticleListCreateView.post` (201 + `Location`) |
| GET | `/api/articles/{id}` | JWT | `ArticleDetailView.get` |
| PUT | `/api/articles/{id}` | JWT | `ArticleDetailView.put` (409 on stale `row_version`) |
| DELETE | `/api/articles/{id}` | JWT | `ArticleDetailView.delete` (204) |
| POST | `/api/articles-concurrent` | JWT | `ArticleBatchCreateView.post` (array; empty/any-invalid → 400; max 100) |

Article payloads use `snake_case` (`article_id`, `row_version`); the pagination
wrapper and token response use `camelCase` (`pageSize`, `expiresAt`) — matching
the intentional casing pinned by the contract.

## Run it

```bash
docker compose up --build
```

Brings up PostgreSQL 16 (with a healthcheck) and the API; the entrypoint waits
for the DB, applies migrations, then starts gunicorn on **http://localhost:8080**.

```bash
# smoke test
curl localhost:8080/health
TOKEN=$(curl -s localhost:8080/auth/token -H 'content-type: application/json' \
  -d '{"username":"admin","password":"admin"}' | python -c 'import sys,json;print(json.load(sys.stdin)["token"])')
curl localhost:8080/api/articles -H "Authorization: Bearer $TOKEN"
```

## Configuration (environment variables)

| Var | Default | Meaning |
|---|---|---|
| `POSTGRES_DB` / `POSTGRES_USER` / `POSTGRES_PASSWORD` | `exampleapi` / `postgres` / `postgres` | DB credentials |
| `POSTGRES_HOST` / `POSTGRES_PORT` | `localhost` / `5432` | DB location |
| `JWT_SECRET` | demo 50-char secret | HS256 signing key (≥ 32 chars) |
| `JWT_ISSUER` / `JWT_AUDIENCE` | `ExampleApi` / `ExampleApiClient` | token `iss` / `aud` |
| `JWT_EXPIRATION_MINUTES` | `60` | token lifetime |
| `DJANGO_DEBUG` | `false` | Django debug flag |

## Build / verification

Docker is unavailable in the build environment, so correctness is proven by
compiling and importing everything against installed dependencies:

```bash
python -m venv .venv && . .venv/bin/activate
pip install -r requirements.txt
python -m py_compile $(find . -name '*.py' -not -path './.venv/*')
python manage.py check
```

`python manage.py check` passes (system checks report no issues). `check` loads
the settings, URL conf, all views, serializers, the auth backend and the
exception handler, proving the application wires up cleanly.
