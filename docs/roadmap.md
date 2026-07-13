# Roadmap

Each phase is its own small cycle: design → build → **conformance green** → into the tables.
Implementations are independent — they can be built in any order once Phase 0 exists.

## Phase 0 — reference + scaffolding ✅ (complete)

- Current .NET code becomes the reference implementation (`implementations/dotnet-vsa`),
  polished to reference quality (proper Auth slice, stale artefacts removed, security bump,
  consistent naming).
- `contract/openapi.yaml` extracted as the single source of truth.
- `conformance/` black-box suite that every implementation must pass.
- `bench/` methodology + harness skeleton; `scripts/` for verify + metrics.
- Repo restructured under `implementations/`, `contract/`, `conformance/`, `bench/`, `docs/`.

Design: [`docs/superpowers/specs/2026-07-12-example-api-multiapproach-showcase-design.md`](superpowers/specs/2026-07-12-example-api-multiapproach-showcase-design.md).

## Phase 1 — all nine sibling implementations ✅ (done, conformant in CI)

Built in one parallel batch to [`CONTRACT-FOR-IMPLEMENTERS.md`](../implementations/CONTRACT-FOR-IMPLEMENTERS.md);
all build/typecheck clean and **pass the conformance suite live in CI (10/10)**. The live run
surfaced and fixed four real parity bugs (2× case-sensitive `Like`→`ILike`, a FastAPI 204-with-body
keep-alive desync, ts-express Prisma-on-Alpine). Code-ceremony numbers in [`comparison.md`](comparison.md).

- Axis A (.NET): `dotnet-minimal`, `dotnet-mvc`, `dotnet-clean`, `dotnet-mediatr`.
- Axis B: `python-fastapi`, `python-django`, `python-flask`, `ts-express`, `ts-nestjs`.

## Phase 2 — real benchmarks ✅ (done)

The [`bench/`](../bench) k6 harness has run against every implementation and the results are live on
the [dashboard](https://petrvalik.github.io/ExampleApi/) + [`comparison.md`](comparison.md):
resource-fair throughput @1 CPU (median of 3), CPU-scaling curves (0.5–4), cold-start, workload
shapes (read/write/paginate), latency-under-load, and a FastAPI multi-worker sweep. Axes kept
separate: axis-A = code/ceremony, axis-B = perf.

## Phase 3+ — depth (optional)

- **Cleaner perf numbers**: re-run the harness on a dedicated host (the current numbers are a median
  on one laptop). `bench/run-median.sh`, `run-sweep.sh`, `run-profiles.sh`, `run-workers.sh`.
- Per-sibling **test suites** (each currently relies on the shared conformance suite only).
- Optional extra variants **in already-present toolchains** (a Go/Rust impl would need those
  toolchains installed first). Explicitly **out of scope**: plain JavaScript and Java. Revisit if that changes.

## Contract versioning

`contract/openapi.yaml` carries `info.version`. Bump it whenever the wire shape changes and
note it here; implementations then catch up and re-run conformance. Additive/back-compatible
changes bump minor/patch; breaking wire changes bump major.

| Contract version | Change |
|------------------|--------|
| 1.0.0 | Initial contract extracted from `dotnet-vsa` (Phase 0). |

## Known deviations to revisit (from Phase 0)

- Handler unit tests use EF InMemory (the "InMemory trap"); real-DB behaviour is covered by
  the integration tier. A later testing-strategy pass could move DB-touching handler tests
  fully into integration.
- Integration tests boot a PostgreSQL container **per test**; a shared collection fixture
  (boot once per assembly) would cut suite time substantially.
