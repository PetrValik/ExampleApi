#!/usr/bin/env bash
# Resource-fair throughput, but repeated for robustness: each impl is measured REPEATS times at
# 1 CPU / 1 GB saturated, and the MEDIAN (rps/p95/p99/memory) is written to bench/results.json.
# Single-run numbers are noisy; the median is the trustworthy replacement.
#
# Env: BENCH_CPUS=1.0  BENCH_MEM=1g  BENCH_WARMUP=6s  BENCH_STEADY=20s  BENCH_VUS=50  REPEATS=3
#
#   bash bench/run-median.sh
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
IMPLS=(dotnet-vsa dotnet-minimal dotnet-mvc dotnet-clean dotnet-mediatr \
       python-fastapi python-django python-flask ts-express ts-nestjs)
export BENCH_CPUS="${BENCH_CPUS:-1.0}"
export BENCH_MEM="${BENCH_MEM:-1g}"
export BENCH_WARMUP="${BENCH_WARMUP:-6s}"
export BENCH_STEADY="${BENCH_STEADY:-20s}"
export BENCH_VUS="${BENCH_VUS:-50}"
REPEATS="${REPEATS:-3}"
OVERRIDE="$ROOT/bench/limits.override.yml"
TMP="$(mktemp -d)"

echo "Median of ${REPEATS} · ${BENCH_CPUS} CPU / ${BENCH_MEM} · saturated"
echo

for impl in "${IMPLS[@]}"; do
  dir="$ROOT/implementations/$impl"
  ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" build ) >/dev/null 2>&1
  for r in $(seq 1 "$REPEATS"); do
    echo "==================== $impl · run $r/$REPEATS ===================="
    ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" down -v ) >/dev/null 2>&1
    ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" up -d ) || { echo "up failed"; continue; }
    healthy=""
    for _ in $(seq 1 90); do curl -sf http://localhost:8080/health >/dev/null 2>&1 && { healthy=1; break; }; sleep 2; done
    if [ -n "$healthy" ]; then
      k6 run --summary-export="$TMP/${impl}__r${r}.json" \
        -e BASE_URL=http://localhost:8080 -e BENCH_WARMUP="$BENCH_WARMUP" -e BENCH_STEADY="$BENCH_STEADY" \
        -e BENCH_VUS="$BENCH_VUS" -e BENCH_SLEEP=0 "$ROOT/bench/k6/articles.js" \
        || echo "$impl r$r: k6 non-zero, keeping summary"
      cid="$( cd "$dir" && docker compose ps -q api 2>/dev/null | head -1 )"
      docker stats --no-stream --format '{{.MemUsage}}' "$cid" 2>/dev/null | awk '{print $1}' > "$TMP/${impl}__r${r}.mem" || true
      [ "$r" = "1" ] && docker inspect "$cid" --format '{{.Image}}' 2>/dev/null | xargs -I{} docker image inspect {} --format '{{.Size}}' > "$TMP/${impl}.size" 2>/dev/null
    else echo "$impl r$r: never healthy"; fi
    ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" down -v ) >/dev/null 2>&1
  done
  echo
done

BENCH_RUNNER="local · ${BENCH_CPUS} CPU / ${BENCH_MEM} · median of ${REPEATS}, saturated" \
  node "$ROOT/bench/aggregate-median.mjs" "$TMP" > "$ROOT/bench/results.json"
echo "=== wrote bench/results.json (median of ${REPEATS}) ==="
cat "$ROOT/bench/results.json"
