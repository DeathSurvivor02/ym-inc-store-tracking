#!/usr/bin/env bash
# ==============================================================================
# YM Inc. Store Tracking Project: Database Migration Runner (Bash)
# ==============================================================================
set -e

HOST="${PGHOST:-localhost}"
PORT="${PGPORT:-5432}"
USER="${PGUSER:-ym_admin}"
DB="${PGDATABASE:-ym_store_tracking}"
INCLUDE_SEEDS="${1:-false}"

echo "=================================================================="
echo "  YM Inc. Store Tracking Project - Database Migration Engine     "
echo "=================================================================="
echo "Connecting to: postgresql://$USER@$HOST:$PORT/$DB"

MIGRATIONS=(
  "db/migrations/001_management_hierarchy.sql"
  "db/migrations/002_stores_and_lifecycle.sql"
  "db/migrations/003_analytics_and_facts.sql"
  "db/migrations/004_indexes.sql"
  "db/migrations/005_views.sql"
  "db/migrations/006_app_users_auth.sql"
)

for migration in "${MIGRATIONS[@]}"; do
  if [ -f "$migration" ]; then
    echo "Applying: $migration..."
    psql -h "$HOST" -p "$PORT" -U "$USER" -d "$DB" -f "$migration"
  else
    echo "Error: Migration file not found: $migration" >&2
    exit 1
  fi
done

if [ "$INCLUDE_SEEDS" = "true" ] || [ "$INCLUDE_SEEDS" = "--seeds" ]; then
  SEED_FILE="db/seeds/01_master_seeds.sql"
  if [ -f "$SEED_FILE" ]; then
    echo "Seeding database: $SEED_FILE..."
    psql -h "$HOST" -p "$PORT" -U "$USER" -d "$DB" -f "$SEED_FILE"
  fi
fi

echo "=================================================================="
echo "  Migrations successfully applied! Database is ready.            "
echo "=================================================================="
