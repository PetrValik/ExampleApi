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

## Phase 1 — first axis-A sibling

- **`dotnet-mvc`** — the sharpest contrast to VSA (controllers + services + repositories),
  same runtime. Pass conformance, then fill the first real axis-A row in
  [`comparison.md`](comparison.md).

## Phase 2 — first axis-B sibling + real benchmarks

- **`python-fastapi`** — fast to write, strong perf contrast to .NET.
- Wire the [`bench/`](../bench) harness (k6) and produce the first axis-B perf table.

## Phase 3+ — breadth

- `dotnet-clean` (Clean Architecture — the `kit-clean-arch-dotnet` pack fits here).
- `dotnet-mediatr` (VSA + MediatR + `Result<T>` — the canonical kit flavour, as its **own**
  implementation rather than a rewrite of the reference).
- `python-django`, `ts-express`.

Explicitly **out of scope**: plain JavaScript and Java (no working knowledge on hand — quality
would suffer). Revisit only if that changes.

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
