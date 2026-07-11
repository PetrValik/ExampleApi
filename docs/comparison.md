# Comparison

Numbers only go in these tables for an implementation that **passes the conformance suite** —
otherwise the comparison isn't apples-to-apples. The two axes are reported separately
(see the [root README](../README.md) for why mixing them misleads).

---

## Axis A — same runtime, different architecture

All .NET implementations run on the same ASP.NET Core runtime and the same PostgreSQL, so
runtime throughput is essentially equal between them. The interesting differences are in the
**shape of the code** — how much structure/ceremony each style needs to deliver the identical
behaviour.

Source metrics exclude tests, EF migrations (generated) and build output. Produced by
[`scripts/metrics.sh`](../scripts/metrics.sh).

| Implementation | Style | Src files | Src LOC | Non-blank | Test files | Test LOC |
|----------------|-------|:---------:|:-------:|:---------:|:----------:|:--------:|
| **dotnet-vsa** | Vertical Slice (hand-rolled handlers + exceptions) | 48 | 1816 | 1589 | 24 | 3881 |
| dotnet-mvc | Controllers / Services / Repositories | _pending_ | | | | |
| dotnet-clean | Clean Architecture (4 layers) | _pending_ | | | | |
| dotnet-mediatr | Vertical Slice + MediatR + Result<T> | _pending_ | | | | |

Planned qualitative columns as siblings land: number of files touched to add one endpoint,
cross-feature coupling (can you delete a feature by deleting a folder?), and where the
business logic lives.

> **Reading axis A:** a lower LOC/file count is not automatically "better" — MediatR/Clean
> trade more files for stronger boundaries and testability. The table exists to make that
> trade **visible**, not to crown a winner.

---

## Axis B — different runtime / framework

Here the runtime is the variable, so throughput and resource use are the point. All
implementations serve the same contract, backed by the same PostgreSQL, under the same load
profile from [`bench/`](../bench). **Pending Phase 2** (needs the harness wired + a host with
Docker).

| Implementation | Runtime | RPS | p50 | p95 | p99 | RAM (idle/load) | Image size | Cold start |
|----------------|---------|:---:|:---:|:---:|:---:|:---------------:|:----------:|:----------:|
| dotnet-vsa | .NET 10 | _pending_ | | | | | | |
| python-fastapi | Python (uvicorn) | _pending_ | | | | | | |
| python-django | Python (gunicorn) | _pending_ | | | | | | |
| ts-express | Node 24 | _pending_ | | | | | | |

Methodology (warm-up, repeated runs, controlled variables, what each metric means) lives in
[`bench/README.md`](../bench/README.md). When the numbers exist, a shareable visual summary
(chart) can be generated from this table.

---

## Methodology guardrails

- **Same contract, same DB, same load.** The only intended variable is the approach/runtime.
- **Conformance first.** No numbers for an implementation that hasn't proven parity.
- **Warm before measuring; measure repeatedly.** Report medians across runs, not a single hot run.
- **Separate the axes.** Never rank a .NET architecture by RPS, or a runtime by file count.
