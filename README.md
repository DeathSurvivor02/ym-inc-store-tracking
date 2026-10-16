# YM Inc. Store Tracking & Performance Platform

[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-336791?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker&logoColor=white)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-Proprietary-red.svg)](LICENSE)

An enterprise-grade relational database and analytics platform engineered for **YM Inc.** (Toronto, ON) to track, manage, and optimize retail operations across **800+ physical storefronts** and 10 operating banners in Canada and the United States.

---

## 🏬 Enterprise Banners Covered

| Banner Code | Banner Name | Brand Tier | Target Demographic |
| :--- | :--- | :--- | :--- |
| **`UP`** | **Urban Planet** | Fast Fashion | Teens & Young Adults (Ages 16–24) |
| **`BN`** | **Bluenotes** | Denim & Casual | Universal Youth & Casual Denim (Ages 14–30) |
| **`SS`** | **Suzy Shier** | Contemporary Women | Modern Working Women (Ages 25–50) |
| **`LC`** | **Le Château** | Contemporary Women | Elevated Occasionwear & Dresses |
| **`SRN`** | **Sirens** | Fast Fashion | Trend-Focused Young Women (Ages 16–28) |
| **`F21`** | **Forever 21 (Canada)** | Fast Fashion | Global Fast Fashion & Everyday Style |
| **`UK`** | **Urban Kids** | Youth | Children & Pre-Teens (Ages 4–16) |
| **`W49`** | **West 49** | Action Sports | Skateboarders & Streetwear Enthusiasts |
| **`STN`** | **Stitches** | Value Apparel | Budget-Friendly Youth Essentials |
| **`UB`** | **Urban Behavior** | Fast Fashion | Affordable Urban Street Trends |
| **`CR`** | **Charlotte Russe** | Junior Fashion | Young Trend-Forward Women (Ages 15–28) |
| **`R21`** | **rue21** | Fast Fashion | Teens & Young Adults Graphic Apparel & Denim (Ages 14–25) |
| **`M`** | **Mandee** | Value Apparel | Contemporary Value Womenswear & Everyday Essentials (Ages 18–40) |

---

## 🏛️ System Architecture: Dual-Track Design

The platform integrates two core enterprise operational workflows into a single unified 3NF relational data warehouse:

```
                                  [ YM INC. ENTERPRISE REPOSITORY ]
                                                  │
                 ┌────────────────────────────────┴────────────────────────────────┐
                 ▼                                                                 ▼
      [ TRACK A: CAPITAL LIFECYCLE ]                                    [ TRACK B: OPERATIONAL WSPR ]
      • Store Construction & NSOs                                       • Weekly Store Performance Review
      • Site Surveys (Ceiling, Plenum, Demarc)                          • POS Sales, Returns & Net Sales
      • Mall Landlord Rules (Union, COI, Badging)                       • Foot-Traffic Optical Sensor Sync
      • Power Poles (Wiremold 25DTP-4 feeds)                            • Labor Scheduling & Efficiency (SPLH)
      • Structured Cabling (Cat6 Plenum, Fluke Cert)                    • Inventory Velocity & Weeks of Supply (WOS)
      • Telecom WAN Delivery (Primary Fiber + LTE)                      • 4-5-4 Retail Fiscal Calendar
      • Critical Path Milestones & Slippage Tracking                    • Executive Donut & Pie Visualizations
```

---

## 🚀 Quick Start (Local Setup with Docker)

### 1. Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.
- [Git](https://git-scm.com/) installed.

### 2. Clone & Configure Environment
```bash
cp .env.example .env
```

### 3. Spin Up PostgreSQL 16 & pgAdmin
```bash
docker compose up -d
```
* **PostgreSQL:** Running on `localhost:5432` (User: `ym_admin`, DB: `ym_store_tracking`)
* **pgAdmin 4:** Accessible at [http://localhost:5050](http://localhost:5050) (User: `admin@yminc.ca`, Pass: `admin_secret_2026`)
* *Note: The database automatically initializes the complete schema and master seeds on first boot via `db/init/`.*

### 4. Running Migrations Manually (Against Any Postgres Instance)

#### On Windows (PowerShell):
```powershell
.\db\apply_migrations.ps1 -HostName localhost -User ym_admin -Database ym_store_tracking -IncludeSeeds
```

#### On Linux / macOS (Bash):
```bash
chmod +x ./db/apply_migrations.sh
./db/apply_migrations.sh --seeds
```

---

## 📊 Database Schema Blueprint

### 📁 Directory Structure
```text
db/
├── migrations/
│   ├── 001_management_hierarchy.sql # Banners, Regions, Districts, Managers, Mall Properties & Rules
│   ├── 002_stores_and_lifecycle.sql # Stores master, Attributes, Surveys, Power Poles, Cabling, Circuits, Milestones
│   ├── 003_analytics_and_facts.sql  # Categories, Fiscal Calendar, Daily POS, Weekly Sales, Labor & Inventory
│   ├── 004_indexes.sql              # Performance B-Tree & partial indexes
│   └── 005_views.sql                # Operational scorecards & executive pie chart SQL views
└── seeds/
    └── 01_master_seeds.sql          # 10 Banners, 6 Regions, 12 Districts, Malls, & Sample Fleet (Week 41)
```

---

## 📈 Executive Reporting Views & Pie Chart Specifications

The schema provides purpose-built database views powering executive reporting and mandatory proportional charts:

| Chart ID | View Name | Display Format | Analytical Purpose |
| :--- | :--- | :--- | :--- |
| **CH-01** | `v_pie_banner_sales` | Donut Chart (45% inner radius) | Proportional net sales contribution across all 10 retail banners |
| **CH-02** | `v_pie_regional_sales` | Pie Chart with Exploded Slices | Revenue distribution across the 6 major geographic regions |
| **CH-03** | `v_pie_real_estate_format` | Dual Metric Donut Chart | Revenue & store count split (Enclosed Mall, Big Box, Outlets, Street Front) |
| **CH-04** | `v_pie_category_mix` | Pie Chart with Margin Tooltips | Merchandise division volume mix (Womenswear, Denim, Menswear, Kids, etc.) |
| **CH-05** | `v_pie_labour_allocation` | Donut Chart with Wage Callouts | Store payroll expense allocation and wage % by banner |
| **CH-06** | `v_pie_fleet_health` | RAG Donut Chart | Fleet-wide store distribution by Budget Attainment Bracket (Tiers 1–4) |

### Operational Views
- **`v_weekly_store_performance`**: Detailed store-level WSPR rollup (Sales, Budget Var, Traffic, Conversion %, UPT, ATV, AUR, SPSF, Labour Cost %).
- **`v_weekly_banner_rollup`**: C-Suite executive scorecard aggregated by banner for any fiscal week.
- **`v_store_lifecycle_status`**: Live tracking of all capital NSO/Remodel stores with days until grand opening and status across site survey, power poles, cabling, and circuits.
- **`v_circuit_readiness_tracker`**: High-availability telecom readiness tracker (Carrier FOC dates, turn-up status, demarc extensions).

---

## 🗺️ Project Roadmap

- [x] **Phase 1: Database Architecture & Migrations**
  - [x] 3NF Relational Data Model for Store Lifecycle & Deployment Tracks
  - [x] Retail Analytics Fact Tables & 4-5-4 Fiscal Calendar
  - [x] High-performance indexes & executive reporting views
  - [x] Docker Compose environment & master seed fixtures
- [ ] **Phase 2: Backend API & Automation Engine**
  - [ ] REST / GraphQL API for store queries and milestone transitions
  - [ ] POS & Traffic Ingestion workers
  - [ ] Scheduled Monday 06:00 AM EST WSPR aggregation engine
  - [ ] Automated Anomaly Detection (Traffic hardware outages, labour leakage > 16.5%)
- [ ] **Phase 3: Executive Frontend Dashboard**
  - [ ] Interactive Canvas & Real-time KPI Scorecards
  - [ ] Donut & Pie Chart visual analytics (Chart.js / Recharts)
  - [ ] Store Lifecycle Kanban & Interactive Gantt slippage viewer
