-- ============================================================================
-- YM Inc. Store Tracking Project: Migration 006
-- File: db/migrations/006_app_users_auth.sql
-- Description: Application Users Authentication & Security with Salt and Pepper
-- ============================================================================

CREATE TABLE IF NOT EXISTS app_users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(64) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    full_name VARCHAR(128) NOT NULL,
    role VARCHAR(32) NOT NULL DEFAULT 'PM',
    password_hash TEXT NOT NULL,
    password_salt TEXT NOT NULL,
    iterations INT NOT NULL DEFAULT 100000,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    failed_attempts INT NOT NULL DEFAULT 0,
    locked_until TIMESTAMPTZ NULL,
    last_login_at TIMESTAMPTZ NULL,
    password_changed_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_app_user_role CHECK (role IN ('PM', 'PA', 'PS1', 'PS2', 'PS3', 'PS4', 'ADMIN'))
);

CREATE INDEX IF NOT EXISTS idx_app_users_username ON app_users (LOWER(username));
CREATE INDEX IF NOT EXISTS idx_app_users_email ON app_users (LOWER(email));
CREATE INDEX IF NOT EXISTS idx_app_users_is_active ON app_users (is_active);
