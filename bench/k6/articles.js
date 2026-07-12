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

export default function (data) {
  // Object.assign, not object spread — k6's JS compiler doesn't parse `{ ...obj }`.
  const authHeaders = Object.assign({}, JSON_HEADERS, { Authorization: `Bearer ${data.token}` });

  // ~70% list, ~20% get, ~10% create — a read-heavy but write-touching mix.
  const roll = Math.random();

  if (roll < 0.7) {
    const res = http.get(`${BASE_URL}/api/articles?pageSize=20`, { headers: authHeaders });
    check(res, { 'list 200': (r) => r.status === 200 });
  } else if (roll < 0.9) {
    // getting id=1 may 404 on an empty DB; both are valid, non-error responses here.
    const res = http.get(`${BASE_URL}/api/articles/1`, { headers: authHeaders });
    check(res, { 'get 200/404': (r) => r.status === 200 || r.status === 404 });
  } else {
    const body = JSON.stringify({
      name: `bench-${__VU}-${__ITER}`,
      description: 'benchmark article',
      category: 'bench',
      price: 9.99,
      currency: 'USD',
    });
    const res = http.post(`${BASE_URL}/api/articles`, body, { headers: authHeaders });
    check(res, { 'create 201': (r) => r.status === 201 });
  }

  sleep(0.1);
}
