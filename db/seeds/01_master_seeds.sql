-- ============================================================================
-- YM Inc. Store Tracking Project: Master Seed Data
-- File: db/seeds/01_master_seeds.sql
-- Description: Master Reference Data, US & Canadian Fleet, Banners & Mall Profiles
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1. Banners (Canadian Portfolio + Major US Chains: Charlotte Russe, rue21, Mandee)
-- ----------------------------------------------------------------------------
INSERT INTO banners (banner_code, banner_name, brand_tier, target_demographic) VALUES
('UP',  'Urban Planet',    'Fast Fashion',        'Teens & Young Adults (Ages 16-24)'),
('BN',  'Bluenotes',       'Denim & Casual',      'Universal Youth & Casual Denim (Ages 14-30)'),
('SS',  'Suzy Shier',      'Contemporary Women',  'Modern Working Women (Ages 25-50)'),
('LC',  'Le Château',      'Contemporary Women',  'Elevated Occasionwear & Contemporary Dresses'),
('SRN', 'Sirens',          'Fast Fashion',        'Trend-Focused Young Women (Ages 16-28)'),
('F21', 'Forever 21',      'Fast Fashion',        'Global Fast Fashion & Everyday Style'),
('UK',  'Urban Kids',      'Youth',               'Children & Pre-Teens (Ages 4-16)'),
('W49', 'West 49',         'Action Sports',       'Skateboarders & Streetwear Enthusiasts'),
('STN', 'Stitches',        'Value Apparel',       'Budget-Friendly Youth Essentials'),
('UB',  'Urban Behavior',  'Fast Fashion',        'Affordable Urban Street Trends'),
-- Major US Operating Banners:
('CR',  'Charlotte Russe', 'Junior Fashion',      'Young Trend-Forward Women (Ages 15-28)'),
('R21', 'rue21',           'Fast Fashion',        'Teens & Young Adults Graphic Apparel & Denim (Ages 14-25)'),
('M',   'Mandee',          'Value Apparel',       'Contemporary Value Womenswear & Everyday Essentials (Ages 18-40)')
ON CONFLICT (banner_code) DO UPDATE SET 
    banner_name = EXCLUDED.banner_name,
    brand_tier = EXCLUDED.brand_tier,
    target_demographic = EXCLUDED.target_demographic;

-- ----------------------------------------------------------------------------
-- 2. Regions (Comprehensive Canadian + US Footprint)
-- ----------------------------------------------------------------------------
INSERT INTO regions (region_code, region_name, country_code) VALUES
('ON-GTA', 'Greater Toronto Area & Central Ontario', 'CA'),
('ON-EOW', 'Eastern & Southwestern Ontario',          'CA'),
('QC',     'Quebec & Eastern Canada',                 'CA'),
('WEST',   'Western Canada (AB, BC, SK, MB)',         'CA'),
('ATL',    'Atlantic Canada (NS, NB, NL, PE)',        'CA'),
-- US Regions:
('US-NE',  'US Northeast & Mid-Atlantic',             'US'),
('US-MW',  'US Midwest & Great Lakes',                'US'),
('US-SE',  'US Southeast & Florida',                  'US'),
('US-TX',  'US South & Texas',                        'US'),
('US-W',   'US West Coast & Southwest',               'US')
ON CONFLICT (region_code) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 3. Regional Managers
-- ----------------------------------------------------------------------------
INSERT INTO regional_managers (region_id, first_name, last_name, email, phone, status) VALUES
(1, 'Marcus', 'Vance', 'mvance@yminc.ca', '416-789-1071 x301', 'Active'),
(2, 'Claire', 'Tremblay', 'ctremblay@yminc.ca', '416-789-1071 x302', 'Active'),
(3, 'Jean-Philippe', 'Roy', 'jproy@yminc.ca', '514-555-0182', 'Active'),
(4, 'Sarah', 'Jenkins', 'sjenkins@yminc.ca', '403-555-0199', 'Active'),
(5, 'David', 'MacLeod', 'dmacleod@yminc.ca', '902-555-0144', 'Active'),
(6, 'Robert', 'Kowalski', 'rkowalski@yminc.com', '201-555-0130', 'Active'),
(7, 'Brian', 'Holloway', 'bholloway@yminc.com', '312-555-0155', 'Active'),
(8, 'Jessica', 'Alvarez', 'jalvarez@yminc.com', '305-555-0188', 'Active'),
(9, 'Carlos', 'Reyes', 'creyes@yminc.com', '713-555-0177', 'Active'),
(10,'Amanda', 'Chen', 'achen@yminc.com', '213-555-0166', 'Active')
ON CONFLICT (email) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 4. Districts
-- ----------------------------------------------------------------------------
INSERT INTO districts (district_code, district_name, region_id) VALUES
-- Canada:
('DIST-GTA-N', 'Toronto North & York Region', 1),
('DIST-GTA-W', 'Mississauga & Halton Region',  1),
('DIST-GTA-E', 'Scarborough & Durham Region',  1),
('DIST-OTT',   'Ottawa Valley & Eastern ON',   2),
('DIST-SWO',   'London, Kitchener & Windsor',  2),
('DIST-MTL-C', 'Montreal Centre & West Island',3),
('DIST-MTL-E', 'Laval & North Shore',          3),
('DIST-CAL',   'Calgary & Southern Alberta',   4),
('DIST-EDM',   'Edmonton & Northern Alberta',  4),
('DIST-VAN',   'Greater Vancouver & Fraser',   4),
('DIST-HAL',   'Halifax Metro & Nova Scotia',  5),
-- US Districts:
('DIST-NYNJ',  'New York Metro & Northern NJ', 6),
('DIST-PAPH',  'Philadelphia Metro & Eastern PA', 6),
('DIST-CHI',   'Greater Chicago & Northern IL', 7),
('DIST-FLS',   'South Florida (Miami/Ft Lauderdale)', 8),
('DIST-HOU',   'Greater Houston & Gulf Coast', 9),
('DIST-DFW',   'Dallas-Fort Worth Metroplex', 9),
('DIST-CALA',  'Greater Los Angeles & Orange County', 10)
ON CONFLICT (district_code) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 5. District Managers
-- ----------------------------------------------------------------------------
INSERT INTO district_managers (district_id, first_name, last_name, email, phone, status) VALUES
(1, 'Elena', 'Rostova', 'erostova@yminc.ca', '416-555-0211', 'Active'),
(2, 'Tariq', 'Mansoor', 'tmansoor@yminc.ca', '905-555-0222', 'Active'),
(3, 'Grace', 'Chow',    'gchow@yminc.ca',    '416-555-0233', 'Active'),
(4, 'Marc',  'Belanger','mbelanger@yminc.ca','613-555-0244', 'Active'),
(5, 'Kelly', 'Armstrong','karmstrong@yminc.ca','519-555-0255', 'Active'),
(6, 'Luc',   'Gagnon',  'lgagnon@yminc.ca',  '514-555-0266', 'Active'),
(7, 'Chloe', 'Dubois',  'cdubois@yminc.ca',  '450-555-0277', 'Active'),
(8, 'Brent', 'Morrison','bmorrison@yminc.ca','403-555-0288', 'Active'),
(9, 'Amy',   'Kowalyk', 'akowalyk@yminc.ca', '780-555-0299', 'Active'),
(10,'Jason', 'Wong',    'jwong@yminc.ca',    '604-555-0311', 'Active'),
(11,'Heather','Campbell','hcampbell@yminc.ca','902-555-0322', 'Active'),
-- US Leaders:
(12,'Michael','Santoro','msantoro@yminc.com', '201-555-0333', 'Active'),
(13,'Danielle','Levy',  'dlevy@yminc.com',    '215-555-0344', 'Active'),
(14,'Kevin', 'Miller',  'kmiller@yminc.com',  '312-555-0355', 'Active'),
(15,'Sofia', 'Castillo','scastillo@yminc.com','305-555-0366', 'Active'),
(16,'Marcus','Washington','mwashington@yminc.com','713-555-0377', 'Active'),
(17,'Rachel','Adams',   'radams@yminc.com',   '214-555-0388', 'Active'),
(18,'Steven','Nguyen',  'snguyen@yminc.com',  '714-555-0399', 'Active')
ON CONFLICT (email) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 6. Merchandise Categories
-- ----------------------------------------------------------------------------
INSERT INTO merchandise_categories (category_code, category_name, division, target_margin_pct) VALUES
('CAT-WDN', 'Womens Denim',               'Denim',                  62.00),
('CAT-MDN', 'Mens Denim',                 'Denim',                  60.00),
('CAT-WTOP','Womens Tops & Blouses',      'Womenswear',             58.50),
('CAT-WDRS','Womens Dresses & Jumpsuits', 'Womenswear',             65.00),
('CAT-MOUT','Mens Outerwear & Hoodies',   'Menswear',               56.00),
('CAT-KAPP','Kids General Apparel',       'Kids & Youth',           54.00),
('CAT-ACC', 'Jewelry & Small Accessories','Footwear & Accessories', 70.00),
('CAT-FTW', 'Casual Footwear & Slides',   'Footwear & Accessories', 52.00)
ON CONFLICT (category_code) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 7. Landmark Commercial Mall Properties (Canada & US)
-- ----------------------------------------------------------------------------
INSERT INTO mall_properties (mall_code, property_name, landlord_company, property_type, address_line1, city, state_province, postal_code, country_code) VALUES
-- Canada:
('MALL-YORK', 'Yorkdale Shopping Centre',       'Oxford Properties',     'Enclosed Regional Mall', '3401 Dufferin St', 'Toronto', 'ON', 'M6A 2T9', 'CA'),
('MALL-EATN', 'CF Toronto Eaton Centre',        'Cadillac Fairview',     'Enclosed Regional Mall', '220 Yonge St',     'Toronto', 'ON', 'M5B 2H1', 'CA'),
('MALL-SQONE','Square One Shopping Centre',     'Oxford Properties',     'Enclosed Regional Mall', '100 City Centre Dr','Mississauga','ON','L5B 2C9', 'CA'),
('MALL-WEM',  'West Edmonton Mall',             'Triple Five Group',     'Enclosed Regional Mall', '8882 170 St NW',   'Edmonton','AB', 'T5T 4J2', 'CA'),
('MALL-METRO','Metropolis at Metrotown',        'Ivanhoé Cambridge',     'Enclosed Regional Mall', '4700 Kingsway',    'Burnaby', 'BC', 'V5H 4M5', 'CA'),
('MALL-VGH',  'Vaughan Mills',                  'Ivanhoé Cambridge',     'Outlet Centre',          '1 Bass Pro Mills Dr','Vaughan','ON','L4K 5W4', 'CA'),
('MALL-HRT',  'Heartland Town Centre',          'Orlando Corporation',   'Power Centre / Big Box', '6045 Mavis Rd',    'Mississauga','ON','L5R 4G6', 'CA'),
-- US Malls:
('MALL-GSP',  'Westfield Garden State Plaza',   'Unibail-Rodamco-Westfield','Enclosed Regional Mall','One Garden State Plaza','Paramus','NJ','07652','US'),
('MALL-ROOS', 'Roosevelt Field',                'Simon Property Group',  'Enclosed Regional Mall', '630 Old Country Rd', 'Garden City', 'NY', '11530', 'US'),
('MALL-KOP',  'King of Prussia Mall',           'Simon Property Group',  'Enclosed Regional Mall', '160 N Gulph Rd',    'King of Prussia','PA','19406','US'),
('MALL-WOOD', 'Woodfield Mall',                 'Simon Property Group',  'Enclosed Regional Mall', '5 Woodfield Mall',  'Schaumburg',  'IL', '60173', 'US'),
('MALL-SAW',  'Sawgrass Mills',                 'Simon Property Group',  'Outlet Centre',          '12801 W Sunrise Blvd','Sunrise',    'FL', '33323', 'US'),
('MALL-GALL', 'The Galleria Houston',           'Simon Property Group',  'Enclosed Regional Mall', '5085 Westheimer Rd','Houston',     'TX', '77056', 'US'),
('MALL-QUEE', 'Queens Center Mall',             'Macerich',              'Enclosed Regional Mall', '90-15 Queens Blvd', 'Elmhurst',    'NY', '11373', 'US'),
('MALL-WBGE', 'Woodbridge Center',              'Jones Lang LaSalle',    'Enclosed Regional Mall', '250 Woodbridge Ctr Dr','Woodbridge','NJ','07095', 'US')
ON CONFLICT (mall_code) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 8. Mall Landlord Requirements
-- ----------------------------------------------------------------------------
INSERT INTO mall_requirements (mall_id, union_labor_required, union_local_designation, coi_required, minimum_general_liability_cad, coi_additional_insured_text, delivery_window_start, delivery_window_end, night_work_only, badging_fee_per_tech_cad)
SELECT 
    m.mall_id,
    CASE WHEN m.landlord_company IN ('Cadillac Fairview', 'Unibail-Rodamco-Westfield') THEN TRUE ELSE FALSE END,
    CASE 
        WHEN m.landlord_company = 'Cadillac Fairview' THEN 'IBEW Local 353 / Carpenters Local 27' 
        WHEN m.landlord_company = 'Unibail-Rodamco-Westfield' THEN 'IBEW Local 164 / NJ Carpenters'
        ELSE NULL 
    END,
    TRUE,
    CASE WHEN m.landlord_company IN ('Cadillac Fairview', 'Oxford Properties', 'Simon Property Group') THEN 10000000.00 ELSE 5000000.00 END,
    'Landlord, its parent entities, managing agents, and mortgagees named as Additional Insureds',
    '06:00:00', '09:00:00',
    CASE WHEN m.landlord_company IN ('Cadillac Fairview', 'Unibail-Rodamco-Westfield') THEN TRUE ELSE FALSE END,
    25.00
FROM mall_properties m
ON CONFLICT (mall_id) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 9. Sample Retail Stores (Canadian Fleet 6xxx + US Chains 6000s)
-- ----------------------------------------------------------------------------
INSERT INTO stores (store_number, banner_id, district_id, mall_id, store_name, project_type, operational_status, overall_health, store_status, address_line1, city, state_province, postal_code, country_code, gross_sqft, sales_sqft, opening_date, target_open_date) VALUES
-- Canadian Fleet (6xxx Series):
('6101', 1, 1, 1, 'Urban Planet Yorkdale',        'Active Fleet',        'Grand Open',            'On Track', 'Active', '3401 Dufferin St CRU-102', 'Toronto',     'ON', 'M6A 2T9', 'CA', 18500, 15000, '2015-08-15', NULL),
('6142', 2, 1, 1, 'Bluenotes Yorkdale Flagship',   'Major Remodel',       'Low-Voltage & Power Poles','At Risk','Active', '3401 Dufferin St CRU-218B','Toronto',     'ON', 'M6A 2T9', 'CA', 8500,  7200,  '2002-11-20', '2026-11-27'),
('6302', 3, 2, 3, 'Suzy Shier Square One',        'Active Fleet',        'Grand Open',            'On Track', 'Active', '100 City Centre Dr #2-104','Mississauga', 'ON', 'L5B 2C9', 'CA', 5200,  4400,  '2010-04-12', NULL),
('6405', 4, 1, 2, 'Le Château CF Eaton Centre',   'Active Fleet',        'Grand Open',            'On Track', 'Active', '220 Yonge St Unit 140',   'Toronto',     'ON', 'M5B 2H1', 'CA', 7800,  6500,  '2021-09-01', NULL),
('6501', 6, 1, 2, 'Forever 21 Eaton Centre',      'Active Fleet',        'Grand Open',            'On Track', 'Active', '220 Yonge St Unit 300',   'Toronto',     'ON', 'M5B 2H1', 'CA', 24000, 20500, '2022-03-15', NULL),
('6601', 8, 1, 6, 'West 49 Vaughan Mills',        'Active Fleet',        'Grand Open',            'On Track', 'Active', '1 Bass Pro Mills Dr',     'Vaughan',     'ON', 'L4K 5W4', 'CA', 6000,  5000,  '2018-05-10', NULL),
('6102', 1, 2, 7, 'Urban Planet Heartland Big Box','Active Fleet',        'Grand Open',            'On Track', 'Active', '6045 Mavis Rd Unit 12',   'Mississauga', 'ON', 'L5R 4G6', 'CA', 22000, 19000, '2016-10-01', NULL),
('6201', 2, 8, 4, 'Bluenotes West Edmonton Mall', 'Active Fleet',        'Grand Open',            'On Track', 'Active', '8882 170 St NW #1204',    'Edmonton',    'AB', 'T5T 4J2', 'CA', 9000,  7600,  '2012-07-22', NULL),
('6701', 5, 3, 1, 'Sirens Scarborough Town Centre','Active Fleet',       'Grand Open',            'On Track', 'Active', '300 Borough Dr CRU-41',   'Scarborough', 'ON', 'M1P 4P5', 'CA', 4800,  4000,  '2014-06-18', NULL),
('6801', 7, 1, 1, 'Urban Kids Yorkdale',          'Active Fleet',        'Grand Open',            'On Track', 'Active', '3401 Dufferin St CRU-105', 'Toronto',     'ON', 'M6A 2T9', 'CA', 6500,  5500,  '2019-11-05', NULL),
('6105', 1, 10,5, 'Urban Planet Metrotown',       'New Store Opening',   'Site Survey',           'On Track', 'Active', '4700 Kingsway Unit M12',  'Burnaby',     'BC', 'V5H 4M5', 'CA', 19500, 16500, '2026-12-15', '2026-12-15'),

-- US Stores (Strictly 6000s Series):
-- Charlotte Russe (CR):
('6001', 11, 12, 8, 'Charlotte Russe Garden State Plaza', 'Active Fleet', 'Grand Open',          'On Track', 'Active', 'One Garden State Plaza Spc 2104','Paramus','NJ','07652','US', 7500, 6200, '2020-03-01', NULL),
('6002', 11, 12, 9, 'Charlotte Russe Roosevelt Field',    'Active Fleet', 'Grand Open',          'On Track', 'Active', '630 Old Country Rd Spc 1140',   'Garden City','NY','11530','US', 8200, 6900, '2021-08-14', NULL),
('6003', 11, 13, 10,'Charlotte Russe King of Prussia',    'Active Fleet', 'Grand Open',          'On Track', 'Active', '160 N Gulph Rd Spc 2410',       'King of Prussia','PA','19406','US', 9100, 7800, '2019-10-22', NULL),
('6004', 11, 15, 12,'Charlotte Russe Sawgrass Mills',     'Major Remodel','Low-Voltage & Power Poles','At Risk','Active', '12801 W Sunrise Blvd Spc 408', 'Sunrise', 'FL', '33323', 'US', 8800, 7400, '2018-04-19', '2026-11-30'),

-- rue21 (R21):
('6010', 12, 16, 13,'rue21 Galleria Houston',            'Active Fleet', 'Grand Open',          'On Track', 'Active', '5085 Westheimer Rd Spc 3600',  'Houston',  'TX', '77056', 'US', 7200, 6000, '2024-09-10', NULL),
('6011', 12, 14, 11,'rue21 Woodfield Mall',              'Active Fleet', 'Grand Open',          'On Track', 'Active', '5 Woodfield Mall Spc H102',     'Schaumburg','IL', '60173', 'US', 7600, 6400, '2024-10-01', NULL),
('6012', 12, 12, 15,'rue21 Woodbridge Center',           'New Store Opening','IT Staging & Telecom','On Track','Active','250 Woodbridge Ctr Spc 120',   'Woodbridge','NJ','07095', 'US', 6800, 5600, '2026-11-15', '2026-11-15'),

-- Mandee (M):
('6020', 13, 12, 14,'Mandee Queens Center',              'Active Fleet', 'Grand Open',          'On Track', 'Active', '90-15 Queens Blvd Spc 115',     'Elmhurst',  'NY', '11373', 'US', 6500, 5400, '2017-06-05', NULL),
('6021', 13, 12, 15,'Mandee Woodbridge Center',           'Active Fleet', 'Grand Open',          'On Track', 'Active', '250 Woodbridge Ctr Spc 240',   'Woodbridge','NJ','07095', 'US', 6200, 5100, '2016-11-18', NULL)
ON CONFLICT (store_number) DO UPDATE SET 
    banner_id = EXCLUDED.banner_id,
    district_id = EXCLUDED.district_id,
    mall_id = EXCLUDED.mall_id,
    store_name = EXCLUDED.store_name,
    project_type = EXCLUDED.project_type,
    operational_status = EXCLUDED.operational_status,
    overall_health = EXCLUDED.overall_health,
    country_code = EXCLUDED.country_code,
    gross_sqft = EXCLUDED.gross_sqft,
    sales_sqft = EXCLUDED.sales_sqft;

-- ----------------------------------------------------------------------------
-- 10. Store Attributes
-- ----------------------------------------------------------------------------
INSERT INTO store_attributes (store_id, real_estate_format, gross_sq_ft, selling_sq_ft, pos_terminal_count, base_annual_rent)
SELECT 
    s.store_id,
    CASE 
        WHEN s.store_name LIKE '%Big Box%' THEN 'Power Centre / Big Box'
        WHEN s.store_name LIKE '%Vaughan Mills%' OR s.store_name LIKE '%Sawgrass Mills%' THEN 'Outlet Centre'
        ELSE 'Enclosed Mall'
    END,
    COALESCE(s.gross_sqft, 8000),
    COALESCE(s.sales_sqft, 6800),
    s.pos_terminal_count,
    COALESCE(s.sales_sqft, 6800) * 45.00
FROM stores s
ON CONFLICT (store_id) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 11. Infrastructure Tracking for Sample Remodel Stores (6142 & 6004)
-- ----------------------------------------------------------------------------
DO $$
DECLARE
    v_bn_id INT;
    v_cr_id INT;
BEGIN
    SELECT store_id INTO v_bn_id FROM stores WHERE store_number = '6142';
    IF v_bn_id IS NOT NULL THEN
        -- Site Survey
        INSERT INTO site_surveys (store_id, survey_vendor, surveyor_name, surveyor_phone, scheduled_date, actual_survey_date, survey_status, ceiling_type, ceiling_height_feet, demarc_location, demarc_conduit_available, mdf_rack_location, mdf_rack_space_adequate)
        VALUES (v_bn_id, 'CompuCom Field Services', 'Dave Miller', '416-555-9012', '2026-09-15', '2026-09-15', 'Completed & Approved', 'Drop Ceiling (2x4 Acoustic Tiles)', 14.50, 'Mall Common Demarc Room B1-14', TRUE, 'Back Office Cash Wrap IDF', TRUE)
        ON CONFLICT DO NOTHING;

        -- Power Pole
        INSERT INTO power_poles (store_id, pole_requirement_status, total_poles_required, poles_installed, finish_color, scheduled_install_date, cash_wrap_feed_drops)
        VALUES (v_bn_id, 'Delivered On Site', 2, 0, 'Matte Black', '2026-11-02', 4)
        ON CONFLICT DO NOTHING;

        -- Cabling
        INSERT INTO cabling_operations (store_id, cabling_vendor, lead_technician_name, operational_status, cable_grade, total_data_drops, pos_drops, ap_wireless_drops, scheduled_start_date, scheduled_completion_date)
        VALUES (v_bn_id, 'Black Box Network Services', 'Terry O''Connor', 'Pathway & J-Hooks Installed', 'Cat6 Plenum (CMP)', 28, 6, 4, '2026-11-04', '2026-11-10')
        ON CONFLICT DO NOTHING;

        -- Telecom Circuits
        INSERT INTO circuit_details (store_id, circuit_role, carrier_name, service_type, order_status, bandwidth_down_mbps, bandwidth_up_mbps, foc_date)
        VALUES 
        (v_bn_id, 'Primary WAN', 'Bell Canada', 'Commercial Fiber DIA', 'FOC Confirmed', 100, 100, '2026-11-08'),
        (v_bn_id, 'Secondary Failover WAN', 'Telus', 'Enterprise 5G / LTE Wireless', 'Tested & Turned Up (Active)', 50, 20, '2026-10-25')
        ON CONFLICT DO NOTHING;
    END IF;

    -- Charlotte Russe Sawgrass Mills (6004) US Infrastructure Tracking
    SELECT store_id INTO v_cr_id FROM stores WHERE store_number = '6004';
    IF v_cr_id IS NOT NULL THEN
        INSERT INTO site_surveys (store_id, survey_vendor, surveyor_name, surveyor_phone, scheduled_date, actual_survey_date, survey_status, ceiling_type, ceiling_height_feet, demarc_location, demarc_conduit_available, mdf_rack_location, mdf_rack_space_adequate)
        VALUES (v_cr_id, 'National Retail Services US', 'Marcus Brody', '954-555-8812', '2026-10-01', '2026-10-02', 'Completed & Approved', 'Drop Ceiling (2x2 Acoustic Tiles)', 15.00, 'Sawgrass Mall Demarc Riser C', TRUE, 'MDF Back Room', TRUE)
        ON CONFLICT DO NOTHING;

        INSERT INTO power_poles (store_id, pole_requirement_status, total_poles_required, poles_installed, finish_color, scheduled_install_date, cash_wrap_feed_drops)
        VALUES (v_cr_id, 'Delivered On Site', 3, 0, 'Anodized Aluminum', '2026-11-05', 4)
        ON CONFLICT DO NOTHING;

        INSERT INTO cabling_operations (store_id, cabling_vendor, lead_technician_name, operational_status, cable_grade, total_data_drops, pos_drops, ap_wireless_drops, scheduled_start_date, scheduled_completion_date)
        VALUES (v_cr_id, 'Granite Telecommunications', 'Hector Ruiz', 'Cable Pull In Progress', 'Cat6 Plenum (CMP)', 32, 6, 6, '2026-11-06', '2026-11-14')
        ON CONFLICT DO NOTHING;

        INSERT INTO circuit_details (store_id, circuit_role, carrier_name, service_type, order_status, bandwidth_down_mbps, bandwidth_up_mbps, foc_date)
        VALUES 
        (v_cr_id, 'Primary WAN', 'Comcast Business', 'Commercial Fiber DIA', 'FOC Confirmed', 200, 200, '2026-11-12'),
        (v_cr_id, 'Secondary Failover WAN', 'AT&T Commercial', 'Enterprise 5G / LTE Wireless', 'Tested & Turned Up (Active)', 100, 25, '2026-10-28')
        ON CONFLICT DO NOTHING;
    END IF;
END $$;

-- ----------------------------------------------------------------------------
-- 12. 4-5-4 Retail Fiscal Calendar 2026 (Sample for Week 41)
-- ----------------------------------------------------------------------------
INSERT INTO fiscal_calendar (calendar_date, fiscal_year, fiscal_quarter, fiscal_month, fiscal_week, day_of_fiscal_week, is_holiday, holiday_name)
SELECT 
    d::DATE,
    2026,
    4,
    10,
    41,
    EXTRACT(DOW FROM d::DATE) + 1,
    CASE WHEN d::DATE = '2026-10-12' THEN TRUE ELSE FALSE END,
    CASE WHEN d::DATE = '2026-10-12' THEN 'Thanksgiving (Canada)' ELSE NULL END
FROM generate_series('2026-10-11'::DATE, '2026-10-17'::DATE, '1 day'::interval) d
ON CONFLICT (calendar_date) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 13. Weekly Store Sales (Fiscal 2026, Week 41)
-- ----------------------------------------------------------------------------
INSERT INTO weekly_store_sales (store_id, fiscal_year, fiscal_week, target_sales_cad, prior_year_sales_cad, gross_sales_cad, refunds_cad, net_sales_cad, cogs_cad, foot_traffic_total, transaction_count, units_sold_total, conversion_rate_pct, average_transaction_value_cad, units_per_transaction, average_unit_retail_cad, sales_per_selling_sq_ft_cad, sales_to_budget_variance_pct, year_over_year_growth_pct, is_comp_store)
SELECT 
    s.store_id,
    2026,
    41,
    -- Target & Prior Year
    CASE 
        WHEN b.banner_code = 'UP'  THEN 145000.00
        WHEN b.banner_code = 'BN'  THEN 82000.00
        WHEN b.banner_code = 'SS'  THEN 64000.00
        WHEN b.banner_code = 'LC'  THEN 75000.00
        WHEN b.banner_code = 'F21' THEN 180000.00
        WHEN b.banner_code = 'CR'  THEN 95000.00
        WHEN b.banner_code = 'R21' THEN 88000.00
        WHEN b.banner_code = 'M'   THEN 58000.00
        ELSE 45000.00
    END,
    CASE 
        WHEN b.banner_code = 'UP'  THEN 138000.00
        WHEN b.banner_code = 'BN'  THEN 79000.00
        WHEN b.banner_code = 'SS'  THEN 61000.00
        WHEN b.banner_code = 'LC'  THEN 72000.00
        WHEN b.banner_code = 'F21' THEN 172000.00
        WHEN b.banner_code = 'CR'  THEN 91000.00
        WHEN b.banner_code = 'R21' THEN 84000.00
        WHEN b.banner_code = 'M'   THEN 55000.00
        ELSE 43000.00
    END,
    -- Gross Sales
    CASE 
        WHEN b.banner_code = 'UP'  THEN 154000.00
        WHEN b.banner_code = 'BN'  THEN 86500.00
        WHEN b.banner_code = 'SS'  THEN 63200.00
        WHEN b.banner_code = 'LC'  THEN 78900.00
        WHEN b.banner_code = 'F21' THEN 192000.00
        WHEN b.banner_code = 'CR'  THEN 102000.00
        WHEN b.banner_code = 'R21' THEN 92500.00
        WHEN b.banner_code = 'M'   THEN 60800.00
        ELSE 48000.00
    END,
    -- Refunds
    CASE 
        WHEN b.banner_code = 'UP'  THEN 5000.00
        WHEN b.banner_code = 'BN'  THEN 2500.00
        WHEN b.banner_code = 'SS'  THEN 2200.00
        WHEN b.banner_code = 'LC'  THEN 3100.00
        WHEN b.banner_code = 'F21' THEN 7000.00
        WHEN b.banner_code = 'CR'  THEN 3500.00
        WHEN b.banner_code = 'R21' THEN 3000.00
        WHEN b.banner_code = 'M'   THEN 1800.00
        ELSE 1500.00
    END,
    -- Net Sales
    CASE 
        WHEN b.banner_code = 'UP'  THEN 149000.00
        WHEN b.banner_code = 'BN'  THEN 84000.00
        WHEN b.banner_code = 'SS'  THEN 61000.00
        WHEN b.banner_code = 'LC'  THEN 75800.00
        WHEN b.banner_code = 'F21' THEN 185000.00
        WHEN b.banner_code = 'CR'  THEN 98500.00
        WHEN b.banner_code = 'R21' THEN 89500.00
        WHEN b.banner_code = 'M'   THEN 59000.00
        ELSE 46500.00
    END,
    -- COGS (approx 40% of net sales)
    CASE 
        WHEN b.banner_code = 'UP'  THEN 59600.00
        WHEN b.banner_code = 'BN'  THEN 33600.00
        WHEN b.banner_code = 'SS'  THEN 24400.00
        WHEN b.banner_code = 'LC'  THEN 28800.00
        WHEN b.banner_code = 'F21' THEN 74000.00
        WHEN b.banner_code = 'CR'  THEN 39400.00
        WHEN b.banner_code = 'R21' THEN 35800.00
        WHEN b.banner_code = 'M'   THEN 23600.00
        ELSE 18600.00
    END,
    -- Traffic, Transactions, Units
    12400, 2150, 5600,
    17.34, 69.30, 2.60, 26.61, 9.93,
    -- Budget Var & YoY Growth
    ROUND((((CASE WHEN b.banner_code='UP' THEN 149000 WHEN b.banner_code='CR' THEN 98500 WHEN b.banner_code='R21' THEN 89500 ELSE 84000 END) - (CASE WHEN b.banner_code='UP' THEN 145000 WHEN b.banner_code='CR' THEN 95000 WHEN b.banner_code='R21' THEN 88000 ELSE 82000 END)) / (CASE WHEN b.banner_code='UP' THEN 145000.0 WHEN b.banner_code='CR' THEN 95000.0 WHEN b.banner_code='R21' THEN 88000.0 ELSE 82000.0 END)) * 100, 2),
    4.50,
    TRUE
FROM stores s
JOIN banners b ON s.banner_id = b.banner_id
WHERE s.store_status = 'Active' AND s.project_type != 'New Store Opening'
ON CONFLICT (store_id, fiscal_year, fiscal_week) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 14. Weekly Store Labour (Fiscal 2026, Week 41)
-- ----------------------------------------------------------------------------
INSERT INTO weekly_store_labour (store_id, fiscal_year, fiscal_week, planned_hours, actual_hours, planned_payroll_cost_cad, actual_payroll_cost_cad, labour_cost_pct_of_sales, sales_per_labour_hour_cad, overtime_hours)
SELECT 
    w.store_id,
    w.fiscal_year,
    w.fiscal_week,
    650.00,
    642.00,
    18200.00,
    18650.00,
    ROUND((18650.00 / NULLIF(w.net_sales_cad, 0)) * 100, 2),
    ROUND(w.net_sales_cad / 642.00, 2),
    12.50
FROM weekly_store_sales w
ON CONFLICT (store_id, fiscal_year, fiscal_week) DO NOTHING;

-- ----------------------------------------------------------------------------
-- 15. Weekly Store Inventory (Fiscal 2026, Week 41)
-- ----------------------------------------------------------------------------
INSERT INTO weekly_store_inventory (store_id, category_id, fiscal_year, fiscal_week, beginning_stock_on_hand_units, received_shipments_units, sales_units, ending_stock_on_hand_units, ending_inventory_retail_cad, ending_inventory_cost_cad, sell_through_pct, weeks_of_supply)
SELECT 
    s.store_id,
    mc.category_id,
    2026,
    41,
    4500,
    1200,
    850,
    4850,
    121250.00,
    48500.00,
    14.91,
    5.70
FROM stores s
CROSS JOIN merchandise_categories mc
WHERE s.store_status = 'Active' AND s.project_type != 'New Store Opening'
LIMIT 60
ON CONFLICT (store_id, category_id, fiscal_year, fiscal_week) DO NOTHING;
