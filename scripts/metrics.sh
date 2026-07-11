#!/usr/bin/env bash
# Code-size metrics per implementation — production source only (tests/build output excluded).
# A cloc-free baseline for the axis-A "ceremony of code" comparison.
#
# Usage: ./scripts/metrics.sh [impl-dir ...]   (default: every dir under implementations/)
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

impls=("$@")
if [[ ${#impls[@]} -eq 0 ]]; then
  impls=()
  for d in "$ROOT"/implementations/*/; do
    [[ -d "$d" ]] && impls+=("implementations/$(basename "$d")")
  done
fi

# Source files, excluding build output, tests, virtualenvs and vendored deps.
find_src() {
  find "$1" -type f \( -name '*.cs' -o -name '*.py' -o -name '*.ts' -o -name '*.js' \) \
    -not -path '*/bin/*' -not -path '*/obj/*' \
    -not -path '*/test/*' -not -path '*/tests/*' -not -path '*[Tt]ests/*' \
    -not -path '*/__pycache__/*' -not -path '*/.venv/*' -not -path '*/node_modules/*' \
    -not -path '*/Migrations/*'
}

printf "%-22s %7s %8s %10s\n" "implementation" "files" "lines" "non-blank"
printf "%-22s %7s %8s %10s\n" "----------------------" "-------" "--------" "----------"
for impl in "${impls[@]}"; do
  dir="$ROOT/$impl"
  [[ -d "$dir" ]] || continue
  count=$(find_src "$dir" | wc -l | tr -d ' ')
  if [[ "$count" -eq 0 ]]; then
    printf "%-22s %7s %8s %10s\n" "$(basename "$impl")" 0 0 0
    continue
  fi
  # Paths in this repo have no spaces, so newline-delimited xargs is safe here.
  total=$(find_src "$dir" | xargs cat 2>/dev/null | wc -l | tr -d ' ')
  nonblank=$(find_src "$dir" | xargs cat 2>/dev/null | grep -cvE '^[[:space:]]*$' | tr -d ' ')
  printf "%-22s %7s %8s %10s\n" "$(basename "$impl")" "$count" "$total" "$nonblank"
done

echo
echo "Note: excludes tests, EF migrations and build output. Migrations are generated, not authored."
