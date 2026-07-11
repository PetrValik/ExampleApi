#!/usr/bin/env bash
# Run the conformance suite against a running implementation.
# Usage: ./run.sh [BASE_URL] [extra pytest args...]   (default BASE_URL=http://localhost:8080)
set -euo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
export BASE_URL="${1:-${BASE_URL:-http://localhost:8080}}"
shift || true

PYTHON="$DIR/.venv/bin/python"
if [[ ! -x "$PYTHON" ]]; then
  echo "No venv found — creating $DIR/.venv and installing requirements…"
  python3 -m venv "$DIR/.venv"
  "$DIR/.venv/bin/pip" install -q --upgrade pip
  "$DIR/.venv/bin/pip" install -q -r "$DIR/requirements.txt"
fi

echo "==> Conformance against $BASE_URL"
"$PYTHON" -m pytest "$DIR" "$@"
