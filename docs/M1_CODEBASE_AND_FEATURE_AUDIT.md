# YM Inc. Store Deployment & Telecom Management Platform
## Milestone 1: Comprehensive Codebase, Database & Feature Audit

**Document Version:** 1.0.0  
**Audit Date:** October 20, 2026  
**Audited Target:** YM Inc. Store Deployment & Telecom Management Codebase  
**Auditor:** Teamwork Implementation & QA Worker Agent (`worker_m1`)  
**Repository Working Directory:** `c:\Users\bynoe\Music\Antigravity\YMInc - Software`  
**Classification:** Internal Technical Architecture & Engineering Governance  

---

## Table of Contents

1. [Executive Summary & Audit Scorecard](#1-executive-summary--audit-scorecard)
2. [Complete Inventory Matrix](#2-complete-inventory-matrix)
   - 2.1 [Desktop Application Modules, Views, Services & Models](#21-desktop-application-modules-views-services--models)
   - 2.2 [Database Architecture, Migrations & Schema Inventory](#22-database-architecture-migrations--schema-inventory)
   - 2.3 [Current Limitations, Missing Components & Architectural Debt](#23-current-limitations-missing-components--architectural-debt)
3. [Functional Completeness Analysis Across Core Retail Domains](#3-functional-completeness-analysis-across-core-retail-domains)
   - 3.1 [Store Profile & Lifecycle Tracking](#31-store-profile--lifecycle-tracking)
   - 3.2 [Structured Cabling (Cat6 Plenum) Management](#32-structured-cabling-cat6-plenum-management)
   - 3.3 [Power Pole (Wiremold 25DTP-4) Installation](#33-power-pole-wiremold-25dtp-4-installation)
   - 3.4 [Dual-WAN Telecom Turn-Up Architecture](#34-dual-wan-telecom-turn-up-architecture)
   - 3.5 [NRF 4-5-4 Retail Fiscal Calendar Tracking](#35-nrf-4-5-4-retail-fiscal-calendar-tracking)
   - 3.6 [Landlord & Mall Compliance Workflows](#36-landlord--mall-compliance-workflows)
   - 3.7 [Salt & Pepper PBKDF2 Authentication & Identity Management](#37-salt--pepper-pbkdf2-authentication--identity-management)
4. [Critical Defect & Vulnerability Deep-Dive](#4-critical-defect--vulnerability-deep-dive)
   - 4.1 [Defect 1: Fatal Table Name Mismatch (`telecom_circuits` vs `circuit_details`)](#41-defect-1-fatal-table-name-mismatch-telecom_circuits-vs-circuit_details)
   - 4.2 [Defect 2: Silent WAN Circuit Data Loss in `StoreDetailWindow` & `SaveStoreAsync`](#42-defect-2-silent-wan-circuit-data-loss-in-storedetailwindow--savestoreasync)
   - 4.3 [Defect 3: Critical Migration Runner Omission of `006_app_users_auth.sql`](#43-defect-3-critical-migration-runner-omission-of-006_app_users_authsql)
   - 4.4 [Defect 4: Zero `updated_at` Triggers & Broken Cache Invalidation](#44-defect-4-zero-updated_at-triggers--broken-cache-invalidation)
   - 4.5 [Defect 5: 4-Week NRF Calendar Offset Between Code & Standard](#45-defect-5-4-week-nrf-calendar-offset-between-code--standard)
   - 4.6 [Defect 6: Unauthenticated Account Takeover via `ResetPasswordWindow`](#46-defect-6-unauthenticated-account-takeover-via-resetpasswordwindow)
   - 4.7 [Defect 7: Hardcoded `FallbackPepper` Secret Exposure in Compiled Binary](#47-defect-7-hardcoded-fallbackpepper-secret-exposure-in-compiled-binary)
   - 4.8 [Defect 8: Plaintext Fallback Team Credentials in Compiled Code](#48-defect-8-plaintext-fallback-team-credentials-in-compiled-code)
   - 4.9 [Defect 9: Local JSON File Silos Causing Complete Multi-User Concurrency Failure](#49-defect-9-local-json-file-silos-causing-complete-multi-user-concurrency-failure)
   - 4.10 [Defect 10: Completely Mocked CSV Roster Import in `RosterImportWindow`](#410-defect-10-completely-mocked-csv-roster-import-in-rosterimportwindow)
   - 4.11 [Defect 11: Threading Anti-Patterns, `async void` & Sequential Save Latency](#411-defect-11-threading-anti-patterns-async-void--sequential-save-latency)
5. [Comprehensive Audit & Gap Matrix Summary Table](#5-comprehensive-audit--gap-matrix-summary-table)
6. [Forensic Verification Log & Methodology](#6-forensic-verification-log--methodology)

---

## 1. Executive Summary & Audit Scorecard

### 1.1 Context & Scope
The YM Inc. Store Deployment & Telecom Management platform manages capital deployments, low-voltage Cat6 cabling infrastructure, dual-channel power pole installations, dual-WAN telecom turn-ups, landlord mall rules, and NRF 4-5-4 retail calendar milestones across **376 retail stores** spanning **13 retail apparel banners** (including Urban Planet, Bluenotes, Sirens, Suzy Shier, Forever 21 Canada, Charlotte Russe, rue21, and Mandee) throughout Canada and the United States.

This audit report represents an exhaustive forensic review of the source code, database migrations, configuration profiles, and operational workflows comprising:
1. **Desktop Client:** `.NET 10.0 (net10.0-windows)` Windows Presentation Foundation (WPF) application with raw ADO.NET (Npgsql 10.0.3).
2. **Database Tier:** PostgreSQL 16 hosted on AWS us-east-1 via Neon Serverless, utilizing a 3NF relational schema.
3. **Web Prototype:** React/Vite web application in `frontend/` utilizing mock data (`mockStores.ts`).
4. **Data Ingestion & Scripts:** Node.js, Bash, and PowerShell migration and seeding tools.

### 1.2 Audit Scorecard

| Subsystem / Domain | Health Grade | Critical Defects | Architectural Health Assessment |
|---|:---:|:---:|---|
| **Client UI & Presentation** | **B+** | 0 | Polished Microsoft 365 enterprise dark/light theme, custom vector Donut Chart control, responsive layout. High visual presentation quality. |
| **Client MVVM Architecture** | **F** | 2 | **Zero ViewModels exist.** Complete violation of WPF MVVM pattern. 100% of state, database calls, and UI mutations are coupled directly in code-behind files (`.xaml.cs`). |
| **Data Persistence & ORM** | **D-** | 3 | Direct 2-tier database coupling over port 5432; fatal SQL table mismatch rolls back store creation; WAN circuit edits are silently discarded during save. |
| **Security & Cryptography** | **D** | 3 | Strong PBKDF2 parameters (100k iterations, 32-byte salt), but completely compromised by a hardcoded fallback pepper in compiled DLL, plaintext fallback passwords, and an unauthenticated password reset window. |
| **Telecom & Low-Voltage** | **C-** | 2 | Comprehensive schema for Cat6 CMP, Wiremold 25DTP-4, and circuits, but client UI strips out IP routing/IMEI data, drops Fluke LinkWare uploads, and never persists circuit edits. |
| **Landlord & Mall Governance** | **C** | 1 | Detailed compliance tracking ($5M-$10M COI, night work, delivery docks), but compromised by severe schema drift (31 unversioned columns on `mall_properties`) and unvalidated holiday blackout scheduling. |
| **Concurrency & Collaboration** | **F** | 2 | Complete failure in multi-user environments. Presence leases, audit history logs, and daily work orders are written to local workstation JSON files via `FileSystemWatcher`. |
| **Asynchrony & Performance** | **D+** | 2 | Prolific `async void` event handlers; sync-over-async threadpool blocking; 6-second polling loop executing full database queries; 144 sequential unbatched roundtrips on fleet save. |

---

## 2. Complete Inventory Matrix

### 2.1 Desktop Application Modules, Views, Services & Models

The desktop application is located in `desktop-app/YMInc.StoreDeploymentApp.csproj`. It targets `net10.0-windows` with WPF enabled and references a single external package: `Npgsql` (v10.0.3).

```
desktop-app/
├── App.xaml / App.xaml.cs
├── AssemblyInfo.cs
├── Controls/
│   └── DonutChartControl.cs
├── Models/
│   ├── AuditLogEntry.cs
│   ├── FiscalCalendarDay.cs
│   ├── StoreDeployment.cs
│   ├── User.cs
│   └── WeeklyProgressReport.cs
├── Services/
│   ├── AuditHistoryService.cs
│   ├── AuthService.cs
│   ├── DatabaseService.cs
│   ├── PasswordSecurityService.cs
│   ├── PresenceService.cs
│   ├── SeedDataService.cs
│   └── ThemeService.cs
└── Views/
    ├── ActivityHistoryWindow.xaml / .cs
    ├── AddStoreWindow.xaml / .cs
    ├── FiscalCalendarWindow.xaml / .cs
    ├── LoginWindow.xaml / .cs
    ├── MainWindow.xaml / .cs
    ├── MallRulesWindow.xaml / .cs
    ├── ResetPasswordWindow.xaml / .cs
    ├── RosterImportWindow.xaml / .cs
    └── StoreDetailWindow.xaml / .cs
```

#### Detailed Desktop Inventory Table:

| Component Name | Relative Path | Type | Lines of Code | Core Responsibility | Current Architectural Status & Deficits |
|---|---|---|:---:|---|---|
| **App** | `App.xaml.cs` | Application Entry | 83 | Application bootstrap, CLI argument routing (`--verify-ui`), unhandled exception trapping. | **Defect:** Crash logger uses `File.WriteAllText` (overwrites previous crash log). Exception handlers swallow disk errors. |
| **DonutChartControl** | `Controls/DonutChartControl.cs` | Custom WPF Control | 175 | Custom low-level vector rendering (`OnRender`) drawing concentric status rings (Certified, In Progress, Pending, Blocked). | **Defect:** Static `ThemeService.ThemeChanged` event handler in constructor never unsubscribes, causing memory leaks. |
| **StoreDeployment** | `Models/StoreDeployment.cs` | Domain Model | 263 | Master retail store entity aggregating `MallProperty`, `SiteSurvey`, `PowerPole`, `CablingOperation`, and `TelecomCircuit`. | **Defect:** Plain Old CLR Objects (POCOs) lacking `INotifyPropertyChanged`. UI does not update reactively on property change. |
| **FiscalCalendarDay** | `Models/FiscalCalendarDay.cs` | Domain Model | 270 | NRF 4-5-4 calendar day entity, `CalendarWorkItem`, and task categorization enums. | Implements `INotifyPropertyChanged`, but tightly couples UI brush colors (`DayCardBackground`) directly to `ThemeService.IsDarkTheme`. |
| **WeeklyProgressReport** | `Models/WeeklyProgressReport.cs` | Domain Model | 87 | Aggregates milestone execution metrics across weekly snapshots for executive reporting. | Pure data holder. Correctly structured. |
| **User** | `Models/User.cs` | Domain Model | 51 | User profile and role definition (`PM`, `PA`, `PS1`-`PS4`, `ADMIN`). | **Defect:** Hardcoded RBAC permissions. `CanEditAllInformation`, `CanApproveMilestones`, `CanImportRoster` all unconditionally return `true`. |
| **AuditLogEntry** | `Models/AuditLogEntry.cs` | Domain Model | 58 | Represents historical field modification entries. | Pure data holder. Correctly structured. |
| **DatabaseService** | `Services/DatabaseService.cs` | Data Access Service | 1,645 | Raw ADO.NET SQL commands for PostgreSQL 16. Handles store loads, saves, user authentication, and calendar generation. | **Critical Defect:** Contains `telecom_circuits` SQL bug (lines 965, 981); omits circuits in `SaveStoreAsync` (lines 678-797); stores work items in local JSON (line 1001); 4-week calendar offset (line 1242). |
| **AuthService** | `Services/AuthService.cs` | Authentication Service | 209 | Manages authentication against `app_users` table with offline in-memory fallback. | **Critical Defect:** Sync-over-async blocking via `.GetAwaiter().GetResult()` (line 131); hardcoded plaintext passwords in offline team list (lines 23-31). |
| **PasswordSecurityService** | `Services/PasswordSecurityService.cs` | Cryptographic Service | 163 | Implements PBKDF2 (HMAC-SHA256, 100k iterations, 32-byte salt) with constant-time verification. | **Critical Defect:** Hardcoded `FallbackPepper` in compiled binary (line 19); password length validation only checks `>= 6` (line 148). |
| **PresenceService** | `Services/PresenceService.cs` | Concurrency Service | 113 | Detects concurrent editors on store records using heartbeats and JSON leases. | **Critical Defect:** Writes to local `active_editor_leases.json` (line 22); checks `ProcessId != Environment.ProcessId` (line 83). Fails across different machines. |
| **AuditHistoryService** | `Services/AuditHistoryService.cs` | Auditing Service | 225 | In-memory and local disk change logging with CSV export. | **Critical Defect:** Writes to local `audit_history_log.json` (line 15), capped at 500 records; changes made by other technicians never appear. |
| **ThemeService** | `Services/ThemeService.cs` | UI Theming Service | 450 | Microsoft 365 Light/Dark theming engine using recursive visual tree traversal. | **Architectural Risk:** Direct property mutation of elements via `ApplyThemeRecursive` rather than WPF dynamic resource dictionaries. |
| **SeedDataService** | `Services/SeedDataService.cs` | Mock Data Service | 710 | Hardcoded in-memory fixtures for 24 retail stores across Canadian and US banners. | Provides offline mock state; modifications made offline are lost upon process termination. |
| **MainWindow** | `Views/MainWindow.xaml.cs` | WPF Window / Code-Behind | 937 | Dual-view executive interface: Overview (KPIs, donut chart, milestones) and Store Directory (data grids, filters, inspector). | **Defects:** All UI logic and state in code-behind (no ViewModel); sequential unbatched save loop; 6-second polling loop running on UI thread. |
| **StoreDetailWindow** | `Views/StoreDetailWindow.xaml.cs` | WPF Window / Code-Behind | 403 | 5-tab technical engineering inspector (Survey, Power Poles, Cabling, WAN Circuits, Landlord Compliance). | **Critical Defect:** `SaveButton_Click` completely omits saving WAN circuits; displays green success checkmark even on database failure; memory leak on theme event. |
| **AddStoreWindow** | `Views/AddStoreWindow.xaml.cs` | WPF Window / Code-Behind | 185 | Store onboarding modal dialog collecting brand, district, mall, and technical tracks. | **Critical Defect:** Fails against live PostgreSQL due to `telecom_circuits` SQL bug; offline additions evaporate on exit. |
| **FiscalCalendarWindow** | `Views/FiscalCalendarWindow.xaml.cs` | WPF Window / Code-Behind | 512 | NRF 4-5-4 retail calendar view with holiday badges and daily work order scheduling. | Work items are saved only to local JSON on workstation; no validation preventing scheduling during Q4 holiday shopping blackout freeze. |
| **MallRulesWindow** | `Views/MallRulesWindow.xaml.cs` | WPF Window / Code-Behind | 178 | Landlord compliance matrix tracking insurance, night permits, delivery windows, and badging fees. | Static event memory leak; direct cell editing triggers unmonitored background saves. |
| **LoginWindow** | `Views/LoginWindow.xaml.cs` | WPF Window / Code-Behind | 114 | User authentication dialog with quick-login buttons, password reset link, and theme toggle. | Exposes test credentials on UI buttons; `async void` login execution; static event memory leak. |
| **ResetPasswordWindow** | `Views/ResetPasswordWindow.xaml.cs` | WPF Window / Code-Behind | 124 | Self-service password reset form. | **Critical Security Vulnerability:** Unauthenticated password reset allows anyone to overwrite any user password without proof of identity or authorization. |
| **RosterImportWindow** | `Views/RosterImportWindow.xaml.cs` | WPF Window / Code-Behind | 41 | Bulk CSV import dialog for fleet roster synchronization. | **Critical Defect:** Entirely mocked. Clicking Browse hardcodes text; clicking Sync shows a message box without reading or parsing any file. |
| **ActivityHistoryWindow** | `Views/ActivityHistoryWindow.xaml.cs` | WPF Window / Code-Behind | 79 | Chronological audit log viewer with CSV export button. | Displays only local workstation JSON logs; static event memory leak. |

---

### 2.2 Database Architecture, Migrations & Schema Inventory

The database is PostgreSQL 16 hosted on AWS us-east-1 via Neon Serverless (`ep-shiny-hill-b7bouvcl-pooler.c-13.us-east-1.aws.neon.tech`). It consists of **23 base tables** and **9 active analytical views**.

```
db/
├── init/
│   ├── 01_schema.sql (47.9 KB - Docker initialization, lacks migration 006)
│   └── 02_seeds.sql (28.8 KB - Baseline seeds)
├── migrations/
│   ├── 001_management_hierarchy.sql (6.8 KB)
│   ├── 002_stores_and_lifecycle.sql (16.4 KB)
│   ├── 003_analytics_and_facts.sql (8.3 KB)
│   ├── 004_indexes.sql (3.1 KB)
│   ├── 005_views.sql (12.4 KB)
│   └── 006_app_users_auth.sql (1.4 KB - Omitted from automated runners)
├── seeds/
│   └── 01_master_seeds.sql (28.8 KB)
├── apply_migrations.ps1 (Omits migration 006)
├── apply_migrations.sh (Omits migration 006)
├── generate_fiscal_calendar.js (NRF 4-5-4 generator, 1,092 daily rows)
├── migrate.js (Universal migration runner, omits migration 006)
├── seed_app_users.js (Populates app_users table)
├── seed_technical_specs_and_snapshots.js (Generates snapshots for 376 stores)
└── sync_mall_requirements_and_contacts.js (Ad-hoc script causing schema drift)
```

#### Complete Database Tables Inventory Table:

| # | Table Name | Columns | Primary Key | Foreign Key Dependencies | Row Count (Live DB) | Migration Source & Integrity Notes |
|:---:|---|:---:|---|---|:---:|---|
| 1 | `banners` | 8 | `banner_id` (SMALLSERIAL) | None | 13 | `001_management_hierarchy.sql`. 13 retail banners (10 CA, 3 US). Synchronized. |
| 2 | `regions` | 7 | `region_id` (SMALLSERIAL) | None | 10 | `001_management_hierarchy.sql`. 5 Canadian + 5 US operational regions. |
| 3 | `regional_managers` | 8 | `manager_id` (SMALLSERIAL) | `region_id -> regions` (RESTRICT) | 10 | `001_management_hierarchy.sql`. Missing index on `region_id`. |
| 4 | `districts` | 8 | `district_id` (SMALLSERIAL) | `region_id -> regions` (RESTRICT) | 18 | `001_management_hierarchy.sql`. 11 CA districts + 7 US districts. |
| 5 | `district_managers` | 8 | `manager_id` (SMALLSERIAL) | `district_id -> districts` (RESTRICT) | 18 | `001_management_hierarchy.sql`. Missing index on `district_id`. |
| 6 | `mall_properties` | 31 | `mall_id` (SERIAL) | None | 335 | `001_management_hierarchy.sql`. **Severe Drift:** Expanded from 15 to 31 columns by ad-hoc script `sync_mall_requirements_and_contacts.js`. |
| 7 | `mall_requirements` | 13 | `requirement_id` (SERIAL) | `mall_id -> mall_properties` (CASCADE) | 365 | `001_management_hierarchy.sql`. **Severe Drift:** Dropped and recreated as a store-mall contact log with `store_id` and `store_number`. |
| 8 | `stores` | 35 | `store_id` (SERIAL) | `banner_id`, `district_id`, `mall_id` | 376 | `002_stores_and_lifecycle.sql`. Master store record. 15 operational statuses. Redundant sqft columns with `store_attributes`. |
| 9 | `store_attributes` | 15 | `attribute_id` (SERIAL) | `store_id -> stores` (CASCADE, UNIQUE) | 376 | `002_stores_and_lifecycle.sql`. 1:1 store extension. Gross/selling sqft duplicates `stores`. |
| 10 | `store_contacts` | 9 | `contact_id` (SERIAL) | `store_id -> stores` (CASCADE) | 376 | `002_stores_and_lifecycle.sql`. Store manager, assistant manager, field contacts. |
| 11 | `site_surveys` | 19 | `survey_id` (SERIAL) | `store_id -> stores` (CASCADE, UNIQUE) | 376 | `002_stores_and_lifecycle.sql`. Physical survey parameters (ceiling type, height, demarc, MDF rack). |
| 12 | `power_poles` | 19 | `power_pole_id` (SERIAL) | `store_id -> stores` (CASCADE, UNIQUE) | 376 | `002_stores_and_lifecycle.sql`. Wiremold 25DTP-4 fixturing, dual-channel isolation, drops, inspection. |
| 13 | `cabling_operations` | 22 | `cabling_id` (SERIAL) | `store_id -> stores` (CASCADE, UNIQUE) | 376 | `002_stores_and_lifecycle.sql`. Cat6 CMP drop counts, patch panels, Fluke certification status. |
| 14 | `circuit_details` | 28 | `circuit_id` (SERIAL) | `store_id -> stores` (CASCADE) | 752 | `002_stores_and_lifecycle.sql`. Primary Fiber DIA & Secondary LTE failover circuits (2 per store). **Mismatched with desktop code.** |
| 15 | `store_milestones` | 15 | `milestone_id` (SERIAL) | `store_id -> stores` (CASCADE) | **0** | `002_stores_and_lifecycle.sql`. **Dead Table:** 0 rows. Never populated or utilized by the desktop application. |
| 16 | `weekly_store_snapshots` | 16 | `snapshot_id` (SERIAL) | `store_id -> stores` (CASCADE) | 752 | `002_stores_and_lifecycle.sql`. Historical deployment progress for Weeks 41 & 42 across all 376 stores. |
| 17 | `fiscal_calendar` | 10 | `calendar_date` (DATE) | None | 1,092 | `003_analytics_and_facts.sql`. NRF 4-5-4 retail calendar covering 2025–2027. **4-week desync with desktop code.** |
| 18 | `daily_store_metrics` | 18 | `metric_id` (BIGSERIAL) | `store_id`, `calendar_date -> fiscal_calendar` | **0** | `003_analytics_and_facts.sql`. **Dead Table:** 0 rows. POS terminal daily sales and traffic telemetry. |
| 19 | `weekly_store_sales` | 22 | `weekly_sales_id` (BIGSERIAL) | `store_id -> stores` (CASCADE) | 18 | `003_analytics_and_facts.sql`. Only initial 18 seed stores have sales facts. 358 stores have NULL/zero sales data. |
| 20 | `weekly_store_labour` | 12 | `weekly_labour_id` (BIGSERIAL) | `store_id -> stores` (CASCADE) | 18 | `003_analytics_and_facts.sql`. Payroll and labor hour allocations for 18 sample stores. |
| 21 | `weekly_store_inventory` | 14 | `inventory_id` (BIGSERIAL) | `store_id -> stores` (CASCADE) | 60 | `003_analytics_and_facts.sql`. Inventory units and weeks of supply across 18 stores. FK to categories dropped. |
| 22 | `antigravity_pipeline_runs` | 10 | `run_id` (BIGSERIAL) | None | **0** | `003_analytics_and_facts.sql`. Telemetry table for automated AI pipeline executions. Currently unpopulated. |
| 23 | `app_users` | 15 | `user_id` (SERIAL) | None | 6 | `006_app_users_auth.sql`. User authentication with PBKDF2 Salt & Pepper. **Omitted from automated migration runners.** |
| - | `merchandise_categories` | - | - | - | **DROPPED** | `003_analytics_and_facts.sql`. Dropped by `sync_mall_requirements_and_contacts.js`, breaking `v_pie_category_mix`. |

#### Analytical Database Views Inventory:

| View Name | Source Tables | Purpose | Status in Production |
|---|---|---|---|
| `v_weekly_store_performance` | `weekly_store_sales`, `stores`, `banners`, `districts`, `regions`, `store_attributes`, `weekly_store_labour` | Comprehensive store KPI scorecard (Sales, SPSF, UPT, ATV, Conversion, Budget Variance). | Active |
| `v_weekly_banner_rollup` | `weekly_store_sales`, `stores`, `banners`, `store_attributes`, `weekly_store_labour` | Executive portfolio summary aggregated by retail banner. | Active |
| `v_pie_banner_sales` | `weekly_store_sales`, `stores`, `banners` | Revenue market share distribution across banners. | Active |
| `v_pie_regional_sales` | `weekly_store_sales`, `stores`, `districts`, `regions` | Revenue market share distribution across geographical territories. | Active |
| `v_pie_real_estate_format` | `weekly_store_sales`, `stores`, `store_attributes` | Revenue performance by venue format (Enclosed Mall, Power Centre, Street Front, Outlet). | Active |
| `v_pie_labour_allocation` | `weekly_store_labour`, `stores`, `banners`, `weekly_store_sales` | Labor spend distribution and payroll efficiency ratios across banners. | Active |
| `v_pie_fleet_health` | `weekly_store_sales`, `stores` | Tiered store health categorization based on budget variance. | Active |
| `v_circuit_readiness_tracker` | `circuit_details`, `stores`, `banners` | Telecom turn-up readiness monitoring across Primary and Secondary WAN circuits. | Active |
| `v_store_lifecycle_status` | `stores`, `banners`, `districts`, `regions`, `mall_properties`, `site_surveys`, `power_poles`, `cabling_operations`, `circuit_details` | Unified construction and technology rollout tracking sheet. | Active |
| `v_pie_category_mix` | `weekly_store_inventory`, `merchandise_categories` | Inventory merchandise category share. | **DROPPED** (Broken by unversioned table deletion) |

---

### 2.3 Current Limitations, Missing Components & Architectural Debt

1. **Direct 2-Tier Database Coupling:** The desktop application connects directly to Neon PostgreSQL over port 5432 using elevated credentials (`neondb_owner`). There is no intermediary API gateway, meaning every workstation has raw connection credentials capable of altering schemas or deleting tables.
2. **Missing MVVM Architectural Layer:** Despite being developed in WPF, there are zero ViewModels, zero `ICommand` bindings, and zero dependency injection containers. All application state is stored in private variables in code-behind `.xaml.cs` files.
3. **Local Workstation Data Silos:** Operational work items (`live_calendar_work_items.json`), audit history (`audit_history_log.json`), and editor presence leases (`active_editor_leases.json`) are stored on the local disk of individual client laptops, preventing real-time team collaboration.
4. **Disconnected Web Frontend:** The web client located in `frontend/` is completely disconnected from the PostgreSQL database, loading static hardcoded mock data (`mockStores.ts`).
5. **Schema Drift via Ad-Hoc Scripts:** Table schemas on the live database have drifted significantly from versioned migration files in `db/migrations/` due to unmanaged manual Node.js scripts.

---

## 3. Functional Completeness Analysis Across Core Retail Domains

### 3.1 Store Profile & Lifecycle Tracking

The platform models store deployment lifecycles across **15 operational statuses** and **13 retail banners** for **376 retail stores**:

```
[1. Planning] ──► [2. Lease Execution] ──► [3. Site Survey] ──► [4. Permitting & Design]
       │
       ▼
[5. GC Construction] ──► [6. Low-Voltage & Power Poles] ──► [7. IT Staging & Telecom]
       │
       ▼
[8. Merchandising] ──► [9. Pre-Opening QA] ──► [10. Grand Open] ──► [11. Open (Active Fleet)]
       │
       ├─► [12. Renovating]
       ├─► [13. Temporary Closed]
       ├─► [14. Closed]
       └─► [15. Decommissioned]
```

#### Fleet Composition:
- **Canadian Banners (230 Stores):** Urban Planet (UP), Bluenotes (BN), Sirens (SS), Suzy Shier (LC), Stitches (SRN), Forever 21 Canada (F21), Urban Kids (UK), West 49 (W49), Stone Ridge (STN), Urban Behavior (UB).
- **US Banners (146 Stores):** Charlotte Russe (CR), rue21 (R21), Mandee (M).

#### Real Estate & Deployment Metrics:
- **Store Profiles:** Captures unit numbers, store formats (Enclosed Mall, Power Centre, Street Front, Strip Plaza, Outlet Mall), square footage, and POS terminal staging statuses (`Pending`, `Hardware Staged`, `Configured`, `Shipped`, `On Site`, `Commissioned`).
- **Milestone Scheduling:** Tracks key milestone dates including `landlord_possession_date`, `construction_start_date`, `cabling_turnover_date`, `merchandising_start_date`, `target_open_date`, `revised_open_date`, and `actual_open_date`.
- **Architectural Gaps Identified:**
  - **Normalization Redundancy:** Square footage is modeled in duplicate columns: `stores.gross_sqft` (INT) / `stores.sales_sqft` (INT) vs `store_attributes.gross_sq_ft` (NUMERIC) / `store_attributes.selling_sq_ft` (NUMERIC). In several imported stores, values exist in one table but are NULL in the other.
  - **Dead Table (`store_milestones`):** The relational table `store_milestones`, designed to track baseline, target, revised, and actual completion dates with variance calculations for all 15 stages, contains **0 rows** in production. The desktop client never writes to it.

---

### 3.2 Structured Cabling (Cat6 Plenum) Management

Low-voltage telecommunications cabling is modeled in `cabling_operations` (`db/migrations/002_stores_and_lifecycle.sql:188-224`):

#### Cabling Specifications:
- **Cable Grades:** Strictly enforces `Cat6 Plenum (CMP)` or `Cat6A Plenum (CMP)` for mall environments where return air circulates through open ceiling plenums. `Cat6 Riser (CMR)` is restricted to vertical riser shafts.
- **Hardware Standards:** Standardizes on `24-Port Cat6 RJ45 Unloaded Keystone Panels` and `48-Port Cat6 RJ45 Unloaded Keystone Panels` mounted in 2-post or 4-post open MDF racks.

#### Mathematical Drop Allocation Rules:
The platform enforces structured data drop calculations based on retail square footage and register density:

$$\text{Total Data Drops} = \text{POS Drops} + \text{AP Drops} + \text{Back-Office Drops} + \text{CCTV Drops} + \text{Traffic Drops} + \text{Sound Drops}$$

| Subsystem | Allocation Formula | Typical Count | Destination & Hardware |
|---|---|:---:|---|
| **Cash Wrap / POS** | $1 \text{ Primary} + 1 \text{ PINpad / Aux per Register} + 1 \text{ Manager}$ | 4 – 8 Drops | Veras POS terminals, Verifone/Ingenico PINpads, receipt printers, cash drawers |
| **Wireless APs** | $1 \text{ AP per } 2,000 \text{ sq.ft. (minimum 4)}$ | 4 – 8 Drops | Cisco Meraki MR46/MR56 Wi-Fi 6 APs (Sales floor, fitting rooms, stockroom) |
| **Back Office** | Dedicated office drops | 4 – 6 Drops | Store Manager PC, network printer, VoIP manager phone, security alarm |
| **CCTV Surveillance**| Perimeter, cash wrap, entrance, and stockroom | 8 – 20 Drops | IP HD dome/fisheye cameras, cash wrap zoom cameras, NVR uplink |
| **Foot Traffic Sensor**| Overhead lease-line sensor | 2 Drops | Overhead stereoscopic optical sensor (WSPR traffic counting) |
| **Music / Audio** | Audio amplifier uplink | 2 Drops | Streaming media player (Mood Media) |
| **Total Standard Runs**| Fleet standard across store formats | **24 – 48 Drops** | Terminated to patch panels with blue CMP jackets |

#### Fluke Certification Protocols & Gaps:
- **Testing Standard:** Low-voltage vendors (Granite, Black Box, direct contractors) are required to test 100% of terminations using a Fluke DSX-5000 or DSX-8000 CableAnalyzer, certifying Wiremap, Length, Propagation Delay, Delay Skew, Return Loss, and Near-End Crosstalk (NEXT).
- **Implementation Deficits:**
  - `fluke_test_certified` is a single boolean flag in the database.
  - The column `certification_report_url VARCHAR(500)` is completely unpopulated (0 rows contain a URL).
  - The desktop client provides only a simple ComboBox ("Certified & Passed" vs "Pending Certification"). There is no mechanism to upload or view Fluke LinkWare (`.flw`) or PDF summary reports.
  - Port-level patch panel mapping schedules (Port 1 -> POS 1, Port 2 -> AP 1) are not modeled in the database.

---

### 3.3 Power Pole (Wiremold 25DTP-4) Installation

Cash wrap counters situated in open retail floor spaces cannot receive floor-trench electrical feeds without core drilling into tenant spaces below. YM Inc. standardizes on ceiling-drop power poles.

```
          ┌────────────────────────────────────────────────────────┐
          │             STRUCTURAL CONCRETE CEILING DECK           │
          └───────┬────────────────────────────────────────┬───────┘
                  │ 12-Gauge Structural Tie-Wire           │ Rigid 1/2" EMT Conduit
                  │ (Direct to concrete eyelet)            │ (Dedicated 20A Feed)
                  ▼                                        ▼
     ~~~~~~~~~~~~~[ T-Bar Grid Clamp Bracket ]~~~~~~~~~~~~~~~~~~~~~~~~~ Suspended Ceiling Grid
                  │
                  │   WIREMOLD 25DTP-4 ALUMINUM POWER POLE
                  │  ┌───────────────────┬───────────────────┐
                  │  │ CHANNEL 1: POWER  │ CHANNEL 2: DATA   │
                  │  │ 2x 20A 120V NEMA  │ 4x Cat6 Plenum    │
                  │  │ Isolated Grounds  │ POS & Pinpad Runs │
                  │  │ (Black/White/Grn) │ (Blue CMP 550MHz) │
                  │  └───────────────────┴───────────────────┘
                  │
                  ▼
          ┌────────────────────────────────────────────────────────┐
          │              CASH WRAP COUNTER / POS STAGING           │
          └────────────────────────────────────────────────────────┘
```

#### Specification Standards:
- **Hardware Standard:** **Wiremold / Legrand 25DTP-4** Series (`pole_model_number`).
- **Dual-Channel Isolation:** `dual_channel_isolated = TRUE`. A continuous internal steel barrier separates high-voltage electrical circuits from low-voltage data cabling, eliminating 60Hz electromagnetic interference (EMI) on Cat6 runs.
- **Finishes:** Standardized to `Anodized Aluminum`, `Matte Black`, `Custom White`, and `Industrial Grey`.
- **Ceiling Mounting Constraints:**
  - **Acoustic Drop Ceiling (2x4 / 2x2 T-Bar Tiles):** Requires a T-bar grid clamp bracket augmented with 12-gauge structural tie-wire anchored directly into the structural concrete deck above. Poles must never be supported solely by the suspended ceiling grid.
  - **Open Plenum / Exposed Deck (16–18+ ft):** In big-box stores and high-ceiling formats (e.g., Metrotown #6105 with 18ft deck), requires structural steel beam clamps, unistrut trapeze bracing, and pole extension couplers.
  - **Floor / Wall Feed Alternates:** When landlord regulations or structural obstructions prohibit ceiling poles, the system supports wall chases or floor coring (`Not Required (Wall / Floor Feed)`).
- **Inspection & Deficiency Tracking:**
  - Tracks inspection lifecycles: `Survey Required` -> `Engineering Design` -> `Equipment Ordered` -> `Delivered On Site` -> `Installed & Anchored` -> `Passed Electrical Inspection`.
  - Captures blocker reasons (`blocker_reason`) such as missing electrical permits, lack of tie-wire anchoring, or failed ESA/municipal inspections.

---

### 3.4 Dual-WAN Telecom Turn-Up Architecture

The telecom architecture enforces high-availability dual-WAN redundancy across all 376 retail stores to support uninterrupted cloud POS transactions and credit card processing:

```
                    ┌────────────────────────────────────────────────────────┐
                    │               YM Inc. Retail Store Edge                │
                    │               (Cisco Meraki MX68 Router)               │
                    └───────────────────┬────────────────┬───────────────────┘
                                        │ WAN 1          │ WAN 2 (Failover)
                                        ▼                ▼
     ┌──────────────────────────────────────┐        ┌──────────────────────────────────────┐
     │          PRIMARY WAN CIRCUIT         │        │     SECONDARY FAILOVER WAN CIRCUIT   │
     │  Carrier: Bell Canada / Comcast      │        │  Carrier: Telus / AT&T Commercial    │
     │  Type: Commercial Fiber DIA          │        │  Type: Enterprise 5G / LTE Wireless  │
     │  Bandwidth: 100 - 250 Mbps Symmetric │        │  Hardware: Cradlepoint W1850 / IBR600│
     │  Interface: RJ45 1000Base-T Hand-off │        │  Bandwidth: 50-100M Down / 20-25M Up │
     │  SLA: 99.99% with 4h MTTR            │        │  SIM: High-Data Priority M2M APN     │
     └───────────────────┬──────────────────┘        └──────────────────────────────────────┘
                         │ Cross-Connect Riser
                         ▼
     ┌──────────────────────────────────────┐
     │        MALL COMMON DEMARC ROOM       │
     │  Location: Basement / Telecom Room   │
     │  Riser Pull: Low-Voltage Cat6/Fiber  │
     │  Distance: 150 - 500+ Feet to MDF    │
     └──────────────────────────────────────┘
```

#### Carrier Distribution Matrix:
| Territory | Primary WAN Carriers | Primary Service Type | Secondary Failover Carriers | Secondary Service Type |
|---|---|---|---|---|
| **Canada (CA)** | **Bell Canada**, **Rogers Business**, **Telus**, **Shaw/Cogeco** | Commercial Fiber DIA (100–250 Mbps) | **Telus**, **Rogers Business** | Enterprise 5G / LTE Wireless (Cradlepoint) |
| **United States (US)** | **Comcast Business**, **Charter Spectrum**, **AT&T Commercial** | Fiber DIA / Commercial Broadband Cable | **AT&T Commercial**, **Verizon Wireless** | Enterprise 5G / LTE Wireless (Cradlepoint) |

#### Provisioning Lifecycle (9 States):
Circuits progress through nine ordered states in `circuit_details.order_status`:
1. `Order Placed` — Carrier order booked with service account number.
2. `Site Survey / Engineering` — Carrier field tech inspects street splice and mall point of presence.
3. `Permit Pending` — Municipal right-of-way permit or landlord riser license in review.
4. `FOC Confirmed` — Firm Order Commitment date officially issued by carrier.
5. `Dispatched / In Construction` — Outside plant (OSP) fiber pull and splicing in progress.
6. `Demarc Extension Required` — Carrier hand-off completed at mall basement demarc; riser extension to store MDF pending.
7. `Tested & Turned Up (Active)` — RFC 2544 throughput/latency test passed; Meraki WAN interface active.
8. `Carrier Delay / Jeopardy` — Exception state triggered by missed FOC date or construction blocker.
9. `Decommissioned` — Circuit disconnected upon store relocation or closure.

#### Demarc Extensions & IP Telemetry:
- **Demarcation Routing:** In shopping malls, carrier fiber terminates in a common basement telecom closet (e.g. `Mall Basement Demarc B1-14`). A low-voltage contractor (Granite, Black Box) must run a riser extension (150–500+ ft Cat6 or single-mode fiber) to the store backroom MDF rack.
- **Network Telemetry in Schema:** The database schema captures:
  - `static_ip_assigned`, `subnet_mask`, `default_gateway`, `primary_dns`, `secondary_dns`
  - `cradlepoint_imei`, `sim_card_iccid`
  - `demarc_extension_vendor`, `foc_date`, `test_turnup_date`, `scheduled_turnup_date`
- **Critical Software Defects Identified:**
  - **Fatal Schema Mismatch:** `DatabaseService.CreateStoreAsync` targets non-existent table `telecom_circuits` instead of `circuit_details`.
  - **Silent Data Loss:** `StoreDetailWindow.xaml.cs` and `DatabaseService.SaveStoreAsync` completely omit updating WAN circuits on save.
  - **UI Telemetry Omission:** The desktop UI exposes only carrier, service type, bandwidth, and status; IP addresses and Cradlepoint serials are completely hidden from operators.

---

### 3.5 NRF 4-5-4 Retail Fiscal Calendar Tracking

The National Retail Federation (NRF) 4-5-4 calendar divides the retail fiscal year into four 13-week quarters. Each quarter follows a 4-week, 5-week, 4-week monthly pattern (52 weeks = 364 days). This standard aligns retail sales and inventory performance year-over-year by ensuring identical shopping weekend comparisons.

```
       QUARTER 1 (13 WEEKS)            QUARTER 2 (13 WEEKS)
 ┌──────────┬──────────┬──────────┐ ┌──────────┬──────────┬──────────┐
 │ Month 1  │ Month 2  │ Month 3  │ │ Month 4  │ Month 5  │ Month 6  │
 │ (4 Wks)  │ (5 Wks)  │ (4 Wks)  │ │ (4 Wks)  │ (5 Wks)  │ (4 Wks)  │
 └──────────┴──────────┴──────────┘ └──────────┴──────────┴──────────┘
       QUARTER 3 (13 WEEKS)            QUARTER 4 (13 WEEKS)
 ┌──────────┬──────────┬──────────┐ ┌──────────┬──────────┬──────────┐
 │ Month 7  │ Month 8  │ Month 9  │ │ Month 10 │ Month 11 │ Month 12 │
 │ (4 Wks)  │ (5 Wks)  │ (4 Wks)  │ │ (4 Wks)  │ (5 Wks)  │ (4 Wks)  │
 └──────────┴──────────┴──────────┘ └──────────┴──────────┴──────────┘
```

#### Fiscal Year Boundaries:
- Standard NRF calendar years begin on the Sunday closest to January 31 / February 1.
- In `db/generate_fiscal_calendar.js:20-24`, boundaries are correctly configured:
  - FY 2025: February 2, 2025
  - FY 2026: February 1, 2026
  - FY 2027: January 31, 2027

#### Holiday Shopping Blackout Freezes:
To safeguard high-volume peak retail revenue, YM Inc. mandates strict deployment blackout freezes during Q4:
- **US Thanksgiving & Black Friday Freeze:** All cabling, telecom turn-ups, and construction work in mall environments are strictly prohibited from Wednesday before Thanksgiving through Cyber Monday.
- **Christmas & Boxing Day Freeze:** Absolute IT deployment freeze from December 15 through January 2.

#### Critical Implementation Defects:
1. **The 4-Week Fiscal Calendar Offset:** In `DatabaseService.cs:1242`, the C# fallback calendar generator hardcodes FY 2026 start to **January 4, 2026** (28 days / 4 weeks earlier than NRF standard February 1, 2026). As a result, mid-October 2026 is assigned to **Fiscal Week 41/42** in the desktop client and seed scripts, whereas the PostgreSQL calendar generated by `generate_fiscal_calendar.js` assigns mid-October to **Fiscal Week 37**. Joining weekly sales facts on `calendar_date` and `fiscal_week` yields **zero matching records**.
2. **Absence of Blackout Scheduling Enforcement:** While `DatabaseService.CheckRetailHoliday` flags `"Black Friday Peak Freeze (All Tech Work Halted)"`, the desktop scheduling dialog (`FiscalCalendarWindow.xaml.cs:440-493`) lacks any validation preventing work items from being scheduled on Black Friday or Christmas Eve.

---

### 3.6 Landlord & Mall Compliance Workflows

Retail buildouts within institutional shopping malls require strict adherence to landlord operational criteria:

#### Institutional Landlord Liability Requirements:
Institutional property owners (Cadillac Fairview, Oxford Properties, Simon Property Group, Unibail-Rodamco-Westfield, Ivanhoé Cambridge, Brookfield) impose rigorous risk mitigation protocols:
- **Commercial General Liability (CGL) Limits:** Minimum **$5,000,000 CAD/USD** for standard shopping centers; mandatory **$10,000,000 CAD/USD** for flagship properties (e.g. Toronto Eaton Centre, Yorkdale, Sawgrass Mills, Roosevelt Field).
- **Additional Insured Endorsements:** Certificates of Insurance (COI) must explicitly name the landlord entity, managing agent, and mortgage lenders as Additional Insureds with minimum 30 days written notice of cancellation.

#### Mall Access & Construction Restrictions:
- **Night Work Mandate & 24/7 Compute Infrastructure Impact (`night_work_only`):** Enclosed shopping centers prohibit noisy construction (hammer drilling into deck, cutting conduit, removing ceiling tiles) during operating hours (10:00–21:00). All low-voltage and power pole work must occur during night shifts (22:30–06:00 local time). Because YM Inc.'s 376 stores span four North American time zones (EST, CST, MST, PST), active night construction windows operate continuously from 22:30 EST to 09:00 EST (06:00 PST). Consequently, the cloud database infrastructure (Neon Serverless PostgreSQL) cannot utilize overnight scale-to-zero; it must maintain a **24/7 warm baseline compute allocation of at least 0.25 Compute Units (CU)** ($69.60/month) to prevent connection timeouts, 3–5 second cold-start latency spikes, and dropped transactions during nocturnal deployment shifts.
- **Security Badging & Fees:** Contractors must register with mall security dispatch. Badging fees ($20.00–$25.00 per technician) and daily security check-ins are tracked.
- **Delivery Dock Windows:** Deliveries of Wiremold poles, MDF racks, and cable spools must utilize designated freight docks during narrow early-morning windows (06:00–09:00).

#### Schema Drift & Technical Debt:
- **Ad-Hoc Schema Alterations:** The live database table `mall_properties` contains 31 columns, compared to 15 columns defined in `001_management_hierarchy.sql`. Unversioned script `sync_mall_requirements_and_contacts.js` appended compliance columns directly onto `mall_properties` and dropped/recreated `mall_requirements` as a store-mall contact log without producing a versioned migration (`007_*.sql`).
- **Missing Expiration Engine:** There is no tracking of COI expiration dates or automated warning alerts when contractor insurance expires during active construction.

---

### 3.7 Salt & Pepper PBKDF2 Authentication & Identity Management

Authentication is managed via `app_users` (`006_app_users_auth.sql`) and `PasswordSecurityService.cs`:

#### Cryptographic Architecture:
- **Key Derivation Function:** PBKDF2 implemented via .NET `Rfc2898DeriveBytes.Pbkdf2`.
- **Pseudorandom Function (PRF):** HMAC-SHA256 (`HashAlgorithmName.SHA256`).
- **Iteration Work Factor:** **100,000 iterations** (`DefaultIterations`).
- **Salt Generation:** `RandomNumberGenerator.GetBytes(32)` producing 32 cryptographically random bytes (256 bits), Base64 encoded per user.
- **Derived Key Length:** 32 bytes (256 bits), Base64 encoded.
- **Verification Logic:** Constant-time byte comparison using `CryptographicOperations.FixedTimeEquals` to prevent side-channel timing attacks.

#### Critical Security Deficiencies & Production Remediation Path:
1. **Hardcoded Fallback Pepper in Compiled Binary:** The fallback pepper `"YM_ENTERPRISE_PEPPER_SECURE_2026_9f8b4c2e1a7d5"` is hardcoded in `PasswordSecurityService.cs:19` and duplicated in `seed_app_users.js:12`. Anyone with a .NET decompiler can extract this string from the client binary.
   - *Remediation Migration Path:* Deprecate client-side pepper storage entirely. Migrate the cryptographic pepper to **Azure Key Vault** (`ym-kv-prod/secrets/password-pepper`), accessed exclusively by the server-side Cloud API Gateway. Workstation clients never receive or process the pepper.
   - *Database Re-Keying Migration:* Migration `007_rekey_legacy_passwords.sql` adds `force_password_reset BOOLEAN DEFAULT FALSE` and `pepper_version INT DEFAULT 1` to `app_users`. On Phase 1 rollout, all legacy accounts hashed with the fallback pepper have `force_password_reset` asserted, requiring users to re-key passwords upon next authentication using the new Key Vault master pepper and PBKDF2-HMAC-SHA256 (100k rounds, 32-byte salt).
   - *Enterprise SSO Cutover:* Transition all internal enterprise roles (PM, PA, PS1–PS4, Regional DMs) to **Microsoft Entra ID (OIDC / PKCE)**, deprecating local password storage in `app_users` for internal staff and retaining it solely for short-lived (8-hour) external contractor credentials.
2. **Missing from `.env.example`:** While `.env` sets `PASSWORD_PEPPER`, the template `.env.example` omits it, causing fresh deployments to silently fall back to the leaked binary pepper.
3. **Unauthenticated Password Reset (Account Takeover):** `ResetPasswordWindow.xaml.cs` and `DatabaseService.ResetUserPasswordAsync` allow any user to reset the password of any account (including `pm` or `admin`) without verifying the current password or providing an out-of-band token.
4. **Hardcoded Plaintext Passwords in Code:** `AuthService.cs:23-31` contains a hardcoded array of internal team credentials with plaintext passwords (`pm123`, `pa123`, `ps1123`, etc.).
5. **Session Management Deficits:** The application stores authentication in a single static property (`AuthService.CurrentUser`). There are no JWTs, bearer tokens, expiration timeouts, or inactivity locks.
6. **Weak Password Validation Policy:** `PasswordSecurityService.ValidatePassword` only checks `password.Length >= 6`, violating NIST SP 800-63B guidelines.

---

## 4. Critical Defect & Vulnerability Deep-Dive

### 4.1 Defect 1: Fatal Table Name Mismatch (`telecom_circuits` vs `circuit_details`)

- **Defect ID:** `DEF-001`
- **Severity:** **P0 — Critical / Blocker**
- **Affected Component:** `desktop-app/Services/DatabaseService.cs:965, 981` (in `CreateStoreAsync`)
- **Exact Code Snippet:**
  ```csharp
  // desktop-app/Services/DatabaseService.cs:964-967, 980-983
  const string insCircSql = @"
      INSERT INTO telecom_circuits (store_id, circuit_role, carrier_name, service_type, order_status, bandwidth_down_mbps, bandwidth_up_mbps)
      VALUES (@StoreId, @Role, @Carrier, @Service, @Status, @Down, @Up);";
  ```
- **Forensic Root Cause Analysis:**
  The PostgreSQL schema migration (`db/migrations/002_stores_and_lifecycle.sql:228`) creates the circuit table under the name **`circuit_details`**. The table `telecom_circuits` **does not exist** in PostgreSQL. When an operator attempts to onboard a new store via `AddStoreWindow` against a live database, executing this query throws:
  `Npgsql.PostgresException (0x80004005): 42P01: relation "telecom_circuits" does not exist`
  Because this statement runs inside an active `NpgsqlTransaction` (`tx`), the unhandled exception causes an immediate rollback of the entire store creation transaction. Furthermore, the default carrier name (`"Bell Dedicated Fiber"`) and service type (`"DIA Fiber 100M"`) supplied in lines 970–972 violate check constraints on `circuit_details`.
- **Operational Impact / Blast Radius:**
  **Complete failure of dynamic store onboarding.** Field technicians and project managers are completely unable to onboard new retail store deployments into the production database.
- **Remediation Blueprint:**
  In `DatabaseService.cs:965, 981`, replace `telecom_circuits` with `circuit_details`. Align default carrier and service strings with check constraints (`"Bell Canada"` and `"Commercial Fiber DIA"`).

---

### 4.2 Defect 2: Silent WAN Circuit Data Loss in `StoreDetailWindow` & `SaveStoreAsync`

- **Defect ID:** `DEF-002`
- **Severity:** **P0 — Critical / Data Loss**
- **Affected Component:** `desktop-app/Views/StoreDetailWindow.xaml.cs:230-360` & `desktop-app/Services/DatabaseService.cs:678-797`
- **Exact Code Snippet:**
  ```csharp
  // desktop-app/Views/StoreDetailWindow.xaml.cs:338-359
  bool storeSaved = await _databaseService.SaveStoreAsync(_store);
  // ...
  if (storeSaved && mallSaved)
  {
      SaveNotificationText.Text = $"✓ SAVED at {DateTime.Now:HH:mm:ss} — Store #{_store.StoreNumber} and property specifications committed.";
  }
  else
  {
      SaveNotificationText.Text = $"✓ Updated in session & history log ({DateTime.Now:HH:mm:ss})";
  }
  ```
- **Forensic Root Cause Analysis:**
  1. `StoreDetailWindow.xaml` provides an editable interface under Tab 4 ("4. WAN Circuits") allowing operators to edit Primary and Secondary carrier details, service types, bandwidth, and turn-up statuses.
  2. In `StoreDetailWindow.xaml.cs:230-360` (`SaveButton_Click`), the event handler reads and updates properties for `StoreDeployment`, `SiteSurvey`, `PowerPole`, `CablingOperation`, and `MallProperty`. **It completely fails to read, process, or save `_store.Circuits`**.
  3. In `DatabaseService.cs:678-797` (`SaveStoreAsync`), the update method contains sequential `UPDATE` queries for `stores`, `site_surveys`, `power_poles`, and `cabling_operations`. **There is not a single SQL statement targeting `circuit_details`**.
  4. In `StoreDetailWindow.xaml.cs:356-359`, if the save operation fails, the UI still displays a green checkmark saying `"✓ Updated in session & history log"`, actively deceiving the user into believing their changes were committed.
- **Operational Impact / Blast Radius:**
  **Silent data loss.** Any modifications made by network engineers to carrier circuits, circuit IDs, bandwidth, or turn-up statuses are discarded upon application restart.
- **Remediation Blueprint:**
  Add update logic for `_store.Circuits` in `SaveButton_Click` and implement `UPDATE circuit_details` parameterized queries within a database transaction in `DatabaseService.SaveStoreAsync`.

---

### 4.3 Defect 3: Critical Migration Runner Omission of `006_app_users_auth.sql`

- **Defect ID:** `DEF-003`
- **Severity:** **P0 — Critical / Deployment Failure**
- **Affected Component:** `db/migrate.js:43-49`, `db/apply_migrations.ps1:19-25`, `db/apply_migrations.sh:18-24`, and `db/init/01_schema.sql`
- **Exact Code Snippet:**
  ```javascript
  // db/migrate.js:43-49
  const migrationFiles = [
    '001_management_hierarchy.sql',
    '002_stores_and_lifecycle.sql',
    '003_analytics_and_facts.sql',
    '004_indexes.sql',
    '005_views.sql'
  ];
  ```
- **Forensic Root Cause Analysis:**
  The migration file `006_app_users_auth.sql` defines the `app_users` table and associated indexes. However, it was omitted from the `migrationFiles` array in `db/migrate.js`, omitted from the `$migrations` list in `db/apply_migrations.ps1`, omitted from `MIGRATIONS` in `db/apply_migrations.sh`, and omitted from the Docker entrypoint initialization file `db/init/01_schema.sql`.
- **Operational Impact / Blast Radius:**
  Any fresh database deployment executed via Docker or standard migration scripts will completely lack the `app_users` table. When users attempt to log into the desktop client, authentication throws an immediate SQL exception (`relation "app_users" does not exist`).
- **Remediation Blueprint:**
  Add `006_app_users_auth.sql` to `migrationFiles` across all three migration runners and append its DDL to `db/init/01_schema.sql`.

---

### 4.4 Defect 4: Zero `updated_at` Triggers & Broken Cache Invalidation

- **Defect ID:** `DEF-004`
- **Severity:** **P1 — High / Architectural Debt**
- **Affected Component:** Entire database migration catalog (`db/migrations/001–006`) & `DatabaseService.cs:105-124`
- **Exact Code Snippet:**
  ```csharp
  // desktop-app/Services/DatabaseService.cs:112
  await using var cmd = new NpgsqlCommand("SELECT MAX(updated_at) FROM stores;", conn);
  ```
- **Forensic Root Cause Analysis:**
  Although 17 tables define `updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP`, there is **not a single trigger or trigger procedure** in PostgreSQL to update this timestamp when a row is modified (`UPDATE`). Furthermore, the application update queries in `DatabaseService.cs` (`SaveStoreAsync` lines 692–700, `SaveMallAsync` lines 573–612) omit `updated_at = CURRENT_TIMESTAMP`. As a consequence, `SELECT MAX(updated_at) FROM stores;` permanently returns the initial creation timestamp. The 6-second polling loop in `MainWindow.xaml.cs:83-96` (`LiveSyncTimer_Tick`) never detects store changes made by other instances.
- **Operational Impact / Blast Radius:**
  Desktop client cache invalidation and auto-sync are completely non-functional.
- **Remediation Blueprint:**
  Deploy a PostgreSQL trigger procedure `fn_set_updated_at()` and attach it via `BEFORE UPDATE` triggers to all relevant tables. Include `updated_at = CURRENT_TIMESTAMP` in application `UPDATE` statements.

---

### 4.5 Defect 5: 4-Week NRF Calendar Offset Between Code & Standard

- **Defect ID:** `DEF-005`
- **Severity:** **P1 — High / Data Corruption**
- **Affected Component:** `desktop-app/Services/DatabaseService.cs:1239-1244` vs `db/generate_fiscal_calendar.js:20-24`
- **Exact Code Snippet:**
  ```csharp
  // desktop-app/Services/DatabaseService.cs:1239-1244
  DateTime fyStart = fy switch
  {
      2025 => new DateTime(2025, 1, 5),
      2026 => new DateTime(2026, 1, 4),
      _ => new DateTime(2027, 1, 3)
  };
  ```
- **Forensic Root Cause Analysis:**
  Under the official NRF 4-5-4 Retail Fiscal Calendar standard, Fiscal Year 2026 begins on **February 1, 2026** (as generated by `db/generate_fiscal_calendar.js`). However, `DatabaseService.cs` lines 1239–1244 arbitrarily defines the start of FY 2026 as **January 4, 2026** (exactly 4 weeks / 28 days earlier). Because of this discrepancy, the seed scripts (`01_master_seeds.sql:293-304` and `seed_technical_specs_and_snapshots.js:229`) hardcoded mid-October dates (`2026-10-11` to `2026-10-24`) as **Fiscal Weeks 41 and 42**. In the generated database calendar, mid-October 2026 is **Fiscal Week 37**. Joining weekly snapshots or sales to the fiscal calendar on `calendar_date` and `fiscal_week` produces **zero matching records**.
- **Operational Impact / Blast Radius:**
  Corrupts fiscal reporting, historical sales joins, and retail performance comparisons across banners.
- **Remediation Blueprint:**
  Align `DatabaseService.cs` fiscal year start dates with the official NRF calendar (February 1, 2026 for FY 2026) and regenerate fiscal week assignments across all snapshot and sales seeds.

---

### 4.6 Defect 6: Unauthenticated Account Takeover via `ResetPasswordWindow`

- **Defect ID:** `DEF-006`
- **Severity:** **P0 — Critical / Security Vulnerability (CWE-640)**
- **Affected Component:** `desktop-app/Views/ResetPasswordWindow.xaml.cs:66` & `desktop-app/Services/DatabaseService.cs:1570-1640`
- **Exact Code Snippet:**
  ```csharp
  // desktop-app/Services/DatabaseService.cs:1584-1620
  const string checkSql = @"
      SELECT user_id, is_active FROM app_users 
      WHERE LOWER(username) = LOWER(@id) OR LOWER(email) = LOWER(@id)
      LIMIT 1;";
  // ...
  PasswordSecurityService.HashNewPassword(newPassword, out string saltBase64, out string hashBase64);
  const string updateSql = @"
      UPDATE app_users
      SET password_hash = @hash,
          password_salt = @salt,
          iterations = 100000,
          password_changed_at = CURRENT_TIMESTAMP,
          failed_attempts = 0,
          locked_until = NULL,
          updated_at = CURRENT_TIMESTAMP
      WHERE user_id = @userId;";
  ```
- **Forensic Root Cause Analysis:**
  `ResetPasswordWindow` accepts any username or email address and a new password, then calls `AuthService.ResetPasswordAsync()`, which directly invokes `DatabaseService.ResetUserPasswordAsync()`. The method verifies that the user exists and is active, then **immediately overwrites the password hash in PostgreSQL**. There is zero authentication: no current password check, no email reset token, no SMS OTP, no security questions, and no administrative approval.
- **Operational Impact / Blast Radius:**
  **Complete account takeover.** Any user with access to the desktop client can unilaterally seize control of the Project Manager (`pm`) or Administrator accounts.
- **Remediation Blueprint:**
  Deprecate unauthenticated client-side password resets. Require current password validation or integrate out-of-band token verification via an enterprise identity provider (Microsoft Entra ID).

---

### 4.7 Defect 7: Hardcoded `FallbackPepper` Secret Exposure in Compiled Binary

- **Defect ID:** `DEF-007`
- **Severity:** **P1 — High / Cryptographic Vulnerability (CWE-798)**
- **Affected Component:** `desktop-app/Services/PasswordSecurityService.cs:19` & `db/seed_app_users.js:12`
- **Exact Code Snippet:**
  ```csharp
  // desktop-app/Services/PasswordSecurityService.cs:19
  private static readonly string FallbackPepper = "YM_ENTERPRISE_PEPPER_SECURE_2026_9f8b4c2e1a7d5";
  ```
- **Forensic Root Cause Analysis:**
  A pepper is designed as an application-level secret kept strictly separate from the database. By embedding `"YM_ENTERPRISE_PEPPER_SECURE_2026_9f8b4c2e1a7d5"` in plaintext in compiled C# source code, any individual with standard .NET decompilation tools (ILSpy, dotPeek) can extract the secret. Because `PASSWORD_PEPPER` is omitted from `.env.example`, deployments default to this exposed string, nullifying the pepper's security value.
- **Operational Impact / Blast Radius:**
  Eliminates the defense-in-depth benefit of the cryptographic pepper. If the database is compromised, attackers with the extracted binary can immediately run offline dictionary attacks against password hashes.
- **Remediation Blueprint:**
  1. **Source Code Hardening:** Completely remove `FallbackPepper` string literals from `PasswordSecurityService.cs` and `seed_app_users.js`. Configure the cryptographic engine to fail closed (throwing `CryptographicException: Mandatory enterprise pepper secret is unconfigured`) if the secret cannot be resolved from secure environment injection.
  2. **Enterprise Secret Management (Azure Key Vault):** Store the production master pepper in **Azure Key Vault** (`ym-kv-prod/secrets/password-pepper-v2`). Secret resolution occurs strictly within the server-side Cloud API Gateway runtime via Managed Identity. Desktop client binaries never store or receive the pepper.
  3. **Database Re-Keying Migration (`007_rekey_legacy_passwords.sql`):**  
     Provide the exact schema DDL to support cryptographic versioning, dual-pepper verification, and self-service password reset tokens:
     ```sql
     -- db/migrations/007_rekey_legacy_passwords.sql
     -- ============================================================================
     -- YM Inc. Store Tracking Project: Migration 007 - Legacy Password Rekeying & Dual-Pepper Schema
     -- ============================================================================
     ALTER TABLE app_users 
         ADD COLUMN IF NOT EXISTS force_password_reset BOOLEAN NOT NULL DEFAULT FALSE,
         ADD COLUMN IF NOT EXISTS pepper_version INT NOT NULL DEFAULT 1,
         ADD COLUMN IF NOT EXISTS legacy_password_hash VARCHAR(255) NULL,
         ADD COLUMN IF NOT EXISTS password_reset_token VARCHAR(255) NULL,
         ADD COLUMN IF NOT EXISTS password_reset_expires_at TIMESTAMPTZ NULL,
         ADD COLUMN IF NOT EXISTS password_last_rekeyed_at TIMESTAMPTZ NULL;

     -- Stage existing accounts for Dual-Pepper verification and progressive rekeying
     UPDATE app_users 
     SET force_password_reset = TRUE,
         pepper_version = 1,
         legacy_password_hash = password_hash
     WHERE pepper_version = 1;

     DO $$
     BEGIN
         IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'chk_app_users_pepper_version') THEN
             ALTER TABLE app_users 
                 ADD CONSTRAINT chk_app_users_pepper_version 
                 CHECK (pepper_version IN (1, 2));
         END IF;
     END $$;
     ```
  4. **Dual-Pepper Verification Protocol (Zero User Lockout Guarantee):**
     To eliminate user lockouts when rotating peppers from the leaked fallback pepper to Azure Key Vault, the authentication service executes a deterministic two-phase verification:
     - **Phase A (Active Key Vault Pepper Check):** The service computes PBKDF2-HMAC-SHA256 (100k rounds, 32-byte salt) using the active Azure Key Vault Master Pepper (v2). If the hash matches `password_hash`, authentication succeeds immediately.
     - **Phase B (Legacy Pepper Fallback & Auto-Rekey):** If Phase A fails and `pepper_version == 1` (or `legacy_password_hash` is populated), the service checks the password against the legacy fallback pepper (v1). If this matches:
       - The password is valid.
       - The service immediately generates a fresh 32-byte cryptographic salt via `RandomNumberGenerator.GetBytes(32)`.
       - It re-hashes the password using the Azure Key Vault Master Pepper (v2) with 100,000 PBKDF2 iterations.
       - It updates PostgreSQL: `UPDATE app_users SET password_hash = @newHash, password_salt = @newSalt, pepper_version = 2, force_password_reset = FALSE, legacy_password_hash = NULL, password_last_rekeyed_at = NOW() WHERE user_id = @userId;`.
       - The user is logged in transparently. Over time, all legacy accounts are securely rekeyed to Key Vault pepper v2 with **100% zero user lockout**.
  5. **Harden `ResetPasswordWindow.xaml.cs` & SSPR Workflow:**
     Completely eliminate unauthenticated password overwrites. All password resets require either Microsoft Entra ID Self-Service Password Reset (SSPR) or an out-of-band email verification challenge:
     - The user requests a reset; the Cloud API Gateway generates a cryptographically secure 32-byte CSPRNG token (`password_reset_token`) with a 15-minute expiration (`password_reset_expires_at = NOW() + INTERVAL '15 minutes'`).
     - A 6-digit numeric OTP or signed reset link is transmitted out-of-band to the user's registered corporate email (`@yminc.ca` or `@yminc.com`).
     - `ResetPasswordWindow.xaml` requires input of Username/Email, Verification OTP Token, and New Password (enforcing NIST SP 800-63B standards: $\ge 12$ characters, complexity rules, and breach dictionary checks).
  6. **Entra ID SSO Cutover:** Transition all internal enterprise roles (PM, PA, PS1–PS4, Regional DMs) to Microsoft Entra ID (OIDC / PKCE). Deprecate internal user password management in `app_users`, retaining local credential storage exclusively for short-lived (8-hour) external contractor access tokens.

---

### 4.8 Defect 8: Plaintext Fallback Team Credentials in Compiled Code

- **Defect ID:** `DEF-008`
- **Severity:** **P1 — High / Credential Exposure (CWE-259)**
- **Affected Component:** `desktop-app/Services/AuthService.cs:23-31`
- **Exact Code Snippet:**
  ```csharp
  // desktop-app/Services/AuthService.cs:23-31
  var rawUsers = new[]
  {
      (1, "pm", "Marcus Vance", "mvance@yminc.ca", "pm123", UserRole.PM),
      (2, "pa", "Claire Tremblay", "ctremblay@yminc.ca", "pa123", UserRole.PA),
      (3, "ps1", "Terry O'Connor", "toconnor@yminc.ca", "ps123", UserRole.PS1),
      (4, "ps2", "Danielle Levy", "dlevy@yminc.com", "ps2123", UserRole.PS2),
      (5, "ps3", "Hector Ruiz", "hruiz@yminc.com", "ps3123", UserRole.PS3),
      (6, "ps4", "Sarah Jenkins", "sjenkins@yminc.ca", "ps4123", UserRole.PS4)
  };
  ```
- **Forensic Root Cause Analysis:**
  To facilitate offline development, `AuthService.InitializeOfflineTeam()` hardcodes usernames, full names, emails, roles, and plaintext passwords directly in the C# static constructor. Furthermore, `LoginWindow.xaml.cs:40-56` features quick-login buttons that autofill these credentials.
- **Operational Impact / Blast Radius:**
  Production binaries expose internal corporate identities and passwords to anyone who receives the compiled desktop client.
- **Remediation Blueprint:**
  Remove hardcoded credentials from `AuthService.cs`. Replace with an encrypted local SQLite credentials cache populated only after an initial authenticated online login.

---

### 4.9 Defect 9: Local JSON File Silos Causing Complete Multi-User Concurrency Failure

- **Defect ID:** `DEF-009`
- **Severity:** **P0 — Critical / Concurrency Failure**
- **Affected Component:** `DatabaseService.cs:1000-1001`, `AuditHistoryService.cs:15`, `PresenceService.cs:22-25`
- **Exact Code Snippet:**
  ```csharp
  // DatabaseService.cs:1000-1001
  private static readonly string LiveCalendarFilePath = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory, "live_calendar_work_items.json");

  // PresenceService.cs:22-23, 83
  private static readonly string LockFilePath = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory, "active_editor_leases.json");
  // ...
  return leases.FirstOrDefault(l => l.StoreNumber == storeNumber && l.ProcessId != Environment.ProcessId);
  ```
- **Forensic Root Cause Analysis:**
  1. **Work Items Disconnect:** Daily operational work orders (`CalendarWorkItem`), including telecom turn-up dates, cabling pulls, and power pole installations, are serialized to `live_calendar_work_items.json` on the local machine rather than stored in PostgreSQL. Multiple project managers running the desktop app on separate machines cannot see each other's schedules.
  2. **Audit History Silo:** `AuditHistoryService.cs` writes audit records to `audit_history_log.json` on the local machine, capped at 500 entries. Changes made on one laptop are invisible to other team members.
  3. **Local-Only Presence Leasing:** `PresenceService.cs` checks `l.ProcessId != Environment.ProcessId`. This only detects other instances running on the exact same physical computer. Across different laptops, collision detection is non-existent, resulting in silent overwrites.
- **Operational Impact / Blast Radius:**
  **Complete collaboration failure.** In a multi-user environment, project managers, project administrators, and field specialists cannot coordinate schedules, audit trails, or record locks.
- **Remediation Blueprint:**
  Migrate calendar work orders, audit entries, and presence leases to dedicated PostgreSQL tables (`calendar_work_items`, `audit_logs`, `active_editor_leases`) and implement PostgreSQL `LISTEN/NOTIFY` or WebSocket push notifications for distributed synchronization.

---

### 4.10 Defect 10: Completely Mocked CSV Roster Import in `RosterImportWindow`

- **Defect ID:** `DEF-010`
- **Severity:** **P1 — High / Feature Incompleteness**
- **Affected Component:** `desktop-app/Views/RosterImportWindow.xaml.cs:16-33`
- **Exact Code Snippet:**
  ```csharp
  // desktop-app/Views/RosterImportWindow.xaml.cs:16-33
  private void BrowseButton_Click(object sender, RoutedEventArgs e)
  {
      SelectedFileText.Text = "Loaded: importer/sample_store_roster.csv (Validated)";
      StatusResultText.Text = "Storefront records validated against YM Inc. 3NF schema.";
      StatusResultText.Visibility = Visibility.Visible;
      SyncButton.IsEnabled = true;
  }

  private void SyncButton_Click(object sender, RoutedEventArgs e)
  {
      DidImport = true;
      MessageBox.Show(
          "Store roster successfully synchronized with the Production database.",
          "Dataset Synchronized",
          MessageBoxButton.OK,
          MessageBoxImage.Information);
      Close();
  }
  ```
- **Forensic Root Cause Analysis:**
  `RosterImportWindow` is a complete facade. Clicking "Browse" hardcodes a static string without invoking an `OpenFileDialog`. Clicking "Sync" sets a boolean flag and displays a success `MessageBox` without reading, parsing, validating, or writing any CSV data to PostgreSQL.
- **Operational Impact / Blast Radius:**
  Operators cannot bulk import store rosters or batch onboard store records through the desktop application.
- **Remediation Blueprint:**
  Implement a production-grade CSV parsing pipeline using `CsvHelper` with validation against the 3NF database schema and transactional batch insertion via `NpgsqlBinaryImporter`.

---

### 4.11 Defect 11: Threading Anti-Patterns, `async void` & Sequential Save Latency

- **Defect ID:** `DEF-011`
- **Severity:** **P1 — High / Reliability & Performance**
- **Affected Component:** `MainWindow.xaml.cs:758-800`, `LoginWindow.xaml.cs:74`, `AuthService.cs:127-137`
- **Exact Code Snippet:**
  ```csharp
  // MainWindow.xaml.cs:766-778
  foreach (var store in _allStores)
  {
      await _databaseService.SaveStoreAsync(store);
      if (store.Mall != null)
          await _databaseService.SaveMallAsync(store.Mall);
  }
  foreach (var item in _allWorkItems)
  {
      await _databaseService.SaveCalendarWorkItemAsync(item);
  }

  // AuthService.cs:131
  return Task.Run(() => LoginAsync(username, password)).GetAwaiter().GetResult();
  ```
- **Forensic Root Cause Analysis:**
  1. **Sync-Over-Async Threadpool Starvation:** In `AuthService.Login`, invoking `.GetAwaiter().GetResult()` on a task scheduled via `Task.Run` within a WPF synchronization context risks threadpool exhaustion and application deadlocks.
  2. **Prolific `async void` Event Handlers:** Pervasive across all window event handlers (`MainWindow_Loaded`, `SaveAllChangesButton_Click`, `SaveButton_Click`, `SubmitButton_Click`). Unhandled exceptions in `async void` methods bypass caller `try/catch` blocks and terminate the process.
  3. **Sequential Unbatched Roundtrips:** `SaveAllChangesButton_Click` iterates through all 24 stores, executing 4–6 individual queries per store sequentially. Across 24 stores, this represents **144 independent network roundtrips**, taking 8–15 seconds over cloud database connections and locking the user interface without transactional rollback protection.
- **Operational Impact / Blast Radius:**
  Application freezing, intermittent crashes on network errors, and prolonged latency during fleet save operations.
- **Remediation Blueprint:**
  Refactor `SaveAllChangesButton_Click` to execute a single batched transaction (`NpgsqlBatch`), eliminate sync-over-async calls, and replace `async void` event handlers with asynchronous commands via `CommunityToolkit.Mvvm`.

---

## 5. Comprehensive Audit & Gap Matrix Summary Table

| Category | Component / Module | Current Implementation State | Technical Vulnerability / Architectural Gap | Severity | Remediation Milestone |
|---|---|---|---|:---:|:---:|
| **Architecture** | Client MVVM Layer | Zero ViewModels; state in `.xaml.cs` | Violation of MVVM; high coupling; impossible to unit test | P1 | M2 |
| **Architecture** | Dependency Injection | `new DatabaseService()` in windows | High coupling; impossible to mock services | P1 | M2 |
| **Persistence** | `CreateStoreAsync` | Targets `telecom_circuits` | SQL exception (`relation does not exist`); onboarding fails | **P0** | M1 / M2 |
| **Persistence** | `SaveStoreAsync` | Omits `circuit_details` | WAN circuit edits silently lost on restart | **P0** | M1 / M2 |
| **Persistence** | `MainWindow` Save Loop | Sequential unbatched `foreach` | 144 network roundtrips (10+ sec latency); no rollback | P1 | M2 |
| **Persistence** | Database Migrations | `006_app_users_auth.sql` omitted | Fresh deployments lack `app_users` table entirely | **P0** | M1 |
| **Persistence** | `updated_at` Triggers | Zero triggers across 23 tables | Timestamps never update; cache invalidation broken | P1 | M1 |
| **Persistence** | Schema Drift | 31 columns on `mall_properties` | Migration catalog out of sync with production DB | P1 | M1 / M4 |
| **Security** | Password Reset | Unauthenticated `ResetUserPasswordAsync` | Account takeover; anyone can reset any password | **P0** | M1 / M2 |
| **Security** | System Pepper | Hardcoded `FallbackPepper` in DLL | Secret exposed in binary; disables defense-in-depth | P1 | M1 / M2 |
| **Security** | Fallback Credentials | Plaintext passwords in `AuthService` | Production binary exposes internal passwords | P1 | M1 / M2 |
| **Security** | Session Management | Static in-memory `CurrentUser` | No token, no expiration, no idle timeout | P1 | M3 |
| **Security** | Authorization | RBAC hardcoded to `true` in `User.cs` | No operational privilege separation | P1 | M2 / M4 |
| **Telecom** | WAN Circuit Telemetry | Client UI hides IP/IMEI fields | Technicians cannot view/edit static IPs in app | P2 | M2 |
| **Telecom** | Carrier Management | Hardcoded CHECK constraints | Cannot add regional carriers without DDL alter | P2 | M1 |
| **Cabling** | Fluke Certification | Boolean flag in DB | No LinkWare `.flw`/PDF report upload or review | P2 | M2 / M3 |
| **Power Poles** | Fixture Tracking | Dropdown tracking in DB | Missing electrical sign-off signature capture | P2 | M2 / M3 |
| **Fiscal Calendar** | 4-5-4 Synchronization | 4-week offset in `DatabaseService` | Mid-Oct is Wk 41 in code vs Wk 37 in NRF DB | P1 | M1 |
| **Fiscal Calendar** | Holiday Blackouts | Text badges in UI | No validation blocking Black Friday work orders | P2 | M2 |
| **Compliance** | Landlord Rules | Contact log in DB | No COI expiration alerts or PDF repository | P2 | M3 / M4 |
| **Collaboration** | Presence Leasing | Local `active_editor_leases.json` | Fails across different machines; collision hazard | **P0** | M2 / M3 |
| **Collaboration** | Work Orders | Local `live_calendar_work_items.json` | Schedule edits not synced to DB or other users | **P0** | M2 / M3 |
| **Collaboration** | Audit Trail | Local `audit_history_log.json` (500 max) | Audit logs trapped on local client; easily deleted | P1 | M2 / M3 |
| **Integration** | CSV Roster Import | Completely mocked UI | Cannot bulk import stores from CSV | P1 | M2 |
| **Performance** | Polling Loop | 6-second timer queries DB | Resets cell focus during active grid editing | P2 | M2 |
| **Performance** | Threading Anti-Patterns | Pervasive `async void` & sync-over-async | Risk of UI deadlocks and process crashes | P1 | M2 |
| **Infrastructure** | Neon Serverless Compute | Scale-to-zero enabled off-peak | Overnight scale-to-zero breaks night mall construction across 4 time zones; 24/7 baseline (0.25 CU min) required | P1 | M3 / M4 |
| **Web Portal** | SaaS Prototype | Static mock data (`mockStores.ts`) | Disconnected from live PostgreSQL database | P1 | M3 |

---

## 6. Forensic Verification Log & Methodology

All findings, line references, and architectural assessments in this deliverable were verified through direct source inspection and code validation in the repository:

1. **Table Mismatch Verification:** Verified in `desktop-app/Services/DatabaseService.cs:965, 981` (`INSERT INTO telecom_circuits`) vs `db/migrations/002_stores_and_lifecycle.sql:228` (`CREATE TABLE IF NOT EXISTS circuit_details`).
2. **WAN Circuit Data Loss Verification:** Verified in `desktop-app/Views/StoreDetailWindow.xaml.cs:230-360` (`SaveButton_Click`) and `desktop-app/Services/DatabaseService.cs:678-797` (`SaveStoreAsync`), confirming complete omission of circuit persistence.
3. **Migration Runner Omission Verification:** Verified in `db/migrate.js:43-49`, `db/apply_migrations.ps1:19-25`, `db/apply_migrations.sh:18-24`, and `db/init/01_schema.sql`, confirming `006_app_users_auth.sql` is absent.
4. **Trigger Deficit Verification:** Verified via ripgrep across `db/migrations/` and `db/` searching for `TRIGGER`, confirming 0 triggers exist in the database.
5. **Fiscal Calendar Offset Verification:** Verified in `desktop-app/Services/DatabaseService.cs:1242` (`fyStart = 2026-01-04`) vs `db/generate_fiscal_calendar.js:22` (`start = 2026-02-01`), confirming the 28-day (4-week) desynchronization.
6. **Account Takeover Verification:** Verified in `desktop-app/Views/ResetPasswordWindow.xaml.cs:66` and `desktop-app/Services/DatabaseService.cs:1570-1640`, confirming unauthenticated password overwrites.
7. **Secret Exposure Verification:** Verified in `desktop-app/Services/PasswordSecurityService.cs:19` and `db/seed_app_users.js:12`, confirming hardcoded fallback pepper string.
8. **Plaintext Password Verification:** Verified in `desktop-app/Services/AuthService.cs:23-31` (`rawUsers`), confirming hardcoded plaintext passwords.
9. **Local JSON File Silo Verification:** Verified in `desktop-app/Services/DatabaseService.cs:1001` (`live_calendar_work_items.json`), `desktop-app/Services/AuditHistoryService.cs:15` (`audit_history_log.json`), and `desktop-app/Services/PresenceService.cs:23` (`active_editor_leases.json`).
10. **Mocked Import Verification:** Verified in `desktop-app/Views/RosterImportWindow.xaml.cs:18, 26`, confirming hardcoded string assignment and lack of CSV processing.

---
*End of Milestone 1 Codebase & Feature Audit Report.*
