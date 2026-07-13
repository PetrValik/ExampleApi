#!/usr/bin/env bash
# Latency + workload-shape profiles. Per impl, at a fixed CPU budget, run four k6 profiles, each
# against a FRESH database (down -v between) so a write-heavy run can't pollute the read runs:
#   latency  — realistic mixed load with think-time (not saturated) → p50/p95/p99 responsiveness
#   read     — 100% list, saturated       → read throughput
#   write    — 100% create, saturated     → write throughput (ORM/serializer cost)
#   paginate — list across pages, saturated → pagination path
# Sequential (one container at a time). Writes bench/profiles.json.
#
# Env: BENCH_CPUS=2  BENCH_MEM=1g  BENCH_WARMUP=5s  BENCH_STEADY=20s
#
#   bash bench/run-profiles.sh
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
IMPLS=(dotnet-vsa dotnet-minimal dotnet-mvc dotnet-clean dotnet-mediatr \
       python-fastapi python-django python-flask ts-express ts-nestjs)
# profile = name:workload:sleep:vus
PROFILES=("latency:mixed:0.1:30" "read:read:0:50" "write:write:0:50" "paginate:paginate:0:50")
export BENCH_CPUS="${BENCH_CPUS:-2}"
export BENCH_MEM="${BENCH_MEM:-1g}"
export BENCH_WARMUP="${BENCH_WARMUP:-5s}"
export BENCH_STEADY="${BENCH_STEADY:-20s}"
OVERRIDE="$ROOT/bench/limits.override.yml"
TMP="$(mktemp -d)"

echo "Profiles at ${BENCH_CPUS} CPU / ${BENCH_MEM}: latency(realistic) + read/write/paginate(saturated)"
echo

for impl in "${IMPLS[@]}"; do
  dir="$ROOT/implementations/$impl"
  ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" build ) >/dev/null 2>&1
  for profile in "${PROFILES[@]}"; do
    IFS=':' read -r name workload sleep vus <<< "$profile"
    echo "==================== $impl · $name ===================="
    ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" down -v ) >/dev/null 2>&1
    ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" up -d ) || { echo "up failed"; continue; }
    healthy=""
    for _ in $(seq 1 90); do curl -sf http://localhost:8080/health >/dev/null 2>&1 && { healthy=1; break; }; sleep 2; done
    if [ -n "$healthy" ]; then
      k6 run --summary-export="$TMP/${impl}__${name}.json" \
        -e BASE_URL=http://localhost:8080 \
        -e BENCH_WARMUP="$BENCH_WARMUP" -e BENCH_STEADY="$BENCH_STEADY" \
        -e BENCH_VUS="$vus" -e BENCH_SLEEP="$sleep" -e BENCH_WORKLOAD="$workload" \
        "$ROOT/bench/k6/articles.js" || echo "$impl $name: k6 non-zero, keeping summary"
    else
      echo "$impl $name: never healthy"
    fi
    ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" down -v ) >/dev/null 2>&1
  done
  echo
done

BENCH_CPUS="$BENCH_CPUS" node "$ROOT/bench/aggregate-profiles.mjs" "$TMP" > "$ROOT/bench/profiles.json"
echo "=== wrote bench/profiles.json ==="
cat "$ROOT/bench/profiles.json"
