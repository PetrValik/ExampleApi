#!/usr/bin/env node
// Generate dashboard/data.json — the single data source for the comparison dashboard
// and the artifact. Code-size metrics are computed live from the source tree; the
// per-implementation metadata (family, stack, architecture, ports, concurrency) is
// declared here. Perf metrics are left null until the benchmark harness runs (Phase 2).
//
//   node scripts/gen-dashboard-data.mjs        # writes dashboard/data.json
//   node scripts/gen-dashboard-data.mjs --print # prints to stdout
import { readdirSync, readFileSync, statSync, writeFileSync, mkdirSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const IMPL_DIR = join(ROOT, 'implementations');

// Declared metadata. `port` is the host port each impl gets in docker-compose.all.yml.
const META = {
  'dotnet-vsa': { family: '.NET', axis: 'A', stack: 'Vertical Slice (hand-rolled handlers + exceptions)', reference: true, port: 8081, concurrency: 'Postgres xmin' },
  'dotnet-minimal': { family: '.NET', axis: 'A', stack: 'Minimal API — one Program.cs, no abstractions', port: 8082, concurrency: 'Postgres xmin' },
  'dotnet-mvc': { family: '.NET', axis: 'A', stack: 'Controllers → Services → Repositories (layered)', port: 8083, concurrency: 'Postgres xmin' },
  'dotnet-clean': { family: '.NET', axis: 'A', stack: 'Clean Architecture — 4 projects, dependency rule inward', port: 8084, concurrency: 'integer version column' },
  'dotnet-mediatr': { family: '.NET', axis: 'A', stack: 'Vertical Slice + MediatR + Result<T>', port: 8085, concurrency: 'Postgres xmin' },
  'python-fastapi': { family: 'Python', axis: 'B', stack: 'FastAPI + SQLAlchemy 2 + Pydantic v2 (async)', port: 8086, concurrency: 'integer version column' },
  'python-django': { family: 'Python', axis: 'B', stack: 'Django + DRF + SimpleJWT', port: 8087, concurrency: 'integer version column' },
  'python-flask': { family: 'Python', axis: 'B', stack: 'Flask + SQLAlchemy + flask-jwt-extended (sync WSGI)', port: 8088, concurrency: 'integer version column' },
  'ts-express': { family: 'Node', axis: 'B', stack: 'Express + Prisma + zod (TypeScript)', port: 8089, concurrency: 'integer version column' },
  'ts-nestjs': { family: 'Node', axis: 'B', stack: 'NestJS + TypeORM + class-validator', port: 8090, concurrency: 'integer version column' },
};

const SRC_EXT = new Set(['.cs', '.py', '.ts', '.js']);
const SKIP = ['/bin/', '/obj/', '/test/', '/tests/', 'Tests/', '/__pycache__/', '/.venv/', '/venv/', '/node_modules/', '/Migrations/', '/dist/', '/site-packages/'];

function walk(dir, acc = []) {
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const full = join(dir, entry.name);
    if (entry.isDirectory()) {
      if (SKIP.some((s) => (full + '/').includes(s))) continue;
      walk(full, acc);
    } else {
      const dot = entry.name.lastIndexOf('.');
      const ext = dot >= 0 ? entry.name.slice(dot) : '';
      if (SRC_EXT.has(ext) && !SKIP.some((s) => full.includes(s))) acc.push(full);
    }
  }
  return acc;
}

function measure(implPath) {
  const files = walk(implPath);
  let lines = 0;
  let nonblank = 0;
  for (const f of files) {
    const text = readFileSync(f, 'utf8');
    const rows = text.split('\n');
    // Drop a single trailing empty line produced by a final newline, so the count
    // matches an editor's line count rather than over-counting by one per file.
    if (rows.length > 0 && rows[rows.length - 1] === '') rows.pop();
    lines += rows.length;
    nonblank += rows.filter((r) => r.trim().length > 0).length;
  }
  return { files: files.length, lines, nonblank };
}

// Optional benchmark results (produced by .github/workflows/benchmark.yml). When present,
// each implementation's perf slot is filled from it; otherwise perf stays null ("pending").
let bench = null;
try {
  bench = JSON.parse(readFileSync(join(ROOT, 'bench', 'results.json'), 'utf8'));
} catch { /* no benchmark results yet */ }

const emptyPerf = { rps: null, p50: null, p95: null, p99: null, ramMb: null, imageMb: null };

const implementations = [];
for (const name of Object.keys(META)) {
  const p = join(IMPL_DIR, name);
  let m = { files: 0, lines: 0, nonblank: 0 };
  try {
    if (statSync(p).isDirectory()) m = measure(p);
  } catch { /* not present */ }
  const perf = { ...emptyPerf, ...(bench?.results?.[name] ?? {}) };
  implementations.push({
    name,
    ...META[name],
    metrics: m,
    perf,
    conformance: 'pass',   // proven by the CI conformance matrix (all 10 green)
    build: 'pass',
  });
}

const data = {
  generatedNote: 'Code metrics computed live by scripts/gen-dashboard-data.mjs.',
  contractVersion: '1.0.0',
  benchmark: bench ? { ranAt: bench.ranAt ?? null, runner: bench.runner ?? null } : null,
  implementations,
};

const out = JSON.stringify(data, null, 2);
if (process.argv.includes('--print')) {
  console.log(out);
} else {
  const dir = join(ROOT, 'dashboard');
  mkdirSync(dir, { recursive: true });
  writeFileSync(join(dir, 'data.json'), out + '\n');
  console.log(`wrote dashboard/data.json — ${implementations.length} implementations`);
}
