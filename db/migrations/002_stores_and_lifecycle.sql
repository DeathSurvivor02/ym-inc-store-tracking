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
