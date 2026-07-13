// Shared load profile for the Example API showcase (axis-B runtime comparison).
//
// Identical for every implementation — only BASE_URL changes. Read-heavy mix
// (list + get dominate; a minority of writes), a warm-up ramp, and tail-latency
// thresholds. Requires k6 (https://k6.io) and a running implementation.
//
//   BASE_URL=http://localhost:8080 k6 run k6/articles.js
import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
const CREDENTIALS = JSON.stringify({ username: 'admin', password: 'admin' });
const JSON_HEADERS = { 'Content-Type': 'application/json' };

// Duration/VUs are env-overridable so CI can run a shorter measurement window than a
// full local run: BENCH_WARMUP (default 15s), BENCH_STEADY (default 60s), BENCH_VUS (default 20).
const WARMUP = __ENV.BENCH_WARMUP || '15s';
const STEADY = __ENV.BENCH_STEADY || '60s';
const VUS = Number(__ENV.BENCH_VUS || 20);
// Per-iteration think-time. Keep a small default for a "realistic load" run; set BENCH_SLEEP=0 to
// SATURATE the server (max-throughput mode) — useful when the container's CPU is capped and you
// want requests/sec-per-CPU rather than latency under a modest offered load.
const SLEEP = Number(__ENV.BENCH_SLEEP != null && __ENV.BENCH_SLEEP !== '' ? __ENV.BENCH_SLEEP : 0.1);
// Request mix: 'mixed' (70% list / 20% get / 10% create), 'read' (all list), 'write' (all create),
// 'paginate' (list across pages). Lets one profile stress a specific code path.
const WORKLOAD = __ENV.BENCH_WORKLOAD || 'mixed';

export const options = {
  scenarios: {
    articles: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: WARMUP, target: VUS }, // warm-up ramp (discarded in analysis)
        { duration: STEADY, target: VUS }, // steady state — the measurement window
        { duration: '5s', target: 0 },     // ramp down
      ],
      gracefulRampDown: '5s',
    },
  },
  // Report p99 in the summary; thresholds are advisory (a slow impl shouldn't fail the bench job).
  summaryTrendStats: ['avg', 'med', 'p(95)', 'p(99)', 'max'],
  thresholds: {
    http_req_failed: ['rate<0.05'],
  },
};

// One token for the whole test; handed to every VU.
export function setup() {
  const res = http.post(`${BASE_URL}/auth/token`, CREDENTIALS, { headers: JSON_HEADERS });
  check(res, { 'token issued': (r) => r.status === 200 });
  return { token: res.json('token') };
}

function doList(h) {
  const res = http.get(`${BASE_URL}/api/articles?pageSize=20`, { headers: h });
  check(res, { 'list 200': (r) => r.status === 200 });
}
function doGet(h) {
  // id=1 may 404 on an empty DB; both are valid, non-error responses.
  const res = http.get(`${BASE_URL}/api/articles/1`, { headers: h });
  check(res, { 'get 200/404': (r) => r.status === 200 || r.status === 404 });
}
function doCreate(h) {
  const body = JSON.stringify({
    name: `bench-${__VU}-${__ITER}`, description: 'benchmark article',
    category: 'bench', price: 9.99, currency: 'USD',
  });
  const res = http.post(`${BASE_URL}/api/articles`, body, { headers: h });
  check(res, { 'create 201': (r) => r.status === 201 });
}
function doPaginate(h) {
  const page = (__ITER % 10) + 1;
  const res = http.get(`${BASE_URL}/api/articles?page=${page}&pageSize=10`, { headers: h });
  check(res, { 'paginate 200': (r) => r.status === 200 });
}

export default function (data) {
  // Object.assign, not object spread — k6's JS compiler doesn't parse `{ ...obj }`.
  const authHeaders = Object.assign({}, JSON_HEADERS, { Authorization: `Bearer ${data.token}` });

  if (WORKLOAD === 'read') {
    doList(authHeaders);
  } else if (WORKLOAD === 'write') {
    doCreate(authHeaders);
  } else if (WORKLOAD === 'paginate') {
    doPaginate(authHeaders);
  } else {
    const roll = Math.random();
    if (roll < 0.7) doList(authHeaders);
    else if (roll < 0.9) doGet(authHeaders);
    else doCreate(authHeaders);
  }

  if (SLEEP > 0) sleep(SLEEP);
}
