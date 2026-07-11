# Benchmarks

Load each implementation **identically** and collect the same metrics under the same
conditions, so axis-B (runtime/framework) comparisons are fair. This directory is the harness;
the results live in [`docs/comparison.md`](../docs/comparison.md).

> **Status: skeleton (Phase 2).** The k6 script below is ready; the measured runs are pending
> a host with Docker + [k6](https://k6.io) installed. Do not read numbers into an empty table.

## What we measure (axis B only)

Per implementation, against the same PostgreSQL and the same load profile:

- **Throughput** — requests/second at a fixed concurrency.
- **Latency** — p50 / p95 / p99 (report tails, not just the mean).
- **Resource use** — container RAM idle and under load, image size, cold-start time.

We do **not** benchmark the .NET architectures against each other for speed — same runtime,
so differences are GC/JIT noise. Those are compared on code shape (axis A). See the
[root README](../README.md).

## Methodology guardrails

1. **Isolate the variable.** Same contract, same DB engine/version, same host, same load
   profile — only the implementation changes. Give each the same CPU/memory limits.
2. **Warm up first.** Discard an initial warm-up window (JIT, connection-pool fill, page cache).
3. **Repeat.** Several runs per implementation; report the median, and note the spread.
4. **Realistic mix.** Reads dominate a typical API — weight the profile toward `GET` with a
   minority of writes (see the k6 script), rather than hammering one endpoint.
5. **Same client.** One load generator (k6) with identical options for every target.
6. **Record the environment.** Host, CPU, RAM, Docker version, image tags — alongside the numbers.

## Run (once wired)

```bash
# 1. boot a target implementation (example: dotnet-vsa)
cd ../implementations/dotnet-vsa && docker compose up --build -d && cd -

# 2. drive load at it
BASE_URL=http://localhost:8080 k6 run k6/articles.js

# 3. capture container resource use in another shell
docker stats --no-stream
```

Repeat for each implementation, then fill the axis-B table in `docs/comparison.md`.

## Files

- `k6/articles.js` — the shared load profile (auth once, then a read-heavy article mix).
