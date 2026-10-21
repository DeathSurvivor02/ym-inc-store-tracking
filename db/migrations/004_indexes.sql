-- ============================================================================
-- YM Inc. Store Tracking Project: Migration 004
-- File: db/migrations/004_indexes.sql
-- Description: High-Performance Composite & Partial Indexes
-- ============================================================================

-- 1. Management Hierarchy & Store Relations
CREATE INDEX IF NOT EXISTS idx_districts_region ON districts (region_id);
CREATE INDEX IF NOT EXISTS idx_stores_banner ON stores (banner_id, store_status);
CREATE INDEX IF NOT EXISTS idx_stores_district ON stores (district_id, store_status);
CREATE INDEX IF NOT EXISTS idx_stores_mall ON stores (mall_id);
CREATE INDEX IF NOT EXISTS idx_stores_status ON stores (operational_status);
CREATE INDEX IF NOT EXISTS idx_stores_health ON stores (overall_health);
CREATE INDEX IF NOT EXISTS idx_stores_target_open ON stores (target_open_date);
CREATE INDEX IF NOT EXISTS idx_store_attributes_format ON store_attributes (real_estate_format);

-- 2. Lifecycle & Infrastructure Tracks
CREATE INDEX IF NOT EXISTS idx_store_contacts_store ON store_contacts (store_id);
CREATE INDEX IF NOT EXISTS idx_store_contacts_role ON store_contacts (contact_role);
CREATE INDEX IF NOT EXISTS idx_site_surveys_store ON site_surveys (store_id);
CREATE INDEX IF NOT EXISTS idx_site_surveys_status ON site_surveys (survey_status);
CREATE INDEX IF NOT EXISTS idx_power_poles_store ON power_poles (store_id);
CREATE INDEX IF NOT EXISTS idx_power_poles_status ON power_poles (pole_requirement_status);
CREATE INDEX IF NOT EXISTS idx_cabling_store ON cabling_operations (store_id);
CREATE INDEX IF NOT EXISTS idx_cabling_status ON cabling_operations (operational_status);
CREATE INDEX IF NOT EXISTS idx_circuits_store ON circuit_details (store_id);
CREATE INDEX IF NOT EXISTS idx_circuits_status ON circuit_details (order_status);
CREATE INDEX IF NOT EXISTS idx_circuits_role ON circuit_details (circuit_role);
CREATE INDEX IF NOT EXISTS idx_milestones_store ON store_milestones (store_id);
CREATE INDEX IF NOT EXISTS idx_milestones_status ON store_milestones (milestone_status);
CREATE INDEX IF NOT EXISTS idx_snapshots_date ON weekly_store_snapshots (snapshot_date);
CREATE INDEX IF NOT EXISTS idx_snapshots_health ON weekly_store_snapshots (overall_health);

-- 3. Analytics & Weekly Aggregations
CREATE INDEX IF NOT EXISTS idx_dsm_store_date ON daily_store_metrics (store_id, calendar_date);
CREATE INDEX IF NOT EXISTS idx_wss_store_week ON weekly_store_sales (store_id, fiscal_year, fiscal_week);
CREATE INDEX IF NOT EXISTS idx_wss_fiscal_lookup ON weekly_store_sales (fiscal_year, fiscal_week, net_sales_cad DESC);
CREATE INDEX IF NOT EXISTS idx_wss_comp_store ON weekly_store_sales (is_comp_store, fiscal_year, fiscal_week) WHERE is_comp_store = TRUE;
CREATE INDEX IF NOT EXISTS idx_wsl_store_week ON weekly_store_labour (store_id, fiscal_year, fiscal_week);
DO $$ 
BEGIN 
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'weekly_store_inventory' AND column_name = 'category_id'
    ) THEN 
        CREATE INDEX IF NOT EXISTS idx_wsi_store_cat ON weekly_store_inventory (store_id, category_id, fiscal_year, fiscal_week);
    END IF; 
END $$;
CREATE INDEX IF NOT EXISTS idx_fiscal_calendar_lookup ON fiscal_calendar (fiscal_year, fiscal_week);
