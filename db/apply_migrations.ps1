# ==============================================================================
# YM Inc. Store Tracking Project: Database Migration Runner (PowerShell)
# ==============================================================================
param(
    [string]$HostName = "localhost",
    [string]$Port = "5432",
    [string]$User = "ym_admin",
    [string]$Database = "ym_store_tracking",
    [switch]$IncludeSeeds
)

$ErrorActionPreference = "Stop"

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  YM Inc. Store Tracking Project - Database Migration Engine     " -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "Connecting to: postgresql://$User@$HostName`:$Port/$Database" -ForegroundColor Yellow

$migrations = @(
    "db/migrations/001_management_hierarchy.sql",
    "db/migrations/002_stores_and_lifecycle.sql",
    "db/migrations/003_analytics_and_facts.sql",
    "db/migrations/004_indexes.sql",
    "db/migrations/005_views.sql"
)

foreach ($migration in $migrations) {
    if (Test-Path $migration) {
        Write-Host "Applying: $migration..." -ForegroundColor Green
        Get-Content $migration -Raw | psql -h $HostName -p $Port -U $User -d $Database
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to apply $migration"
            exit $LASTEXITCODE
        }
    } else {
        Write-Error "Migration file not found: $migration"
        exit 1
    }
}

if ($IncludeSeeds) {
    $seedFile = "db/seeds/01_master_seeds.sql"
    if (Test-Path $seedFile) {
        Write-Host "Seeding database: $seedFile..." -ForegroundColor Magenta
        Get-Content $seedFile -Raw | psql -h $HostName -p $Port -U $User -d $Database
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to seed database"
            exit $LASTEXITCODE
        }
    }
}

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Migrations successfully applied! Database is ready.            " -ForegroundColor Green
Write-Host "==================================================================" -ForegroundColor Cyan
