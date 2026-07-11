# Example API — Multi-Approach Showcase (Design)

**Date:** 2026-07-12
**Status:** Phase 0 approved — in execution
**Branch:** `refactor/phase0-showcase-baseline`

## Vision

Turn this single .NET API into a **multi-approach showcase**: the *same* small API
(article/product CRUD + auth + health) implemented several times — in different
architectural styles on the same runtime, and in different language runtimes — so the
approaches can be compared side by side. The name "Example API" becomes literal.

The comparison only means something if it is **fair**, so the repo is built around three
shared, implementation-independent assets:

1. **One contract** (`contract/openapi.yaml`) — the single source of truth every
   implementation must satisfy. Same routes, same request/response shapes, same status codes.
2. **One conformance suite** (`conformance/`) — black-box HTTP tests, parameterised by
   `BASE_URL`, that *any* implementation must pass. This proves behavioural parity, so the
   comparison is apples-to-apples rather than "N unrelated apps".
3. **One benchmark harness** (`bench/`) — identical load against each implementation,
   collecting the same metrics under the same controlled conditions.

### The two comparison axes (keep them separate)

Mixing these two axes is the trap that makes numbers meaningless:

| Axis | Members | What actually differs | What to measure |
| --- | --- | --- | --- |
| **A — same runtime, different architecture** | .NET: VSA, MVC, Clean Architecture, MediatR | code shape / ceremony, not speed (all ASP.NET Core → near-identical throughput) | **developer-facing metrics**: LOC, file count, cyclomatic complexity, coupling, testability |
| **B — different runtime / framework** | .NET vs FastAPI vs Django vs Express | the runtime itself | **runtime perf**: RPS, p50/p95/p99 latency, RAM, cold start, image size |

Reporting "VSA is 2% faster than MediatR" is noise from GC/JIT. Reporting "VSA needs N fewer
files and has lower coupling than MVC" (axis A) and "FastAPI serves X RPS vs .NET's Y" (axis B)
is the real story.

## Target repository layout

```
example-api/
├── contract/                 # openapi.yaml (source of truth) + fixtures/ + README
├── conformance/              # pytest + httpx black-box suite (BASE_URL) + schemathesis smoke
├── bench/                    # methodology + k6 skeleton (real runs = Phase 2)
├── implementations/
│   └── dotnet-vsa/           # the current .NET code moves here = reference implementation
│       ├── src/  test/  ExampleApi.slnx  Dockerfile  docker-compose.yml
├── docs/                     # comparison.md (tables), roadmap, methodology, superpowers/specs
├── scripts/                  # verify-impl.sh, metrics.sh
└── README.md                 # showcase overview, the two axes, impl table, roadmap
```

Each future implementation is a self-contained folder under `implementations/`, containerised,
using the **same PostgreSQL** (so the DB is never the variable), and must pass `conformance/`.

## Phase 0 — scope (this spec's actionable unit)

Goal: current .NET → clean **reference implementation**; extract contract + conformance;
restructure the repo. Build + all tests green at the end. Behaviour of the API does **not** change.

### 0.A Repo restructure
- `git mv` the whole .NET tree (`src/`, `test/`, `ExampleApi.slnx`, `Dockerfile`,
  `docker-compose.yml`, `Example.http`) into `implementations/dotnet-vsa/`. Relative paths
  (slnx → projects, Dockerfile COPY, compose `context:.`) stay valid because the set moves together.
- Root scaffolding dirs: `contract/ conformance/ bench/ implementations/ docs/ scripts/`.
- Extend `.gitignore` (add `*.db`, Python `__pycache__/`, `.venv/`, `node_modules/`).
- Verify `dotnet build` + unit tests from the new location.

### 0.B .NET polish → reference quality (behaviour-preserving)
- **Auth slice** — restructure `Features/Auth/` into a proper slice `Features/Auth/GetToken/`:
  `GetTokenEndpoint` (thin) + `GetTokenHandler`/`IGetTokenHandler` (JWT logic moved out of the
  endpoint) + `TokenRequest` (moved) + `TokenResponse` (typed; **must keep the same wire shape**
  `{ "token": ..., "expiresAt": ... }` — the existing `IntegrationTestBase` deserialises it) +
  `AuthRegistration.AddAuthFeature()` + `README.md`. Handler returns `TokenResponse?`
  (`null` = bad credentials → 401), so it is unit-testable without HTTP. Same route, same demo
  credentials (`admin`/`admin`), same JWT claims/expiry.
- **Stale artefacts** — fix `AddDatabaseContext` XML doc ("SQLite" → "PostgreSQL (Npgsql)");
  `git rm` the tracked `ExampleApi-dev.db`; add `*.db` to `.gitignore`;
  rename `DeleteArticle/Readme.md` → `README.md`.
- **Style consistency** — lambda param `a =>` → `article =>` (Delete/Update handlers);
  `id` → `articleId` in the DeleteArticle slice.
- **Security** — bump `Microsoft.OpenApi` off the known-vulnerable 2.0.0 (build must stay green).

### 0.C Contract
- `contract/openapi.yaml` — hand-curated OpenAPI 3.1 covering the real surface:
  `GET /health`, `POST /auth/token`, `GET/POST /api/articles`, `GET/PUT/DELETE /api/articles/{id}`,
  `POST /api/articles-concurrent`. Schemas: `ArticleRequest`, `ArticleResponse`,
  `PagedArticleResponse`, `TokenRequest`, `TokenResponse`, `ProblemDetails`, validation problem.
  Bearer security scheme. Supported currency enum from `CurrencyCodes`.
- `contract/fixtures/` — shared example payloads (valid article, invalid article, credentials).
- `contract/README.md` — "source of truth; every implementation must conform; how to validate".

### 0.D Conformance
- `conformance/` — **pytest + httpx**, parameterised by `BASE_URL`. Covers: health; auth 200/401;
  article CRUD lifecycle; validation 400s (empty name, price>0 without currency, unsupported
  currency, over-length); 404s; 409 optimistic-concurrency conflict; pagination + name/category
  filters; batch create. Plus an optional **schemathesis** smoke driven by `openapi.yaml`.
- `conformance/run.sh <base_url>`, `requirements.txt`, `README.md`.
- `scripts/verify-impl.sh <impl-dir>` — `docker compose up` the impl, wait for health, run
  conformance, tear down. **Requires Docker.**

### 0.E Auth tests
- Unit: `GetTokenHandlerTests` (valid creds → token issued with correct claims/expiry;
  invalid → null). Integration: `GetTokenEndpointTests` (200 + token; 401 on bad creds).

### 0.F Docs
- Root `README.md` — showcase overview, the two axes, implementation table (dotnet-vsa =
  reference/done, others = planned), how to run conformance, roadmap.
- `docs/comparison.md` — code-metrics table (dotnet-vsa row filled via `scripts/metrics.sh`);
  perf table placeholder (Phase 2).
- `docs/roadmap.md` — the phases below.
- `Features/Auth/GetToken/README.md` — slice reference.

## Known deviations (recorded, intentionally out of Phase 0 scope)
- **Handler unit tests use EF InMemory.** The kit flags this as the "InMemory trap" (false greens
  vs a real relational store). Real-DB behaviour *is* covered by the integration tier, so these
  stay for now; noted for a later testing-strategy pass.
- **Integration tests boot a PostgreSQL container per test** (`IntegrationTestBase` news up a
  fresh `TestWebApplicationFactory` in `InitializeAsync`). The kit's slice-testing guidance is
  "boot once per assembly via a shared collection fixture". This is a real perf smell in the test
  suite; deferred (bigger test-infra change) to avoid destabilising Phase 0.
- **Hand-rolled `I{Slice}Handler` + exceptions**, not MediatR + `Result<T>`. This is a legitimate
  VSA variant and the kit's own brownfield rule says match the existing convention. The MediatR
  flavour, if wanted, becomes its **own** implementation folder (`dotnet-mediatr`) rather than a
  rewrite of the reference.

## Roadmap (post-Phase-0; each phase = its own spec → plan → implement cycle)
- **Phase 1** — add `dotnet-mvc` (sharpest contrast to VSA, same runtime) → conformance green →
  first axis-A code-metrics table.
- **Phase 2** — add `python-fastapi` (fast to write, strong perf contrast) → wire the real
  benchmark harness (k6) → first axis-B perf table.
- **Phase 3+** — `dotnet-clean`, `dotnet-mediatr`, `python-django`, `ts-express` as independent,
  repeatable add-ons. (Plain JS / Java explicitly out — user has no working knowledge; quality
  would suffer.)

## Environment notes (this session)
- Docker daemon is **down** in the working session → integration tests (Testcontainers) and the
  live conformance run (docker compose) **cannot be executed now**. Everything is authored and
  **statically** verified (dotnet build, unit tests, `openapi.yaml` validation,
  `pytest --collect-only`); the live run is one command away once Docker is up.
- `cloc` / `k6` absent → metrics via a small `scripts/metrics.sh`; bench is a skeleton anyway.

## Verification (definition of done for Phase 0)
- `dotnet build` green; unit tests green (baseline: 125 passing).
- `vsa-guardian` gate on `implementations/dotnet-vsa` — no high/medium findings.
- `openapi.yaml` validates; `conformance/` collects and (Docker permitting) runs green against
  dotnet-vsa.
- Root README + comparison + roadmap present; repo restructured; branch commits per step.
