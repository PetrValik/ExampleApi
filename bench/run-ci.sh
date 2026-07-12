#!/usr/bin/env bash
# Benchmark every implementation with the shared k6 profile and write bench/results.json.
# Boots each impl via its own docker compose, waits for health, runs k6, tears it down.
# Env: BENCH_WARMUP (8s), BENCH_STEADY (25s), BENCH_VUS (20). Requires docker + k6.
#
#   bash bench/run-ci.sh
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
IMPLS=(dotnet-vsa dotnet-minimal dotnet-mvc dotnet-clean dotnet-mediatr \
       python-fastapi python-django python-flask ts-express ts-nestjs)
export BENCH_WARMUP="${BENCH_WARMUP:-8s}"
export BENCH_STEADY="${BENCH_STEADY:-25s}"
export BENCH_VUS="${BENCH_VUS:-20}"
TMP="$(mktemp -d)"

for impl in "${IMPLS[@]}"; do
  echo "::group::bench $impl"
  dir="$ROOT/implementations/$impl"
  ( cd "$dir" && docker compose up -d --build ) || { echo "$impl: compose up failed"; echo "::endgroup::"; continue; }

  healthy=""
  for _ in $(seq 1 90); do
    curl -sf http://localhost:8080/health >/dev/null 2>&1 && { healthy=1; break; }
    sleep 3
  done

  if [ -n "$healthy" ]; then
    k6 run --summary-export="$TMP/$impl.json" \
      -e BASE_URL=http://localhost:8080 \
      -e BENCH_WARMUP="$BENCH_WARMUP" -e BENCH_STEADY="$BENCH_STEADY" -e BENCH_VUS="$BENCH_VUS" \
      "$ROOT/bench/k6/articles.js" || echo "$impl: k6 returned non-zero (thresholds), keeping summary"
    cid="$( cd "$dir" && docker compose ps -q api 2>/dev/null | head -1 )"
    img="$( docker inspect "$cid" --format '{{.Image}}' 2>/dev/null )"
    docker image inspect "$img" --format '{{.Size}}' > "$TMP/$impl.size" 2>/dev/null || echo "" > "$TMP/$impl.size"
  else
    echo "$impl: never became healthy — skipping"
  fi

  ( cd "$dir" && docker compose down -v ) >/dev/null 2>&1
  echo "::endgroup::"
done

node "$ROOT/bench/aggregate.mjs" "$TMP" > "$ROOT/bench/results.json"
echo "wrote bench/results.json:"
cat "$ROOT/bench/results.json"
