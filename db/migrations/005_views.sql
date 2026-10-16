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
