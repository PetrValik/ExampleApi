#!/usr/bin/env bash
# CPU sweep: measure every implementation across several CPU budgets to see how each SCALES.
# For each impl, build once, then for each CPU level bring it up under that cap, saturate it with
# k6, capture throughput/latency/memory, tear down. Sequential (one container at a time) so the
# measurements aren't contaminated. Writes bench/sweep.json.
#
# Env: BENCH_CPU_LEVELS="0.5 1 2 4"  BENCH_MEM=1g  BENCH_WARMUP=6s  BENCH_STEADY=20s  BENCH_VUS=64
#
#   bash bench/run-sweep.sh
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
IMPLS=(dotnet-vsa dotnet-minimal dotnet-mvc dotnet-clean dotnet-mediatr \
       python-fastapi python-django python-flask ts-express ts-nestjs)
# shellcheck disable=SC2206
LEVELS=(${BENCH_CPU_LEVELS:-0.5 1 2 4})
export BENCH_MEM="${BENCH_MEM:-1g}"
export BENCH_WARMUP="${BENCH_WARMUP:-6s}"
export BENCH_STEADY="${BENCH_STEADY:-20s}"
export BENCH_VUS="${BENCH_VUS:-64}"
OVERRIDE="$ROOT/bench/limits.override.yml"
TMP="$(mktemp -d)"

echo "CPU sweep over levels: ${LEVELS[*]}  (mem ${BENCH_MEM}, ${BENCH_VUS} VUs saturating, ${BENCH_STEADY} steady)"
echo

for impl in "${IMPLS[@]}"; do
  dir="$ROOT/implementations/$impl"
  built=""
  for cpu in "${LEVELS[@]}"; do
    echo "==================== $impl @ ${cpu} CPU ===================="
    export BENCH_CPUS="$cpu"
    build=""
    [ -z "$built" ] && build="--build"
    built=1
    # shellcheck disable=SC2086
    ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" up -d $build ) \
      || { echo "$impl @ $cpu: up failed"; continue; }

    healthy=""
    for _ in $(seq 1 90); do
      curl -sf http://localhost:8080/health >/dev/null 2>&1 && { healthy=1; break; }
      sleep 2
    done
    if [ -n "$healthy" ]; then
      k6 run --summary-export="$TMP/${impl}__${cpu}.json" \
        -e BASE_URL=http://localhost:8080 \
        -e BENCH_WARMUP="$BENCH_WARMUP" -e BENCH_STEADY="$BENCH_STEADY" \
        -e BENCH_VUS="$BENCH_VUS" -e BENCH_SLEEP=0 \
        "$ROOT/bench/k6/articles.js" || echo "$impl @ $cpu: k6 non-zero, keeping summary"
      cid="$( cd "$dir" && docker compose ps -q api 2>/dev/null | head -1 )"
      docker stats --no-stream --format '{{.MemUsage}}' "$cid" 2>/dev/null | awk '{print $1}' > "$TMP/${impl}__${cpu}.mem" || true
    else
      echo "$impl @ $cpu: never became healthy"
    fi

    ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" down -v ) >/dev/null 2>&1
  done
  echo
done

node "$ROOT/bench/aggregate-sweep.mjs" "$TMP" "${LEVELS[*]}" > "$ROOT/bench/sweep.json"
echo "=== wrote bench/sweep.json ==="
cat "$ROOT/bench/sweep.json"
