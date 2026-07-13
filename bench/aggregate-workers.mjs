#!/usr/bin/env node
// Aggregate the multi-worker sweep into bench/workers.json.
// Usage: node bench/aggregate-workers.mjs <tmpdir> "<worker levels>" <cpu>
import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

const tmp = process.argv[2];
const levels = (process.argv[3] || '').trim().split(/\s+/).filter(Boolean).map(Number);
const cpu = Number(process.argv[4] || 4);
if (!tmp) {
  console.error('usage: node bench/aggregate-workers.mjs <tmpdir> "<levels>" <cpu>');
  process.exit(1);
}

const round1 = (n) => (n == null ? null : Math.round(n * 10) / 10);
const results = {};
for (const file of readdirSync(tmp)) {
  const m = file.match(/^(.+)__(\d+)\.json$/);
  if (!m) continue;
  const [, impl, w] = m;
  let s;
  try { s = JSON.parse(readFileSync(join(tmp, file), 'utf8')); } catch { continue; }
  const dur = s.metrics?.http_req_duration ?? {};
  const reqs = s.metrics?.http_reqs ?? {};
  results[impl] ??= {};
  results[impl][w] = { rps: round1(reqs.rate), p95: round1(dur['p(95)']), p99: round1(dur['p(99)']) };
}

console.log(JSON.stringify({
  ranAt: new Date().toISOString(),
  cpu,
  workerLevels: levels,
  runner: `local · ${cpu} CPU · throughput vs worker processes (uvicorn --workers)`,
  results,
}, null, 2));
