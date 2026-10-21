-- ============================================================================
-- YM Inc. Store Tracking Project: Migration 003
-- File: db/migrations/003_analytics_and_facts.sql
-- Description: Retail Analytics, Fiscal Calendar, Daily POS & Weekly Facts
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1. Merchandise Department Categories
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS merchandise_categories (
    category_id SMALLSERIAL PRIMARY KEY,
    category_code VARCHAR(15) NOT NULL UNIQUE,
    category_name VARCHAR(50) NOT NULL,
    division VARCHAR(40) NOT NULL CHECK (division IN (
        'Womenswear', 'Menswear', 'Kids & Youth', 'Denim', 
        'Footwear & Accessories', 'Seasonal & Novelty'
    )),
    target_margin_pct NUMERIC(5,2) NOT NULL DEFAULT 55.00,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 2. Retail Fiscal Calendar (YM Inc. 4-5-4 Calendar)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS fiscal_calendar (
    calendar_date DATE PRIMARY KEY,
    fiscal_year SMALLINT NOT NULL,
    fiscal_quarter SMALLINT NOT NULL CHECK (fiscal_quarter BETWEEN 1 AND 4),
    fiscal_month SMALLINT NOT NULL CHECK (fiscal_month BETWEEN 1 AND 12),
    fiscal_week SMALLINT NOT NULL CHECK (fiscal_week BETWEEN 1 AND 53),
    day_of_fiscal_week SMALLINT NOT NULL CHECK (day_of_fiscal_week BETWEEN 1 AND 7), -- 1=Sun, 7=Sat
    is_holiday BOOLEAN NOT NULL DEFAULT FALSE,
    holiday_name VARCHAR(50)
);

-- ----------------------------------------------------------------------------
-- 3. Daily Store Activity (Granular Staging & Audit)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS daily_store_metrics (
    metric_id BIGSERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    calendar_date DATE NOT NULL REFERENCES fiscal_calendar(calendar_date),
    foot_traffic_count INT NOT NULL DEFAULT 0,
    gross_sales_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    returns_refunds_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    discounts_promotions_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    net_sales_cad NUMERIC(12,2) GENERATED ALWAYS AS (gross_sales_cad - returns_refunds_cad) STORED,
    transaction_count INT NOT NULL DEFAULT 0,
    units_sold_count INT NOT NULL DEFAULT 0,
    cost_of_goods_sold_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    labour_hours_worked NUMERIC(6,2) NOT NULL DEFAULT 0.00,
    labour_cost_cad NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_store_date UNIQUE (store_id, calendar_date),
    CONSTRAINT chk_counts_positive CHECK (foot_traffic_count >= 0 AND transaction_count >= 0 AND units_sold_count >= 0),
    CONSTRAINT chk_labour_positive CHECK (labour_hours_worked >= 0 AND labour_cost_cad >= 0)
);

-- ----------------------------------------------------------------------------
-- 4. Weekly Store Consolidated Sales (Core Analytics Fact)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS weekly_store_sales (
    weekly_sales_id BIGSERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    fiscal_year SMALLINT NOT NULL,
    fiscal_week SMALLINT NOT NULL,
    
    -- Targets & Historical Baseline
    target_sales_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    prior_year_sales_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    
    -- Actual Realized Metrics
    gross_sales_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    refunds_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    net_sales_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    cogs_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    gross_margin_cad NUMERIC(12,2) GENERATED ALWAYS AS (net_sales_cad - cogs_cad) STORED,
    gross_margin_pct NUMERIC(6,2),
    
    -- Traffic & Transactions
    foot_traffic_total INT NOT NULL DEFAULT 0,
    transaction_count INT NOT NULL DEFAULT 0,
    units_sold_total INT NOT NULL DEFAULT 0,
    
    -- Retail Mathematics KPIs
    conversion_rate_pct NUMERIC(6,2),
    average_transaction_value_cad NUMERIC(8,2), -- ATV
    units_per_transaction NUMERIC(5,2),         -- UPT
    average_unit_retail_cad NUMERIC(8,2),       -- AUR
    sales_per_selling_sq_ft_cad NUMERIC(8,2),   -- SPSF
    
    -- Variance & Comp Status
    sales_to_budget_variance_pct NUMERIC(6,2),
    year_over_year_growth_pct NUMERIC(6,2),
    is_comp_store BOOLEAN NOT NULL DEFAULT TRUE,
    
    snapshot_timestamp TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_store_fiscal_week UNIQUE (store_id, fiscal_year, fiscal_week),
    CONSTRAINT chk_fiscal_week_val CHECK (fiscal_week BETWEEN 1 AND 53)
);

-- ----------------------------------------------------------------------------
-- 5. Weekly Store Labour & Scheduling Efficiency
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS weekly_store_labour (
    weekly_labour_id BIGSERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    fiscal_year SMALLINT NOT NULL,
    fiscal_week SMALLINT NOT NULL,
    
    planned_hours NUMERIC(7,2) NOT NULL DEFAULT 0.00,
    actual_hours NUMERIC(7,2) NOT NULL DEFAULT 0.00,
    hours_variance NUMERIC(7,2) GENERATED ALWAYS AS (actual_hours - planned_hours) STORED,
    
    planned_payroll_cost_cad NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    actual_payroll_cost_cad NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    payroll_variance_cad NUMERIC(10,2) GENERATED ALWAYS AS (actual_payroll_cost_cad - planned_payroll_cost_cad) STORED,
    
    labour_cost_pct_of_sales NUMERIC(6,2),
    sales_per_labour_hour_cad NUMERIC(8,2), -- SPLH
    
    overtime_hours NUMERIC(6,2) NOT NULL DEFAULT 0.00,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_store_labour_week UNIQUE (store_id, fiscal_year, fiscal_week)
);

-- ----------------------------------------------------------------------------
-- 6. Weekly Store Inventory & Stock Velocity
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS weekly_store_inventory (
    inventory_id BIGSERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    category_id SMALLINT NOT NULL REFERENCES merchandise_categories(category_id) ON UPDATE CASCADE,
    fiscal_year SMALLINT NOT NULL,
    fiscal_week SMALLINT NOT NULL,
    
    beginning_stock_on_hand_units INT NOT NULL DEFAULT 0,
    received_shipments_units INT NOT NULL DEFAULT 0,
    transfers_in_units INT NOT NULL DEFAULT 0,
    transfers_out_units INT NOT NULL DEFAULT 0,
    sales_units INT NOT NULL DEFAULT 0,
    damaged_shrink_units INT NOT NULL DEFAULT 0,
    ending_stock_on_hand_units INT NOT NULL DEFAULT 0,
    
    ending_inventory_retail_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    ending_inventory_cost_cad NUMERIC(12,2) NOT NULL DEFAULT 0.00,
    sell_through_pct NUMERIC(6,2),
    weeks_of_supply NUMERIC(5,2),
    
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_store_cat_week UNIQUE (store_id, category_id, fiscal_year, fiscal_week)
);

ALTER TABLE weekly_store_inventory ADD COLUMN IF NOT EXISTS category_id SMALLINT REFERENCES merchandise_categories(category_id) ON UPDATE CASCADE;

-- ----------------------------------------------------------------------------
-- 7. Audit & Pipeline Execution Telemetry
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS antigravity_pipeline_runs (
    run_id BIGSERIAL PRIMARY KEY,
    job_name VARCHAR(100) NOT NULL,
    fiscal_year SMALLINT NOT NULL,
    fiscal_week SMALLINT NOT NULL,
    execution_start_time TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    execution_end_time TIMESTAMPTZ,
    status VARCHAR(25) NOT NULL DEFAULT 'In Progress' CHECK (status IN (
        'In Progress', 'Completed', 'Failed', 'Completed with Warnings'
    )),
    stores_processed INT NOT NULL DEFAULT 0,
    anomaly_count INT NOT NULL DEFAULT 0,
    error_log TEXT
);
