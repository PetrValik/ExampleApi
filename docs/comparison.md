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

Here the runtime is the variable, so **throughput and resource use are the point**. Each
implementation was benchmarked **one at a time under the same budget — 1 CPU / 1 GB per container —
saturated** (50 VUs, no think-time), so `req/s` reflects **max throughput per CPU** (efficiency),
not who grabbed more cores. Latency is high because the CPU is deliberately saturated; compare it
*across* rows, not to an SLA. Produced by [`bench/run-local.sh`](../bench/run-local.sh) →
[`bench/results.json`](../bench/results.json); the dashboard reads the same numbers.

Sorted by throughput per CPU (all serve the identical contract, same PostgreSQL):

| Implementation | Runtime / stack | req/s @1CPU | p95 | p99 | RAM (load) | Image |
|----------------|-----------------|:-----------:|:---:|:---:|:----------:|:-----:|
| **ts-express** | Express + Prisma (Node) | **1986** | 52 ms | 58 ms | **43 MB** | 112 MB |
| ts-nestjs | NestJS + TypeORM (Node) | 1874 | 46 ms | 56 ms | 64 MB | 70 MB |
| dotnet-clean | Clean Architecture (.NET) | 1850 | 78 ms | 96 ms | 84 MB | 91 MB |
| dotnet-minimal | Minimal API (.NET) | 1790 | 81 ms | 98 ms | 88 MB | 91 MB |
| dotnet-vsa | Vertical Slice (.NET) | 1665 | 83 ms | 96 ms | 87 MB | 92 MB |
| dotnet-mediatr | VSA + MediatR (.NET) | 1575 | 84 ms | 100 ms | 81 MB | 91 MB |
| python-flask | Flask + SQLAlchemy (sync) | 1507 | 57 ms | 62 ms | 112 MB | **58 MB** |
| dotnet-mvc | Controllers (.NET) | 1506 | 87 ms | 103 ms | 84 MB | 91 MB |
| python-fastapi | FastAPI + SQLAlchemy (async) | 1367 | 69 ms | 99 ms | 61 MB | 63 MB |
| python-django | Django + DRF | 369 | 168 ms | 190 ms | 148 MB | 58 MB |

**What it says:**

- **Node (Express/Nest) tops throughput *and* memory** — ~1900–2000 req/s at 43–64 MB. Express's
  Prisma engine makes its image the largest (112 MB) despite the leanest runtime memory.
- **.NET clusters tightly (1506–1850 req/s, ~85 MB) across all five architectures** — the clearest
  proof of the whole thesis: on the same runtime, **architecture barely moves performance** (~20%
  spread, mostly noise + MediatR/controller indirection). So choose a .NET architecture on *code
  ceremony* (axis A), not speed.
- **Django/DRF is the outlier at 369 req/s** — DRF's serializer/validation stack plus sync workers
  cost ~4–5× the throughput of every other runtime here, at the highest memory (148 MB). Flask and
  FastAPI (same language) are 4× faster, so this is a *framework* cost, not a *Python* cost.
- **Smallest images are Python/Django-family (58 MB)**; .NET is a consistent ~91 MB; Prisma inflates
  ts-express to 112 MB.

> **Caveats:** a single run on one developer machine (Apple Silicon), saturated at 1 CPU — a
> *relative* comparison, not a datacenter benchmark. Re-run with `bash bench/run-local.sh`
> (tune `BENCH_CPUS` / `BENCH_VUS`), or `BENCH_SLEEP=0.1` for a latency-under-realistic-load view.
> The [CI benchmark](../.github/workflows/benchmark.yml) runs the same profile on a 2-vCPU runner.

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
