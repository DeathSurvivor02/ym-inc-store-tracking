-- ============================================================================
-- YM Inc. Store Tracking Project: Migration 008
-- File: db/migrations/008_collaborative_presence.sql
-- Description: Google Docs-style real-time collaborator presence & live broadcasting
-- ============================================================================

CREATE TABLE IF NOT EXISTS app_user_presence (
    session_id VARCHAR(64) PRIMARY KEY,
    user_id INT,
    user_name VARCHAR(128) NOT NULL,
    user_role VARCHAR(64) NOT NULL,
    user_initials VARCHAR(8) NOT NULL,
    store_number VARCHAR(16),
    active_section VARCHAR(64),
    active_field VARCHAR(64),
    last_heartbeat TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_presence_store ON app_user_presence(store_number);
CREATE INDEX IF NOT EXISTS idx_presence_heartbeat ON app_user_presence(last_heartbeat);

CREATE OR REPLACE FUNCTION notify_presence_change()
RETURNS TRIGGER AS $$
DECLARE
    payload JSON;
BEGIN
    payload := json_build_object(
        'session_id', COALESCE(NEW.session_id, OLD.session_id),
        'user_name', COALESCE(NEW.user_name, OLD.user_name),
        'user_role', COALESCE(NEW.user_role, OLD.user_role),
        'user_initials', COALESCE(NEW.user_initials, OLD.user_initials),
        'store_number', COALESCE(NEW.store_number, OLD.store_number),
        'active_section', COALESCE(NEW.active_section, OLD.active_section),
        'active_field', COALESCE(NEW.active_field, OLD.active_field),
        'action', TG_OP,
        'timestamp', EXTRACT(EPOCH FROM NOW())
    );

    PERFORM pg_notify('store_presence_events', payload::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_presence_notify ON app_user_presence;
CREATE TRIGGER trg_presence_notify
AFTER INSERT OR UPDATE OR DELETE ON app_user_presence
FOR EACH ROW EXECUTE FUNCTION notify_presence_change();
