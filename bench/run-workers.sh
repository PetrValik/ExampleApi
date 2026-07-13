#!/usr/bin/env bash
# Multi-worker demo: FastAPI was flat past 1 CPU in the sweep because a single uvicorn worker is
# one process on one core. Give it N worker processes (at a fixed multi-core budget) and it should
# climb — proving the flatness was the process model, not the runtime. We override only the
# container COMMAND (the conformant image is untouched); FastAPI's _init_db retries, so concurrent
# workers racing on create_all is safe. Writes bench/workers.json.
#
# Env: BENCH_CPUS=4  BENCH_MEM=1g  BENCH_WARMUP=5s  BENCH_STEADY=20s  BENCH_VUS=64  WORKER_LEVELS="1 2 4"
#
#   bash bench/run-workers.sh
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
IMPL="python-fastapi"
dir="$ROOT/implementations/$IMPL"
# shellcheck disable=SC2206
WORKERS=(${WORKER_LEVELS:-1 2 4})
export BENCH_CPUS="${BENCH_CPUS:-4}"
export BENCH_MEM="${BENCH_MEM:-1g}"
export BENCH_WARMUP="${BENCH_WARMUP:-5s}"
export BENCH_STEADY="${BENCH_STEADY:-20s}"
export BENCH_VUS="${BENCH_VUS:-64}"
OVERRIDE="$ROOT/bench/limits.override.yml"
TMP="$(mktemp -d)"

( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" build ) >/dev/null 2>&1

for w in "${WORKERS[@]}"; do
  echo "==================== $IMPL · ${w} worker(s) @ ${BENCH_CPUS} CPU ===================="
  cmd_override="$(mktemp).yml"
  cat > "$cmd_override" <<YAML
services:
  api:
    command: ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8080", "--workers", "${w}"]
YAML
  ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" -f "$cmd_override" down -v ) >/dev/null 2>&1
  ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" -f "$cmd_override" up -d ) || { echo "up failed"; rm -f "$cmd_override"; continue; }
  healthy=""
  for _ in $(seq 1 90); do curl -sf http://localhost:8080/health >/dev/null 2>&1 && { healthy=1; break; }; sleep 2; done
  if [ -n "$healthy" ]; then
    k6 run --summary-export="$TMP/${IMPL}__${w}.json" \
      -e BASE_URL=http://localhost:8080 -e BENCH_WARMUP="$BENCH_WARMUP" -e BENCH_STEADY="$BENCH_STEADY" \
      -e BENCH_VUS="$BENCH_VUS" -e BENCH_SLEEP=0 "$ROOT/bench/k6/articles.js" || echo "$IMPL w$w: k6 non-zero"
  else
    echo "$IMPL w$w: never healthy"
  fi
  ( cd "$dir" && docker compose -f docker-compose.yml -f "$OVERRIDE" -f "$cmd_override" down -v ) >/dev/null 2>&1
  rm -f "$cmd_override"
done

node "$ROOT/bench/aggregate-workers.mjs" "$TMP" "${WORKERS[*]}" "$BENCH_CPUS" > "$ROOT/bench/workers.json"
echo "=== wrote bench/workers.json ==="
cat "$ROOT/bench/workers.json"
