# Comparison

Ten implementations of the identical API. The two axes are reported separately (see the
[root README](../README.md) for why mixing them misleads).

> **Verification status:** all 10 implementations **pass the black-box conformance suite live in
> [CI](../.github/workflows/ci.yml)** (each booted via its own compose, tested on every push) — so the
> numbers below compare like-for-like behaviour. Performance figures are a median of local runs on one
> Apple-Silicon laptop (relative, not a datacenter benchmark).

---

## Axis A — same runtime, different architecture (.NET)

All five run on the same ASP.NET Core + PostgreSQL, so throughput is ~equal between them. What
differs is the **shape and amount of code** to deliver the identical behaviour. Production source
only (tests and generated EF migrations excluded); from [`scripts/metrics.sh`](../scripts/metrics.sh).

| Implementation | Architecture | Src files | Src LOC | Non-blank | Conforms |
|----------------|--------------|:---------:|:-------:|:---------:|:--------:|
| **dotnet-minimal** | one `Program.cs`, raw Minimal API, no abstractions | **1** | **382** | **327** | ✅ CI |
| **dotnet-mvc** | Controllers → Services → Repositories (classic layered) | 29 | 1097 | 917 | ✅ CI |
| **dotnet-clean** | Clean Architecture, 4 projects, dependency rule inward | 40 | 1401 | 1185 | ✅ CI |
| **dotnet-mediatr** | Vertical Slice + MediatR + `Result<T>` + pipeline validation | 46 | 1550 | 1312 | ✅ CI |
| **dotnet-vsa** *(reference)* | Vertical Slice, hand-rolled handlers + exceptions | 48 | 1823 | 1596 | ✅ CI |

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
*across* rows, not to an SLA. **Median of 3 runs** (`bash bench/run-median.sh` →
[`bench/results.json`](../bench/results.json)); the dashboard reads the same numbers.

Sorted by throughput per CPU (all serve the identical contract, same PostgreSQL):

| Implementation | Runtime / stack | req/s @1CPU | p95 | p99 | RAM (load) | Image |
|----------------|-----------------|:-----------:|:---:|:---:|:----------:|:-----:|
| **ts-express** | Express + Prisma (Node) | **1744** | 57 ms | 63 ms | **45 MB** | 112 MB |
| ts-nestjs | NestJS + TypeORM (Node) | 1692 | 51 ms | 63 ms | 63 MB | 70 MB |
| dotnet-vsa | Vertical Slice (.NET) | 1614 | 87 ms | 101 ms | 79 MB | 92 MB |
| dotnet-clean | Clean Architecture (.NET) | 1410 | 92 ms | 109 ms | 78 MB | 91 MB |
| dotnet-minimal | Minimal API (.NET) | 1406 | 91 ms | 109 ms | 75 MB | 91 MB |
| dotnet-mediatr | VSA + MediatR (.NET) | 1362 | 93 ms | 114 ms | 78 MB | 91 MB |
| dotnet-mvc | Controllers (.NET) | 1311 | 95 ms | 113 ms | 80 MB | 91 MB |
| python-flask | Flask + SQLAlchemy (sync) | 1176 | 67 ms | 72 ms | 111 MB | **58 MB** |
| python-fastapi | FastAPI + SQLAlchemy (async) | 1118 | 80 ms | 117 ms | 62 MB | 63 MB |
| python-django | Django + DRF | 329 | 180 ms | 190 ms | 145 MB | 58 MB |

**What it says:**

- **Node (Express/Nest) tops throughput *and* memory** — ~1700 req/s at 45–63 MB. Express's
  Prisma engine makes its image the largest (112 MB) despite the leanest runtime memory.
- **.NET clusters tightly (1311–1614 req/s, ~78 MB) across all five architectures** — the clearest
  proof of the whole thesis: on the same runtime, **architecture barely moves performance** (~20%
  spread, mostly noise + MediatR/controller indirection). So choose a .NET architecture on *code
  ceremony* (axis A), not speed.
- **Django/DRF is the outlier at 329 req/s** — DRF's serializer/validation stack plus sync workers
  cost ~4–5× the throughput of every other runtime here, at the highest memory (145 MB). Flask and
  FastAPI (same language) are ~3.5× faster, so this is a *framework* cost, not a *Python* cost.
- **Smallest images are Python/Django-family (58 MB)**; .NET is a consistent ~91 MB; Prisma inflates
  ts-express to 112 MB.

> **Caveats:** median of 3 runs on one developer machine (Apple Silicon), saturated at 1 CPU — a
> *relative* comparison, not a datacenter benchmark. Re-run with `bash bench/run-median.sh`
> (tune `BENCH_CPUS` / `BENCH_VUS`), or `BENCH_SLEEP=0.1` for a latency-under-realistic-load view.
> The [CI benchmark](../.github/workflows/benchmark.yml) runs the same profile on a 2-vCPU runner.

### Scaling — throughput vs CPU (the concurrency model, made visible)

Each implementation, saturated, at 0.5 / 1 / 2 / 4 CPU (`bash bench/run-sweep.sh` →
[`bench/sweep.json`](../bench/sweep.json); the dashboard draws it as line charts per runtime). The
**shape of the curve is the story** — it exposes how many cores a single process can actually use:

| Implementation | 0.5 | 1 | 2 | 4 | 0.5→max |
|----------------|:---:|:-:|:-:|:-:|:-------:|
| dotnet-clean | 618 | 1,532 | 6,033 | 6,904 | **11.2×** |
| dotnet-minimal | 564 | 1,835 | 5,732 | 6,815 | 12.1× |
| dotnet-mediatr | 624 | 1,590 | 5,871 | 6,759 | 10.8× |
| dotnet-vsa | 499 | 1,601 | 5,772 | 5,882 | 11.8× |
| dotnet-mvc | 511 | 1,388 | 5,368 | 6,202 | 12.1× |
| ts-express | 682 | 1,873 | 2,780 | 2,668 | 3.9× |
| python-django | 119 | 368 | 627 | 590 | 4.9× |
| python-flask | 512 | 1,400 | 1,998 | 1,968 | 3.8× |
| ts-nestjs | 695 | 1,751 | 2,123 | 2,154 | 3.1× |
| python-fastapi | 627 | 1,326 | 1,316 | 1,372 | 2.2× |

- **.NET scales near-linearly to 4 cores (~11–12×)** and pulls far ahead at scale (~6–7k req/s) —
  the ASP.NET Core thread pool genuinely uses every core. All five architectures scale *identically*,
  so once again architecture is perf-neutral; only the runtime's concurrency model matters.
- **Node plateaus after ~2 cores (~3–4×).** A single V8 event loop can overlap I/O (helped by libuv's
  thread pool up to ~2 cores) but can't parallelise CPU work in one process — to use 4 cores you'd run
  a cluster / multiple PM2 instances behind a load balancer.
- **FastAPI (async, single uvicorn worker) is flat after 1 core (2.2×).** All its gain is 0.5→1;
  beyond that a single async worker is pinned to one core. `uvicorn --workers N` would unlock the rest.
- **Flask/Django (sync, gunicorn) climb to ~2 cores then flatten** — bounded by the configured worker
  count, not the language. Django's absolute ceiling stays low (DRF overhead).

The headline: **at 0.5–1 CPU everyone is within ~3–4× of each other, but give them 4 cores and the
concurrency model dominates** — multi-threaded .NET pulls 2.5–5× ahead of the single-process runtimes.
Choosing a runtime for a multi-core box is really choosing a concurrency model (or committing to run
N worker processes).

### Workload shapes + latency (2 CPU)

Same budget, different request mix (`bash bench/run-profiles.sh` → [`bench/profiles.json`](../bench/profiles.json)).
Read/write/paginate are saturated (req/s); latency is a separate realistic-load run (30 VUs with
think-time) so p50/p95/p99 reflect responsiveness, not saturation.

| Implementation | read r/s | write r/s | paginate r/s | latency p50/p95/p99 |
|----------------|:--------:|:---------:|:------------:|:-------------------:|
| dotnet-minimal | 7,142 | 8,192 | 8,050 | 7 / 16 / 23 ms |
| dotnet-vsa | 8,342 | 7,493 | 8,120 | 7 / 15 / 19 ms |
| dotnet-clean | 6,338 | 6,064 | 5,837 | 6 / 16 / 20 ms |
| dotnet-mvc | 6,256 | 5,567 | 5,583 | 8 / 16 / 19 ms |
| ts-express | 2,846 | 2,993 | 2,996 | 12 / 29 / 34 ms |
| dotnet-mediatr | 5,631 | 2,863 | 2,827 | 6 / 14 / 17 ms |
| ts-nestjs | 2,350 | 2,123 | 2,378 | 7 / 19 / 29 ms |
| python-flask | 2,059 | 1,234 | 1,728 | 5 / 12 / 16 ms |
| python-fastapi | 1,414 | 1,113 | 1,358 | 6 / **117 / 131** ms |
| python-django | 609 | 534 | 547 | 6 / 11 / 18 ms |

- **FastAPI's p99 tail is ~131 ms under load** vs ~15–35 ms for everyone else — the single async worker's
  event-loop tail (requests queue behind one core). Its p50 (6 ms) is fine; the *tail* is the problem.
- **MediatR halves its own write throughput** (5,631 read → 2,863 write) — the validation/behaviour
  pipeline runs per command; the other .NET styles don't show that read/write gap.
- **Django has good latency but the lowest throughput** — at moderate offered load it responds fine
  (6/11/18 ms); it just can't push volume (DRF serialization + per-request overhead).
- Writes are broadly as cheap as reads for the fast runtimes; only MediatR and the Python trio pay a
  visible write penalty.

### Multi-worker — does adding processes unlock the cores?

FastAPI was the flat-liner in the scaling test (one uvicorn worker = one process on one core). Give
it more worker processes at a fixed 4-CPU budget (`bash bench/run-workers.sh` →
[`bench/workers.json`](../bench/workers.json)):

| uvicorn workers | 1 | 2 | 4 |
|-----------------|:-:|:-:|:-:|
| req/s | 1,275 | **1,835** | 1,305 |
| p99 | 144 ms | 84 ms | 143 ms |

**Workers help — but only up to a point.** Two workers lifted throughput ~44% and halved p99; four
workers **regressed** back to the single-worker level. This app is DB-bound (every request hits
Postgres via async SQLAlchemy), so past ~2 workers the extra processes oversubscribe the database
and context-switch more than they parallelise. The lesson isn't "add workers to scale" — it's "the
process/worker count is a knob you tune to the workload," and the multi-threaded-runtime advantage
(.NET here) is getting that parallelism *without* running and coordinating N separate processes.
*(Flask and Django already run 2 and 3 gunicorn workers by default — which is why they didn't flatten
as hard in the scaling test. Node/Express would need the `cluster` module for the same effect.)*

### Startup — cold start (app-isolated, median of 3)

Time from container start to first healthy response, with a fast DB healthcheck so it measures the
*app's* boot, not the healthcheck interval (`bench/coldstart.py`). Lower is better.

| ts-nestjs | python-flask | ts-express | dotnet (all 5) | python-fastapi | python-django |
|:---------:|:------------:|:----------:|:--------------:|:--------------:|:-------------:|
| 2.15 s | 2.27 s | 2.31 s | ~2.33 s | 2.53 s | 2.68 s |

Everything is within ~0.5 s. Notably **.NET cold-starts as fast as the scripting runtimes (~2.3 s)** —
the "JIT is slow to boot" intuition doesn't hold for modern .NET here. Django is slowest (migrations
run in its entrypoint). *(With the default 5 s Postgres healthcheck instead, every stack is ~6.3 s —
startup is gated by your healthcheck config, not the runtime.)*

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
