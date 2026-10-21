-- ============================================================================
-- YM Inc. Store Tracking Project: Migration 007
-- File: db/migrations/007_realtime_sync.sql
-- Description: Real-time broadcast triggers for store and hardware updates
-- ============================================================================

CREATE OR REPLACE FUNCTION notify_store_change()
RETURNS TRIGGER AS $$
DECLARE
    payload JSON;
    sid INT;
BEGIN
    IF TG_TABLE_NAME = 'stores' THEN
        sid := COALESCE(NEW.store_id, OLD.store_id);
    ELSIF TG_TABLE_NAME = 'site_surveys' THEN
        sid := COALESCE(NEW.store_id, OLD.store_id);
    ELSIF TG_TABLE_NAME = 'power_poles' THEN
        sid := COALESCE(NEW.store_id, OLD.store_id);
    ELSIF TG_TABLE_NAME = 'cabling_operations' THEN
        sid := COALESCE(NEW.store_id, OLD.store_id);
    ELSIF TG_TABLE_NAME = 'circuit_details' THEN
        sid := COALESCE(NEW.store_id, OLD.store_id);
    ELSIF TG_TABLE_NAME = 'mall_properties' OR TG_TABLE_NAME = 'mall_requirements' THEN
        -- Map mall to store_id if possible
        SELECT s.store_id INTO sid FROM stores s WHERE s.mall_id = COALESCE(NEW.mall_id, OLD.mall_id) LIMIT 1;
        IF sid IS NULL THEN
            sid := 0;
        END IF;
    ELSE
        sid := 0;
    END IF;

    payload := json_build_object(
        'store_id', sid,
        'table', TG_TABLE_NAME,
        'action', TG_OP,
        'timestamp', EXTRACT(EPOCH FROM NOW())
    );

    PERFORM pg_notify('store_realtime_events', payload::text);
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger on stores
DROP TRIGGER IF EXISTS trg_stores_notify ON stores;
CREATE TRIGGER trg_stores_notify 
AFTER INSERT OR UPDATE OR DELETE ON stores
FOR EACH ROW EXECUTE FUNCTION notify_store_change();

-- Trigger on site surveys
DROP TRIGGER IF EXISTS trg_surveys_notify ON site_surveys;
CREATE TRIGGER trg_surveys_notify 
AFTER INSERT OR UPDATE OR DELETE ON site_surveys
FOR EACH ROW EXECUTE FUNCTION notify_store_change();

-- Trigger on power poles
DROP TRIGGER IF EXISTS trg_power_poles_notify ON power_poles;
CREATE TRIGGER trg_power_poles_notify 
AFTER INSERT OR UPDATE OR DELETE ON power_poles
FOR EACH ROW EXECUTE FUNCTION notify_store_change();

-- Trigger on cabling
DROP TRIGGER IF EXISTS trg_cabling_notify ON cabling_operations;
CREATE TRIGGER trg_cabling_notify 
AFTER INSERT OR UPDATE OR DELETE ON cabling_operations
FOR EACH ROW EXECUTE FUNCTION notify_store_change();

-- Trigger on circuits
DROP TRIGGER IF EXISTS trg_circuits_notify ON circuit_details;
CREATE TRIGGER trg_circuits_notify 
AFTER INSERT OR UPDATE OR DELETE ON circuit_details
FOR EACH ROW EXECUTE FUNCTION notify_store_change();

-- Trigger on mall properties
DROP TRIGGER IF EXISTS trg_mall_properties_notify ON mall_properties;
CREATE TRIGGER trg_mall_properties_notify 
AFTER INSERT OR UPDATE OR DELETE ON mall_properties
FOR EACH ROW EXECUTE FUNCTION notify_store_change();

-- Trigger on mall requirements
DROP TRIGGER IF EXISTS trg_mall_requirements_notify ON mall_requirements;
CREATE TRIGGER trg_mall_requirements_notify 
AFTER INSERT OR UPDATE OR DELETE ON mall_requirements
FOR EACH ROW EXECUTE FUNCTION notify_store_change();
