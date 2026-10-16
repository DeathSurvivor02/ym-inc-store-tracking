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
