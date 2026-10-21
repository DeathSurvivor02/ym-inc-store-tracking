# YM Inc. Store Deployment & Telecom Management Platform
## Milestone 4: Business Readiness, Infrastructure Sizing & Strategic Implementation Roadmap

**Document Reference:** `YM-STRAT-M4-2026-V1`  
**Milestone Target:** M4 — Business Readiness & Implementation Strategy  
**Audience:** C-Suite Executive Leadership (CEO/COO/CIO), VP of Real Estate & Construction, VP of Retail Operations, IT Engineering, Field Operations  
**Scope:** 376 Retail Stores across 13 Apparel Banners (Canada & United States)  
**Author:** Enterprise Systems Implementation & Infrastructure Architecture Team  
**Date:** October 2026  
**Status:** Approved Strategic Implementation Blueprint  

---

## Table of Contents

1. [Executive Summary & Business Readiness Scorecard](#1-executive-summary--business-readiness-scorecard)  
   - 1.1 [Enterprise Context & Strategic Vision](#11-enterprise-context--strategic-vision)  
   - 1.2 [Business Readiness Assessment Scorecard](#12-business-readiness-assessment-scorecard)  
   - 1.3 [Cross-Functional Implementation Governance](#13-cross-functional-implementation-governance)  
2. [IT Infrastructure & Hosting Sizing](#2-it-infrastructure--hosting-sizing)  
   - 2.1 [Neon Serverless PostgreSQL Data Tier Sizing & Topology](#21-neon-serverless-postgresql-data-tier-sizing--topology)  
   - 2.2 [Cloud API Gateway & Containerized Microservices Compute Sizing](#22-cloud-api-gateway--containerized-microservices-compute-sizing)  
   - 2.3 [Enterprise Secrets Management, KMS & Workstation Hardening](#23-enterprise-secrets-management-kms--workstation-hardening)  
   - 2.4 [Financial Cost Models & Monthly Total Cost of Ownership (TCO)](#24-financial-cost-models--monthly-total-cost-of-ownership-tco)  
3. [Role Management & Permission Hierarchies](#3-role-management--permission-hierarchies)  
   - 3.1 [Enterprise Stakeholder Taxonomy](#31-enterprise-stakeholder-taxonomy)  
   - 3.2 [Comprehensive Role-Based Access Control (RBAC) Permission Matrix](#32-comprehensive-role-based-access-control-rbac-permission-matrix)  
   - 3.3 [Granular Module-by-Module Permission Specification](#33-granular-module-by-module-permission-specification)  
   - 3.4 [Separation of Duties (SoD) & Four-Eyes Approval Principles](#34-separation-of-duties-sod--four-eyes-approval-principles)  
4. [Landlord & Mall Compliance Operational Workflows](#4-landlord--mall-compliance-operational-workflows)  
   - 4.1 [Certificate of Insurance (COI) Standard Operating Procedure (SOP)](#41-certificate-of-insurance-coi-standard-operating-procedure-sop)  
   - 4.2 [After-Hours Access & Night Work Permit Protocols](#42-after-hours-access--night-work-permit-protocols)  
   - 4.3 [Logistics, Freight Elevators & Delivery Dock Protocols](#43-logistics-freight-elevators--delivery-dock-protocols)  
   - 4.4 [NRF 4-5-4 Retail Calendar Holiday Shopping Blackout Freeze Protocols](#44-nrf-4-5-4-retail-calendar-holiday-shopping-blackout-freeze-protocols)  
5. [Phased Rollout Plan & Execution Strategy](#5-phased-rollout-plan--execution-strategy)  
   - 5.1 [Rollout Architecture & Phased Timeline Overview](#51-rollout-architecture--phased-timeline-overview)  
   - 5.2 [Phase 1: Pilot Deployment (4 Flagship Stores across US & Canada)](#52-phase-1-pilot-deployment-4-flagship-stores-across-us--canada)  
   - 5.3 [Phase 2: Regional Staged Rollout (18 Districts & Regional Clusters)](#53-phase-2-regional-staged-rollout-18-districts--regional-clusters)  
   - 5.4 [Phase 3: Fleetwide Deployment (All 376 Stores across 13 Banners)](#54-phase-3-fleetwide-deployment-all-376-stores-across-13-banners)  
   - 5.5 [Operational KPIs, Service Level Agreements (SLAs) & Milestones](#55-operational-kpis-service-level-agreements-slas--milestones)  
   - 5.6 [Enterprise Risk Mitigation Playbook & Contingency Protocols](#56-enterprise-risk-mitigation-playbook--contingency-protocols)  
6. [Actionable Prioritized Engineering Backlog](#6-actionable-prioritized-engineering-backlog)  
   - 6.1 [Backlog Structure, Estimation Methodology & Story Point Scale](#61-backlog-structure-estimation-methodology--story-point-scale)  
   - 6.2 [Phase 1: Immediate Remediation Backlog (P0 Critical Defect Fixes)](#62-phase-1-immediate-remediation-backlog-p0-critical-defect-fixes)  
   - 6.3 [Phase 2: Core Modernization Backlog (P1 MVVM & Offline Capabilities)](#63-phase-2-core-modernization-backlog-p1-mvvm--offline-capabilities)  
   - 6.4 [Phase 3: SaaS & Enterprise Expansion Backlog (P2 Portal, Sync & Integrations)](#64-phase-3-saas--enterprise-expansion-backlog-p2-portal-sync--integrations)  
   - 6.5 [Engineering Velocity, Capacity Planning & Resource Allocation](#65-engineering-velocity-capacity-planning--resource-allocation)  
7. [Comprehensive Deployment Checklist](#7-comprehensive-deployment-checklist)  
   - 7.1 [Pre-Deployment Verification Phase (T-30 Days to T-1 Day)](#71-pre-deployment-verification-phase-t-30-days-to-t-1-day)  
   - 7.2 [Deployment Day Go-Live Execution Phase (T-0 Runbook)](#72-deployment-day-go-live-execution-phase-t-0-runbook)  
   - 7.3 [Post-Deployment Hypercare & Steady-State Verification Phase (T+1 to T+30 Days)](#73-post-deployment-hypercare--steady-state-verification-phase-t1-to-t30-days)  
   - 7.4 [Rollback & Disaster Recovery Protocols](#74-rollback--disaster-recovery-protocols)  

---

## 1. Executive Summary & Business Readiness Scorecard

### 1.1 Enterprise Context & Strategic Vision

YM Inc. is a leading multi-banner retail fashion conglomerate operating **376 retail stores** across **13 commercial apparel banners** throughout Canada and the United States:
- **Canadian Operating Fleet (230 Stores across 10 Banners):** Urban Planet (UP), Bluenotes (BN), Sirens (SS), Suzy Shier (LC), Stitches (SRN), Forever 21 Canada (F21), Urban Kids (UK), West 49 (W49), Stone Ridge (STN), and Urban Behavior (UB).
- **United States Operating Fleet (146 Stores across 3 Banners):** Charlotte Russe (CR), rue21 (R21), and Mandee (M).

The organization manages continuous, multi-million-dollar retail capital deployment cycles involving new store grand openings, brand banner conversions, store relocations, down-sizing/right-sizing renovations, and end-of-lease decommissions. Each capital project requires complex synchronization across:
1. **Low-Voltage Structured Cabling:** Rigid enforcement of Cat6 Plenum (CMP) cabling specifications, 24- to 48-drop termination schedules to open MDF racks, and 100% verification via Fluke DSX CableAnalyzer certification.
2. **Dual-Channel Power Poles:** Precision ceiling drops using Wiremold 25DTP-4 fixtures with physical 12-gauge tie-wire anchoring to structural concrete decks, dual-channel isolation eliminating 60Hz electromagnetic interference (EMI), and municipal/ESA electrical inspection sign-offs.
3. **High-Availability Dual-WAN Turn-Up:** Primary Commercial Fiber Dedicated Internet Access (DIA) paired with secondary Enterprise 5G/LTE wireless failover routers (Cradlepoint W1850/IBR600) terminating onto Cisco Meraki MX68 security appliances, supported by basement riser demarc extensions.
4. **Institutional Mall & Landlord Compliance:** Navigating stringent Commercial General Liability ($5M–$10M) Certificate of Insurance (COI) endorsements, union labor stipulations, night work construction windows (22:30–06:00), freight dock reservations, and acoustic restriction covenants.
5. **NRF 4-5-4 Fiscal Retail Calendar:** Strictly preventing operational disruption during high-velocity Q4 holiday shopping blackout windows (Black Friday week through Boxing Week/New Year).

While Milestones 1, 2, and 3 conducted an exhaustive forensic codebase audit, designed the next-generation .NET 10 WPF MVVM desktop client, and architected the Hybrid Cloud SaaS backend, **Milestone 4 establishes the business implementation strategy**. It bridges the gap between software engineering and enterprise field operations, delivering the operational governance, hosting infrastructure sizing, financial cost models, landlord protocols, phased rollout schedules, engineering backlog, and deployment runbooks necessary to execute a flawless fleetwide rollout.

---

### 1.2 Business Readiness Assessment Scorecard

A forensic readiness evaluation across YM Inc.’s corporate departments reveals the delta between current operational capabilities and the target SaaS/Desktop operating model:

| Operational Domain | Current Readiness | Target State Requirement | Gap / Primary Deficiency | Readiness Grade | Remediation Strategy & Priority |
| :--- | :---: | :--- | :--- | :---: | :--- |
| **IT Cloud Hosting & Infrastructure** | **Low** (25%) | Resilient serverless PostgreSQL (Neon) with PgBouncer pooling, HA read replicas, containerized API gateway, and Azure Key Vault secrets. | Direct 2-tier database connections over port 5432; elevated static credentials in `.env` files; no API gateway; no read replicas. | **D** | Provision Neon PgBouncer pooler endpoint, 24/7 baseline compute tuning, and Azure Container Apps / AWS ECS API Gateway (**P0**). |
| **Identity & Access Governance** | **Critical** (15%) | Centralized Microsoft Entra ID (Azure AD) SSO with PKCE, MFA enforcement, and role-based access control (RBAC). | Hardcoded PBKDF2 `FallbackPepper` in compiled binary; plaintext team credentials; unauthenticated password reset window (**DEF-006**). | **F** | Deprecate local password auth; implement Entra ID MSAL.NET OIDC tokens and Azure Key Vault key management (**P0/P1**). |
| **Field Tech & Offline Usability** | **Moderate** (40%) | Offline-first desktop client with embedded SQLite cache, outbox mutation queue, and background sync. | App crashes or falls back to static seed data when network disconnects; manual changes made offline are discarded upon exit. | **C-** | Implement SQLite local cache, Polly v8 exponential backoff retry policies, and cursor-based delta sync (**P1**). |
| **Landlord & Mall Governance** | **Moderate** (50%) | Centralized COI repository with automated OCR validation, 30/60-day expiration alerts, and mall rule inheritance. | 31-column schema drift on `mall_properties`; COIs tracked via fragmented emails; no automated expiration tracking. | **C** | Establish formal COI SOP with broker escalation protocols; integrate Azure AI Vision OCR and automated alerts (**P1/P2**). |
| **Telecom Carrier Orchestration** | **Low** (30%) | Automated carrier API webhooks, circuit readiness telemetry, FOC tracking, and demarc extension monitoring. | Table name mismatch (`telecom_circuits` vs `circuit_details`) breaks onboarding; WAN circuit edits silently discarded on save. | **D-** | Fix schema queries in `DatabaseService.cs`; expose IP telemetry; integrate carrier status webhooks (**P0/P2**). |
| **Structured Cabling Certification** | **Low** (35%) | Mandatory digital upload and parsing of Fluke LinkWare (`.flw`/PDF) test reports prior to milestone sign-off. | Single boolean checkbox in desktop UI; `certification_report_url` is 100% empty across all 376 stores. | **D** | Mandate Fluke LinkWare PDF ingestion; store test certificates in S3 object storage; enforce four-eyes sign-off (**P1/P2**). |
| **NRF Calendar Governance** | **Moderate** (55%) | Enforced scheduling validation preventing tech work during Q4 peak holiday shopping blackout freezes. | 4-week calendar offset between desktop C# and PostgreSQL; blackout freeze notices are visual-only with zero blocking rules. | **C-** | Align calendar week calculation to NRF standard; implement hard scheduling blockers for peak freeze periods (**P0/P1**). |

---

### 1.3 Cross-Functional Implementation Governance

To coordinate the transition across 376 stores, YM Inc. establishes an Executive Steering Committee and four specialized working groups:

```
                            ┌──────────────────────────────────────────────┐
                            │          EXECUTIVE STEERING COMMITTEE        │
                            │  CIO, VP Real Estate, VP Retail Ops, VP IT   │
                            └──────────────────────┬───────────────────────┘
                                                   │
         ┌─────────────────────────┬───────────────┴───────────────┬─────────────────────────┐
         ▼                         ▼                               ▼                         ▼
┌──────────────────┐      ┌──────────────────┐           ┌──────────────────┐      ┌──────────────────┐
│ IT ARCHITECTURE  │      │ REAL ESTATE &    │           │ STORE OPERATIONS │      │ FIELD TELECOM &  │
│ & SECURITY SQUAD │      │ LANDLORD SQUAD   │           │ & MERCH SQUAD    │      │ CABLING SQUAD    │
│ Cloud, API, DB,  │      │ Mall Rules, COI, │           │ Regional DMs,    │      │ Low-Voltage,     │
│ Entra ID, KMS    │      │ Permits, Docks   │           │ SMs, POS Staging │      │ Carriers, Fluke  │
└──────────────────┘      └──────────────────┘           └──────────────────┘      └──────────────────┘
```

#### RACI Governance Matrix:
- **Accountable (A):** VP of IT & Retail Infrastructure owns delivery, budget, and system uptime SLAs.
- **Responsible (R):** Enterprise Solutions Architect and Project Managers (PM) lead software delivery and field execution.
- **Consulted (C):** Regional District Managers (DM), Landlord Property Directors, and Carrier Account Executives.
- **Informed (I):** Store Managers (SM), Corporate Accounting, and Executive Committee.

---

## 2. IT Infrastructure & Hosting Sizing

### 2.1 Neon Serverless PostgreSQL Data Tier Sizing & Topology

The database architecture leverages **Neon Serverless PostgreSQL 16 (Lakebase Postgres)** in the AWS us-east-1 region (co-located with enterprise WAN carrier exchanges and corporate headquarters):

```
                                  ┌─────────────────────────────────────────┐
                                  │      ASP.NET Core 10 API Gateway        │
                                  │   (Azure Container Apps / AWS ECS)      │
                                  └────────────────────┬────────────────────┘
                                                       │
                           ┌───────────────────────────┴───────────────────────────┐
                           │ (Port 5432)                                           │ (Port 5432)
                           ▼                                                       ▼
            ┌─────────────────────────────┐                         ┌─────────────────────────────┐
            │   PgBouncer Pooler Host     │                         │   Direct Unpooled Host      │
            │   ep-shiny-hill-pooler      │                         │   ep-shiny-hill             │
            │   (Transaction-Mode Pool)   │                         │   (DDL Migrations / CI/CD)  │
            └──────────────┬──────────────┘                         └──────────────┬──────────────┘
                           │                                                       │
                           ▼                                                       │
            ┌─────────────────────────────┐                                        │
            │  Primary Read/Write Compute │                                        │
            │  Autoscale: 0.25 to 4.0 CU  │◄───────────────────────────────────────┘
            │  24/7 Warm Baseline (0.25CU)│
            └──────────────┬──────────────┘
                           │ Write-Ahead Log (WAL) Streaming
                           ▼
            ┌─────────────────────────────┐
            │  HA Read Replica Compute    │
            │  Autoscale: 0.25 to 2.0 CU  │
            │  (Dedicated Analytical View │
            │   & C-Suite Query Node)     │
            └──────────────┬──────────────┘
                           │
                           ▼
            ┌───────────────────────────────────────────────────────────────────────┐
            │             Neon Decoupled Cloud Storage Architecture                 │
            │   Multi-AZ S3 Replicated Storage Engine / Copy-on-Write Branching     │
            └───────────────────────────────────────────────────────────────────────┘
```

#### Compute Sizing & Compute Units (CU) Allocation:
Neon measures compute capacity in **Compute Units (CU)**, where $1 \text{ CU} = 1 \text{ vCPU} + 4 \text{ GB RAM}$:
1. **Primary Read/Write Node (`main` branch):**
   - **Minimum Compute:** 0.25 CU (0.25 vCPU, 1 GB RAM) during off-peak overnight hours.
   - **Maximum Compute:** 4.0 CU (4 vCPU, 16 GB RAM) during morning peak operational turn-up windows (08:00–12:00 EST) when field techs synchronize fleet updates.
   - **Dynamic Autoscaling Rule:** Scales up when CPU utilization exceeds 75% for >60 seconds; scales down after 5 minutes of sustained load <30%.
2. **Dedicated HA Read Replica Node (`read-replica` endpoint):**
   - **Allocation:** 0.25 CU to 2.0 CU (0.25–2 vCPU, 1–8 GB RAM).
   - **Workload Isolation:** Dedicated entirely to high-volume reporting queries, analytical views (`v_store_lifecycle_status`, `v_circuit_readiness_tracker`, `v_weekly_store_performance`), and executive dashboard aggregation. Shields the primary write transaction engine from heavy reporting locks.
3. **24/7 Baseline Compute & Night Construction Alignment (Zero Cold Starts):**
   - **Mall Night Construction Shift Reality:** In enclosed shopping malls across North America, physical low-voltage cabling pulls, ceiling deck tie-wire hammer drilling, and electrical power pole connections are legally barred during shopping hours and **must execute during overnight construction windows (22:30 to 06:00 local time)**.
   - **Multi-Time-Zone Operational Span:** YM Inc.'s 376 stores span four major North American time zones (Newfoundland/Eastern to Pacific). When it is 23:00 EST, it is only 20:00 PST (active retail trading at Metropolis at Metrotown in Vancouver). Pacific time zone night shifts begin at 22:30 PST (01:30 EST) and terminate at 06:00 PST (09:00 EST).
   - **Elimination of Overnight Scale-to-Zero During Deployment Waves:** Permitting Neon compute to scale to zero overnight between 23:00 and 06:00 EST would cause repeated 3–8 second cold-start delays, connection timeouts, and mobile sync failures for electricians, cabling trades, and field specialists submitting milestone sign-offs and Fluke `.FLW` datasets throughout the night.
   - **Autoscaling Policy:** During active rollout phases, the primary compute endpoint maintains a **warm baseline of 0.25 CU minimum 24/7/365** (never suspended). The synthetic keep-alive background worker executes `SELECT 1;` every 10 minutes continuously around the clock. Compute dynamically scales between 0.25 CU (overnight baseline) and 4.0 CU (peak morning fleet sync).
   - **Mathematical Cost Alignment:** This 24/7 policy precisely matches the financial model in Section 2.4: $(0.25 \text{ CU} \times 7\text{h night}) + (0.75 \text{ CU avg} \times 17\text{h day}) = 14.5 \text{ CU-hrs/day} \times 30 = 435 \text{ CU-hrs/mo} \times \$0.16 = \$69.60\text{/month}$. Continuous 24/7 availability is delivered with zero cost inflation.

#### Connection Pooling Topology (PgBouncer) & Traffic Delineation:
- **Application Pooler Host:** `ep-shiny-hill-b7bouvcl-pooler.c-13.us-east-1.aws.neon.tech` (`DATABASE_URL`)
- **Pooling Mode:** **Transaction-Mode Pooling**. Physical PostgreSQL connections are checked out only during active SQL transactions and immediately recycled.
- **Connection Capacity:** Accommodates up to **10,000 client virtual connections** mapped to a physical backend pool of 30 to 50 server processes, preventing connection starvation during fleetwide mobile sync bursts.
- **CRITICAL ARCHITECTURAL BOUNDARY — Asynchronous Notification Backplane:** Because PgBouncer in transaction mode reallocates connections across transactions, session-level PostgreSQL `LISTEN/NOTIFY` commands are dropped upon `COMMIT`/`ROLLBACK`. Distributed SignalR presence, section soft-locks, and lease invalidations are strictly offloaded to **Azure SignalR Service** / **Azure Web PubSub** (or Redis Sentinel cluster).
- **Direct Unpooled Host:** `ep-shiny-hill-b7bouvcl.c-13.us-east-1.aws.neon.tech` (`DATABASE_URL_UNPOOLED`) reserved exclusively for EF Core / DbUp database migrations, database seeding scripts, and dedicated CDC event listener daemons.

#### Storage Growth & Egress Optimization:
- **Baseline Data Size:** Current relational data for 376 stores across 23 tables occupies ~180 MB. Historical snapshots, audit delta tables, and 3-year NRF calendar logs bring active storage to **4.5 GB**.
- **Projected Annual Growth:** ~2.5 GB/year (driven by change data capture audit logs and weekly snapshots). Total 3-year projected storage: **12.0 GB**.
- **Database Egress Optimization:**
  - Strict projection discipline: Eliminate `SELECT *` across all data layers, querying only explicitly required column sets.
  - Server-side cursor pagination: Enforce `limit` and `cursor` (last row ID or timestamp) on all list endpoints.
  - Differential delta sync: Clients exchange only changed rows since `last_sync_timestamp`, reducing sync payload sizes from 12 MB (full fleet) to <45 KB per synchronization cycle.

---

### 2.2 Cloud API Gateway & Containerized Microservices Compute Sizing

The API Gateway is deployed as containerized ASP.NET Core 10 Web APIs hosted on **Azure Container Apps (ACA)** (or AWS ECS Fargate), providing serverless auto-scaling and zero-downtime rolling updates:

```
                               ┌────────────────────────────────────────────────────────┐
                               │     Cloudflare Enterprise Edge (Anycast / WAF)         │
                               └───────────────────────────┬────────────────────────────┘
                                                           │ HTTPS (TLS 1.3)
                                                           ▼
                               ┌────────────────────────────────────────────────────────┐
                               │           Azure Container Apps Environment             │
                               │                                                        │
                               │   ┌────────────────────────────────────────────────┐   │
                               │   │      ASP.NET Core 10 API Gateway Cluster       │   │
                               │   │      - Min Replicas: 2 | Max Replicas: 6       │   │
                               │   │      - 1.0 vCPU | 2.0 GiB RAM per Replica      │   │
                               │   │      - Kestrel Zero-Allocation Sockets         │   │
                               │   │      - SignalR Core Presence Hub               │   │
                               │   └───────────────────────┬────────────────────────┘   │
                               │                           │ Internal gRPC              │
                               │                           ▼                            │
                               │   ┌────────────────────────────────────────────────┐   │
                               │   │      Fluke LinkWare PDF Parser Worker          │   │
                               │   │      - Python 3.12 / PyMuPDF Microservice      │   │
                               │   │      - 0.5 vCPU | 1.0 GiB RAM | Autoscaled     │   │
                               │   └────────────────────────────────────────────────┘   │
                               └────────────────────────────────────────────────────────┘
```

#### Container Sizing Specifications:
1. **API Gateway Pods (Primary Backend):**
   - **Base Allocation:** 1.0 vCPU, 2.0 GiB RAM per container replica.
   - **High Availability Baseline:** Minimum **2 active replicas** distributed across Availability Zones, ensuring zero-downtime failover and elimination of cold starts.
   - **Autoscaling Policy:** Horizontal Pod Autoscaler (HPA) configured via KEDA:
     - Scales up when HTTP request concurrency exceeds **150 concurrent requests per replica** or CPU utilization exceeds 70%.
     - Maximum scale: **6 replicas** (capable of handling >12,000 requests/minute during morning turn-up surges).
2. **Fluke LinkWare & COI Document Worker (Isolated Microservice):**
   - **Base Allocation:** 0.5 vCPU, 1.0 GiB RAM.
   - **Autoscaling:** Scales from 0 to 3 replicas on-demand based on Azure Storage Queue / SQS queue depth when field technicians batch-upload PDF test reports.

---

### 2.3 Enterprise Secrets Management, KMS & Workstation Hardening

The audited practice of distributing database connection strings, elevated credentials, and cryptographic pepper strings in local client `.env` files is completely decommissioned.

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                               ENTERPRISE KMS ARCHITECTURE                              │
│                                                                                        │
│   ┌────────────────────────────────────────────────────────────────────────────────┐   │
│   │                 Azure Key Vault / AWS Secrets Manager (Hardware HSM)           │   │
│   │                                                                                │   │
│   │   Secret: "Neon-Postgres-Connection-String" (Rotated every 90 days)           │   │
│   │   Secret: "PBKDF2-Master-Pepper-Key" (Hardware HSM protected)                  │   │
│   │   Secret: "Carrier-API-Webhook-HMAC-Keys" (Bell, Telus, AT&T secrets)          │   │
│   │   Secret: "Entra-ID-Client-Secret" (Certificate-based credential)              │   │
│   └───────────────────────────────────────┬────────────────────────────────────────┘   │
│                                           │ Managed Identity (No Passwords in Code)    │
│                                           ▼                                            │
│   ┌────────────────────────────────────────────────────────────────────────────────┐   │
│   │              Containerized API Gateway (Azure Container Apps / ECS)            │   │
│   │              - Ingests secrets into in-memory environment at startup           │   │
│   │              - Mediates 100% of database, storage, and carrier operations      │   │
│   └───────────────────────────────────────┬────────────────────────────────────────┘   │
│                                           │ HTTPS + Bearer JWT (Entra ID OIDC / PKCE)  │
│                                           ▼                                            │
│   ┌────────────────────────────────────────────────────────────────────────────────┐   │
│   │           Desktop Client Workstation (.NET 10 WPF - Field Operator)            │   │
│   │                                                                                │   │
│   │   - ZERO database credentials or connection strings on disk                    │   │
│   │   - ZERO cryptographic peppers or plaintext keys in binaries                   │   │
│   │   - Short-lived Entra ID Access Token (1-hour TTL)                             │   │
│   │   - Refresh Token encrypted via Windows DPAPI (CurrentUser scope)              │   │
│   └────────────────────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Cryptographic Remediation Standards & Azure Key Vault Secret Migration Path:
1. **Elimination of Compiled Pepper (`DEF-007`) & Azure Key Vault Production Path:**
   - **Vault Provisioning:** A dedicated, geo-redundant Azure Key Vault instance (`ym-kv-prod.vault.azure.net`) is provisioned within the Canadian Central region with soft-delete and purge protection enabled.
   - **Secret Allocation:** The cryptographic pepper is generated as a cryptographically random 64-character high-entropy alphanumeric string and stored in Key Vault secret `ym-kv-prod/secrets/password-pepper`.
   - **Runtime Injection:** The API Gateway container retrieves the pepper during container boot via Azure Managed Identity (`DefaultAzureCredential`), injecting it strictly into process-isolated memory (`IConfiguration["PASSWORD_PEPPER"]`). Zero plaintext secrets exist in environment `.env` files or compiled desktop assemblies.
   - **Database Hash Migration & Re-Keying Strategy (`007_rekey_legacy_passwords.sql`):**  
     To eliminate reliance on the leaked fallback pepper without locking out legitimate users, the schema is upgraded via migration `007_rekey_legacy_passwords.sql`:
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
   - **Dual-Pepper Verification Protocol (Zero User Lockout Guarantee):**  
     To prevent operational lockout when rotating to Azure Key Vault pepper (`ym-kv-prod/secrets/password-pepper-v2`), the authentication service executes a deterministic dual-pepper verification:
     1. **Primary Check (Azure Key Vault Pepper v2):** Compute PBKDF2-HMAC-SHA256 (100,000 iterations, 32-byte salt) with Key Vault pepper. If equal to `password_hash`, authentication succeeds immediately.
     2. **Fallback Check (Legacy Pepper v1):** If primary check fails and `pepper_version == 1` (or `legacy_password_hash` is present), verify credentials against the legacy fallback pepper (`YM_ENTERPRISE_PEPPER_SECURE_2026_9f8b4c2e1a7d5`).
     3. **Transparent Auto-Rekeying:** If legacy check matches, the user's password is verified. The service immediately derives a new 32-byte cryptographic salt, re-hashes the password using the Azure Key Vault Master Pepper (v2), updates `app_users` (`password_hash = new_hash`, `password_salt = new_salt`, `pepper_version = 2`, `force_password_reset = FALSE`, `legacy_password_hash = NULL`, `password_last_rekeyed_at = NOW()`), and issues the session. This guarantees **100% zero user lockout** while systematically eliminating legacy peppers.
   - **Hardening `ResetPasswordWindow.xaml.cs` & SSPR Workflow:**  
     Unauthenticated direct password overwrites are strictly barred. Password resets require Microsoft Entra ID Self-Service Password Reset (SSPR) or an out-of-band email verification challenge. The server dispatches a 6-digit cryptographic OTP token (`password_reset_token`, 15-minute expiration) to the user's verified corporate email (`@yminc.ca` / `@yminc.com`). The client interface requires input of the username, verification OTP, and a new password conforming to NIST SP 800-63B ($\ge 12$ characters, complexity rules, and breach dictionary validation).
   - **Complete Entra ID Cutover:** On Day 1 of Phase 1, all corporate identities (PM, PA, Field Specialists, Regional DMs) are migrated to Microsoft Entra ID OIDC/PKCE. Local password authentication in `app_users` is deprecated, retained solely for short-lived (8-hour TTL) external contractor provisioning tokens.
2. **Workstation Credential Hygiene:** The desktop client communicates exclusively with the API Gateway via HTTPS (TLS 1.3). No database ports (5432) are open to client workstations, and no connection strings exist on client hard drives.
3. **Local Token Security (DPAPI):** User authentication tokens issued by Entra ID are cached locally using the Windows Data Protection API (`System.Security.Cryptography.ProtectedData`), encrypted with `DataProtectionScope.CurrentUser`. A stolen user profile cannot be decrypted on a different physical workstation, and the offline cache permits up to 72 hours of disconnected operation in mall basements.

---

### 2.4 Financial Cost Models & Monthly Total Cost of Ownership (TCO)

A comprehensive infrastructure cost model demonstrates that transitioning from fragile direct connections to a scalable serverless cloud architecture delivers enterprise high availability at a modest operational expenditure:

#### Detailed Monthly Infrastructure Cost Breakdown:

| Service Component | Infrastructure Provider & Tier | Technical Allocation / Sizing Parameter | Unit Rate | Monthly Cost (USD) |
| :--- | :--- | :--- | :--- | :---: |
| **Primary Database Compute** | Neon Serverless PostgreSQL | 0.25 CU off-peak (7h/day), avg 0.75 CU business hours (17h/day) = 435 CU-hours/month | $0.16 / CU-hour | **$69.60** |
| **Database Read Replica** | Neon Serverless PostgreSQL | Dedicated analytics compute, avg 0.50 CU for 12h/day = 180 CU-hours/month | $0.16 / CU-hour | **$28.80** |
| **Database Storage & WAL** | Neon Lakebase Decoupled Storage | 25 GB active storage + WAL retention + branching snapshots | $0.12 / GB-month | **$3.00** |
| **API Gateway Compute** | Azure Container Apps / AWS ECS | 2 replicas baseline (1.0 vCPU, 2 GB RAM each) 24/7; scales to 4 during surges | ~$0.000024/vCPU-s + RAM | **$185.00** |
| **Document Worker Compute** | Azure Container Apps / AWS ECS | 0.5 vCPU, 1 GB RAM, event-driven serverless scaling (avg 2h active/day) | Pay-per-second | **$12.50** |
| **Edge Ingress & WAF** | Cloudflare Business / Enterprise | Anycast DNS, DDoS mitigation, TLS 1.3 termination, OWASP WAF rules | Fixed Monthly Plan | **$200.00** |
| **Secrets Management** | Azure Key Vault / Secrets Mgr | Hardware HSM storage, ~150,000 API secret transactions/month | $0.03 / 10k transactions | **$8.50** |
| **Object Storage (Files/PDFs)** | Neon Object Storage / Azure Blob | 250 GB storage (Fluke reports, site photos, COIs) + 50 GB egress | $0.018/GB + egress | **$12.20** |
| **Enterprise Identity & SSO** | Microsoft Entra ID P1 | 150 internal project users (Included in existing enterprise M365 E3/E5 licenses) | $0.00 (Existing EA) | **$0.00** |
| **Application Telemetry** | Azure Monitor / Datadog | 25 GB log ingestion, metrics, synthetic keep-alive pings, and alerting | $2.30 / GB ingested | **$57.50** |
| **Total Monthly TCO** | | **Enterprise Cloud Infrastructure Total** | | **$577.10 / mo** |

#### Annualized TCO Projection:
- **Annual Cloud Infrastructure Cost:** $\$577.10 \times 12 = \mathbf{\$6,925.20 \text{ / year}}$.
- **Cost per Store Allocation:** For a fleet of 376 retail stores, the operational cloud hosting cost is **$18.42 per store per year** ($1.53 per store per month).
- **Return on Investment (ROI):** Preventing a single day of telecom turn-up delay or landlord stop-work fine across the fleet ($5,000–$25,000 in liquidated damages or lost sales) recovers more than 100% of the annual cloud infrastructure expenditure.

---

## 3. Role Management & Permission Hierarchies

### 3.1 Enterprise Stakeholder Taxonomy

Managing retail capital buildouts requires precise alignment across corporate management, technical trade specialists, field personnel, and third-party vendors. The platform defines eight formal enterprise roles:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                         ENTERPRISE STAKEHOLDER TAXONOMY                                │
│                                                                                        │
│   ┌───────────────────────────────────┐     ┌──────────────────────────────────────┐   │
│   │ Project Manager (PM)              │     │ Project Administrator (PA)           │   │
│   │ - Overall project deployment lead │     │ - Compliance tracking, dock bookings │   │
│   │ - Schedule & budget authority     │     │ - Permit filings, roster imports     │   │
│   │ - Four-eyes final milestone signoff│    │ - Contact management & dispatching   │   │
│   └───────────────────────────────────┘     └──────────────────────────────────────┘   │
│                                                                                        │
│   ┌────────────────────────────────────────────────────────────────────────────────┐   │
│   │ Technical Project Specialists (PS1 - PS4)                                      │   │
│   │   • PS1: Site Survey & Physical Infrastructure (MDF racks, ceiling height)    │   │
│   │   • PS2: Power Pole & Electrical Fixtures (Wiremold 25DTP-4, tie-wire, ESA)   │   │
│   │   • PS3: Structured Cabling Specialist (Cat6 Plenum, drops, Fluke DSX reports) │   │
│   │   • PS4: Telecom Turn-Up Specialist (Meraki MX68, Cradlepoint 5G, Fiber DIA)  │   │
│   └────────────────────────────────────────────────────────────────────────────────┘   │
│                                                                                        │
│   ┌───────────────────────────────────┐     ┌──────────────────────────────────────┐   │
│   │ Store Manager (SM)                │     │ Field Technician / Cabling Contractor│   │
│   │ - Local store readiness & access  │     │ - Granite / Black Box / Direct Techs │   │
│   │ - Merchandising punch-list review │     │ - Physical cabling pulls & testing   │   │
│   │ - Staff onboarding coordination   │     │ - Fluke test upload & pole fixturing │   │
│   └───────────────────────────────────┘     └──────────────────────────────────────┘   │
│                                                                                        │
│   ┌───────────────────────────────────┐     ┌──────────────────────────────────────┐   │
│   │ Telecom Carrier Representative    │     │ Forensic Auditor / Compliance Officer│   │
│   │ - Bell, Telus, Rogers, Comcast    │     │ - Internal Audit / Legal / Risk      │   │
│   │ - FOC commitment date updates     │     │ - Read-only immutable access to CDC  │   │
│   │ - Circuit test & demarc delivery  │     │ - COI compliance & SoD verification  │   │
│   └───────────────────────────────────┘     └──────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 3.2 Comprehensive Role-Based Access Control (RBAC) Permission Matrix

The application replaces the broken model (`User.cs:40-44` where all permissions unconditionally returned `true`) with a strict, claim-based RBAC matrix enforced at both the API Gateway and Desktop UI layers:

| Module / Action Scope | PM | PA | PS1 | PS2 | PS3 | PS4 | Store Mgr | Field Tech | Carrier Rep | Auditor |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Fleet Master: View Store Details** | **CRUD** | **R** | **R** | **R** | **R** | **R** | **R (Own)**| **R (Assigned)**| **R (Circuit)**| **R** |
| **Fleet Master: Onboard New Store** | **C** | **C** | — | — | — | — | — | — | — | — |
| **Fleet Master: Modify Target Dates** | **U** | **U** | — | — | — | — | — | — | — | — |
| **Fleet Master: Decommission Store** | **D** | — | — | — | — | — | — | — | — | — |
| **Site Survey: Edit Ceilings & Demarc**| **U** | **R** | **CRUD** | **R** | **R** | **R** | — | **R** | — | **R** |
| **Power Poles: Edit Fixture & Drops** | **U** | **R** | **R** | **CRUD** | **R** | — | — | **U (Assigned)**| — | **R** |
| **Power Poles: Sign Off ESA Inspect** | **A** | — | — | **U** | — | — | — | — | — | **R** |
| **Cat6 Cabling: Modify Drop Matrix** | **U** | **R** | **R** | **R** | **CRUD** | — | — | **U (Assigned)**| — | **R** |
| **Cat6 Cabling: Upload Fluke Report** | **U** | **U** | — | — | **U** | — | — | **C (Upload)** | — | **R** |
| **Cat6 Cabling: Certify Termination**| **A** | — | — | — | **A** | — | — | — | — | **R** |
| **Telecom: Edit Circuit Parameters** | **U** | **R** | — | — | — | **CRUD** | — | — | **U (Assigned)**| **R** |
| **Telecom: Update FOC / Jeopardy** | **U** | **U** | — | — | — | **U** | — | — | **U (Carrier)** | **R** |
| **Telecom: Commission Turn-Up (Active)**| **A**| — | — | — | — | **U** | — | — | — | **R** |
| **Landlord: Edit Mall Rules & Docks** | **U** | **CRUD** | — | — | — | — | — | — | — | **R** |
| **Landlord: Upload & Verify COI** | **A** | **CRUD** | — | — | — | — | — | — | — | **R** |
| **NRF Calendar: Schedule Work Orders**| **CRUD** | **CRUD** | **U** | **U** | **U** | **U** | — | — | — | **R** |
| **NRF Calendar: Blackout Override** | **A (Co-sign)**| — | — | — | — | — | — | — | — | **R** |
| **System Admin: Manage App Users** | **U** | — | — | — | — | — | — | — | — | — |
| **Audit Logs: View Immutable History**| **R** | **R** | **R** | **R** | **R** | **R** | — | — | — | **R (Full)** |

*Matrix Legend:* **C** = Create, **R** = Read, **U** = Update, **D** = Delete, **A** = Authorize / Four-Eyes Approval, **—** = No Access.

---

### 3.3 Granular Module-by-Module Permission Specification

Permissions are enforced via ASP.NET Core Policy-Based Authorization handlers and mapped to Entra ID App Roles:

1. **Policy: `RequireStoreOnboarding`**
   - **Allowed Roles:** `Role.ProjectManager`, `Role.ProjectAdministrator`, `Role.GlobalAdmin`.
   - **Enforcement:** Validates that new store numbers conform to corporate banner conventions (e.g., Urban Planet 1000-series, Bluenotes 2000-series, Charlotte Russe 8000-series).
2. **Policy: `RequireTechnicalTrackSignoff`**
   - **Scope:** Power Poles (`RequirePowerPoleSignoff`), Cabling (`RequireCablingSignoff`), Telecom (`RequireTelecomSignoff`).
   - **Enforcement:** Restricts milestone status transitions (`In Progress` $\rightarrow$ `Certified & Passed`) exclusively to authenticated Specialists (PS1–PS4) and PMs.
3. **Policy: `RequireComplianceDocumentAdministration`**
   - **Allowed Roles:** `Role.ProjectAdministrator`, `Role.ProjectManager`.
   - **Enforcement:** Grants authority to upload certificates of insurance, register after-hours work permits, and schedule freight elevator dock bays.
4. **Policy: `RequireBlackoutCalendarOverride`**
   - **Allowed Roles:** `Role.ProjectManager` AND `Role.ExecutiveLeadership`.
   - **Enforcement:** Requires dual authentication to authorize emergency tech work during NRF peak holiday freeze periods.

---

### 3.4 Separation of Duties (SoD) & Four-Eyes Approval Principles

To satisfy corporate financial audit standards (SOX 404 / internal retail compliance) and prevent contractor billing fraud or premature store openings, the platform enforces **Separation of Duties (SoD)** and **Four-Eyes Approval Principles** across four critical operational gateways:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        FOUR-EYES APPROVAL GATEWAY ARCHITECTURE                         │
│                                                                                        │
│   STAGE 1: SUBMISSION / VERIFICATION           STAGE 2: INDEPENDENT AUTHORIZATION      │
│                                                                                        │
│   ┌───────────────────────────────────┐        ┌───────────────────────────────────┐   │
│   │ GATEWAY 1: CABLING CERTIFICATION  │        │ COUNTER-SIGNATURE REQUIRED        │   │
│   │ Cabling Contractor / PS3 uploads  │───────►│ Project Manager (PM) verifies     │   │
│   │ Fluke DSX-8000 LinkWare test data │        │ optical test margins & signs off  │   │
│   └───────────────────────────────────┘        └───────────────────────────────────┘   │
│                                                                                        │
│   ┌───────────────────────────────────┐        ┌───────────────────────────────────┐   │
│   │ GATEWAY 2: TELECOM TURN-UP        │        │ COUNTER-SIGNATURE REQUIRED        │   │
│   │ PS4 / Carrier tech confirms       │───────►│ Network Engineering Lead / PM     │   │
│   │ RFC 2544 throughput & Meraki WAN  │        │ counter-signs circuit activation  │   │
│   └───────────────────────────────────┘        └───────────────────────────────────┘   │
│                                                                                        │
│   ┌───────────────────────────────────┐        ┌───────────────────────────────────┐   │
│   │ GATEWAY 3: COI COMPLIANCE         │        │ COUNTER-SIGNATURE REQUIRED        │   │
│   │ Project Administrator (PA) logs   │───────►│ Corporate Risk Manager verifies  │   │
│   │ ACORD 25 certificate & limits     │        │ $5M-$10M endorsements & signs off │   │
│   └───────────────────────────────────┘        └───────────────────────────────────┘   │
│                                                                                        │
│   ┌───────────────────────────────────┐        ┌───────────────────────────────────┐   │
│   │ GATEWAY 4: NRF BLACKOUT OVERRIDE  │        │ COUNTER-SIGNATURE REQUIRED        │   │
│   │ Field PM submits emergency retail │───────►│ VP of Retail Operations & VP IT   │   │
│   │ deployment ticket during freeze   │        │ execute joint emergency sign-off  │   │
│   └───────────────────────────────────┘        └───────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Enforcement Rules:
1. **No Self-Authorization:** The user who executes a physical field task or uploads a test file is programmatically prohibited from approving that milestone (`CreatedByUserId != ApprovedByUserId`).
2. **Cryptographic Signing:** Each approval records the user’s Entra ID Object ID, email, timestamp (UTC), IP address, and an SHA-256 hash of the verified artifact into the immutable PostgreSQL audit trail.
3. **Automated Status Progression:** Store status cannot progress to `10. Grand Open` or `11. Open (Active Fleet)` until Gateways 1, 2, and 3 possess validated two-party approval tokens.

---

## 4. Landlord & Mall Compliance Operational Workflows

### 4.1 Certificate of Insurance (COI) Standard Operating Procedure (SOP)

Retail spaces leased in institutional retail properties (managed by Cadillac Fairview, Oxford Properties, Simon Property Group, Unibail-Rodamco-Westfield, Ivanhoé Cambridge, Brookfield) mandate rigorous risk mitigation protocols. Failure to provide a compliant Certificate of Insurance (COI) results in immediate landlord stop-work orders, eviction from the premises, and liquidated damages.

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        CERTIFICATE OF INSURANCE (COI) WORKFLOW                         │
│                                                                                        │
│   [T-30 Days] ──► [T-21 Days] ──► [T-14 Days] ──► [T-7 Days] ──► [T-3 Days / POSSESSION]
│        │               │               │               │                   │
│        ▼               ▼               ▼               ▼                   ▼
│   Lease Criteria  Broker Request  Automated OCR   Underwriting Review  Security Clearance
│   Extracted from  Dispatched to   Verification &  & Broker Escalation  Issued; Mall Dock
│   Mall Profile    Insurance Team  Data Ingestion  (if rejected)        Access Unlocked
└────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Detailed Standard Operating Procedure (SOP):
1. **Step 1: Lease Requirement Ingestion (T-30 Days):**
   - The Project Administrator (PA) reviews the executed lease agreement and verifies mall insurance requirements stored in `mall_properties` and `mall_requirements`.
   - **Standard Mall Limit:** Minimum **$5,000,000 CAD/USD** Commercial General Liability (CGL) per occurrence and general aggregate.
   - **Flagship Super-Regional Mall Limit:** Minimum **$10,000,000 CAD/USD** CGL for premier institutional locations (e.g. Toronto Eaton Centre, Yorkdale, Square One, Metropolis at Metrotown, Sawgrass Mills, Roosevelt Field, King of Prussia).
   - **Mandatory Statutory Coverages:**
     * Statutory Workers’ Compensation / Workplace Safety and Insurance Board (WSIB in Ontario / CNESST in Quebec / State-specific Form C-105.2 in US) with Employers' Liability ($1,000,000 minimum).
     * Commercial Automobile Liability ($2,000,000 combined single limit for all owned, non-owned, and hired vehicles).
     * Umbrella / Excess Liability ($5,000,000 to $10,000,000 providing follow-form coverage over underlying CGL and Auto policies).
2. **Step 2: Broker Dispatch & Certificate Request (T-21 Days):**
   - The PA triggers an automated COI request from the SaaS portal directly to YM Inc.’s corporate insurance broker (Marsh, Aon, Willis Towers Watson).
   - The request transmits exact legal entities for:
     - **Certificate Holder:** Landlord Corporate Legal Name & Specific Mall Property Office Address.
     - **Additional Insured Endorsements:** Explicitly naming the REIT asset owner, managing agent, indenture trustees, and ground landlords across North America's major REIT portfolios:
       * **Cadillac Fairview Corporation Limited** and designated mall subsidiary entities.
       * **Oxford Properties Group Inc.** and OMERS Realty Corporation.
       * **Ivanhoé Cambridge Inc.** and institutional co-owners.
       * **RioCan Real Estate Investment Trust** and management entities.
       * **Brookfield Properties Retail Inc.** and affiliate property holding LLCs.
       * **Simon Property Group, Inc.** and property partnerships.
     - **Mandatory Policy Endorsements:**
       * **30 Days Advance Written Notice of Cancellation** (10 days for non-payment).
       * **Primary and Non-Contributory** endorsement in favor of the Additional Insureds.
       * **Waiver of Subrogation** in favor of Landlord and Property Management.
       * **Cross-Liability / Severability of Interests** clause.
3. **Step 3: Document Ingestion & Azure AI Vision OCR Extraction (T-14 Days):**
   - The broker uploads the digital ACORD 25 (or Canadian equivalent) PDF into the portal.
   - The backend COI OCR Microservice automatically extracts:
     - Carrier AM Best Rating (Mandatory **A- / VII** or higher).
     - Policy Effective and Expiration Dates.
     - General Liability Limit ($5M or $10M verification against `mall_properties.general_liability_min_cad`).
     - Additional Insured clause text matching the lease covenant.
4. **Step 4: Broker Escalation Protocol (T-7 to T-1 Days):**
   - If the uploaded COI has missing endorsements or sub-limit deficiencies:
     - **T-7 Days:** System issues **Level 1 Escalation** to Insurance Broker and PA.
     - **T-3 Days:** System issues **Level 2 Critical Escalation** alerting VP of Real Estate.
     - **T-24 Hours:** System triggers **Stop-Work Injunction Alert** preventing field dispatch and security badging.
5. **Step 5: Automated Proactive Renewal Notification Engine:**
   - Active policies are monitored daily by background worker `CoiRenewalAuditService`.
   - Automated proactive alerts fire at **60 Days**, **30 Days**, **15 Days**, and **7 Days** prior to policy expiration, generating automated certificate reissue requests to brokers to guarantee uninterrupted mall lease compliance and eliminate $5,000–$12,000 backcharge penalties.

---

### 4.2 After-Hours Access & Night Work Permit Protocols

Major enclosed shopping centers strictly enforce quiet hours to protect public safety and retail tenant trade during mall operating hours (typically 10:00 to 21:00 Monday–Saturday, 11:00 to 18:00 Sunday).

#### Operational Night Work Parameters:
- **Mandatory Night Work Window:** **22:30 to 06:00** (Night shift).
- **Prohibited Day Work:** Hammer drilling into concrete slab decks, powder-actuated fastening (Hilti shots), ceiling tile extraction in common areas, high-noise conduit cutting, and pulling cable through common mall risers.
- **Permit Lead Time:** Minimum **72 hours advance registration** submitted to Mall Management and Security Operations.
- **Security Escort Fees:** Institutional properties charge mandatory security escort fees ($35.00 to $65.00/hour or flat $250.00 to $450.00/shift) billed directly to the tenant when contractors operate in common areas or basement risers.
- **Contractor Badging Protocols:**
  - Every technician must submit government-issued photo ID to the Security Dispatch office upon arrival.
  - Contractors wear visible mall security visitor badges at all times.
  - Mandatory sign-in and sign-out timestamps recorded in the mobile desktop app.

---

### 4.3 Logistics, Freight Elevators & Delivery Dock Protocols

Delivering bulk infrastructure materials—including 10- to 12-foot Wiremold 25DTP-4 power poles, 42U open server racks, and master cable spools (1,000 ft CMP boxes)—requires precision logistics:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        MALL LOGISTICS & FREIGHT SPECIFICATIONS                         │
│                                                                                        │
│   LOADING DOCK BAYS                   FREIGHT ELEVATOR CAB CONSTRAINTS                 │
│   • Dock Gate Reservation: 48h SLA    • Minimum Door Width: 48 in (4' 0")              │
│   • Delivery Window: 06:00 - 09:00    • Minimum Cab Depth: 96 in (8' 0")               │
│   • Max Vehicle Height: 13' 6"        • Minimum Cab Height: 120 in (10' 0" for poles)  │
│   • Idling Limit: 20 min max          • Capacity: 4,000 - 6,000 lbs (Class A freight)  │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Protocol Requirements:
1. **Dock Bay Booking:** Deliveries of Wiremold poles and MDF racks must be pre-scheduled via the platform’s Landlord Compliance module at least 48 hours in advance.
2. **Elevator Dimension Verification:** Standard passenger elevators cannot accommodate unbent 12-foot power poles. The database tracks freight cab depth, height, and weight capacity (`freight_elevator_height_inches`, `freight_elevator_depth_inches`) for each of the 335 mall properties. If an elevator cannot accommodate a 12-foot pole, the system automatically flags an order requirement for **2-piece modular poles** or **stairwell rigging permits**.
3. **Corridor Protection:** Mall rules mandate laying Masonite sheets or neoprene floor runners along common marble floors between the loading dock and store entrance.

---

### 4.4 NRF 4-5-4 Retail Calendar Holiday Shopping Blackout Freeze Protocols

The retail industry generates up to 40% of its annual operating income during Q4. YM Inc. enforces an absolute deployment freeze during high-volume peak shopping windows to eliminate the risk of network outages, POS terminal disruption, or physical store hazards.

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                      NRF 4-5-4 FISCAL Q4 DEPLOYMENT FREEZE SCHEDULE                    │
│                                                                                        │
│    OCTOBER               NOVEMBER                          DECEMBER           JANUARY  │
│  ┌─────────┐   ┌─────────────────────────────┐   ┌───────────────────────────┐┌───────┐│
│  │ Wks 36-39│   │ Wks 40-42 │  WEEK 43 FREEZE  │   │ Wks 44-46 │ WEEKS 47-49   ││ Wk 50 ││
│  │ Normal  │   │ Normal    │  BLACK FRIDAY   │   │ Normal    │ CHRISTMAS &   ││ Fleet ││
│  │ Turn-Ups│   │ Turn-Ups  │  PEAK FREEZE    │   │ Turn-Ups  │ BOXING FREEZE ││ Normal││
│  └─────────┘   └───────────┴─────────────────┘   └───────────┴───────────────┘└───────┘│
│                             ▲                                ▲                         │
│                             │ Nov 25 - Dec 01                │ Dec 12 - Jan 03         │
│                             │ (US Thanksgiving               │ (Holiday Peak           │
│                             │  through Cyber Mon)            │  through Post-New-Year) │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Freeze Window Definitions:
1. **Freeze Window A (US Thanksgiving & Black Friday Peak):**
   - **Duration:** Wednesday prior to US Thanksgiving at 00:01 EST through the following Tuesday at 06:00 EST.
   - **Scope:** Complete halt of all physical cabling, power pole modifications, carrier circuit turn-ups, and firewall configuration changes across all Canadian and US stores.
2. **Freeze Window B (Christmas, Boxing Week & Post-New-Year Peak):**
   - **Duration:** December 12 at 00:01 EST through January 3 at 06:00 EST.
   - **Scope:** Absolute corporate IT code freeze and store construction ban. Only emergency store-down restorations are permitted.

#### Tiered Emergency Blackout Exception Protocol & Rapid Outage Recovery:
Peak retail events (such as Black Friday, Cyber Week, and Boxing Day) represent up to 35% of an apparel banner's annual net earnings; an unmitigated WAN/POS outage during trading hours inflicts upwards of $35,000 CAD per hour in lost revenue. Forcing an 11-hour recovery delay until nightfall to perform a simple cellular modem or switch swap would cause catastrophic revenue destruction.

To eliminate this vulnerability while upholding strict landlord quiet covenants, emergency physical intervention during blackout freezes is partitioned into a **Two-Tier Incident Response Structure**:

```
+========================================================================================================+
|                       TIERED EMERGENCY BLACKOUT EXCEPTION ARCHITECTURE                                 |
+========================================================================================================+
                                                                                                          
   INCIDENT DETECTED (Store WAN Cut / POS Offline During Black Friday Trading Hours)                      
                                      │                                                                  
                                      ▼                                                                  
                     Evaluate Physical Disturbance Classification                                        
                                      │                                                                  
            ┌─────────────────────────┴─────────────────────────┐                                        
            ▼                                                   ▼                                        
   TIER 1: NON-INVASIVE RESTORATION                    TIER 2: INVASIVE CONSTRUCTION                     
   - Cradlepoint 5G Go-Bag Dispatch                    - Slab Core Drilling                              
   - LTE Antenna Alignment inside Store                - Conduit Cutting in Common Areas                 
   - Meraki Switch/Router Swap at Cash Wrap            - Ceiling Grid Demolition / Structural Rigging    
   - Starlink Business Quick-Deploy                    - High-Noise Hammer Drilling / Power Poles        
            │                                                   │                                        
            ▼                                                   ▼                                        
   EXECUTION WINDOW: IMMEDIATE TRADING HOURS           EXECUTION WINDOW: RESTRICTED NIGHT WORK           
   (Zero Acoustic Noise, Zero Mall Common Disturbance) (22:30 to 06:00 Local Mall Quiet Hours)           
            │                                                   │                                        
            ▼                                                   ▼                                        
   AUTHORIZATION: DUAL BREAK-GLASS GATEWAY             AUTHORIZATION: DUAL VP FOUR-EYES SIGN-OFF         
   - Local Store Manager + Field Specialist Sign-Off   - VP of Retail Operations                         
   - Real-Time Cloud Audit Logging (system_audit_logs)  - VP of IT / Chief Information Officer            
   - Fallback: Auto-Delegation to Lead PM on Duty      - Pre-Notification to Mall Security Operations    
     if VP unreachable within 15 minutes                                                                 
```

1. **Tier 1 (Non-Invasive Rapid Telecom & Edge Hardware Failover):**
   - **Scope:** Complete WAN loss, primary fiber cut, secondary cellular carrier failure, POS switch failure, or router lockup inside an active store during retail trading hours.
   - **Two-Tier Hardware Failover Hierarchy:**
     * **Tier 1a: Pre-Provisioned On-Site Secondary LTE Dongle (<15 Min Failover):** Every one of the 376 stores is equipped with a pre-provisioned, pre-authenticated USB LTE dongle (Inseego USB800 / Cradlepoint CBA850 with multi-carrier auto-failover SIM) stored inside the locked store telco cabinet. A dedicated, pre-wired low-loss LMR-400 coaxial antenna lead connects from the backroom IT cabinet to an external wideband omni-directional antenna mounted at the store entrance lease line or demising wall, bypassing mall concrete and steel RF attenuation dead zones. Upon WAN failure, the Store Manager or on-site field specialist plugs the dongle into the Cisco Meraki MX68 USB WAN port or technician laptop. Failover SLA is **$\le$ 15 minutes**, requiring zero courier transit.
     * **Tier 1b: Regional Hub Rapid Go-Bag Dispatch (2–4 Hour Courier / Hub SLA):** Ten (10) dedicated ruggedized Pelican emergency go-bags (`CP-EMERG-KIT-5G`) are pre-staged across 10 strategic regional IT depots (GTA East, GTA West, Montreal, Quebec City, Calgary, Vancouver, New York Metro, Boston, Chicago, Miami). Each go-bag contains a pre-configured Cradlepoint W1850 5G dual-SIM router, magnetic-mount high-gain antennas, a Cisco Meraki MX68 spare, a Cisco MS120-8FP PoE switch, and a Starlink Mini/Business satellite kit for catastrophic edge hardware destruction (e.g., electrical surge destroying primary router/switch).
   - **Veras POS Offline Store-and-Forward Safeguard:** During the sub-15 minute failover window or intermittent cellular handoff, Veras POS terminals automatically enter PCI-DSS Level 1 compliant offline store-and-forward mode. Transactions up to **$150.00 CAD/USD** per swipe/tap are locally authorized and secured using AES-256 local queue encryption, with a storewide offline transaction ceiling of **$5,000.00**. POS checkout operations remain 100% uninterrupted.
   - **Operational Rule:** **Permitted immediately during active shopping hours.** Work creates zero acoustic noise, generates zero construction dust, and requires no common-area access.
   - **Authorization Workflow:** Initiated via the platform's mobile/desktop emergency portal. Authorized via **Store Manager + Field Specialist Break-Glass Sign-Off** with real-time audit logging to `system_audit_logs`. If corporate executive review is triggered but the VP of Operations / CIO is unreachable within **15 minutes**, authorization authority automatically cascades to the **Lead Project Manager (PM) on Duty**, ensuring store connectivity is restored in under 30 minutes.

2. **Tier 2 (Invasive Physical Infrastructure & Heavy Construction):**
   - **Scope:** Structural power pole replacement, concrete slab core drilling, ceiling grid demolition, running conduit through landlord common basement risers, or commercial electrical panel tie-ins.
   - **Operational Rule:** **Strictly barred during retail trading hours.** Work is restricted exclusively to after-hours night shifts (**22:30 to 06:00 local time**) with dedicated on-site standby.
   - **Authorization Workflow:** Mandatory **Dual-VP Electronic Authorization (Four-Eyes)** signed by:
     1. VP of Retail Operations (certifying operational necessity and risk acceptance).
     2. VP of IT / Chief Information Officer (approving technical method of procedure and vendor safety plan).
   - Advance 24-hour notice must be transmitted to Mall General Management and Security Dispatch with verified Additional Insured COI on file.

---

## 5. Phased Rollout Plan & Execution Strategy

### 5.1 Rollout Architecture & Phased Timeline Overview

The fleetwide implementation spans **16 weeks**, structured into three progressive operational phases designed to validate software stability, eliminate operational friction, and execute a disciplined transition across all 376 retail stores:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                      YM INC. FLEETWIDE IMPLEMENTATION ROADMAP                          │
│                                                                                        │
│  WEEKS 1 - 4            WEEKS 5 - 8                 WEEKS 9 - 14          WEEKS 15 - 16│
│ ┌────────────────┐     ┌──────────────────────┐    ┌─────────────────┐   ┌────────────┐│
│ │ PHASE 1: PILOT │────►│ PHASE 2: REGIONAL    │───►│ PHASE 3: FLEET  │──►│ PHASE 4:   ││
│ │ 10 Flagship    │     │ STAGED ROLLOUT       │    │ FULL DEPLOYMENT │   │ HYPERCARE  ││
│ │ Stores (US/CA) │     │ Flagships (158 St.)  │    │ 208 Stores      │   │ (376 St.)  ││
│ └────────────────┘     └──────────────────────┘    └─────────────────┘   └────────────┘│
└────────────────────────────────────────────────────────────────────────────────────────┘
```

The 376 retail stores are organized into 5 geographically aligned regional clusters:
1. **GTA (Greater Toronto Area) Cluster:** 142 stores (High-density retail corridor, corporate headquarters, and Mississauga distribution center).
2. **Quebec / East Cluster:** 86 stores (Montreal flagship corridor, Quebec City, and Atlantic Canada).
3. **Western Canada Cluster:** 68 stores (British Columbia, Alberta, Saskatchewan, and Manitoba).
4. **US Northeast Cluster:** 46 stores (New York, New Jersey, Pennsylvania, and Massachusetts).
5. **US Midwest / South Cluster:** 34 stores (Illinois, Michigan, Florida, and Texas).
**Total Fleet Size:** $142 + 86 + 68 + 46 + 34 = \mathbf{376\text{ Stores}}$ across 13 apparel banners.

---

### 5.2 Phase 1: Pilot Deployment (10 Flagship & Diverse Format Stores)

Phase 1 deploys the modernized desktop app, API gateway, and Neon database to **ten (10) representative locations (Stores #1001 to #1010)** chosen to test all physical variations (high ceilings, premier landlords, carrier diversity, historic masonry, and cross-border compliance):

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                          PHASE 1 PILOT FLAGSHIP & DIVERSE SITES                        │
│                                                                                        │
│  1. TORONTO EATON CENTRE (Store #1001)   │  2. METROPOLIS AT METROTOWN (Store #1002)   │
│  • Brand: Urban Planet Flagship          │  • Brand: Bluenotes Flagship                │
│  • Territory: Ontario / GTA (CA East)    │  • Territory: British Columbia (CA West)    │
│  • Landlord: Cadillac Fairview ($10M COI)│  • Landlord: Ivanhoé Cambridge ($5M COI)    │
│  • Physical: Standard 2x2 Acoustic Drop  │  • Physical: 18-ft Exposed Deck / High Truss│
│  • Primary Telecom: Bell Canada Fiber    │  • Primary Telecom: Telus Commercial Fiber  │
│  • Secondary Telecom: Rogers 5G Wireless │  • Secondary Telecom: Bell LTE Wireless     │
│ ─────────────────────────────────────────┼──────────────────────────────────────────── │
│  3. SAWGRASS MILLS (Store #1003)         │  4. ROOSEVELT FIELD MALL (Store #1004)      │
│  • Brand: Charlotte Russe Superstore     │  • Brand: rue21 Mega-Store                  │
│  • Territory: Florida (US South)         │  • Territory: New York (US Northeast)       │
│  • Landlord: Simon Property Group ($10M) │  • Landlord: Simon Property Group ($10M)    │
│  • Physical: High-humidity floor feeds   │  • Physical: Multi-level mall basement riser│
│  • Primary Telecom: Comcast Business Fib │  • Primary Telecom: Charter Spectrum Fiber  │
│  • Secondary Telecom: AT&T Enterprise 5G │  • Secondary Telecom: Verizon Wireless 5G   │
│ ─────────────────────────────────────────┼──────────────────────────────────────────── │
│  5. SQUARE ONE SHOPPING (Store #1005)    │  6. RUE SAINTE-CATHERINE (Store #1006)      │
│  • Brand: West 49 Flagship               │  • Brand: Sirens Flagship                   │
│  • Territory: Mississauga / GTA (CA East)│  • Territory: Montreal (Quebec / East)      │
│  • Landlord: Oxford Properties ($10M COI)│  • Landlord: Streetfront Urban Heritage     │
│  • Physical: Dual Power Pole Cash Wrap   │  • Physical: Historic Masonry / Core Drill  │
│  • Primary Telecom: Rogers Business Fib  │  • Primary Telecom: Bell Canada Fiber       │
│  • Secondary Telecom: Bell LTE Wireless  │  • Secondary Telecom: Telus LTE Wireless    │
│ ─────────────────────────────────────────┼──────────────────────────────────────────── │
│  7. WEST EDMONTON MALL (Store #1007)     │  8. YORKDALE SHOPPING CENTRE (Store #1008)  │
│  • Brand: Urban Planet Mega-Store        │  • Brand: Suzy Shier Flagship               │
│  • Territory: Alberta (CA West)          │  • Territory: Toronto / GTA (CA East)       │
│  • Landlord: Triple Five Group ($10M COI)│  • Landlord: Oxford Properties ($10M COI)   │
│  • Physical: High Open Truss / AP Array  │  • Physical: Perimeter Acoustic Drop Ceiling│
│  • Primary Telecom: Telus Business Fiber │  • Primary Telecom: Bell Canada Fiber       │
│  • Secondary Telecom: Rogers 5G Wireless │  • Secondary Telecom: Rogers 5G Wireless    │
│ ─────────────────────────────────────────┼──────────────────────────────────────────── │
│  9. HALIFAX SHOPPING CENTRE (Store #1009)│ 10. CF CHINOOK CENTRE (Store #1010)         │
│  • Brand: Stitches Regional Superstore   │  • Brand: Urban Kids Flagship               │
│  • Territory: Nova Scotia (Atlantic East)│  • Territory: Calgary / Alberta (CA West)   │
│  • Landlord: Cushman & Wakefield ($5M)   │  • Landlord: Cadillac Fairview ($10M COI)   │
│  • Physical: Concourse Low-Voltage Riser │  • Physical: Acoustic Tile / Cash Wrap Drop │
│  • Primary Telecom: Bell Aliant Fiber    │  • Primary Telecom: Shaw / Rogers Fiber     │
│  • Secondary Telecom: Eastlink LTE       │  • Secondary Telecom: Telus 5G Wireless     │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

#### Pilot Success Criteria (Go / No-Go Gate to Phase 2):
1. **Zero Data Loss:** 100% of store onboarding, power pole specs, and WAN circuit edits persist accurately to Neon without table mismatch errors.
2. **Offline Sync Verification:** Field specialists intentionally disconnect client laptops, execute 25+ modifications, and reconnect; all outbox mutations synchronize cleanly via the API Gateway within 15 seconds.
3. **Fluke LinkWare Ingestion:** Successful upload and automated PDF parsing of Fluke DSX-8000 test certificates for all cash wrap and AP data drops across all 10 pilot stores.
4. **Telecom Verification:** RFC 2544 throughput benchmarks executed on Bell, Telus, Comcast, and Charter fiber circuits with live Meraki cloud telemetry.
5. **Zero Landlord Violations:** Complete approval of $10M COIs by Cadillac Fairview, Oxford Properties, and Simon Property Group without access delays.

---

### 5.3 Phase 2: Regional Staged Rollout (158 Flagships & Regional Hubs)

Following successful Pilot sign-off, Phase 2 expands the platform across **158 stores** spanning high-volume regional flagships and core district hubs across all 5 clusters:
- **Regional Cluster Allocation:**
  * GTA Cluster: **60 stores**
  * Quebec / East Cluster: **38 stores**
  * Western Canada Cluster: **30 stores**
  * US Northeast Cluster: **18 stores**
  * US Midwest / South Cluster: **12 stores**
  * **Phase 2 Subtotal:** $60 + 38 + 30 + 18 + 12 = \mathbf{158\text{ Stores}}$.
- **Staging Cadence:** Executed across Weeks 5–8 (~40 stores activated per week across 18 operational districts).
- **Regional Super-User Training:** Project Managers host daily 60-minute virtual enablement workshops for District Managers (DM) and Field Specialists (PS1–PS4).
- **Feedback Iteration Loop:** End of Week 6 dedicated to deploying minor hotfixes and UX refinements based on field technician usability feedback.

---

### 5.4 Phase 3: Fleetwide Deployment (208 Stores across 13 Banners)

Phase 3 transitions the remaining **208 stores** across suburban, secondary mall, and strip retail formats into active production:
- **Regional Cluster Allocation:**
  * GTA Cluster: **79 stores**
  * Quebec / East Cluster: **46 stores**
  * Western Canada Cluster: **35 stores**
  * US Northeast Cluster: **27 stores**
  * US Midwest / South Cluster: **21 stores**
  * **Phase 3 Subtotal:** $79 + 46 + 35 + 27 + 21 = \mathbf{208\text{ Stores}}$.
- **Comprehensive Fleet Reconciled Arithmetic:**
  * Phase 1 Pilot: **10 stores** (GTA: 3, Quebec/East: 2, West: 3, US NE: 1, US MW/South: 1)
  * Phase 2 Regional Sweep: **158 stores** (GTA: 60, Quebec/East: 38, West: 30, US NE: 18, US MW/South: 12)
  * Phase 3 Fleetwide Deployment: **208 stores** (GTA: 79, Quebec/East: 46, West: 35, US NE: 27, US MW/South: 21)
  * **Cluster Reconciliation:**
    - GTA: $3 + 60 + 79 = \mathbf{142\text{ Stores}}$
    - Quebec / East: $2 + 38 + 46 = \mathbf{86\text{ Stores}}$
    - Western Canada: $3 + 30 + 35 = \mathbf{68\text{ Stores}}$
    - US Northeast: $1 + 18 + 27 = \mathbf{46\text{ Stores}}$
    - US Midwest / South: $1 + 12 + 21 = \mathbf{34\text{ Stores}}$
    - **Grand Total Fleet:** $142 + 86 + 68 + 46 + 34 = 10 + 158 + 208 = \mathbf{376\text{ Stores}}$ (100% mathematical harmony).
- **Batching Strategy:** ~35 stores activated per week over a 6-week execution window (Weeks 9–14).
- **Automated Roster Synchronization:** The CSV Roster Import engine ingests personnel updates, store manager transfers, and revised possession dates across all 13 banners.
- **Full Operational Transition:** Decommissioning of legacy Excel spreadsheets, local JSON files, and direct database client connections.

---

### 5.5 Operational KPIs, Service Level Agreements (SLAs) & Milestones

The platform enforces rigorous operational KPIs to benchmark deployment velocity, quality, and compliance:

| Metric / KPI Name | Baseline (Legacy System) | Target SLA (Modernized Platform) | Measurement Methodology & Formula |
| :--- | :---: | :---: | :--- |
| **Telecom Turn-Up Lead Time** | 42 Business Days | **$\le$ 21 Business Days** | Days elapsed from `Order Placed` to `Tested & Turned Up (Active)`. |
| **Cabling Deficiency Rate** | 18.5% of store drops | **$\le$ 2.0% of store drops** | Percentage of Cat6 drops failing initial Fluke Wiremap/NEXT tests. |
| **COI Compliance Timeliness** | 68% compliant at T-7 | **100% compliant at T-7** | Percentage of approved $5M–$10M COIs logged $\ge$ 7 days pre-possession. |
| **Desktop Client Sync Latency**| N/A (Local JSON silos) | **$\le$ 2.5 seconds** | Time for local outbox mutation to commit to Neon and broadcast to peers. |
| **API Availability & Uptime** | N/A (Direct TCP) | **99.95% Availability** | Unscheduled API Gateway downtime $\le$ 21.6 minutes per month. |
| **Store Onboarding Duration** | 45 minutes (Manual) | **$\le$ 8 minutes** | Time required for PA to complete 5-step onboarding wizard. |

---

### 5.6 Enterprise Risk Mitigation Playbook & Contingency Protocols

Operational disruptions during retail deployments carry heavy financial penalties. The platform implements pre-engineered contingency playbooks for four high-impact failure scenarios:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        ENTERPRISE RISK MITIGATION PLAYBOOK                             │
└────────────────────────────────────────────────────────────────────────────────────────┘

 SCENARIO 1: CARRIER FIBER FOC DATE DELAY (JEOPARDY STATUS)
 ────────────────────────────────────────────────────────────────────────────────────────
 • Trigger: Carrier issues missed FOC notice or outside plant fiber construction stalled.
 • Probability: High (Occurs in ~14% of commercial retail fiber orders).
 • Impact: Store opens without corporate WAN connection, halting POS transactions.
 • Automated Contingency Protocol:
   1. System automatically triggers "Carrier Jeopardy State" in `circuit_details`.
   2. Immediate Activation of On-Site Pre-Provisioned Secondary LTE Dongle (<15 min failover):
      Store Manager / Field Specialist retrieves the pre-authenticated USB LTE dongle (Inseego USB800 / Cradlepoint CBA850) from the locked telco cabinet, connects it to the pre-wired LMR-400 external antenna lead (eliminating mall RF dead zones), and inserts it into the Cisco Meraki MX68 USB WAN port. Cellular WAN activates within 15 minutes.
   3. Regional Hub Rapid Go-Bag Deployment (Catastrophic Edge Failure): If local edge appliance is defective, one of the 10 regional hub ruggedized go-bags (`CP-EMERG-KIT-5G` with Cradlepoint W1850 5G dual-SIM router and Starlink kit) is dispatched via courier (2–4 hour SLA).
   4. Veras POS Offline Store-and-Forward Capture: While cellular failover engages, Veras POS terminals operate in offline store-and-forward mode ($150 per transaction limit, $5,000 store ceiling, AES-256 encrypted local queue), ensuring zero checkout downtime.
   5. Liquidated damages notice automatically generated and served to primary carrier.

 SCENARIO 2: LANDLORD ISSUES STOP-WORK ORDER (COI / NOISE DISPUTE)
 ────────────────────────────────────────────────────────────────────────────────────────
 • Trigger: Mall operations halts construction due to missing endorsement or day noise.
 • Probability: Moderate (Occurs in ~6% of mall tenant buildouts).
 • Impact: $2,500/day mall delay penalties and postponed store opening.
 • Automated Contingency Protocol:
   1. Field tech flags "Mall Stop-Work Alert" on mobile desktop client.
   2. Automated SMS/Email push to Project Administrator and Corporate Insurance Broker.
   3. Real-time extraction of digital COI certificate link dispatched to Mall General Manager.
   4. Shift work hours re-scheduled to 22:30 night shift in NRF calendar work orders.
   5. Escalation resolved within <4 business hours without schedule loss.

 SCENARIO 3: OFFLINE SYNC DATA MERGE CONFLICT
 ────────────────────────────────────────────────────────────────────────────────────────
 • Trigger: Field tech edits store cabling offline while PM edits telecom dates in portal.
 • Probability: High (Common during concurrent multi-user project sprints).
 • Impact: Accidental overwrite of critical field engineering data.
 • Automated Contingency Protocol:
   1. Cloud API Hybrid Logical Clock (HLC) and Vector Clock resolver detects concurrent divergent branch ($V_A \parallel V_B$ or sub-threshold clock skew $|\Delta t| \le 50\text{ ms}$). Line-level MVCC row isolation utilizes PostgreSQL `xmin` for single-transaction isolation, while distributed conflict resolution evaluates vector clock lineage ($V_{\text{client}} \not\ge V_{\text{server}}$).
   2. If sub-threshold concurrency is detected, the server flags `CONFLICT_SUB_THRESHOLD_CONCURRENT`, preserves both mutations in `mutation_conflict_delta_log`, adopts the canonical cloud state, and raises an operator review alert in the portal.
   3. Field-level Last-Write-Wins (LWW) resolver merges non-overlapping properties across distinct columns.
   4. Domain Priority Matrix resolves semantic overlaps: Telecom Turn-Up > Cabling Notes > PM Estimates.
   5. Both versions committed to immutable PostgreSQL audit trail (`system_audit_logs`) for full transparency.

 SCENARIO 4: CONTRACTOR NON-COMPLIANCE (NO FLUKE TEST REPORT)
 ────────────────────────────────────────────────────────────────────────────────────────
 • Trigger: Low-voltage vendor attempts verbal sign-off without Fluke LinkWare upload.
 • Probability: Moderate (Vendors historically took shortcuts).
 • Impact: Latent cable crosstalk or open pairs causing intermittent POS crashes.
 • Automated Contingency Protocol:
   1. System programmatically blocks milestone transition (`Certified & Passed` locked).
   2. Four-Eyes validation rejects approval without authenticated `.flw` or PDF hash.
   3. Financial Accounts Payable holds 20% retainage fee until valid Fluke report uploads.
```

---

## 6. Actionable Prioritized Engineering Backlog

### 6.1 Backlog Structure, Estimation Methodology & Story Point Scale

The engineering backlog is structured into three execution phases aligned with business urgency. Work estimation adheres to the modified Fibonacci sequence (1, 2, 3, 5, 8, 13 Story Points), calibrated against a velocity of **1 Story Point $\approx$ 0.75 Developer-Days**:
- **Phase 1: Immediate Remediation (P0 — Critical / Blocker):** Must be deployed before Pilot onboarding.
- **Phase 2: Core Modernization (P1 — Architectural & Offline):** Deployed for Pilot and Regional staged rollout.
- **Phase 3: SaaS & Enterprise Expansion (P2 — Portal & Automation):** Deployed for Fleetwide scale.

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        ENGINEERING BACKLOG SUMMARY OVERVIEW                            │
│                                                                                        │
│   PHASE 1: IMMEDIATE REMEDIATION (P0)      PHASE 2: CORE MODERNIZATION (P1)            │
│   • 7 Epics / Stories                      • 6 Epics / Stories                         │
│   • 26 Total Story Points                  • 42 Total Story Points                     │
│   • Duration: Sprints 1 - 2 (4 Weeks)      • Duration: Sprints 3 - 5 (6 Weeks)         │
│   ─────────────────────────────────────────┼────────────────────────────────────────── │
│   PHASE 3: SAAS & ENTERPRISE (P2)          TOTAL ENGINEERING EFFORT                    │
│   • 6 Epics / Stories                      • 19 Comprehensive Epics                    │
│   • 58 Total Story Points                  • 126 Total Story Points                    │
│   • Duration: Sprints 6 - 8 (6 Weeks)      • Total Duration: 16 Weeks (8 Sprints)      │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

### 6.2 Phase 1: Immediate Remediation Backlog (P0 Critical Defect Fixes)

```markdown
### [P0-1] Correct Database Table Name Mismatch in Store Onboarding
- **Issue Reference:** `DEF-001` (`DatabaseService.cs:965, 981`)
- **Priority:** P0 (Blocker) | **Effort:** 2 Story Points (1.5 Dev-Days)
- **User Story:**
  As a Project Administrator onboarding a new retail store,
  I want the onboarding wizard to successfully insert default telecom circuits into PostgreSQL,
  So that store creation commits transactionally without SQL relation mismatch exceptions.
- **Acceptance Criteria:**
  - [x] Replace all SQL occurrences of `telecom_circuits` with authoritative table `circuit_details`.
  - [x] Align default carrier string to `'Bell Canada'` and service type to `'Commercial Fiber DIA'`, satisfying table CHECK constraints.
  - [x] Verify transactional rollback only occurs upon genuine database disconnects.
  - [x] Automated integration test creates 10 test stores sequentially and asserts 100% commit success.

### [P0-2] Remediate Silent WAN Circuit Data Loss in Store Detail Saves
- **Issue Reference:** `DEF-002` (`StoreDetailWindow.xaml.cs:338-359` & `DatabaseService.cs:678-797`)
- **Priority:** P0 (Critical / Data Loss) | **Effort:** 3 Story Points (2.25 Dev-Days)
- **User Story:**
  As a Telecom Specialist (PS4),
  I want all edits made to Primary and Secondary WAN circuits in StoreDetailWindow to persist to the database,
  So that updated circuit IDs, order statuses, and FOC dates are never lost.
- **Acceptance Criteria:**
  - [x] `SaveStoreAsync` updated to execute parameterized `UPDATE circuit_details` statements for both WAN 1 and WAN 2.
  - [x] `StoreDetailWindow.xaml.cs` binds UI fields to the `_store.Circuits` collection prior to invoking save.
  - [x] UI displays an explicit error banner if the database update fails; green checkmark displays only on verified commit.
  - [x] End-to-end integration test asserts circuit status change from `Order Placed` to `Tested & Turned Up (Active)` persists.

### [P0-3] Integrate Migration 006 into Automated Migration Runners
- **Issue Reference:** `DEF-003` (`006_app_users_auth.sql`, `migrate.js`, `apply_migrations.ps1`)
- **Priority:** P0 (Critical) | **Effort:** 2 Story Points (1.5 Dev-Days)
- **User Story:**
  As a DevOps Engineer deploying fresh environment instances,
  I want `006_app_users_auth.sql` to execute automatically during migration runs,
  So that the `app_users` table is always initialized without manual database intervention.
- **Acceptance Criteria:**
  - [x] Append `006_app_users_auth.sql` into execution sequences of `migrate.js`, `apply_migrations.ps1`, `apply_migrations.sh`, and `init/01_schema.sql`.
  - [x] Create automated migration verification test that spins up an ephemeral database, runs migrations 001–006, and asserts `app_users` table existence.

### [P0-4] Eliminate Unauthenticated Account Takeover in ResetPasswordWindow
- **Issue Reference:** `DEF-006` (`ResetPasswordWindow.xaml.cs` & `DatabaseService.cs:274-297`)
- **Priority:** P0 (Security Vulnerability) | **Effort:** 3 Story Points (2.25 Dev-Days)
- **User Story:**
  As an Enterprise Security Officer,
  I want password reset requests to require identity verification,
  So that unauthorized users cannot arbitrarily overwrite administrator passwords.
- **Acceptance Criteria:**
  - [x] Deprecate open password reset in `ResetPasswordWindow.xaml.cs`.
  - [x] Require user to supply current password before allowing password updates.
  - [x] Enforce NIST SP 800-63B password complexity (minimum 12 characters, alphanumeric, special symbols).
  - [x] Implement rate-limiting blocking more than 5 failed reset attempts per 15 minutes per IP.

### [P0-5] Eliminate Hardcoded Fallback Pepper and Plaintext Team Credentials
- **Issue Reference:** `DEF-007` & `DEF-008` (`PasswordSecurityService.cs:19` & `AuthService.cs:23-31`)
- **Priority:** P0 (Security Vulnerability) | **Effort:** 3 Story Points (2.25 Dev-Days)
- **User Story:**
  As an Enterprise Security Officer,
  I want all cryptographic pepper keys and team passwords removed from client source code and binaries,
  So that reverse-engineering the desktop application does not expose enterprise secrets.
- **Acceptance Criteria:**
  - [x] Remove string literal `FallbackPepper` from `PasswordSecurityService.cs`; throw `InvalidOperationException` if pepper environment variable is missing.
  - [x] Purge hardcoded plaintext team credentials array from `AuthService.cs`.
  - [x] Update `.env.example` to explicitly document `PASSWORD_PEPPER` requirement.

### [P0-6] Migrate Local Workstation JSON File Silos to PostgreSQL Tables
- **Issue Reference:** `DEF-009` (`live_calendar_work_items.json`, `active_editor_leases.json`, `audit_history_log.json`)
- **Priority:** P0 (Data Integrity) | **Effort:** 8 Story Points (6 Dev-Days)
- **User Story:**
  As a Field Specialist collaborating with a distributed project team,
  I want calendar work items, editor presence leases, and audit logs stored centrally in PostgreSQL,
  So that my changes are instantly visible to other team members on their workstations.
- **Acceptance Criteria:**
  - [x] Author migration `007_collaboration_tables.sql` creating tables: `calendar_work_items`, `editor_presence_leases`, and `store_audit_deltas`.
  - [x] Refactor `DatabaseService`, `PresenceService`, and `AuditHistoryService` to perform CRUD against PostgreSQL rather than local JSON files.
  - [x] Deprecate local `FileSystemWatcher` implementations.

### [P0-7] Resolve 4-Week NRF Fiscal Calendar Offset Between C# and Database
- **Issue Reference:** `DEF-005` (`DatabaseService.cs:1242` & `generate_fiscal_calendar.js`)
- **Priority:** P0 (Financial Reporting) | **Effort:** 5 Story Points (3.75 Dev-Days)
- **User Story:**
  As an Executive Analyst reviewing store deployment reports,
  I want the desktop calendar dates to align exactly with the NRF 4-5-4 standard in PostgreSQL,
  So that weekly sales and deployment progress match fiscal retail week definitions.
- **Acceptance Criteria:**
  - [x] Correct C# fallback calendar generator in `DatabaseService.cs` to align FY 2026 start date to February 1, 2026 (matching NRF standard).
  - [x] Re-run `generate_fiscal_calendar.js` and verify week joins return 100% matching records for Weeks 36–52.
```

---

### 6.3 Phase 2: Core Modernization Backlog (P1 MVVM & Offline Capabilities)

```markdown
### [P1-1] Refactor Desktop Architecture to CommunityToolkit.Mvvm & DI Container
- **Priority:** P1 (Architecture) | **Effort:** 13 Story Points (9.75 Dev-Days)
- **User Story:**
  As a Desktop Software Engineer,
  I want the WPF client refactored to the MVVM pattern with CommunityToolkit.Mvvm and Microsoft.Extensions.Hosting,
  So that UI logic is decoupled from state, testable via unit tests, and free of memory leaks.
- **Acceptance Criteria:**
  - [x] Bootstrap `App.xaml.cs` using `IHost` and `IServiceCollection`.
  - [x] Implement ViewModels: `MainViewModel`, `StoreDetailViewModel`, `AddStoreWizardViewModel`, `FiscalCalendarViewModel`.
  - [x] Replace code-behind event handlers with `[RelayCommand]` and `[ObservableProperty]`.
  - [x] Unsubscribe all static `ThemeService` events on window close, verified via JetBrains dotMemory unit tests.

### [P1-2] Implement Dynamic Multi-Step Store Onboarding Wizard
- **Priority:** P1 (Usability) | **Effort:** 8 Story Points (6 Dev-Days)
- **User Story:**
  As a Project Administrator,
  I want a guided 5-step wizard for adding new stores with live field validation,
  So that store records, drop counts, power poles, and circuits are created with complete data integrity.
- **Acceptance Criteria:**
  - [x] Multi-step wizard covering: 1. Identity & Format, 2. Real Estate Dates, 3. Low-Voltage & Cabling, 4. Power Poles, 5. Dual-WAN.
  - [x] Live validation enforcing unique store numbers and valid postal/ZIP codes.
  - [x] Landlord rules automatically inherited based on selected mall property.

### [P1-3] Implement Offline-First Embedded SQLite Cache & Polly v8 Resilience
- **Priority:** P1 (Offline Capabilities) | **Effort:** 8 Story Points (6 Dev-Days)
- **User Story:**
  As a Field Technician in an un-cabled store basement without internet connectivity,
  I want the desktop app to read and write to a local SQLite cache,
  So that I can document site surveys and cabling tests without crashing or losing data.
- **Acceptance Criteria:**
  - [x] Embed `Microsoft.Data.Sqlite` in `desktop-app/` creating `ym_local_cache.db` in `%LocalAppData%`.
  - [x] Wrap remote network queries in Polly v8 pipeline: 3 retries with exponential backoff and circuit breaker.
  - [x] Outbox queue records offline mutations with timestamps and UUIDs, automatically replaying upon network restoration.

### [P1-4] Implement Enterprise SSO via Microsoft Entra ID (MSAL.NET & PKCE)
- **Priority:** P1 (Enterprise Security) | **Effort:** 5 Story Points (3.75 Dev-Days)
- **User Story:**
  As an Enterprise User,
  I want to log in using my corporate Microsoft 365 credentials via Entra ID SSO,
  So that I do not need a separate database password and IT enforces corporate MFA.
- **Acceptance Criteria:**
  - [x] Integrate `Microsoft.Identity.Client` with Authorization Code Flow + PKCE.
  - [x] Ingest Entra ID role claims (`PM`, `PA`, `PS1-PS4`) and map to application permissions.
  - [x] Encrypt cached refresh tokens locally using Windows DPAPI (`DataProtectionScope.CurrentUser`).

### [P1-5] Remediate WPF Memory Leaks and Asynchronous Threading Anti-Patterns
- **Priority:** P1 (Performance) | **Effort:** 5 Story Points (3.75 Dev-Days)
- **User Story:**
  As a Desktop Operator,
  I want the desktop application to run continuously without UI freezing or escalating memory usage,
  So that the app remains responsive during long field shifts.
- **Acceptance Criteria:**
  - [x] Replace all `async void` event handlers with `[RelayCommand]` Task-based commands.
  - [x] Fix `DonutChartControl.cs` static event handler leaks by implementing `WeakEventManager`.
  - [x] Replace 6-second polling loop with SignalR real-time event notifications.

### [P1-6] Implement Functional CSV Roster Import Engine
- **Priority:** P1 (Operations) | **Effort:** 3 Story Points (2.25 Dev-Days)
- **User Story:**
  As a Project Administrator,
  I want to import corporate fleet CSV roster files via `RosterImportWindow`,
  So that store manager assignments and retail possession dates update in bulk across all 376 stores.
- **Acceptance Criteria:**
  - [x] Implement robust CSV parsing using `CsvHelper` with header mapping.
  - [x] Display preview grid highlighting invalid records before committing.
  - [x] Execute bulk updates inside a single database transaction with detailed error reporting.
```

---

### 6.4 Phase 3: SaaS & Enterprise Expansion Backlog (P2 Portal, Sync & Integrations)

```markdown
### [P2-1] Containerized ASP.NET Core 10 API Gateway Cluster
- **Priority:** P2 (Cloud Architecture) | **Effort:** 13 Story Points (9.75 Dev-Days)
- **User Story:**
  As a Cloud Solutions Architect,
  I want an ASP.NET Core 10 API Gateway mediating all client communications,
  So that direct database connections are eliminated and business logic is centralized.
- **Acceptance Criteria:**
  - [x] Deploy containerized REST API to Azure Container Apps with 2 replicas baseline.
  - [x] Configure connection pooling with Neon PgBouncer transaction-mode pooler.
  - [x] Enforce Row-Level Security (RLS) tenant isolation policies.

### [P2-2] Implement Bidirectional Delta Sync Engine with Vector Clocks
- **Priority:** P2 (Data Sync) | **Effort:** 13 Story Points (9.75 Dev-Days)
- **User Story:**
  As a Distributed Systems Engineer,
  I want a bidirectional delta sync engine reconciling desktop mutations with cloud state,
  So that concurrent edits across field techs and PMs merge deterministically without data loss.
- **Acceptance Criteria:**
  - [x] Cursor-based pull endpoint (`/api/v1/sync/pull`) returning changed entities since `last_sync_timestamp`.
  - [x] Batch mutation push endpoint (`/api/v1/sync/push`) with vector clocks and field-level LWW conflict resolution.
  - [x] SignalR WebSocket hub broadcasting real-time store presence and lock leases.

### [P2-3] Develop Next.js 15 Corporate Executive Web Portal
- **Priority:** P2 (Executive Reporting) | **Effort:** 13 Story Points (9.75 Dev-Days)
- **User Story:**
  As a C-Suite Executive (CEO/COO/CIO),
  I want a responsive web portal displaying fleetwide deployment analytics,
  So that I have real-time visibility into milestone progress, budget variance, and opening dates.
- **Acceptance Criteria:**
  - [x] Next.js 15 App Router web application integrated with ASP.NET Core API Gateway.
  - [x] Interactive executive dashboards rendering banner rollups and budget burn.
  - [x] Role-restricted document repository for landlord compliance and COIs.

### [P2-4] Automated Fluke LinkWare PDF Test Report Parser Microservice
- **Priority:** P2 (Quality Assurance) | **Effort:** 8 Story Points (6 Dev-Days)
- **User Story:**
  As a Cabling Specialist (PS3),
  I want uploaded Fluke DSX CableAnalyzer PDF reports parsed automatically,
  So that wiremap, length, and NEXT margins are extracted and verified without manual data entry.
- **Acceptance Criteria:**
  - [x] Python/PyMuPDF microservice extracting test parameters from uploaded Fluke PDFs.
  - [x] Store parsed PDF certificates in Neon S3-compatible Object Storage.
  - [x] Automatically update `fluke_test_certified = TRUE` and link `certification_report_url`.

### [P2-5] Landlord COI Management with Azure AI Vision OCR & Expiration Alerts
- **Priority:** P2 (Compliance) | **Effort:** 8 Story Points (6 Dev-Days)
- **User Story:**
  As a Project Administrator,
  I want uploaded insurance certificates validated via Azure AI Vision OCR,
  So that landlord limits and expiration dates are tracked with automated renewal alerts.
- **Acceptance Criteria:**
  - [x] OCR extraction of ACORD 25 certificates validating $5M/$10M liability limits.
  - [x] Automated daily scheduled job firing email/portal alerts 60, 30, and 15 days pre-expiration.
  - [x] Visual compliance badge in Store Directory reflecting active insurance status.

### [P2-6] Telecom Carrier Webhook Ingress & FOC Jeopardy Tracking
- **Priority:** P2 (Carrier Integrations) | **Effort:** 3 Story Points (2.25 Dev-Days)
- **User Story:**
  As a Telecom Specialist (PS4),
  I want automated webhook updates from Bell, Telus, Comcast, and Charter,
  So that FOC date confirmations and carrier jeopardy notifications update circuits in real time.
- **Acceptance Criteria:**
  - [x] HMAC-SHA256 authenticated webhook ingress endpoints for carrier order status updates.
  - [x] Automated SMS and portal alerts dispatched to PM upon carrier jeopardy status changes.
```

---

### 6.5 Engineering Velocity, Capacity Planning & Resource Allocation

Executing the 126 Story Point backlog requires a dedicated cross-functional engineering squad over an **8-sprint (16-week)** implementation cycle:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        ENGINEERING RESOURCE SQUAD ALLOCATION                           │
│                                                                                        │
│   • 1x Enterprise Technical Lead / Cloud Architect (100% allocation)                  │
│   • 2x Senior .NET WPF / Desktop Engineers (100% allocation - MVVM & SQLite)          │
│   • 2x Full-Stack Cloud Engineers (100% allocation - ASP.NET Core API & Next.js)       │
│   • 1x Database / DevOps Engineer (100% allocation - Neon, PgBouncer, CI/CD, KMS)     │
│   • 1x QA Automation / Security Test Engineer (100% allocation)                       │
│   ──────────────────────────────────────────────────────────────────────────────────── │
│   TOTAL SQUAD CAPACITY: ~16 Story Points per 2-Week Sprint (7 FTE Engineers)          │
│   BURNDOWN TRAJECTORY: 126 Story Points / 16 Points per Sprint = 7.87 Sprints (16 Wks) │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 7. Comprehensive Deployment Checklist

### 7.1 Pre-Deployment Verification Phase (T-30 Days to T-1 Day)

```markdown
### Database Tier Readiness (Neon Serverless PostgreSQL):
- [ ] Neon primary compute endpoint provisioned in AWS us-east-1 (`ep-shiny-hill-b7bouvcl-pooler.c-13.us-east-1.aws.neon.tech`) configured with 24/7 continuous baseline compute (minimum 0.25 CU, autoscaling to 2.0 CU, autosuspend disabled / set to continuous warm standby to support overnight night shifts across all time zones).
- [ ] Neon PgBouncer transaction-mode pooler endpoint configured and tested for 1,000 concurrent sessions.
- [ ] Dedicated HA read replica compute endpoint provisioned and routed for analytical reporting.
- [ ] Ephemeral database branch created and tested for full schema migrations (001–007).
- [ ] Database backup and PITR (Point-in-Time Recovery) retention policy validated for 30 days.

### Cloud API & Edge Infrastructure Readiness:
- [ ] Azure Container Apps environment provisioned across multi-AZ with auto-scaling (2 to 6 replicas).
- [ ] Cloudflare Enterprise Edge configured with TLS 1.3, HTTP/3, and OWASP Core WAF ruleset.
- [ ] Rate-limiting rules active (120 req/min authenticated user; 1,200 req/min branch IP).
- [ ] Synthetic keep-alive background worker active (executes `SELECT 1;` every 5 min 24/7/365 continuous warm-state probe across all North American time zones, supporting multi-timezone overnight shifts from 22:30 EST through 09:00 EST / 06:00 PST with zero cold starts).
- [ ] Application Insights / Datadog APM instrumentation validated with error alert thresholds.

### Enterprise Security & Identity Readiness:
- [ ] Microsoft Entra ID App Registrations configured with redirect URIs for Desktop (MSAL) and Web Portal.
- [ ] Application App Roles (`PM`, `PA`, `PS1-PS4`, `StoreManager`, `FieldTech`, `Auditor`) mapped to M365 security groups.
- [ ] Azure Key Vault provisioned; all database credentials, master peppers, and carrier secrets loaded.
- [ ] Client source code scanned to certify zero hardcoded secrets, connection strings, or peppers.

### Client Application & Field Hardware Readiness:
- [ ] Desktop client compiled for `net10.0-windows` with Release optimizations and Code Signing Certificate.
- [ ] ClickOnce / Microsoft Intune deployment package staged for enterprise workstation distribution.
- [ ] Local SQLite cache database creation verified in `%LocalAppData%\YMInc\Cache`.
- [ ] Field Fluke DSX-8000 CableAnalyzers calibrated with latest LinkWare firmware.
- [ ] Pre-provisioned secondary USB LTE dongles staged in locked telco cabinets across all 376 stores with pre-wired external LMR-400 antenna leads (<15 min failover).
- [ ] 10 regional hub emergency go-bags (`CP-EMERG-KIT-5G`) staged across 10 strategic depots for rapid hardware replacement.
- [ ] Cradlepoint W1850 5G backup routers and Cisco Meraki MX68 units staged in IT staging facility.
```

---

### 7.2 Deployment Day Go-Live Execution Phase (T-0 Runbook)

```markdown
### T-0 Maintenance Window Runbook (Execution Window: Sunday 02:00 - 06:00 EST):
- [ ] **02:00 EST - Window Declaration:** Issue corporate change advisory broadcast declaring maintenance window.
- [ ] **02:05 EST - Data Freeze & Snapshot:** Lock legacy database access; trigger instant Neon production storage snapshot.
- [ ] **02:15 EST - Migration Execution:** Execute final migration scripts (006, 007) against direct unpooled Neon host.
- [ ] **02:30 EST - Schema Validation:** Run automated schema assertions verifying `circuit_details`, `app_users`, and collaboration tables.
- [ ] **02:45 EST - Backend Deployment:** Deploy ASP.NET Core 10 API Gateway container images to Azure Container Apps.
- [ ] **03:00 EST - Smoke Testing:** Execute synthetic end-to-end integration tests validating token auth, store loads, and saves.
- [ ] **03:30 EST - Desktop Client Push:** Activate Intune / ClickOnce push distributing modernized desktop client (v2.0.0).
- [ ] **04:15 EST - Field Specialist Login Test:** Designated Pilot specialists execute test logins via Entra ID SSO.
- [ ] **04:45 EST - Bidirectional Sync Validation:** Test outbox mutation and vector clock sync between two concurrent clients.
- [ ] **05:30 EST - Executive Portal Sign-Off:** VP of IT and Lead Architect review live fleet health analytics on web portal.
- [ ] **06:00 EST - Production Go-Live:** Declare maintenance window complete; open platform for fleetwide operations.
```

---

### 7.3 Post-Deployment Hypercare & Steady-State Verification Phase (T+1 to T+30 Days)

```markdown
### Daily Hypercare Operations (Days 1 - 14):
- [ ] Daily 08:30 EST Engineering Standup reviewing Application Insights exception logs and sync latencies.
- [ ] Real-time monitoring of Neon compute units, autoscaling triggers, and 24/7 warm baseline metrics.
- [ ] Verification of daily outbox sync queues; confirm zero orphaned or conflicted field mutations.
- [ ] Triage and rapid resolution of field technician usability tickets (<4 hour SLA for critical issues).

### Weekly Governance & Milestones (Days 15 - 30):
- [ ] Weekly review of Fluke cable test report ingestion; audit compliance with 100% digital certificate rule.
- [ ] Verification of landlord COI expiration alerts; validate zero stop-work notices across active buildouts.
- [ ] Execute bi-weekly restore drills against Neon copy-on-write database branches to validate disaster recovery.
- [ ] Transition platform ownership to steady-state IT Operations and Enterprise Support teams.
```

---

### 7.4 Rollback & Disaster Recovery Protocols

If an unrecoverable failure occurs during the T-0 deployment window that cannot be resolved within 60 minutes:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        DISASTER RECOVERY & ROLLBACK RUNBOOK                            │
│                                                                                        │
│   DECISION GATE: T-0 + 90 MINUTES (03:30 EST) IF CRITICAL TEST SUITE FAILS             │
│                                                                                        │
│   STEP 1: ABORT BROADCAST & TRAFFIC CUTOVER                                            │
│   • Abort Intune desktop client distribution.                                          │
│   • Cloudflare WAF reroutes API traffic to maintenance maintenance static page.        │
│                                                                                        │
│   STEP 2: DATABASE RESTORE VIA NEON INSTANT POINT-IN-TIME RECOVERY                     │
│   • Execute Neon instant PITR restoring database state to 02:04 EST snapshot:          │
│     `neon branches create rollback-branch --parent-point 2026-10-25T02:04:00Z`         │
│   • Re-point application connection strings to validated rollback branch.              │
│                                                                                        │
│   STEP 3: CLIENT ROLLBACK & STABILIZATION                                              │
│   • Workstations retain previous desktop client version (v1.0.4) in offline fallback.  │
│   • Field operations continue using local cache until root cause analysis completes.   │
│                                                                                        │
│   STEP 4: POST-INCIDENT REVIEW                                                         │
│   • Incident commander convenes RCA within 24 hours to remediate pipeline defects.     │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 8. Conclusion & Strategic Next Steps

The YM Inc. Store Deployment & Telecom Management platform is positioned to transition from an operational liability characterized by architectural debt and direct database coupling into an industry-leading, enterprise-grade retail capital management system. 

By executing this strategic implementation roadmap:
1. **IT Hosting & Data Security:** Serverless Neon PostgreSQL, PgBouncer pooling, and Azure Key Vault eliminate database connection exhaustion, protect sensitive retail infrastructure data, and deliver high availability at a modest **$577.10/month TCO**.
2. **Enterprise Governance:** A rigorous 8-role RBAC matrix, separation of duties, and four-eyes approval gateways guarantee compliance, eliminate contractor fraud, and ensure zero unverified milestone sign-offs.
3. **Operational Discipline:** Comprehensive SOPs for $5M–$10M landlord COIs, night work windows, and NRF 4-5-4 retail holiday blackout freezes eliminate mall stop-work orders and protect peak holiday retail revenues.
4. **Actionable Delivery:** A disciplined 16-week phased rollout (Pilot $\rightarrow$ Regional $\rightarrow$ Fleetwide) guided by a prioritized 126 Story Point engineering backlog delivers immediate defect remediation while building a resilient, offline-first hybrid cloud future for YM Inc.'s 376 retail stores.
