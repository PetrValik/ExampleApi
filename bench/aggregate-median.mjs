#!/usr/bin/env node
// Aggregate repeated resource-fair runs into bench/results.json, taking the MEDIAN per metric.
// Reads <tmpdir>/<impl>__r<N>.json (k6 summaries), <impl>__r<N>.mem, <impl>.size.
// Usage: node bench/aggregate-median.mjs <tmpdir>
import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

const tmp = process.argv[2];
if (!tmp) {
  console.error('usage: node bench/aggregate-median.mjs <tmpdir>');
  process.exit(1);
}

const round1 = (n) => (n == null ? null : Math.round(n * 10) / 10);
const median = (xs) => {
  const a = xs.filter((x) => x != null).sort((x, y) => x - y);
  if (!a.length) return null;
  const m = Math.floor(a.length / 2);
  return a.length % 2 ? a[m] : (a[m - 1] + a[m]) / 2;
};
function memToMb(raw) {
  const m = String(raw).trim().match(/^([\d.]+)\s*([KMG])i?B$/i);
  if (!m) return null;
  const v = parseFloat(m[1]); const u = m[2].toUpperCase();
  return Math.round(u === 'G' ? v * 1024 : u === 'K' ? v / 1024 : v);
}

// Collect per-impl arrays across runs.
const runs = {};
for (const file of readdirSync(tmp)) {
  const m = file.match(/^(.+)__r(\d+)\.json$/);
  if (!m) continue;
  const [, impl] = m;
  let s;
  try { s = JSON.parse(readFileSync(join(tmp, file), 'utf8')); } catch { continue; }
  const dur = s.metrics?.http_req_duration ?? {};
  const reqs = s.metrics?.http_reqs ?? {};
  (runs[impl] ??= { rps: [], p95: [], p99: [], ram: [] });
  runs[impl].rps.push(reqs.rate ?? null);
  runs[impl].p95.push(dur['p(95)'] ?? null);
  runs[impl].p99.push(dur['p(99)'] ?? null);
  const memFile = file.replace(/\.json$/, '.mem');
  try { runs[impl].ram.push(memToMb(readFileSync(join(tmp, memFile), 'utf8'))); } catch { /* none */ }
}

const results = {};
for (const [impl, r] of Object.entries(runs)) {
  let imageMb = null;
  try {
    const bytes = parseInt(readFileSync(join(tmp, `${impl}.size`), 'utf8').trim(), 10);
    if (Number.isFinite(bytes) && bytes > 0) imageMb = Math.round(bytes / 1024 / 1024);
  } catch { /* none */ }
  results[impl] = {
    rps: round1(median(r.rps)),
    p95: round1(median(r.p95)),
    p99: round1(median(r.p99)),
    ramMb: median(r.ram),
    imageMb,
    runs: r.rps.length,
  };
}

console.log(JSON.stringify({
  ranAt: new Date().toISOString(),
  runner: process.env.BENCH_RUNNER || 'local · median of repeats · saturated',
  results,
}, null, 2));
