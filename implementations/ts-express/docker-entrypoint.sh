#!/bin/sh
set -e

# Sync the database schema before starting the API. The compose healthcheck already
# waits for Postgres to accept connections, but we retry here too so a slow first
# boot (fresh volume) never crashes the container.
echo "Syncing database schema with 'prisma db push'..."
attempt=0
max_attempts=15
until npx prisma db push --skip-generate --accept-data-loss; do
  attempt=$((attempt + 1))
  if [ "$attempt" -ge "$max_attempts" ]; then
    echo "Database not ready after ${max_attempts} attempts, giving up."
    exit 1
  fi
  echo "prisma db push failed (attempt ${attempt}/${max_attempts}); retrying in 3s..."
  sleep 3
done

echo "Schema in sync. Starting API on port ${PORT:-8080}..."
exec node dist/index.js
