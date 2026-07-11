# Conformance suite

Black-box HTTP tests that **any** implementation must pass. They talk only to a running
server at `BASE_URL` — no language coupling, no database access, no shared code with any
implementation. Passing this suite is what "conforms to the contract" means, and it is the
thing that makes the cross-approach comparison fair: every implementation is proven to
behave identically before its numbers go in a table.

## Design rules

- **Self-seeding, self-scoping.** Every test creates its own uniquely named rows (UUID
  suffixes) and scopes assertions to them. The suite never asserts global counts, so it can
  run repeatedly against a shared instance without flaking.
- **Contract, not implementation.** Assertions are on status codes, wire shapes and observable
  behaviour — never on internals.
- **One token per session**, reused across tests (`conftest.py`).

## What it covers

| File | Area |
|------|------|
| `test_health.py` | `GET /health` |
| `test_auth.py` | token issuance (200/401) + the protected-endpoint gate |
| `test_articles_crud.py` | create → read → update → delete, 404s, free article |
| `test_articles_validation.py` | every failing validation rule → 400 problem+json |
| `test_articles_list.py` | filtering (name/category) + pagination metadata + clamp |
| `test_articles_batch.py` | `POST /api/articles-concurrent` |
| `test_articles_concurrency.py` | optimistic concurrency → 409; missing `row_version` → 400 |

## Run it

```bash
# 1. start an implementation so it serves on BASE_URL (e.g. dotnet-vsa on :8080)
cd ../implementations/dotnet-vsa && docker compose up --build -d && cd -

# 2. run conformance (creates the venv + installs requirements on first run)
./run.sh http://localhost:8080
```

Or, end to end (boot → wait for health → conformance → tear down) via the repo script:

```bash
../scripts/verify-impl.sh implementations/dotnet-vsa
```

> **Requires Docker** to boot an implementation with its PostgreSQL. Without a running
> server the suite cannot execute (it will fail fast obtaining a token).

## Optional: property-based smoke

`smoke.sh` fuzzes every documented operation straight from `contract/openapi.yaml` using
[schemathesis](https://schemathesis.readthedocs.io/). It is optional (not installed by
default) and complements — does not replace — the behavioural suite above:

```bash
./.venv/bin/pip install schemathesis
./smoke.sh http://localhost:8080
```
