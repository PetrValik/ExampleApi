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

## Phase 1 — all nine sibling implementations ✅ (built, pending live conformance)

Built in one parallel batch to [`CONTRACT-FOR-IMPLEMENTERS.md`](../implementations/CONTRACT-FOR-IMPLEMENTERS.md);
all build/typecheck clean and are Docker-ready. First code-ceremony numbers in
[`comparison.md`](comparison.md).

- Axis A (.NET): `dotnet-minimal`, `dotnet-mvc`, `dotnet-clean`, `dotnet-mediatr`.
- Axis B: `python-fastapi`, `python-django`, `python-flask`, `ts-express`, `ts-nestjs`.

**Remaining for Phase 1 to be truly "done": run the live conformance suite against each** (needs
Docker) and fix any parity gaps it surfaces — `./scripts/verify-impl.sh implementations/<name>`.
The build already surfaced one reference bug (case-sensitive name filter → fixed with `ILike`);
expect the live run to surface a few more per impl. Sibling test suites are also follow-on.

## Phase 2 — real benchmarks (the main remaining value)

- Wire the [`bench/`](../bench) k6 harness on a host with Docker; run the identical load profile
  against each implementation; fill the axis-B perf table (RPS, p50/p95/p99, RAM, image size).
- Keep the axes separate: axis-A stays a code/ceremony comparison, axis-B the perf one.

## Phase 3+ — depth

- Per-sibling **test suites** (each currently relies on the shared conformance suite only).
- Optional extra variants **in already-present toolchains** (e.g. a Go/Rust impl would need those
  toolchains installed first). Explicitly **out of scope**: plain JavaScript and Java (no working
  knowledge on hand). Revisit only if that changes.

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
