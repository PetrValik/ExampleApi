#!/bin/sh
set -e

echo "Applying database migrations (waiting for PostgreSQL)..."
# Retry the migration until the database accepts connections. Compose's
# depends_on healthcheck usually makes this succeed on the first attempt;
# the loop is a safety net for slower DB startups.
attempts=0
until python manage.py migrate --noinput; do
    attempts=$((attempts + 1))
    if [ "$attempts" -ge 30 ]; then
        echo "Database still unavailable after ${attempts} attempts — giving up."
        exit 1
    fi
    echo "Database not ready yet (attempt ${attempts}) — retrying in 2s..."
    sleep 2
done

echo "Migrations applied. Starting gunicorn on 0.0.0.0:8080..."
exec gunicorn config.wsgi:application --bind 0.0.0.0:8080 --workers 3 --timeout 60
