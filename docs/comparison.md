# Comparison

Ten implementations of the identical API. The two axes are reported separately (see the
[root README](../README.md) for why mixing them misleads).

> **Verification status:** all implementations **build/typecheck** clean and are Docker-ready.
> The **live conformance run** (which proves behavioural parity, and therefore that these numbers
> compare like-for-like) is pending a Docker daemon — run
> `scripts/verify-impl.sh implementations/<name>` once Docker is up. Until then, read the code-size
> numbers as real and the "conforms" column as *by construction, not yet executed*.

---

## Axis A — same runtime, different architecture (.NET)

All five run on the same ASP.NET Core + PostgreSQL, so throughput is ~equal between them. What
differs is the **shape and amount of code** to deliver the identical behaviour. Production source
only (tests and generated EF migrations excluded); from [`scripts/metrics.sh`](../scripts/metrics.sh).

| Implementation | Architecture | Src files | Src LOC | Non-blank | Conforms |
|----------------|--------------|:---------:|:-------:|:---------:|:--------:|
| **dotnet-minimal** | one `Program.cs`, raw Minimal API, no abstractions | **1** | **382** | **327** | by-construction |
| **dotnet-mvc** | Controllers → Services → Repositories (classic layered) | 29 | 1097 | 917 | by-construction |
| **dotnet-clean** | Clean Architecture, 4 projects, dependency rule inward | 40 | 1401 | 1185 | by-construction |
| **dotnet-mediatr** | Vertical Slice + MediatR + `Result<T>` + pipeline validation | 46 | 1550 | 1312 | by-construction |
| **dotnet-vsa** *(reference)* | Vertical Slice, hand-rolled handlers + exceptions | 48 | 1823 | 1596 | ✅ suite ready |

**Reading it:** the spread is ~48× in file count and ~4.8× in LOC for the *same* behaviour.
`dotnet-minimal` is the low-water mark — everything inline, nothing abstracted. The structured
styles (VSA, MediatR, Clean) trade more files/LOC for boundaries, testability and "add a feature by
adding a folder". `dotnet-vsa` reads highest partly because it carries a handler **interface per
slice** plus dense XML-doc comments (non-blank strips blank lines, not comments) — a reminder that
LOC is a proxy, not a verdict. The honest takeaway: **more structure is not free, and minimal is not
automatically better** — the table makes the trade visible.

> Only `dotnet-vsa` has a test suite so far (132 unit + 29 conformance). The siblings are built to
> the same contract; adding their tests is follow-on work.

---

## Axis B — different runtime / framework

Here the runtime is the variable, so **throughput and resource use are the point** — pending the
benchmark harness (Phase 2, needs Docker). Code size is shown only for context; LOC across languages
is not directly comparable.

| Implementation | Runtime / stack | Src files | Src LOC | RPS · p50/p95/p99 · RAM · image |
|----------------|-----------------|:---------:|:-------:|:-------------------------------:|
| python-flask | Flask + SQLAlchemy (sync WSGI) | 11 | 611 | _pending Phase 2_ |
| python-fastapi | FastAPI + SQLAlchemy 2 (async) | 14 | 755 | _pending Phase 2_ |
| python-django | Django + DRF | 16 | 721 | _pending Phase 2_ |
| ts-express | Express + Prisma (Node 24) | 14 | 747 | _pending Phase 2_ |
| ts-nestjs | NestJS + TypeORM (Node 24) | 21 | 850 | _pending Phase 2_ |
| dotnet-vsa | .NET 10 | 48 | 1823 | _pending Phase 2_ |

Even before perf numbers, an axis-A-style observation leaks in: within Node, **Express (14 files) vs
NestJS (21 files)** is the same ceremony trade as minimal-vs-VSA in .NET — NestJS's modules/decorators
cost files, Express stays lean. And the Python trio (11–16 files) is markedly leaner than any
structured .NET style for the same behaviour.

> Numbers are from [`scripts/gen-dashboard-data.mjs`](../scripts/gen-dashboard-data.mjs) (canonical;
> excludes build output incl. TS `dist/`) — the same source the dashboard reads.

Methodology (warm-up, repeated runs, controlled variables) is in [`bench/README.md`](../bench/README.md).
When perf numbers exist, a shareable chart can be generated from this table.

---

## Implementation notes (parity details)

- **Optimistic concurrency (`row_version`):** the .NET impls (`vsa`, `mvc`, `mediatr`) use Postgres'
  `xmin` system column; `dotnet-clean`, all Python and all TS impls use a portable integer `version`
  column incremented per update. Both satisfy the contract (stale `row_version` on PUT → 409).
- **Validation → 400 problem+json:** every impl normalises its framework's native validation error
  (FastAPI/DRF 422, Nest/Express defaults) to `400 application/problem+json` with an `errors` map, as
  the contract requires.
- **Batch:** `dotnet-vsa`/`dotnet-mvc` do genuine parallel inserts (one DbContext per item); the
  others create sequentially in order. The contract only requires ordered creation, so both conform.

## Methodology guardrails

- **Same contract, same DB, same load.** The only intended variable is the approach/runtime.
- **Conformance first.** Numbers are provisional until the live suite runs green (needs Docker).
- **Warm before measuring; measure repeatedly.** Report medians across runs.
- **Separate the axes.** Never rank a .NET architecture by RPS, or a runtime by file count.
