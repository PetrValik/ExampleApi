#!/usr/bin/env bash
# Resource-fair local benchmark: run every implementation ONE AT A TIME under the SAME CPU/memory
# budget, load-test it with the shared k6 profile, capture throughput + latency + memory, tear down.
# Throughput then reflects efficiency per unit of resource rather than who grabbed more cores.
#
# Env (with defaults):
#   BENCH_CPUS=1.0  BENCH_MEM=1g  BENCH_WARMUP=8s  BENCH_STEADY=30s  BENCH_VUS=20
#
#   bash bench/run-local.sh
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
IMPLS=(dotnet-vsa dotnet-minimal dotnet-mvc dotnet-clean dotnet-mediatr \
       python-fastapi python-django python-flask ts-express ts-nestjs)
export BENCH_CPUS="${BENCH_CPUS:-1.0}"
export BENCH_MEM="${BENCH_MEM:-1g}"
export BENCH_WARMUP="${BENCH_WARMUP:-8s}"
export BENCH_STEADY="${BENCH_STEADY:-30s}"
export BENCH_VUS="${BENCH_VUS:-50}"     # enough to saturate a 1-CPU container
export BENCH_SLEEP="${BENCH_SLEEP:-0}"  # no think-time → measure max throughput per CPU
OVERRIDE="$ROOT/bench/limits.override.yml"
TMP="$(mktemp -d)"

echo "Resource budget per implementation: ${BENCH_CPUS} CPU / ${BENCH_MEM} memory"
echo "Load: ${BENCH_VUS} VUs, ${BENCH_WARMUP} warm-up + ${BENCH_STEADY} steady"
echo

for impl in "${IMPLS[@]}"; do
  echo "==================== $impl ===================="
  dir="$ROOT/implementations/$impl"
  ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" up -d --build ) \
    || { echo "$impl: compose up failed, skipping"; continue; }

  healthy=""
  for _ in $(seq 1 90); do
    curl -sf http://localhost:8080/health >/dev/null 2>&1 && { healthy=1; break; }
    sleep 2
  done
  if [ -z "$healthy" ]; then
    echo "$impl: never became healthy — skipping"
    ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" down -v ) >/dev/null 2>&1
    continue
  fi

  k6 run --summary-export="$TMP/$impl.json" \
    -e BASE_URL=http://localhost:8080 \
    -e BENCH_WARMUP="$BENCH_WARMUP" -e BENCH_STEADY="$BENCH_STEADY" \
    -e BENCH_VUS="$BENCH_VUS" -e BENCH_SLEEP="$BENCH_SLEEP" \
    "$ROOT/bench/k6/articles.js" || echo "$impl: k6 non-zero (threshold), keeping summary"

  # Container still up → capture settled-under-load memory + image size before teardown.
  cid="$( cd "$dir" && docker compose ps -q api 2>/dev/null | head -1 )"
  docker stats --no-stream --format '{{.MemUsage}}' "$cid" 2>/dev/null | awk '{print $1}' > "$TMP/$impl.mem" || true
  img="$( docker inspect "$cid" --format '{{.Image}}' 2>/dev/null )"
  docker image inspect "$img" --format '{{.Size}}' > "$TMP/$impl.size" 2>/dev/null || echo "" > "$TMP/$impl.size"

  ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" down -v ) >/dev/null 2>&1
  echo
done

BENCH_RUNNER="local · ${BENCH_CPUS} CPU / ${BENCH_MEM} per container (resource-fair)" \
  node "$ROOT/bench/aggregate.mjs" "$TMP" > "$ROOT/bench/results.json"
echo "=== wrote bench/results.json ==="
cat "$ROOT/bench/results.json"
