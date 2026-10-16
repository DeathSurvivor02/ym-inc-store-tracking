-- ============================================================================
-- YM Inc. Store Tracking Project: Migration 001
-- File: db/migrations/001_management_hierarchy.sql
-- Description: Management Hierarchy, Geographic Territories, Banners, & Malls
-- ============================================================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ----------------------------------------------------------------------------
-- 1. Master Retail Banners
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS banners (
    banner_id SMALLSERIAL PRIMARY KEY,
    banner_code VARCHAR(10) NOT NULL UNIQUE,
    banner_name VARCHAR(50) NOT NULL,
    brand_tier VARCHAR(30) NOT NULL DEFAULT 'Core Fashion',
    target_demographic VARCHAR(100),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_banner_tier CHECK (brand_tier IN (
        'Fast Fashion', 'Denim & Casual', 'Contemporary Women', 
        'Action Sports', 'Value Apparel', 'Youth', 'Core Fashion', 'Junior Fashion'
    ))
);

-- ----------------------------------------------------------------------------
-- 2. Geographic Regions
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS regions (
    region_id SMALLSERIAL PRIMARY KEY,
    region_code VARCHAR(10) NOT NULL UNIQUE,
    region_name VARCHAR(100) NOT NULL,
    country_code CHAR(2) NOT NULL DEFAULT 'CA',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_region_country CHECK (country_code IN ('CA', 'US'))
);

-- ----------------------------------------------------------------------------
-- 3. Regional Managers
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS regional_managers (
    rm_id SERIAL PRIMARY KEY,
    region_id SMALLINT NOT NULL REFERENCES regions(region_id) ON UPDATE CASCADE ON DELETE RESTRICT,
    first_name VARCHAR(60) NOT NULL,
    last_name VARCHAR(60) NOT NULL,
    email VARCHAR(120) NOT NULL UNIQUE,
    phone VARCHAR(30) NOT NULL,
    mobile VARCHAR(30),
    status VARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'On Leave', 'Inactive')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 4. Operational Districts
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS districts (
    district_id SMALLSERIAL PRIMARY KEY,
    district_code VARCHAR(20) NOT NULL UNIQUE,
    district_name VARCHAR(100) NOT NULL,
    region_id SMALLINT NOT NULL REFERENCES regions(region_id) ON UPDATE CASCADE ON DELETE RESTRICT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 5. District Managers
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS district_managers (
    dm_id SERIAL PRIMARY KEY,
    district_id SMALLINT NOT NULL REFERENCES districts(district_id) ON UPDATE CASCADE ON DELETE RESTRICT,
    first_name VARCHAR(60) NOT NULL,
    last_name VARCHAR(60) NOT NULL,
    email VARCHAR(120) NOT NULL UNIQUE,
    phone VARCHAR(30) NOT NULL,
    mobile VARCHAR(30),
    status VARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (status IN ('Active', 'On Leave', 'Inactive')),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 6. Commercial Retail Properties (Malls / Power Centres)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS mall_properties (
    mall_id SERIAL PRIMARY KEY,
    mall_code VARCHAR(30) NOT NULL UNIQUE,
    property_name VARCHAR(150) NOT NULL,
    landlord_company VARCHAR(150) NOT NULL,
    property_type VARCHAR(50) DEFAULT 'Enclosed Regional Mall' CHECK (property_type IN (
        'Enclosed Regional Mall', 'Power Centre / Big Box', 'Power Centre / Strip', 
        'Urban Street Front', 'Outlet Centre', 'Strip Plaza'
    )),
    address_line1 VARCHAR(200) NOT NULL,
    city VARCHAR(100) NOT NULL,
    state_province VARCHAR(50) NOT NULL,
    postal_code VARCHAR(20) NOT NULL,
    country_code CHAR(2) NOT NULL DEFAULT 'CA',
    main_phone VARCHAR(30),
    security_dispatch_phone VARCHAR(30),
    dock_master_phone VARCHAR(30),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 7. Mall Requirements & Landlord Compliance Rules
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS mall_requirements (
    requirement_id SERIAL PRIMARY KEY,
    mall_id INT NOT NULL UNIQUE REFERENCES mall_properties(mall_id) ON DELETE CASCADE,
    union_labor_required BOOLEAN NOT NULL DEFAULT FALSE,
    union_local_designation VARCHAR(100),
    coi_required BOOLEAN NOT NULL DEFAULT TRUE,
    minimum_general_liability_cad DECIMAL(12, 2) NOT NULL DEFAULT 5000000.00,
    coi_additional_insured_text TEXT,
    work_permit_required BOOLEAN NOT NULL DEFAULT TRUE,
    work_permit_lead_time_days INT NOT NULL DEFAULT 5,
    delivery_dock_restricted BOOLEAN NOT NULL DEFAULT TRUE,
    delivery_window_start TIME NOT NULL DEFAULT '06:00:00',
    delivery_window_end TIME NOT NULL DEFAULT '09:30:00',
    night_work_only BOOLEAN NOT NULL DEFAULT FALSE,
    noisy_work_restricted_hours VARCHAR(150) DEFAULT '10:00 to 21:00 (Mall Operating Hours)',
    security_badging_required BOOLEAN NOT NULL DEFAULT TRUE,
    badging_fee_per_tech_cad DECIMAL(8, 2) DEFAULT 0.00,
    hot_work_permit_policy VARCHAR(255) DEFAULT '24-hour advance written notice to Landlord Operations required',
    ceiling_access_protocol TEXT,
    landlord_telecom_riser_fee_cad DECIMAL(10, 2) DEFAULT 0.00,
    approved_waste_haul_vendor VARCHAR(100),
    special_mall_rules TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);
-- ============================================================================
-- YM Inc. Store Tracking Project: Migration 002
-- File: db/migrations/002_stores_and_lifecycle.sql
-- Description: Retail Stores Master, Attributes, Infrastructure & Lifecycle
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1. Retail Stores Master Table
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stores (
    store_id SERIAL PRIMARY KEY,
    store_number VARCHAR(20) NOT NULL UNIQUE,
    banner_id SMALLINT NOT NULL REFERENCES banners(banner_id) ON UPDATE CASCADE ON DELETE RESTRICT,
    district_id SMALLINT NOT NULL REFERENCES districts(district_id) ON UPDATE CASCADE ON DELETE RESTRICT,
    mall_id INT REFERENCES mall_properties(mall_id) ON UPDATE CASCADE ON DELETE SET NULL,
    store_name VARCHAR(150) NOT NULL,
    project_type VARCHAR(50) NOT NULL DEFAULT 'Active Fleet' CHECK (project_type IN (
        'Active Fleet', 'New Store Opening', 'Major Remodel', 'Minor Refresh', 
        'Brand Conversion', 'Relocation', 'Store Closure'
    )),
    operational_status VARCHAR(50) NOT NULL DEFAULT 'Grand Open' CHECK (operational_status IN (
        'Planning', 'Lease Execution', 'Site Survey', 'Permitting & Design',
        'GC Construction', 'Low-Voltage & Power Poles', 'IT Staging & Telecom',
        'Merchandising', 'Pre-Opening QA', 'Grand Open', 'Temporary Closed', 
        'Renovating', 'Decommissioned'
    )),
    overall_health VARCHAR(20) NOT NULL DEFAULT 'On Track' CHECK (overall_health IN (
        'On Track', 'At Risk', 'Critical Blocker', 'Complete'
    )),
    store_status VARCHAR(20) NOT NULL DEFAULT 'Active' CHECK (store_status IN (
        'Active', 'Temporary Closed', 'Renovating', 'Decommissioned'
    )),
    unit_number VARCHAR(50),
    address_line1 VARCHAR(200) NOT NULL,
    address_line2 VARCHAR(100),
    city VARCHAR(100) NOT NULL,
    state_province VARCHAR(50) NOT NULL,
    postal_code VARCHAR(20) NOT NULL,
    country_code CHAR(2) NOT NULL DEFAULT 'CA',
    phone_number VARCHAR(30),
    
    -- Real Estate & Technical Specs
    gross_sqft INT CHECK (gross_sqft > 0),
    sales_sqft INT CHECK (sales_sqft > 0),
    pos_terminal_count INT NOT NULL DEFAULT 3 CHECK (pos_terminal_count >= 1),
    pos_system_version VARCHAR(30) DEFAULT 'Veras Retail v4.2',
    pos_staging_status VARCHAR(30) DEFAULT 'Commissioned' CHECK (pos_staging_status IN (
        'Pending', 'Hardware Staged', 'Configured', 'Shipped', 'On Site', 'Commissioned'
    )),
    
    -- Critical Lifecycle Dates
    opening_date DATE NOT NULL,
    closing_date DATE,
    landlord_possession_date DATE,
    construction_start_date DATE,
    cabling_turnover_date DATE,
    merchandising_start_date DATE,
    target_open_date DATE,
    revised_open_date DATE,
    actual_open_date DATE,
    
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 2. Store Real Estate & Operational Attributes
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS store_attributes (
    attribute_id SERIAL PRIMARY KEY,
    store_id INT NOT NULL UNIQUE REFERENCES stores(store_id) ON DELETE CASCADE,
    real_estate_format VARCHAR(40) NOT NULL DEFAULT 'Enclosed Mall' CHECK (real_estate_format IN (
        'Enclosed Mall', 'Power Centre / Big Box', 'Outlet Centre', 'Urban Street Front', 'Strip Plaza'
    )),
    gross_sq_ft NUMERIC(8,2) NOT NULL,
    selling_sq_ft NUMERIC(8,2) NOT NULL,
    storage_sq_ft NUMERIC(8,2) GENERATED ALWAYS AS (gross_sq_ft - selling_sq_ft) STORED,
    lease_type VARCHAR(40) NOT NULL DEFAULT 'Net Lease',
    pos_terminal_count SMALLINT NOT NULL DEFAULT 4,
    traffic_counter_type VARCHAR(50) NOT NULL DEFAULT 'Overhead Stereoscopic Optical',
    has_curbside_pickup BOOLEAN NOT NULL DEFAULT FALSE,
    base_annual_rent NUMERIC(12,2),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_sq_ft CHECK (gross_sq_ft > 0 AND selling_sq_ft > 0 AND gross_sq_ft >= selling_sq_ft)
);

-- ----------------------------------------------------------------------------
-- 3. Stakeholder Contacts Directory
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS store_contacts (
    contact_id SERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    contact_role VARCHAR(50) NOT NULL CHECK (contact_role IN (
        'Landlord Operations Manager', 'Mall Security Director', 'General Contractor PM',
        'General Contractor Superintendent', 'Electrical Contractor Lead', 
        'Low-Voltage Cabling Lead', 'Telecom Carrier Field Tech', 'Store Manager',
        'YM Inc Project Manager', 'YM Inc IT Systems Engineer'
    )),
    full_name VARCHAR(120) NOT NULL,
    organization VARCHAR(150) NOT NULL,
    email VARCHAR(120) NOT NULL,
    phone_office VARCHAR(30),
    phone_mobile VARCHAR(30) NOT NULL,
    is_primary_for_role BOOLEAN NOT NULL DEFAULT TRUE,
    emergency_contact BOOLEAN NOT NULL DEFAULT FALSE,
    notes VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 4. Site Surveys
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS site_surveys (
    survey_id SERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    survey_vendor VARCHAR(100) NOT NULL,
    surveyor_name VARCHAR(100) NOT NULL,
    surveyor_phone VARCHAR(30),
    scheduled_date DATE NOT NULL,
    actual_survey_date DATE,
    survey_status VARCHAR(30) NOT NULL DEFAULT 'Scheduled' CHECK (survey_status IN (
        'Scheduled', 'In Progress', 'Completed & Approved', 
        'Conditional Pass', 'Action Required / Failed', 'Cancelled'
    )),
    ceiling_type VARCHAR(50) NOT NULL CHECK (ceiling_type IN (
        'Drop Ceiling (2x4 Acoustic Tiles)', 'Drop Ceiling (2x2 Acoustic Tiles)', 
        'Open Plenum / Exposed Deck', 'Drywall / Hard Lid', 'Mixed Ceiling'
    )),
    ceiling_height_feet DECIMAL(5, 2) NOT NULL,
    ceiling_structure_notes TEXT,
    demarc_location VARCHAR(150) NOT NULL,
    demarc_to_mdf_distance_feet INT,
    demarc_conduit_available BOOLEAN NOT NULL DEFAULT TRUE,
    mdf_rack_location VARCHAR(100) NOT NULL DEFAULT 'Back Office / Cash Wrap',
    mdf_rack_space_adequate BOOLEAN NOT NULL DEFAULT TRUE,
    power_outlet_ready_for_rack BOOLEAN NOT NULL DEFAULT FALSE,
    dedicated_circuits_count INT DEFAULT 2,
    existing_cabling_reuse_viable BOOLEAN NOT NULL DEFAULT FALSE,
    wireless_ap_mounting_viable BOOLEAN NOT NULL DEFAULT TRUE,
    photo_repository_url VARCHAR(500),
    survey_report_pdf_url VARCHAR(500),
    identified_deficiencies TEXT,
    approved_by VARCHAR(100),
    approved_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 5. Power Poles & Fixturing
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS power_poles (
    power_pole_id SERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    pole_requirement_status VARCHAR(50) NOT NULL DEFAULT 'Survey Required' CHECK (pole_requirement_status IN (
        'Not Required (Wall / Floor Feed)', 'Survey Required', 'Engineering Design',
        'Equipment Ordered', 'Delivered On Site', 'Installed & Anchored',
        'Passed Electrical Inspection', 'Deficiency Hold'
    )),
    total_poles_required INT NOT NULL DEFAULT 1 CHECK (total_poles_required >= 0),
    poles_installed INT NOT NULL DEFAULT 0 CHECK (poles_installed <= total_poles_required),
    pole_manufacturer VARCHAR(80) DEFAULT 'Wiremold / Legrand',
    pole_model_number VARCHAR(80) DEFAULT '25DTP-4',
    finish_color VARCHAR(50) DEFAULT 'Anodized Aluminum' CHECK (finish_color IN (
        'Anodized Aluminum', 'Matte Black', 'Custom White', 'Industrial Grey'
    )),
    ceiling_mount_type VARCHAR(80) DEFAULT 'T-Bar Grid Clamp with Structural Tie-Wire',
    electrical_contractor VARCHAR(100),
    electrician_permit_number VARCHAR(50),
    dual_channel_isolated BOOLEAN NOT NULL DEFAULT TRUE,
    cash_wrap_feed_drops INT NOT NULL DEFAULT 4,
    scheduled_delivery_date DATE,
    actual_delivery_date DATE,
    scheduled_install_date DATE,
    actual_install_date DATE,
    electrical_inspection_date DATE,
    inspector_signoff_name VARCHAR(100),
    blocker_reason VARCHAR(255),
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 6. Low-Voltage Cabling Operations
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS cabling_operations (
    cabling_id SERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    cabling_vendor VARCHAR(120) NOT NULL,
    lead_technician_name VARCHAR(100),
    lead_technician_phone VARCHAR(30),
    operational_status VARCHAR(50) NOT NULL DEFAULT 'Vendor Assigned' CHECK (operational_status IN (
        'Vendor Assigned', 'Pathway & J-Hooks Installed', 'Cable Pull In Progress',
        'Termination & Patch Panels', 'Fluke Testing & Certification',
        'Punch List Remediation', 'Completed & Certified', 'Delayed / Access Blocked'
    )),
    cable_grade VARCHAR(20) NOT NULL DEFAULT 'Cat6 Plenum (CMP)' CHECK (cable_grade IN (
        'Cat6 Riser (CMR)', 'Cat6 Plenum (CMP)', 'Cat6A Plenum (CMP)'
    )),
    total_data_drops INT NOT NULL CHECK (total_data_drops > 0),
    pos_drops INT NOT NULL DEFAULT 4 CHECK (pos_drops >= 1),
    ap_wireless_drops INT NOT NULL DEFAULT 4 CHECK (ap_wireless_drops >= 1),
    back_office_drops INT NOT NULL DEFAULT 4,
    cctv_camera_drops INT NOT NULL DEFAULT 8,
    traffic_counter_drops INT NOT NULL DEFAULT 2,
    music_sound_drops INT NOT NULL DEFAULT 2,
    patch_panel_type VARCHAR(80) DEFAULT '24-Port Cat6 RJ45 Unloaded Keystone',
    racks_installed_count INT NOT NULL DEFAULT 1,
    fluke_test_certified BOOLEAN NOT NULL DEFAULT FALSE,
    certification_report_url VARCHAR(500),
    scheduled_start_date DATE NOT NULL,
    scheduled_completion_date DATE NOT NULL,
    actual_completion_date DATE,
    punch_list_items TEXT,
    vendor_invoice_number VARCHAR(50),
    total_cabling_cost_cad DECIMAL(10, 2),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 7. Telecom Circuit Details
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS circuit_details (
    circuit_id SERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    circuit_role VARCHAR(30) NOT NULL CHECK (circuit_role IN ('Primary WAN', 'Secondary Failover WAN', 'Out-of-Band Mgt')),
    carrier_name VARCHAR(80) NOT NULL CHECK (carrier_name IN (
        'Bell Canada', 'Telus', 'Rogers Business', 'Shaw Business',
        'AT&T Commercial', 'Comcast Business', 'Charter Spectrum', 'Cogeco'
    )),
    service_type VARCHAR(50) NOT NULL CHECK (service_type IN (
        'Commercial Fiber DIA', 'Business Broadband Cable', 'VDSL / FTTN',
        'Enterprise 5G / LTE Wireless', 'Satellite Starlink'
    )),
    circuit_id_carrier VARCHAR(100),
    billing_account_number VARCHAR(60),
    order_status VARCHAR(50) NOT NULL DEFAULT 'Order Placed' CHECK (order_status IN (
        'Order Placed', 'Site Survey / Engineering', 'Permit Pending',
        'FOC Confirmed', 'Dispatched / In Construction', 'Demarc Extension Required',
        'Tested & Turned Up (Active)', 'Carrier Delay / Jeopardy', 'Decommissioned'
    )),
    bandwidth_down_mbps INT NOT NULL CHECK (bandwidth_down_mbps > 0),
    bandwidth_up_mbps INT NOT NULL CHECK (bandwidth_up_mbps > 0),
    static_ip_assigned VARCHAR(45),
    subnet_mask VARCHAR(45),
    default_gateway VARCHAR(45),
    primary_dns VARCHAR(45) DEFAULT '8.8.8.8',
    secondary_dns VARCHAR(45) DEFAULT '1.1.1.1',
    cradlepoint_imei VARCHAR(30),
    sim_card_iccid VARCHAR(30),
    foc_date DATE,
    scheduled_turnup_date DATE,
    test_turnup_date DATE,
    demarc_extended BOOLEAN NOT NULL DEFAULT FALSE,
    demarc_extension_vendor VARCHAR(100),
    demarc_location_room VARCHAR(100),
    handoff_interface VARCHAR(30) DEFAULT 'RJ45 1000Base-T',
    monthly_recurring_cad DECIMAL(8, 2),
    jeopardy_reason VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- ----------------------------------------------------------------------------
-- 8. Store Milestones Schedule
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS store_milestones (
    milestone_id SERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    milestone_name VARCHAR(100) NOT NULL CHECK (milestone_name IN (
        'Lease Execution', 'Site Survey Approved', 'Mall Landlord Approval',
        'Permit Issuance', 'General Contractor Mobilization', 'Demarc Extension Completed',
        'Power Poles Installed', 'Cabling Certification Completed', 'Primary Circuit Turn-up',
        'Secondary LTE Turn-up', 'POS Equipment Staging & Shipped', 'POS Commissioning & Test',
        'Merchandising Turnover', 'Pre-Opening Operational Sign-off', 'Grand Opening'
    )),
    baseline_date DATE NOT NULL,
    target_date DATE NOT NULL,
    revised_date DATE,
    actual_completion_date DATE,
    milestone_status VARCHAR(30) NOT NULL DEFAULT 'Pending' CHECK (milestone_status IN (
        'Pending', 'In Progress', 'Completed On Time', 'Completed Delayed', 'Critical Path Delayed'
    )),
    days_variance INT GENERATED ALWAYS AS (
        COALESCE(actual_completion_date, revised_date, target_date) - target_date
    ) STORED,
    owner_role VARCHAR(50) NOT NULL,
    notes VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_store_milestone UNIQUE (store_id, milestone_name)
);

-- ----------------------------------------------------------------------------
-- 9. Weekly Project Operational Snapshots (Frozen Friday History)
-- ----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS weekly_store_snapshots (
    snapshot_id SERIAL PRIMARY KEY,
    store_id INT NOT NULL REFERENCES stores(store_id) ON DELETE CASCADE,
    snapshot_date DATE NOT NULL,
    fiscal_year INT NOT NULL,
    fiscal_week INT NOT NULL,
    operational_status VARCHAR(50) NOT NULL,
    overall_health VARCHAR(20) NOT NULL CHECK (overall_health IN ('On Track', 'At Risk', 'Critical Blocker', 'Complete')),
    site_survey_status VARCHAR(30) NOT NULL,
    power_pole_status VARCHAR(50) NOT NULL,
    cabling_status VARCHAR(50) NOT NULL,
    primary_circuit_status VARCHAR(50) NOT NULL,
    pos_readiness_status VARCHAR(30) NOT NULL,
    primary_blocker_category VARCHAR(50) CHECK (primary_blocker_category IN (
        'None', 'Landlord / Mall Dispute', 'Union Labor Dispute', 'Municipal Permitting Delay',
        'Carrier Circuit Delay (FOC Miss)', 'Demarc Extension Blocked', 'Power Pole Supply Chain',
        'Electrical Contractor Absence', 'Cabling Certification Failure', 'POS Hardware Delivery'
    )),
    blocker_details TEXT,
    days_to_target_opening INT,
    reported_by VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_store_snapshot_date UNIQUE (store_id, snapshot_date)
);
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
CREATE INDEX IF NOT EXISTS idx_wsi_store_cat ON weekly_store_inventory (store_id, category_id, fiscal_year, fiscal_week);
CREATE INDEX IF NOT EXISTS idx_fiscal_calendar_lookup ON fiscal_calendar (fiscal_year, fiscal_week);
-- ============================================================================
-- YM Inc. Store Tracking Project: Migration 005
-- File: db/migrations/005_views.sql
-- Description: Operational Views & Executive Pie/Donut Chart SQL Engines
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1. VIEW: Unified Weekly Store Performance (Store Level Granularity)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_weekly_store_performance AS
SELECT 
    s.store_id,
    s.store_number,
    s.store_name,
    b.banner_id,
    b.banner_code,
    b.banner_name,
    r.region_id,
    r.region_code,
    r.region_name,
    d.district_id,
    d.district_code,
    d.district_name,
    sa.real_estate_format,
    sa.selling_sq_ft,
    w.fiscal_year,
    w.fiscal_week,
    w.target_sales_cad,
    w.prior_year_sales_cad,
    w.net_sales_cad,
    w.cogs_cad,
    w.gross_margin_cad,
    w.gross_margin_pct,
    w.foot_traffic_total,
    w.transaction_count,
    w.units_sold_total,
    w.conversion_rate_pct,
    w.average_transaction_value_cad,
    w.units_per_transaction,
    w.average_unit_retail_cad,
    w.sales_per_selling_sq_ft_cad,
    w.sales_to_budget_variance_pct,
    w.year_over_year_growth_pct,
    w.is_comp_store,
    l.planned_hours,
    l.actual_hours AS labour_hours_worked,
    l.actual_payroll_cost_cad AS labour_cost_cad,
    l.labour_cost_pct_of_sales,
    l.sales_per_labour_hour_cad
FROM weekly_store_sales w
JOIN stores s ON w.store_id = s.store_id
JOIN banners b ON s.banner_id = b.banner_id
JOIN districts d ON s.district_id = d.district_id
JOIN regions r ON d.region_id = r.region_id
LEFT JOIN store_attributes sa ON s.store_id = sa.store_id
LEFT JOIN weekly_store_labour l ON w.store_id = l.store_id 
     AND w.fiscal_year = l.fiscal_year 
     AND w.fiscal_week = l.fiscal_week;

-- ----------------------------------------------------------------------------
-- 2. VIEW: Weekly Banner Rollup (C-Suite Multi-Banner Executive Scorecard)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_weekly_banner_rollup AS
SELECT 
    b.banner_id,
    b.banner_code,
    b.banner_name,
    w.fiscal_year,
    w.fiscal_week,
    COUNT(DISTINCT s.store_id) AS active_store_count,
    SUM(w.target_sales_cad) AS total_target_sales,
    SUM(w.prior_year_sales_cad) AS total_prior_year_sales,
    SUM(w.net_sales_cad) AS total_net_sales,
    SUM(w.gross_margin_cad) AS total_gross_margin,
    ROUND((SUM(w.gross_margin_cad) / NULLIF(SUM(w.net_sales_cad), 0)) * 100, 2) AS blended_gross_margin_pct,
    SUM(w.foot_traffic_total) AS total_foot_traffic,
    SUM(w.transaction_count) AS total_transactions,
    SUM(w.units_sold_total) AS total_units_sold,
    ROUND((SUM(w.transaction_count)::NUMERIC / NULLIF(SUM(w.foot_traffic_total), 0)) * 100, 2) AS blended_conversion_pct,
    ROUND(SUM(w.net_sales_cad) / NULLIF(SUM(w.transaction_count), 0), 2) AS blended_atv,
    ROUND(SUM(w.units_sold_total)::NUMERIC / NULLIF(SUM(w.transaction_count), 0), 2) AS blended_upt,
    ROUND(SUM(w.net_sales_cad) / NULLIF(SUM(sa.selling_sq_ft), 0), 2) AS blended_weekly_spsf,
    ROUND(((SUM(w.net_sales_cad) - SUM(w.target_sales_cad)) / NULLIF(SUM(w.target_sales_cad), 0)) * 100, 2) AS budget_variance_pct,
    ROUND(((SUM(w.net_sales_cad) - SUM(w.prior_year_sales_cad)) / NULLIF(SUM(w.prior_year_sales_cad), 0)) * 100, 2) AS yoy_growth_pct,
    SUM(l.actual_payroll_cost_cad) AS total_labour_cost,
    ROUND((SUM(l.actual_payroll_cost_cad) / NULLIF(SUM(w.net_sales_cad), 0)) * 100, 2) AS blended_labour_pct
FROM weekly_store_sales w
JOIN stores s ON w.store_id = s.store_id
JOIN banners b ON s.banner_id = b.banner_id
LEFT JOIN store_attributes sa ON s.store_id = sa.store_id
LEFT JOIN weekly_store_labour l ON w.store_id = l.store_id 
     AND w.fiscal_year = l.fiscal_year 
     AND w.fiscal_week = l.fiscal_week
WHERE s.store_status = 'Active'
GROUP BY b.banner_id, b.banner_code, b.banner_name, w.fiscal_year, w.fiscal_week;

-- ----------------------------------------------------------------------------
-- 3. VIEW: Store Lifecycle & Deployment Tracking (NSO / Capital Projects)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_store_lifecycle_status AS
SELECT 
    s.store_id,
    s.store_number,
    s.store_name,
    b.banner_code,
    b.banner_name,
    r.region_name,
    d.district_name,
    mp.property_name AS mall_name,
    s.project_type,
    s.operational_status,
    s.overall_health,
    s.pos_staging_status,
    s.target_open_date,
    s.revised_open_date,
    s.actual_open_date,
    (COALESCE(s.revised_open_date, s.target_open_date) - CURRENT_DATE) AS days_until_opening,
    ss.survey_status AS site_survey_status,
    pp.pole_requirement_status AS power_pole_status,
    co.operational_status AS cabling_status,
    co.fluke_test_certified AS cabling_certified,
    c_pri.order_status AS primary_circuit_status,
    c_pri.carrier_name AS primary_carrier,
    c_pri.foc_date AS primary_circuit_foc_date,
    c_sec.order_status AS failover_circuit_status
FROM stores s
JOIN banners b ON s.banner_id = b.banner_id
JOIN districts d ON s.district_id = d.district_id
JOIN regions r ON d.region_id = r.region_id
LEFT JOIN mall_properties mp ON s.mall_id = mp.mall_id
LEFT JOIN site_surveys ss ON s.store_id = ss.store_id
LEFT JOIN power_poles pp ON s.store_id = pp.store_id
LEFT JOIN cabling_operations co ON s.store_id = co.store_id
LEFT JOIN circuit_details c_pri ON s.store_id = c_pri.store_id AND c_pri.circuit_role = 'Primary WAN'
LEFT JOIN circuit_details c_sec ON s.store_id = c_sec.store_id AND c_sec.circuit_role = 'Secondary Failover WAN'
WHERE s.project_type != 'Active Fleet';

-- ----------------------------------------------------------------------------
-- 4. VIEW: Telecom & Carrier Circuit Readiness Tracker
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_circuit_readiness_tracker AS
SELECT 
    s.store_number,
    s.store_name,
    b.banner_name,
    cd.circuit_id,
    cd.circuit_role,
    cd.carrier_name,
    cd.service_type,
    cd.circuit_id_carrier,
    cd.order_status,
    cd.bandwidth_down_mbps,
    cd.bandwidth_up_mbps,
    cd.foc_date,
    cd.test_turnup_date,
    cd.demarc_extended,
    cd.jeopardy_reason
FROM circuit_details cd
JOIN stores s ON cd.store_id = s.store_id
JOIN banners b ON s.banner_id = b.banner_id;

-- ----------------------------------------------------------------------------
-- 5. VIEW: CHART 1 Engine (Banner Sales Donut)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_pie_banner_sales AS
SELECT 
    w.fiscal_year,
    w.fiscal_week,
    b.banner_name AS slice_name,
    b.banner_code,
    SUM(w.net_sales_cad) AS net_sales_cad,
    ROUND((SUM(w.net_sales_cad) / SUM(SUM(w.net_sales_cad)) OVER(PARTITION BY w.fiscal_year, w.fiscal_week)) * 100, 2) AS share_pct
FROM weekly_store_sales w
JOIN stores s ON w.store_id = s.store_id
JOIN banners b ON s.banner_id = b.banner_id
WHERE s.store_status = 'Active'
GROUP BY w.fiscal_year, w.fiscal_week, b.banner_id, b.banner_name, b.banner_code;

-- ----------------------------------------------------------------------------
-- 6. VIEW: CHART 2 Engine (Regional Sales Pie)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_pie_regional_sales AS
SELECT 
    w.fiscal_year,
    w.fiscal_week,
    r.region_name AS slice_name,
    r.region_code,
    SUM(w.net_sales_cad) AS regional_net_sales_cad,
    ROUND((SUM(w.net_sales_cad) / SUM(SUM(w.net_sales_cad)) OVER(PARTITION BY w.fiscal_year, w.fiscal_week)) * 100, 2) AS share_pct
FROM weekly_store_sales w
JOIN stores s ON w.store_id = s.store_id
JOIN districts d ON s.district_id = d.district_id
JOIN regions r ON d.region_id = r.region_id
WHERE s.store_status = 'Active'
GROUP BY w.fiscal_year, w.fiscal_week, r.region_id, r.region_name, r.region_code;

-- ----------------------------------------------------------------------------
-- 7. VIEW: CHART 3 Engine (Real Estate Format Donut)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_pie_real_estate_format AS
SELECT 
    w.fiscal_year,
    w.fiscal_week,
    COALESCE(sa.real_estate_format, 'Unspecified') AS slice_name,
    COUNT(DISTINCT s.store_id) AS store_count,
    SUM(w.net_sales_cad) AS total_sales_cad,
    ROUND((SUM(w.net_sales_cad) / SUM(SUM(w.net_sales_cad)) OVER(PARTITION BY w.fiscal_year, w.fiscal_week)) * 100, 2) AS sales_share_pct
FROM weekly_store_sales w
JOIN stores s ON w.store_id = s.store_id
LEFT JOIN store_attributes sa ON s.store_id = sa.store_id
WHERE s.store_status = 'Active'
GROUP BY w.fiscal_year, w.fiscal_week, sa.real_estate_format;

-- ----------------------------------------------------------------------------
-- 8. VIEW: CHART 4 Engine (Merchandise Category Mix Pie)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_pie_category_mix AS
SELECT 
    wsi.fiscal_year,
    wsi.fiscal_week,
    mc.division AS slice_name,
    SUM(wsi.sales_units) AS total_units_sold,
    SUM(wsi.sales_units * wsi.ending_inventory_retail_cad / NULLIF(wsi.ending_stock_on_hand_units, 0)) AS estimated_sales_revenue_cad,
    ROUND((SUM(wsi.sales_units)::NUMERIC / SUM(SUM(wsi.sales_units)) OVER(PARTITION BY wsi.fiscal_year, wsi.fiscal_week)) * 100, 2) AS unit_share_pct
FROM weekly_store_inventory wsi
JOIN merchandise_categories mc ON wsi.category_id = mc.category_id
GROUP BY wsi.fiscal_year, wsi.fiscal_week, mc.division;

-- ----------------------------------------------------------------------------
-- 9. VIEW: CHART 5 Engine (Labour Cost Allocation Donut)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_pie_labour_allocation AS
SELECT 
    wsl.fiscal_year,
    wsl.fiscal_week,
    b.banner_name AS slice_name,
    SUM(wsl.actual_payroll_cost_cad) AS total_payroll_cad,
    ROUND((SUM(wsl.actual_payroll_cost_cad) / SUM(SUM(wsl.actual_payroll_cost_cad)) OVER(PARTITION BY wsl.fiscal_year, wsl.fiscal_week)) * 100, 2) AS payroll_share_pct,
    ROUND((SUM(wsl.actual_payroll_cost_cad) / NULLIF(SUM(wss.net_sales_cad), 0)) * 100, 2) AS labour_cost_pct_of_sales
FROM weekly_store_labour wsl
JOIN stores s ON wsl.store_id = s.store_id
JOIN banners b ON s.banner_id = b.banner_id
JOIN weekly_store_sales wss ON wsl.store_id = wss.store_id 
     AND wsl.fiscal_year = wss.fiscal_year 
     AND wsl.fiscal_week = wss.fiscal_week
WHERE s.store_status = 'Active'
GROUP BY wsl.fiscal_year, wsl.fiscal_week, b.banner_id, b.banner_name;

-- ----------------------------------------------------------------------------
-- 10. VIEW: CHART 6 Engine (Store Fleet Operational Health Tiers)
-- ----------------------------------------------------------------------------
CREATE OR REPLACE VIEW v_pie_fleet_health AS
WITH classified_stores AS (
    SELECT 
        w.fiscal_year,
        w.fiscal_week,
        w.store_id,
        CASE 
            WHEN w.sales_to_budget_variance_pct >= 5.0 THEN 'Tier 1: Exceeding Budget (> +5%)'
            WHEN w.sales_to_budget_variance_pct BETWEEN -2.0 AND 4.99 THEN 'Tier 2: On Target (-2% to +5%)'
            WHEN w.sales_to_budget_variance_pct BETWEEN -10.0 AND -2.01 THEN 'Tier 3: Moderate Lag (-2% to -10%)'
            ELSE 'Tier 4: Critical Underperformance (< -10%)'
        END AS health_tier,
        CASE 
            WHEN w.sales_to_budget_variance_pct >= 5.0 THEN 1
            WHEN w.sales_to_budget_variance_pct BETWEEN -2.0 AND 4.99 THEN 2
            WHEN w.sales_to_budget_variance_pct BETWEEN -10.0 AND -2.01 THEN 3
            ELSE 4
        END AS sort_order
    FROM weekly_store_sales w
    JOIN stores s ON w.store_id = s.store_id
    WHERE s.store_status = 'Active'
)
SELECT 
    fiscal_year,
    fiscal_week,
    health_tier AS slice_name,
    sort_order,
    COUNT(store_id) AS store_count,
    ROUND((COUNT(store_id)::NUMERIC / SUM(COUNT(store_id)) OVER(PARTITION BY fiscal_year, fiscal_week)) * 100, 2) AS fleet_pct
FROM classified_stores
GROUP BY fiscal_year, fiscal_week, health_tier, sort_order;
