#!/usr/bin/env bash
# OPTIONAL property-based smoke test driven directly by the OpenAPI contract (schemathesis).
# It complements the hand-written behavioural suite by fuzzing every documented operation.
# Usage: ./smoke.sh [BASE_URL]   (default http://localhost:8080)
set -euo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BASE_URL="${1:-${BASE_URL:-http://localhost:8080}}"
CONTRACT="$DIR/../contract/openapi.yaml"
ST="$DIR/.venv/bin/schemathesis"

if [[ ! -x "$ST" ]]; then
  echo "schemathesis is not installed (it is optional)."
  echo "Install it into the conformance venv with:"
  echo "  $DIR/.venv/bin/pip install schemathesis"
  exit 0
fi

echo "==> Fetching a token"
TOKEN=$(curl -s -X POST "$BASE_URL/auth/token" \
  -H 'Content-Type: application/json' \
  -d '{"username":"admin","password":"admin"}' \
  | "$DIR/.venv/bin/python" -c 'import sys, json; print(json.load(sys.stdin)["token"])')

echo "==> schemathesis smoke against $BASE_URL"
# NB: schemathesis CLI flags vary by major version; adjust --url/--base-url as needed.
"$ST" run "$CONTRACT" --url "$BASE_URL" -H "Authorization: Bearer $TOKEN"
