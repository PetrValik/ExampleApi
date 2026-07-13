#!/usr/bin/env node
// Aggregate the latency + workload-shape profiles into bench/profiles.json.
// Usage: node bench/aggregate-profiles.mjs <tmpdir>
import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

const tmp = process.argv[2];
if (!tmp) {
  console.error('usage: node bench/aggregate-profiles.mjs <tmpdir>');
  process.exit(1);
}

const round1 = (n) => (n == null ? null : Math.round(n * 10) / 10);
const results = {};

for (const file of readdirSync(tmp)) {
  const match = file.match(/^(.+)__(latency|read|write|paginate)\.json$/);
  if (!match) continue;
  const [, impl, profile] = match;
  let summary;
  try {
    summary = JSON.parse(readFileSync(join(tmp, file), 'utf8'));
  } catch {
    continue;
  }
  const dur = summary.metrics?.http_req_duration ?? {};
  const reqs = summary.metrics?.http_reqs ?? {};
  results[impl] ??= {};
  results[impl][profile] = {
    rps: round1(reqs.rate),
    p50: round1(dur.med),
    p95: round1(dur['p(95)']),
    p99: round1(dur['p(99)']),
  };
}

console.log(JSON.stringify({
  ranAt: new Date().toISOString(),
  cpu: Number(process.env.BENCH_CPUS || 2),
  runner: `local · ${process.env.BENCH_CPUS || 2} CPU · latency=realistic(think-time), read/write/paginate=saturated`,
  results,
}, null, 2));
