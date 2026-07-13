#!/usr/bin/env node
// Aggregate a CPU sweep into bench/sweep.json.
// Usage: node bench/aggregate-sweep.mjs <tmpdir> "<space-separated cpu levels>"
import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

const tmp = process.argv[2];
const levels = (process.argv[3] || '').trim().split(/\s+/).filter(Boolean).map(Number);
if (!tmp) {
  console.error('usage: node bench/aggregate-sweep.mjs <tmpdir> "<levels>"');
  process.exit(1);
}

const round1 = (n) => (n == null ? null : Math.round(n * 10) / 10);
function memToMb(raw) {
  const m = String(raw).trim().match(/^([\d.]+)\s*([KMG])i?B$/i);
  if (!m) return null;
  const value = parseFloat(m[1]);
  const unit = m[2].toUpperCase();
  const mb = unit === 'G' ? value * 1024 : unit === 'K' ? value / 1024 : value;
  return Math.round(mb);
}

const results = {};
for (const file of readdirSync(tmp)) {
  const match = file.match(/^(.+)__([\d.]+)\.json$/);
  if (!match) continue;
  const [, impl, cpu] = match;
  let summary;
  try {
    summary = JSON.parse(readFileSync(join(tmp, file), 'utf8'));
  } catch {
    continue;
  }
  const dur = summary.metrics?.http_req_duration ?? {};
  const reqs = summary.metrics?.http_reqs ?? {};
  let ramMb = null;
  try {
    ramMb = memToMb(readFileSync(join(tmp, `${impl}__${cpu}.mem`), 'utf8'));
  } catch { /* no mem */ }

  results[impl] ??= {};
  results[impl][cpu] = {
    rps: round1(reqs.rate),
    p95: round1(dur['p(95)']),
    p99: round1(dur['p(99)']),
    ramMb,
  };
}

console.log(JSON.stringify({
  ranAt: new Date().toISOString(),
  runner: process.env.BENCH_RUNNER || 'local CPU sweep — saturated, one container at a time',
  cpuLevels: levels,
  results,
}, null, 2));
