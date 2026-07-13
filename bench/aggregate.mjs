#!/usr/bin/env node
// Aggregate per-implementation k6 summary exports (+ image sizes) into bench/results.json.
// Usage: node bench/aggregate.mjs <tmpdir>   (prints JSON to stdout)
import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

const tmp = process.argv[2];
if (!tmp) {
  console.error('usage: node bench/aggregate.mjs <tmpdir>');
  process.exit(1);
}

const round1 = (n) => (n == null ? null : Math.round(n * 10) / 10);

// Parse a docker-stats memory string ("234.5MiB", "1.05GiB", "512KiB") into whole MB.
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
  if (!file.endsWith('.json')) continue;
  const impl = file.replace(/\.json$/, '');
  let summary;
  try {
    summary = JSON.parse(readFileSync(join(tmp, file), 'utf8'));
  } catch {
    continue;
  }
  const dur = summary.metrics?.http_req_duration ?? {};
  const reqs = summary.metrics?.http_reqs ?? {};

  let imageMb = null;
  try {
    const bytes = parseInt(readFileSync(join(tmp, `${impl}.size`), 'utf8').trim(), 10);
    if (Number.isFinite(bytes) && bytes > 0) imageMb = Math.round(bytes / 1024 / 1024);
  } catch { /* no size captured */ }

  let ramMb = null;
  try {
    ramMb = memToMb(readFileSync(join(tmp, `${impl}.mem`), 'utf8'));
  } catch { /* no memory captured */ }

  results[impl] = {
    rps: round1(reqs.rate),
    p95: round1(dur['p(95)']),
    p99: round1(dur['p(99)']),
    ramMb,
    imageMb,
  };
}

const out = {
  ranAt: new Date().toISOString(),
  runner: process.env.BENCH_RUNNER
    || 'github-actions ubuntu-latest (2 vCPU) — indicative, relative comparison only',
  results,
};
console.log(JSON.stringify(out, null, 2));
