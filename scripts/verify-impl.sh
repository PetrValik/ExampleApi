#!/usr/bin/env bash
# Verify one implementation end to end: boot it via its docker-compose, wait for health,
# run the conformance suite against it, then tear it down.
#
# Usage: ./scripts/verify-impl.sh <impl-dir> [BASE_URL]
#   e.g. ./scripts/verify-impl.sh implementations/dotnet-vsa
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
IMPL="${1:?usage: verify-impl.sh <impl-dir> [BASE_URL]}"
BASE_URL="${2:-${BASE_URL:-http://localhost:8080}}"
COMPOSE_DIR="$ROOT/$IMPL"

if [[ ! -f "$COMPOSE_DIR/docker-compose.yml" ]]; then
  echo "No docker-compose.yml under $IMPL" >&2
  exit 2
fi

if ! docker info >/dev/null 2>&1; then
  echo "Docker daemon not available — cannot run live verification." >&2
  exit 2
fi

cleanup() { (cd "$COMPOSE_DIR" && docker compose down -v >/dev/null 2>&1) || true; }
trap cleanup EXIT

echo "==> Booting $IMPL"
(cd "$COMPOSE_DIR" && docker compose up -d --build)

echo "==> Waiting for $BASE_URL/health"
for attempt in $(seq 1 60); do
  if curl -sf "$BASE_URL/health" >/dev/null 2>&1; then
    echo "    healthy after ${attempt}x2s"
    break
  fi
  sleep 2
  if [[ "$attempt" -eq 60 ]]; then
    echo "    timed out waiting for health" >&2
    exit 1
  fi
done

echo "==> Running conformance"
BASE_URL="$BASE_URL" "$ROOT/conformance/run.sh" "$BASE_URL"
echo "==> $IMPL conforms ✓"
