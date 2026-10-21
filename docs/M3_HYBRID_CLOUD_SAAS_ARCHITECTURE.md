# Hybrid Cloud SaaS Architecture Design Specification
## YM Inc. Store Deployment & Telecom Management Platform

**Document Identifier:** `YM-ARCH-M3-2026-V1`  
**Milestone:** M3 — Hybrid Cloud SaaS Architecture Design  
**Author:** YM Inc. Enterprise Cloud Architecture Team  
**Date:** October 2026  
**Status:** Approved Technical Architecture Blueprint  
**Target Systems:** .NET 10 WPF Desktop Application, Next.js 15 Corporate Management Portal, Neon Serverless PostgreSQL, Microsoft Entra ID (Azure AD), Cloudflare Enterprise Edge  

---

## Executive Summary & System Vision

YM Inc. operates a multi-brand retail apparel enterprise comprising 376 retail stores across 13 distinct brand banners (including Charlotte Russe, Bluenotes, rue21, Urban Planet, Suzy Shier, and West 49) spanning Canada and the United States. Managing continuous capital store deployments, Cat6 plenum structured cabling, Wiremold 25DTP-4 dual-channel power poles, carrier dual-WAN telecom turn-ups (Fiber DIA + 5G/LTE failover), institutional landlord compliance mandates, and NRF 4-5-4 retail calendar blackouts requires an enterprise-grade, highly available, distributed cloud architecture.

The existing deployment infrastructure operates as an on-premise, two-tier architecture where .NET 10 WPF desktop clients establish direct, unpooled TCP connections to PostgreSQL 16 over port 5432 using elevated static database credentials stored in client-side `.env` files. Furthermore, critical operational records—such as daily calendar work orders, editor presence leases, and change audit histories—reside in disconnected local JSON files on individual operator workstations. This causes severe data loss, concurrent write collisions, zero visibility for executive leadership, and complete failure during offline field engineering.

This document specifies the **Target Four-Tier Hybrid Cloud SaaS Architecture** designed to resolve these structural defects:
1. **Tier 1 (Client Layer):** An offline-first .NET 10 WPF desktop application for field specialists and project managers, paired with a modern Next.js 15 corporate executive portal for C-suite and regional leadership.
2. **Tier 2 (Ingress & Edge Layer):** Cloudflare Enterprise Edge providing DDoS mitigation, WAF, TLS 1.3 termination, and intelligent routing to an ASP.NET Core 10 / Kestrel API Gateway.
3. **Tier 3 (Application & Microservices Layer):** A stateless, containerized API Gateway mediating all client communications, handling Microsoft Entra ID OIDC/PKCE token validation, fine-grained Role-Based Access Control (RBAC), bidirectional delta synchronization, and real-time distributed WebSocket presence.
4. **Tier 4 (Data & Persistence Layer):** Neon Serverless PostgreSQL (Lakebase Postgres) leveraging PgBouncer connection pooling, 24/7 baseline autoscaling, read replicas, Row-Level Security (RLS) multi-tenant isolation, immutable audit delta triggers, and S3-compatible object storage.

```
+======================================================================================================+
|                                      GLOBAL CLIENT TIER                                              |
|                                                                                                      |
|   +------------------------------------+              +------------------------------------------+   |
|   |   .NET 10 WPF Desktop Client       |              |   Next.js 15 Corporate Web Portal        |   |
|   |   (Field PMs, PAs, Tech Specialists|              |   (Executive Leadership, Landlords,      |   |
|   |   Local SQLite Replica, WSS Sync)  |              |   Regional Directors, Vendors)           |   |
|   +-----------------+------------------+              +--------------------+---------------------+   |
+=====================|======================================================|=========================+
                      | HTTPS / TLS 1.3 (REST / Delta Sync)                  | HTTPS (SSR / React 19)
                      | WSS (Bi-Directional Presence Leases)                 | WSS (Live Analytics)
+=====================|======================================================|=========================+
|                     ▼                                                      ▼                         |
|   +----------------------------------------------------------------------------------------------+   |
|   |                     EDGE LAYER (Cloudflare Enterprise / Azure Front Door)                    |   |
|   |      WAF (OWASP Top 10) | DDoS L3/L4/L7 Mitigation | Anycast DNS | TLS 1.3 Hardware Term     |   |
|   +----------------------------------------------+-----------------------------------------------+   |
+==================================================|===================================================+
                                                   ▼
+======================================================================================================+
|                       TIER 2: API GATEWAY & MICROSERVICES CLUSTER                                    |
|                                                                                                      |
|   +----------------------------------------------------------------------------------------------+   |
|   |                 ASP.NET Core 10 Kestrel High-Throughput API Gateway                          |   |
|   |     - Microsoft Entra ID JWT / OIDC / PKCE Token Validation & Claims Transformation          |   |
|   |     - Policy-Based RBAC Enforcement (PM, PA, PS1-PS4, Store Mgr, Field Tech, Contractor)     |   |
|   |     - Cursor-Based Delta Synchronization & Vector Clock Conflict Resolution Engine           |   |
|   |     - WebSocket Presence Hub & Section-Level Lease Coordinator (SignalR Core)                |   |
|   |     - RFC 7807 Problem Details Standardized Error Pipeline                                   |   |
|   +----------------------------------------------+-----------------------------------------------+   |
|                                                  |                                                   |
|         +----------------------------------------+---------------------------------------+           |
|         ▼                                        ▼                                       ▼           |
|  +--------------------+               +--------------------+                  +--------------------+ |
|  | Fluke LinkWare PDF |               | COI Verification   |                  | Carrier Webhook    | |
|  | Parser Microservice|               | & OCR Engine       |                  | Ingress Pipeline   | |
|  | (Python / PyMuPDF) |               | (Azure AI Vision)  |                  | (Bell, Telus, AT&T)| |
|  +--------------------+               +--------------------+                  +--------------------+ |
+==================================================|===================================================+
                                                   ▼
+======================================================================================================+
|                       TIER 3: DATA & PERSISTENCE LAYER (NEON POSTGRESQL)                             |
|                                                                                                      |
|   +----------------------------------------------------------------------------------------------+   |
|   |                    PgBouncer Distributed Connection Pooler                                   |   |
|   |   (ep-shiny-hill-b7bouvcl-pooler.c-13.us-east-1.aws.neon.tech / Max 10,000 Clients)          |   |
|   +----------------------------------------------+-----------------------------------------------+   |
|                                                  |                                                   |
|         +----------------------------------------+---------------------------------------+           |
|         ▼ (Write Transactions)                                                           ▼ (Queries) |
|  +---------------------------------------------+               +-----------------------------------+ |
|  | Primary Compute Endpoint (Read/Write)       |               | Read Replica Endpoint (Autoscale) | |
|  | - 24/7 Warm Baseline (0.25 to 4 CU Dynamic) |               | - Dedicated Reporting & Analytics | |
|  | - PostgreSQL 16 Engine                      |==== WAL =====>| - Serves v_store_lifecycle_status | |
|  | - Row-Level Security (RLS) Tenant Policies  |   Replication | - Serves v_circuit_readiness_track| |
|  | - Append-Only Immutable CDC Audit Triggers  |               | - Serves NRF 4-5-4 Rollups        | |
|  +---------------------------------------------+               +-----------------------------------+ |
|         │                                                                                            |
|         ▼                                                                                            |
|  +-----------------------------------------------------------------------------------------------+   |
|  | Lakebase Decoupled Storage Engine (Multi-AZ Replicated S3 Storage Architecture)               |   |
|  | Instant Copy-on-Write Branching for Staging, CI/CD Schema Validation & Disaster Recovery       |   |
|  +-----------------------------------------------------------------------------------------------+   |
|                                                                                                      |
|  +-----------------------------------------------------------------------------------------------+   |
|  | Neon Object Storage (S3-Compatible)                                                           |   |
|  | /fluke-certifications/*  |  /site-survey-photos/*  |  /landlord-coi-docs/*                    |   |
|  +-----------------------------------------------------------------------------------------------+   |
+======================================================================================================+
```

---

## 1. Multi-Tenant Cloud Backend Architecture

### 1.1 High-Performance API Gateway Framework Selection & Design

To eliminate the brittle two-tier direct database connection pattern where end-user workstations run raw SQL over port 5432, all communication must be mediated by a dedicated API Gateway.

#### Evaluation of Framework Candidates

| Evaluation Criteria | ASP.NET Core 10 (Selected) | FastAPI (Python 3.12) | Node.js (Fastify / NestJS) |
| :--- | :--- | :--- | :--- |
| **Throughput & Raw Concurrency** | **Exceptional:** Kestrel socket engine achieves >1.1M RPS in TechEmpower benchmarks; native zero-allocation memory pooling. | **Moderate:** Uvicorn/Starlette ASGI processes achieve ~70k RPS; GIL bottlenecks on CPU-bound serialization. | **High:** Fastify V8 event loop handles ~120k RPS; single-threaded event loop vulnerable to compute starvation. |
| **Ecosystem & Shared Code** | **100% Shared Contracts:** Common C# models, validation attributes, and DTOs shared directly with the .NET 10 WPF client. | **Zero Code Sharing:** Requires duplicating data structures between Pydantic and C# POCOs. | **Zero Code Sharing:** Requires duplicating TypeScript interfaces and C# classes manually or via code-gen. |
| **Real-Time WebSockets** | **Native SignalR Core:** Distributed backplane support (Redis/Azure SignalR), automated reconnection, message pack binary serialization. | **WebSockets via ASGI:** Requires manual connection lifecycle, heartbeat tracking, and broadcast room management. | **Socket.io / ws:** Mature ecosystem, but requires external microservices for enterprise clustering. |
| **Database Driver Performance** | **Npgsql 10.0.3:** Industry-leading PostgreSQL ADO.NET driver supporting binary protocol, array binding, and `NpgsqlBatch`. | **asyncpg:** High performance, but lacks seamless LINQ/EF Core schema tooling for enterprise migrations. | **pg / Prisma:** High memory overhead; Prisma engine introduces Rust-based query engine latency overhead. |
| **Enterprise Security & Auth** | **Native Microsoft Identity Web:** Direct out-of-the-box integration with Entra ID, Azure Key Vault, and Windows DPAPI. | Requires third-party libraries (`python-jose`, `authlib`); manual token validation and role mapping. | Requires manual Passport.js or `@azure/msal-node` configuration with custom claim validation handlers. |

#### Architectural Decision: ASP.NET Core 10 Web API
The gateway is implemented in **ASP.NET Core 10 (C# 13)** utilizing Minimal APIs for ultra-low latency routing and Controller endpoints for complex multi-step transactions. Python is retained strictly as an isolated, containerized worker service for PyMuPDF/OCR parsing of vendor Fluke cable test reports.

#### Edge Routing, WAF & Security Ingress
The API Gateway is fronted by **Cloudflare Enterprise Edge** (or Azure Front Door):
1. **DDoS Protection:** Layer 3/4 SYN flood mitigation and Layer 7 behavioral rate limiting (120 requests/minute per authenticated user; 1,200 requests/minute per corporate branch IP).
2. **WAF Core Ruleset:** OWASP Top 10 protection blocking SQL injection, cross-site scripting (XSS), path traversal, and header manipulation attempts.
3. **TLS Termination & Cipher Suites:** TLS 1.3 mandatory with strict cipher suites (`TLS_AES_256_GCM_SHA384`, `TLS_CHACHA20_POLY1305_SHA256`). HSTS enforced with a 2-year `max-age` and preload directive.
4. **Compression & HTTP/3:** QUIC/HTTP/3 enabled with dynamic Brotli compression for JSON responses exceeding 1 KB, reducing mobile field telemetry latency by 68%.

---

### 1.2 Neon Serverless PostgreSQL Data Layer Architecture

The database layer transitions from an unmanaged, directly exposed database instance to a managed **Neon Serverless PostgreSQL (Lakebase Postgres)** topology.

```
                           +----------------------------------------+
                           |       API Gateway Connection Router    |
                           +-------------------+--------------------+
                                               |
                     +-------------------------+-------------------------+
                     |                                                   |
                     ▼ (Port 5432)                                       ▼ (Port 5432)
       +----------------------------+                       +----------------------------+
       |   PgBouncer Pooler Endpoint|                       |   Direct Unpooled Endpoint |
       |   (-pooler.us-east-1...neon) |                     |   (us-east-1.aws.neon.tech)|
       |   Transaction-Mode Pooling |                       |   DDL Migrations & Schema  |
       +--------------+-------------+                       +--------------+-------------+
                      |                                                    |
                      ▼                                                    |
       +----------------------------+                                      |
       |  Primary Read/Write Compute|                                      |
       |  Autoscaling: 0.25 - 4 CU  |<-------------------------------------+
       |  24/7 Baseline (0.25 CU min|
       +--------------+-------------+
                      |
                      | Write-Ahead Log (WAL) Streaming
                      ▼
       +----------------------------+
       |  Read Replica Compute Node |
       |  Autoscaling: 0.25 - 2 CU  |
       |  High-Volume Analytics     |
       +--------------+-------------+
                      |
                      ▼
       +------------------------------------------------------------+
       |           Lakebase Decoupled Storage Architecture          |
       |      Multi-AZ Cloud Storage / Copy-on-Write Branching      |
       +------------------------------------------------------------+
```

#### Connection Pooling Topology (PgBouncer) & Traffic Delineation
Neon provides an integrated PgBouncer pooler that mediates client connections to PostgreSQL compute nodes. However, enterprise architecture mandates strict separation of transactional database workloads from asynchronous notification channels:

1. **Application Pooler Host (Transactional Query Traffic):**
   - **Endpoint:** `ep-shiny-hill-b7bouvcl-pooler.c-13.us-east-1.aws.neon.tech` (`DATABASE_URL`)
   - **Pooling Mode:** **Transaction-Mode Pooling**. In transaction mode, a physical PostgreSQL connection is checked out from the pool only for the lifespan of an active transaction. Upon `COMMIT` or `ROLLBACK`, the physical backend connection is returned to the pool immediately.
   - **Capacity:** Supports up to **10,000 concurrent client virtual connections** mapped down to a physical pool of 30–50 PostgreSQL backends, entirely preventing connection exhaustion during fleetwide synchronization events.
   - **CRITICAL ARCHITECTURAL CONSTRAINT — `LISTEN/NOTIFY` Failure in Transaction Pooling:** In PostgreSQL, the `LISTEN` command registers an event listener on a specific physical database backend session (`BackendId`). Because PgBouncer in transaction mode reallocates physical server connections across transaction boundaries, session-level listener state is discarded the instant a transaction completes. Subsequent `NOTIFY` events from other clients are either dropped or delivered to unrelated backend processes with no active listener. **PostgreSQL `LISTEN/NOTIFY` cannot and must not run over transaction-pooled connections.**

2. **Delineation of Pooled Queries vs. Asynchronous Notification Backplanes:**
   - **Transactional & Analytical Traffic:** All REST/GraphQL CRUD requests, delta sync transactions (`/api/v1/sync/*`), and business mutations route strictly through the PgBouncer pooled endpoint (`DATABASE_URL`).
   - **Real-Time Presence & Lease Notification Backplane:** Distributed SignalR presence, section-level lease acquisitions, and cache invalidation notifications are strictly decoupled from the database connection pool. They are offloaded to **Azure SignalR Service** / **Azure Web PubSub** or a dedicated **Redis Sentinel / Managed Redis cluster** acting as the distributed multi-node message backplane.
   - **Database-Originated Change Event Daemon (Unpooled):** If application microservices require listening directly to PostgreSQL table-level triggers or WAL changes, the event listener runs as an isolated, single-instance background worker (`PostgresNotificationListenerService`) connecting exclusively to the **Direct Unpooled Endpoint** (`DATABASE_URL_UNPOOLED` at `ep-shiny-hill-b7bouvcl.c-13.us-east-1.aws.neon.tech`), maintaining a persistent long-lived session that republishes change events into the Redis/SignalR bus.

3. **Direct Unpooled Host:**
   - **Endpoint:** `ep-shiny-hill-b7bouvcl.c-13.us-east-1.aws.neon.tech` (`DATABASE_URL_UNPOOLED`)
   - **Purpose:** Reserved exclusively for DDL schema migrations, database seeding scripts, the dedicated notification listener daemon, and long-running reporting extracts, preventing statement timeouts and transaction-pool starvation.

#### 24/7 Warm Baseline Autoscaling & Elimination of Scale-to-Zero
Neon decouples compute (stateless CPU/RAM nodes) from storage (persistent cloud object storage):
1. **Dynamic Autoscaling:** Compute nodes scale dynamically between **0.25 Compute Units (CU)** (0.25 vCPU, 1 GB RAM) during off-peak hours and **4.0 CU** (4 vCPU, 16 GB RAM) during peak retail morning operational turn-up windows.
2. **Elimination of Overnight Scale-to-Zero:** In retail store deployment, enclosed shopping centers legally prohibit acoustic noise and core drilling during daytime trading hours (10:00–21:00), forcing 100% of structured cabling, power pole anchoring, and electrical work into nocturnal windows (**22:30 to 06:00 local time**). Because YM Inc.'s 376 retail stores span four North American time zones (Eastern, Central, Mountain, Pacific), active night shifts operate continuously across the fleet from 22:30 EST to 09:00 EST (06:00 PST). Permitting Neon compute to scale to zero overnight between 23:00 and 06:00 EST would cause repeated 3–8 second cold-start latency spikes, connection timeouts, and mobile sync failures for electricians and field specialists uploading Fluke `.FLW` datasets and certifying milestones throughout the night.
3. **Continuous 24/7 Warm Baseline (0.25 CU Minimum):** The production compute endpoint is configured with a **24/7 warm baseline of at least 0.25 CU minimum** ($0.16/CU-hour $\times$ 435 CU-hours/month = **$69.60/month**, matching the M4 financial model). To guarantee the node is never suspended, a lightweight background timer within the API Gateway executes a synthetic keep-alive query (`SELECT 1;`) every 10 minutes continuously around the clock, guaranteeing sub-millisecond query response for both daytime retail planners and nocturnal field trades.
4. **Read Replica Routing:** A dedicated read replica compute node is provisioned to serve read-only analytical workloads (`v_store_lifecycle_status`, `v_circuit_readiness_tracker`, `v_weekly_store_performance`) and C-suite reporting queries. The API Gateway utilizes an `IDbContextFactory` or Npgsql data source splitting reads to the replica and writes to the primary, isolating operational transactions from heavy reporting scans.

#### Copy-on-Write Database Branching Architecture
Neon’s storage engine enables instant, copy-on-write branching without duplicating underlying data:
- **Production Branch (`main`):** The authoritative, live data store holding 376 stores and 13 apparel banners.
- **Staging Branch (`staging`):** An isolated clone updated weekly from production WAL logs, allowing complete end-to-end integration testing of new software releases against realistic store data without risking production corruption.
- **Ephemeral Feature Branches (`feat-*`):** Created dynamically via the Neon CLI during GitHub Actions CI/CD pipeline runs (`neon checkout test-pr-42 --create`). Migrations (such as 006 and 007) are applied and verified against automated integration test suites before the ephemeral branch is instantly destroyed.

---

### 1.3 Multi-Tenant Tenant Isolation Model

YM Inc. operates a federated corporate structure consisting of 13 commercial apparel banners, grouped across two sovereign corporate operating subsidiaries:
1. **YM Inc. Canada (Retail Banner Portfolio):** Urban Planet (UP), Bluenotes (BN), Suzy Shier (SS), Stitches (STN), West 49 (W49), Sirens (SRN), Urban Kids (UK), Forever 21 Canada (F21), Urban Behavior (UB).
2. **YM Inc. USA (Retail Banner Portfolio):** Charlotte Russe (CR), rue21 (R21), Mandee (M).

#### Isolation Architecture Analysis: RLS vs. Schema Isolation vs. Database-per-Tenant

```
+--------------------------------------------------------------------------------------------------+
| MULTI-TENANCY ISOLATION COMPARISON                                                               |
+------------------------------------+--------------------------+----------------------------------+
| Architectural Pattern              | Advantages               | Disadvantages                    |
+------------------------------------+--------------------------+----------------------------------+
| 1. Database-per-Tenant             | Maximum physical data    | Extreme operational cost;        |
|                                    | separation; zero risk of | impossible to execute fleetwide  |
|                                    | cross-tenant data leaks. | executive reporting or NRF roll- |
|                                    |                          | ups across banners; 26+ pools.   |
+------------------------------------+--------------------------+----------------------------------+
| 2. Schema-per-Tenant               | Separate PostgreSQL      | Migration complexity; connection |
|    (Dedicated Schema per Banner)   | namespace per banner;    | pool exhaustion across schemas;  |
|                                    | custom column support.   | high latency when joining malls. |
+------------------------------------+--------------------------+----------------------------------+
| 3. Shared Database with            | Sub-millisecond joins    | Requires rigorous policy author- |
|    Row-Level Security (RLS)        | across banners; single   | ing and testing; leaky tenant    |
|    (SELECTED HYBRID MODEL)         | migration pipeline; high | filters if context is unpinned.  |
|                                    | connection pool reuse.   |                                  |
+------------------------------------+--------------------------+----------------------------------+
```

#### The YM Inc. Hybrid Isolation Blueprint
YM Inc. implements a **Hybrid Tenant Isolation Architecture**:
1. **Row-Level Security (RLS) for Banner & Subsidiary Portfolios:** All core operational tables (`stores`, `circuit_details`, `cabling_operations`, `power_poles`, `site_surveys`, `weekly_store_sales`) reside in the public schema with mandatory tenant discriminator columns (`tenant_id UUID`, `banner_id SMALLINT`). Row-level security policies enforce that store managers and district specialists can only query and mutate stores assigned to their banner jurisdictions, while C-suite executives and lead Project Managers possess fleetwide bypass visibility.
2. **Dedicated Schema Sandbox for External Contractors:** External low-voltage cabling vendors (Granite, Black Box) and general contractors are isolated to a restricted schema (`contractor_sandbox`) utilizing PostgreSQL database views and updatable foreign tables. They have zero direct access to financial sales facts, leasing terms, or landlord rent figures.

#### Production Row-Level Security (RLS) DDL Implementation

```sql
-- ============================================================================
-- MIGRATION: 008_multi_tenant_rls_architecture.sql
-- Description: Establishes enterprise RLS policies and tenant session contexts
-- ============================================================================

-- 1. Enable RLS on core operational tables
ALTER TABLE stores ENABLE ROW LEVEL SECURITY;
ALTER TABLE circuit_details ENABLE ROW LEVEL SECURITY;
ALTER TABLE cabling_operations ENABLE ROW LEVEL SECURITY;
ALTER TABLE power_poles ENABLE ROW LEVEL SECURITY;
ALTER TABLE site_surveys ENABLE ROW LEVEL SECURITY;

-- 2. Add tenant and subsidiary identifiers
ALTER TABLE stores 
    ADD COLUMN IF NOT EXISTS tenant_id UUID NOT NULL DEFAULT 'a0000000-0000-0000-0000-000000000001',
    ADD COLUMN IF NOT EXISTS subsidiary_code VARCHAR(10) NOT NULL DEFAULT 'YM_CANADA';

-- Create composite index for high-speed tenant lookups
CREATE INDEX IF NOT EXISTS idx_stores_tenant_banner 
    ON stores (tenant_id, banner_id, operational_status);

-- 3. Define the Global Security Isolation Function
CREATE OR REPLACE FUNCTION fn_current_user_has_banner_access(target_banner_id SMALLINT)
RETURNS BOOLEAN AS $$
DECLARE
    v_user_role VARCHAR(32);
    v_allowed_banners SMALLINT[];
BEGIN
    -- Read session variables injected by API Gateway
    v_user_role := NULLIF(current_setting('app.current_user_role', true), '');
    
    -- Executive, PM, PA, and Auditor roles have unrestricted cross-banner access
    IF v_user_role IN ('ADMIN', 'PM', 'PA', 'AUDITOR') THEN
        RETURN TRUE;
    END IF;

    -- Retrieve array of banner IDs authorized in user's JWT claim
    v_allowed_banners := string_to_array(NULLIF(current_setting('app.authorized_banner_ids', true), ''), ',')::SMALLINT[];
    
    IF v_allowed_banners IS NOT NULL AND target_banner_id = ANY(v_allowed_banners) THEN
        RETURN TRUE;
    END IF;

    RETURN FALSE;
END;
$$ LANGUAGE plpgsql STABLE SECURITY DEFINER;

-- 4. Apply RLS Policy on Stores
DROP POLICY IF EXISTS tenant_store_isolation_policy ON stores;
CREATE POLICY tenant_store_isolation_policy ON stores
    FOR ALL
    USING (
        -- Enforce tenant GUID match
        tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::UUID
        AND fn_current_user_has_banner_access(banner_id)
    )
    WITH CHECK (
        tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::UUID
        AND fn_current_user_has_banner_access(banner_id)
    );

-- 5. Child Table Cascading RLS Policy (circuit_details)
DROP POLICY IF EXISTS tenant_circuit_isolation_policy ON circuit_details;
CREATE POLICY tenant_circuit_isolation_policy ON circuit_details
    FOR ALL
    USING (
        EXISTS (
            SELECT 1 FROM stores s 
            WHERE s.store_id = circuit_details.store_id
        )
    );
```

#### API Gateway Session Context Injection
On every transaction pulled from PgBouncer, the API Gateway inspects the validated JWT claims and executes an atomic setup block within the connection before querying:
```csharp
// Infrastructure/Data/TenantSessionInterceptor.cs
public async Task SetTenantSessionContextAsync(NpgsqlConnection conn, ClaimsPrincipal user)
{
    var tenantId = user.FindFirst("tenant_id")?.Value ?? "a0000000-0000-0000-0000-000000000001";
    var userRole = user.FindFirst(ClaimTypes.Role)?.Value ?? "FIELD_TECH";
    var bannerIds = string.Join(",", user.FindAll("banner_access").Select(c => c.Value));

    const string sql = @"
        SELECT set_config('app.current_tenant_id', @tenantId, true);
        SELECT set_config('app.current_user_role', @userRole, true);
        SELECT set_config('app.authorized_banner_ids', @bannerIds, true);";

    await using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("tenantId", tenantId);
    cmd.Parameters.AddWithValue("userRole", userRole);
    cmd.Parameters.AddWithValue("bannerIds", bannerIds);
    await cmd.ExecuteNonQueryAsync();
}
```

---

### 1.4 Centralized Immutable Audit Logging Engine

The desktop application previously wrote audit trails to a local workstation file (`audit_history_log.json`) capped at 500 entries. This allowed local tampering, provided zero corporate visibility, and discarded operational history upon application reinstallation.

The Hybrid Cloud SaaS Architecture establishes a **Centralized Immutable Audit Logging Engine** implemented directly inside PostgreSQL using Change Data Capture (CDC) triggers and an append-only, partitioned audit ledger.

```
       Client Action (REST / Web / Desktop)
                       │
                       ▼
         +----------------------------+
         |     Target SQL Mutation    |
         |  (UPDATE / INSERT / DELETE)|
         +-------------+--------------+
                       │
                       ▼
         +----------------------------+
         |  PostgreSQL CDC Trigger    |
         |  (fn_audit_log_capture)    |
         +-------------+--------------+
                       │
         +-------------+------------------------------------+
         │ Calculates JSONB Delta (OLD vs NEW)              │
         │ Captures User, Role, IP, Session, Vector Clock   │
         │ Computes SHA-256 Cryptographic Hash Chain        │
         +-------------+------------------------------------+
                       │
                       ▼
         +--------------------------------------------------+
         |  system_audit_logs Table (Append-Only Partition) |
         |  - Range Partitioned by Month                    |
         |  - Immutable: UPDATE & DELETE Strictly Revoked   |
         +-------------+------------------------------------+
                       │
                       ▼ Asynchronous Stream
         +--------------------------------------------------+
         |  CloudWatch / Azure Monitor WORM Compliance Vault|
         |  - 7-Year Retained Immutability for Retail SOX   |
         +--------------------------------------------------+
```

#### Audit Schema DDL with SHA-256 Cryptographic Hash Chaining

```sql
-- ============================================================================
-- MIGRATION: 009_immutable_audit_logging_engine.sql
-- Description: Centralized, append-only, cryptographically verifiable audit log
-- ============================================================================

CREATE TABLE system_audit_logs (
    audit_id BIGSERIAL,
    event_timestamp TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    tenant_id UUID NOT NULL,
    entity_name VARCHAR(64) NOT NULL,
    entity_id VARCHAR(64) NOT NULL,
    action VARCHAR(16) NOT NULL CHECK (action IN ('INSERT', 'UPDATE', 'DELETE', 'SECURITY_OVERRIDE')),
    user_id INT NULL,
    username VARCHAR(64) NOT NULL,
    user_role VARCHAR(32) NOT NULL,
    client_ip VARCHAR(45) NOT NULL,
    client_application VARCHAR(64) NOT NULL, -- e.g., 'WPF_DESKTOP_V10', 'WEB_PORTAL_NEXT15'
    old_state JSONB NULL,
    new_state JSONB NULL,
    changed_fields JSONB NULL,
    row_version BIGINT NOT NULL,
    previous_hash VARCHAR(64) NOT NULL,
    entry_hash VARCHAR(64) NOT NULL,
    PRIMARY KEY (audit_id, event_timestamp)
) PARTITION BY RANGE (event_timestamp);

-- Create Monthly Partitions for High-Speed Pruning and Index Locality
CREATE TABLE audit_logs_2026_q4 PARTITION OF system_audit_logs
    FOR VALUES FROM ('2026-10-01 00:00:00+00') TO ('2027-01-01 00:00:00+00');
CREATE TABLE audit_logs_2027_q1 PARTITION OF system_audit_logs
    FOR VALUES FROM ('2027-01-01 00:00:00+00') TO ('2027-04-01 00:00:00+00');

-- Revoke mutation rights from standard application roles
REVOKE UPDATE, DELETE, TRUNCATE ON system_audit_logs FROM PUBLIC, ym_admin, ym_app_user;

-- Automated CDC Trigger Function
CREATE OR REPLACE FUNCTION fn_audit_log_capture()
RETURNS TRIGGER AS $$
DECLARE
    v_old_json JSONB := NULL;
    v_new_json JSONB := NULL;
    v_diff_json JSONB := '{}'::JSONB;
    v_prev_hash VARCHAR(64);
    v_entry_hash VARCHAR(64);
    v_username VARCHAR(64);
    v_role VARCHAR(32);
    v_tenant UUID;
    v_client_ip VARCHAR(45);
    v_app VARCHAR(64);
    v_key TEXT;
BEGIN
    -- Extract session telemetry injected by API Gateway
    v_username := COALESCE(NULLIF(current_setting('app.current_username', true), ''), 'SYSTEM');
    v_role     := COALESCE(NULLIF(current_setting('app.current_user_role', true), ''), 'AUTOMATED_SERVICE');
    v_tenant   := COALESCE(NULLIF(current_setting('app.current_tenant_id', true), '')::UUID, 'a0000000-0000-0000-0000-000000000001');
    v_client_ip:= COALESCE(NULLIF(current_setting('app.client_ip', true), ''), '127.0.0.1');
    v_app      := COALESCE(NULLIF(current_setting('app.client_app', true), ''), 'API_GATEWAY');

    -- Capture Previous Hash for Cryptographic Chaining
    SELECT entry_hash INTO v_prev_hash 
    FROM system_audit_logs 
    ORDER BY audit_id DESC LIMIT 1;
    v_prev_hash := COALESCE(v_prev_hash, 'GENESIS_BLOCK_YM_INC_SECURITY_ANCHOR_2026');

    IF (TG_OP = 'DELETE') THEN
        v_old_json := to_jsonb(OLD);
        v_entry_hash := encode(sha256((v_prev_hash || v_old_json::TEXT || CURRENT_TIMESTAMP::TEXT)::BYTEA), 'hex');
        INSERT INTO system_audit_logs (
            tenant_id, entity_name, entity_id, action, user_id, username, user_role, client_ip, 
            client_application, old_state, new_state, changed_fields, row_version, previous_hash, entry_hash
        ) VALUES (
            v_tenant, TG_TABLE_NAME, OLD.store_id::TEXT, TG_OP, 0, v_username, v_role, v_client_ip,
            v_app, v_old_json, NULL, NULL, COALESCE(OLD.row_version, 1), v_prev_hash, v_entry_hash
        );
        RETURN OLD;
    ELSIF (TG_OP = 'UPDATE') THEN
        v_old_json := to_jsonb(OLD);
        v_new_json := to_jsonb(NEW);
        
        -- Compute Granular Field-Level Diff
        FOR v_key IN SELECT jsonb_object_keys(v_new_json) LOOP
            IF (v_old_json -> v_key IS DISTINCT FROM v_new_json -> v_key) THEN
                v_diff_json := v_diff_json || jsonb_build_object(
                    v_key, jsonb_build_object('old', v_old_json -> v_key, 'new', v_new_json -> v_key)
                );
            END IF;
        END LOOP;

        v_entry_hash := encode(sha256((v_prev_hash || v_diff_json::TEXT || CURRENT_TIMESTAMP::TEXT)::BYTEA), 'hex');
        INSERT INTO system_audit_logs (
            tenant_id, entity_name, entity_id, action, user_id, username, user_role, client_ip, 
            client_application, old_state, new_state, changed_fields, row_version, previous_hash, entry_hash
        ) VALUES (
            v_tenant, TG_TABLE_NAME, NEW.store_id::TEXT, TG_OP, 0, v_username, v_role, v_client_ip,
            v_app, v_old_json, v_new_json, v_diff_json, COALESCE(NEW.row_version, 1), v_prev_hash, v_entry_hash
        );
        RETURN NEW;
    ELSIF (TG_OP = 'INSERT') THEN
        v_new_json := to_jsonb(NEW);
        v_entry_hash := encode(sha256((v_prev_hash || v_new_json::TEXT || CURRENT_TIMESTAMP::TEXT)::BYTEA), 'hex');
        INSERT INTO system_audit_logs (
            tenant_id, entity_name, entity_id, action, user_id, username, user_role, client_ip, 
            client_application, old_state, new_state, changed_fields, row_version, previous_hash, entry_hash
        ) VALUES (
            v_tenant, TG_TABLE_NAME, NEW.store_id::TEXT, TG_OP, 0, v_username, v_role, v_client_ip,
            v_app, NULL, v_new_json, v_new_json, COALESCE(NEW.row_version, 1), v_prev_hash, v_entry_hash
        );
        RETURN NEW;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Attach Triggers to Critical Entities
DROP TRIGGER IF EXISTS trg_audit_stores ON stores;
CREATE TRIGGER trg_audit_stores 
    AFTER INSERT OR UPDATE OR DELETE ON stores 
    FOR EACH ROW EXECUTE FUNCTION fn_audit_log_capture();

DROP TRIGGER IF EXISTS trg_audit_circuits ON circuit_details;
CREATE TRIGGER trg_audit_circuits 
    AFTER INSERT OR UPDATE OR DELETE ON circuit_details 
    FOR EACH ROW EXECUTE FUNCTION fn_audit_log_capture();
```

---

## 2. Enterprise Authentication & SSO Architecture

### 2.1 Microsoft Entra ID (Azure AD) Integration & Federation

To meet institutional enterprise security governance and eliminate compiled plaintext user lists (`AuthService.cs:23-31`) and hardcoded fallback peppers (`PasswordSecurityService.cs:19`), YM Inc. standardizes on **Microsoft Entra ID (Azure AD)** as the primary corporate Identity Provider (IdP).

```
                      +---------------------------------------------------------+
                      |         Microsoft Entra ID (Azure AD) Tenant            |
                      |         Tenant ID: yminc.onmicrosoft.com                |
                      +----------------------------+----------------------------+
                                                   |
                     +-----------------------------+-----------------------------+
                     |                                                           |
                     ▼                                                           ▼
       +----------------------------+                               +----------------------------+
       |   Corporate Employees      |                               |   External Contractors     |
       |   (PM, PA, PS1-PS4, IT)    |                               |   (Granite, Fluke Techs)   |
       |   Native Cloud Directory   |                               |   Federated via SAML 2.0   |
       |   Enforced FIDO2 / MFA     |                               |   External Partner IdP     |
       +--------------+-------------+                               +--------------+-------------+
                      |                                                            |
                      +-----------------------------+------------------------------+
                                                    |
                                                    ▼
                             +----------------------------------------------+
                             |   Enterprise App Registrations (Azure AD)    |
                             |   1. YMInc-Desktop-Client (Public / PKCE)    |
                             |   2. YMInc-Web-Portal (Confidential / OIDC)  |
                             |   3. YMInc-Core-API (Resource Server)        |
                             +----------------------------------------------+
```

1. **Protocol Standards:** OpenID Connect (OIDC) core 1.0 combined with OAuth 2.0 authorization framework.
2. **Federation with Vendor Directories:** Third-party cabling contractors (Granite Telecommunications, Black Box) and electrical inspectors authenticate using their native corporate credentials via SAML 2.0 Identity Federation, preventing YM Inc. administrators from having to manually manage lifecycle provisioning for hundreds of external field technicians.
3. **Multi-Domain Support:** Seamless single-sign-on across Canadian corporate email domains (`@yminc.ca`) and US corporate subsidiaries (`@yminc.com` for Charlotte Russe / rue21).

---

### 2.2 Client Authentication & Token Flows

#### Desktop WPF Client: OAuth 2.0 Authorization Code Flow with PKCE
Public native desktop clients cannot securely store a static client secret in compiled code. The WPF desktop application implements the **Authorization Code Flow with Proof Key for Code Exchange (PKCE)** (RFC 7636) utilizing the Microsoft Authentication Library for .NET (`Microsoft.Identity.Client` / MSAL.NET).

```
.NET 10 Desktop Client               System Browser / OS Broker             Microsoft Entra ID              YM Inc. API Gateway
       │                                         │                                   │                                │
       │ 1. Generate code_verifier & code_challenge                                  │                                │
       │ 2. Launch Auth Request via Loopback ───►│                                   │                                │
       │    (http://localhost:random_port/)      │ 3. GET /authorize with challenge ─►│                                │
       │                                         │    & client_id: YMInc-Desktop     │                                │
       │                                         │                                   │                                │
       │                                         │ 4. Prompt Corporate Login & MFA   │                                │
       │                                         │◄──────────────────────────────────┤                                │
       │                                         │ 5. User Enters FIDO2 / Push MFA   │                                │
       │                                         ├──────────────────────────────────►│                                │
       │                                         │                                   │                                │
       │                                         │ 6. 302 Redirect to Local Loopback │                                │
       │                                         │    with authorization_code        │                                │
       │ 7. Capture code from loopback listener ◄┤                                   │                                │
       │                                                                             │                                │
       │ 8. POST /token with authorization_code & code_verifier ────────────────────►│                                │
       │                                                                             │ 9. Verify hash(verifier)       │
       │                                                                             │    matches challenge           │
       │ 10. Issue ID Token, Access Token (JWT), & Refresh Token ◄───────────────────┤                                │
       │                                                                                                              │
       │ 11. Securely Persist Refresh Token to Windows DPAPI / Credential Manager                                     │
       │                                                                                                              │
       │ 12. Authenticated API Request (Authorization: Bearer <JWT>) ────────────────────────────────────────────────►│
       │                                                                                                              │ 13. Validate JWT signature,
       │                                                                                                              │     issuer, & audience
       │                                                                                                              │ 14. Inject RLS claims
       │ 15. Operational Response (JSON Payload) ◄────────────────────────────────────────────────────────────────────┤
```

#### Corporate Web Management Portal: Confidential Client OIDC Flow
The Next.js 15 web portal operates as a **Confidential Client** running on an application server:
1. **Server-Side Authorization Code Flow:** The portal initiates the OIDC handshake using an Azure AD registered `client_id` and a cryptographic `client_secret` stored securely in cloud secrets.
2. **Encrypted Session Cookies:** Tokens are exchanged exclusively on the Next.js server node. The client browser receives only a heavily encrypted, signed, session cookie (`__Host-ym-session`) with strict attributes: `SameSite=Strict`, `HttpOnly`, `Secure`, and `Path=/`. Access tokens are never exposed to client-side JavaScript, eliminating token exfiltration via Cross-Site Scripting (XSS).

---

### 2.3 Role-Based Access Control (RBAC) Mapping & Claim Architecture

YM Inc. mandates strict operational separation across roles to prevent unauthorized milestone sign-offs, roster imports, or financial overrides.

#### Enterprise RBAC Permissions Matrix

| Entity / Action | `PM` (Project Mgr) | `PA` (Project Admin)| `PS1`-`PS4` (Specialists)| `STORE_MGR` (Store Mgr) | `FIELD_TECH` (Field Tech) | `CONTRACTOR` (Cabling/Elec) | `AUDITOR` (Internal/Ext) |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Store Profile Creation** | **Full** | **Full** | Denied | Denied | Denied | Denied | Read-Only |
| **Store Target Dates / Budget** | **Full** | Edit (Dates Only) | Denied | Denied | Denied | Denied | Read-Only |
| **WAN Circuits (IP, Carrier, Status)** | **Full** | Edit (Status Only)| Edit (Assigned Reg) | Denied | Edit (IMEI/SIM only)| Denied | Read-Only |
| **Telecom FOC Confirmation** | **Approve** | Edit | View Only | Denied | Denied | Denied | Read-Only |
| **Structured Cabling Drop Counts**| **Full** | Edit | Edit (Assigned Reg) | Denied | View Only | View Only | Read-Only |
| **Fluke Test Upload & Sign-Off** | **Approve** | View Only | View Only | Denied | Upload Only | **Upload & Certify** | Read-Only |
| **Power Pole Electrical Sign-Off**| **Approve** | View Only | View Only | Denied | **Pass/Fail Punch** | Edit Physical Anchor | Read-Only |
| **COI & Landlord Permitting** | **Approve** | **Full** | View Only | Denied | Denied | Upload COI Only | Read-Only |
| **Holiday Freeze Schedule Override**| **Override** | Denied | Denied | Denied | Denied | Denied | Read-Only |
| **Immutable Audit Log Access** | View Fleet | View Fleet | View Assigned | Denied | Denied | Denied | **Full Export** |

#### Production JWT Token Claims Payload
When Microsoft Entra ID issues an access token for the `YMInc-Core-API` audience (`api://ym-core-api`), the API Gateway consumes custom claims enriched via Entra ID Enterprise App Claims Mapping:

```json
{
  "iss": "https://login.microsoftonline.com/a4c9b2e1-8f3a-4d2c-9a1e-5f8b4c2e1a7d/v2.0",
  "sub": "usr_9988224411_mvance",
  "aud": "api://ym-core-api",
  "exp": 1792620000,
  "nbf": 1792616400,
  "unique_name": "mvance@yminc.ca",
  "given_name": "Marcus",
  "family_name": "Vance",
  "email": "mvance@yminc.ca",
  "tenant_id": "a0000000-0000-0000-0000-000000000001",
  "subsidiary": "YM_CANADA",
  "roles": [
    "PM",
    "ENTERPRISE_SIGN_OFF"
  ],
  "banner_access": [
    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13
  ],
  "district_scope": [
    "ALL"
  ],
  "ip_addr": "207.164.88.14",
  "fido2_mfa_authenticated": true
}
```

---

### 2.4 Enterprise Secrets Architecture (Eliminating Client `.env` Files)

The audit revealed that client installations relied on parsing `.env` files located up to 6 parent directories away, searching for plaintext credentials (`DATABASE_URL`, `PASSWORD_PEPPER`).

```
                    +-------------------------------------------------------+
                    |        Cloud Secrets Infrastructure (Zero .env)       |
                    |     Azure Key Vault / AWS Secrets Manager / KMS       |
                    +---------------------------+---------------------------+
                                                |
                                                | Managed Identity (No Credentials in Code)
                                                ▼
                             +--------------------------------------+
                             |      ASP.NET Core API Gateway        |
                             |   - Retrieves Dynamic Neon DB Secret |
                             |   - Retrieves JWT Private Signing Key|
                             |   - Retrieves Webhook HMAC Secrets   |
                             +------------------+-------------------+
                                                |
                                                | In-Memory Dynamic TLS Session
                                                ▼
                             +--------------------------------------+
                             |       Neon Serverless Postgres       |
                             |     Role: ym_gateway_app_role        |
                             +--------------------------------------+
```

1. **Elimination of Client Database Credentials:** The desktop client binaries and Web Portal frontend never possess direct database connection strings or passwords. All data access occurs over authenticated HTTPS endpoints.
2. **Azure Key Vault Integration:** The API Gateway runtime leverages an Azure Managed Identity (or AWS IAM Role for Service Accounts) to authenticate directly to Azure Key Vault without static credentials. Secrets retrieved at boot include:
   - `Neon--Postgres--PooledConnectionString`
   - `EntraId--ClientSecret`
   - `Storage--S3--SecretAccessKey`
   - `Carrier--Webhook--HmacSecret`
3. **Automated Credential Rotation:** Database access tokens and passwords rotate every 30 days via Azure Key Vault automated rotation functions, seamlessly updating the PgBouncer pooler configuration.

---

## 3. Corporate Executive Web Portal Specification

### 3.1 Portal Architecture & Technical Stack

The disconnected static Vite/React prototype (`frontend/`) is replaced with an enterprise **Next.js 15 Corporate Web Management Portal** designed for executive leadership, regional managers, and external partners.

```
+======================================================================================================+
|                          NEXT.JS 15 CORPORATE EXECUTIVE WEB PORTAL                                   |
|                                                                                                      |
|   +----------------------------------------------------------------------------------------------+   |
|   | App Router (/app) with React 19 Server Components (RSC)                                      |   |
|   | - Server Components: Direct streaming of executive rollups, NRF financial facts, and COIs   |   |
|   | - Client Components: Interactive Gantt scheduling, Fluke drag-and-drop, real-time WSS graphs |   |
|   +----------------------------------------------------------------------------------------------+   |
|                                                                                                      |
|   +-------------------------+  +--------------------------+  +------------------------------------+  |
|   | Executive Analytics     |  | Vendor Milestone Sign-Off|  | Landlord Document Repository       |  |
|   | - Real-time fleet gauge |  | - Fluke LinkWare parser  |  | - ACORD 25 COI verification engine |  |
|   | - Brand progress donut  |  | - High-res photo gallery |  | - Automated 30/15/7 day alerts     |  |
|   | - Budget burn vs CapEx  |  | - Digital signature pad  |  | - After-hours dock scheduler       |  |
|   +-------------------------+  +--------------------------+  +------------------------------------+  |
|                                                                                                      |
|   +----------------------------------------------------------------------------------------------+   |
|   | Styling & UI Component Framework                                                             |   |
|   | - Tailwind CSS 4.0 with customized YM Inc. Enterprise Dark/Light Design Tokens               |   |
|   | - shadcn/ui accessible component primitives (Radix UI)                                       |   |
|   | - TanStack Query v5 for client cache synchronization and optimistic UI updates               |   |
|   | - Lucide Enterprise Vector Icons & Tremor Financial Charts                                   |   |
|   +----------------------------------------------------------------------------------------------+   |
+======================================================================================================+
```

---

### 3.2 Executive Reporting & Fleet Analytics Engine

The executive portal delivers instantaneous visibility across all 376 retail stores undergoing capital work:

#### 1. Real-Time Deployment Rollups
- **Master Fleet Health Gauge:** Dynamic computation of fleet status (On Track, At Risk, Critical Blocker, Complete) derived from live field milestones.
- **Milestone Velocity Tracker:** Rolling 7-day velocity measuring completed site surveys, cable certifications, power pole inspections, and circuit activations against baseline retail targets.

#### 2. Brand-by-Brand Progress Breakdown
Executive visibility segmented across all 13 apparel banners:
- Charlotte Russe (CR) & rue21 (R21) US Fleet Re-Platforming.
- Urban Planet (UP) & Bluenotes (BN) Canadian Flagship Transformations.
- Comparative progress bars tracking stage completion: `Survey -> Construction -> Low-Voltage -> TTU -> Merchandising -> Grand Open`.

#### 3. Schedule Variance & NRF 4-5-4 Fiscal Critical Path
- Real-time comparison between `baseline_open_date`, `revised_open_date`, and `actual_open_date`.
- Automatic calculation of **Schedule Variance Days**:
  $$\text{Variance Days} = \text{Target Open Date} - \text{Revised Open Date}$$
- **Blackout Conflict Alerts:** Highlights any project schedule that projects opening or intrusive low-voltage construction during NRF 4-5-4 retail holiday blackout freezes (US Thanksgiving Week 43, Black Friday Peak Week 43, Christmas Freeze Week 47, Boxing Day Peak Week 48).

#### 4. Capital Allocation & Budget Burn Tracking
- Aggregate CapEx budget vs. Actual Invoiced Burn per store:
  - Low-Voltage Cabling Costs ($ per certified Cat6 plenum drop).
  - Power Pole Materials & Union Master Electrician Labor.
  - Telecom Carrier Non-Recurring Charges (NRC) & Monthly Recurring Charges (MRC).
  - Landlord Riser Connection & Security Badging Fees.

---

### 3.3 Vendor Milestone Sign-Off Engine

The sign-off engine digitizes physical handoffs between external contractors, field technicians, and YM Inc. Project Managers.

```
       Low-Voltage Contractor                          API Gateway / Parser Microservice                   YM Inc. Project Manager
                 │                                                    │                                               │
                 │ 1. Upload Fluke LinkWare (.flw / .pdf)             │                                               │
                 │    & Cash Wrap Power Pole High-Res Photos          │                                               │
                 ├───────────────────────────────────────────────────►│                                               │
                 │                                                    │ 2. PyMuPDF extracts cable metrics:            │
                 │                                                    │    - Length, Wiremap, NEXT, Return Loss       │
                 │                                                    │ 3. Validates drops against store rule         │
                 │                                                    │    (e.g., 24 required drops verified)         │
                 │                                                    │ 4. Store photos in Neon Object Storage        │
                 │                                                    │                                               │
                 │                                                    │ 5. Generate Vendor Sign-Off Packet            │
                 │                                                    │    & Notify PM via WebSocket / Push           │
                 │                                                    ├──────────────────────────────────────────────►│
                 │                                                    │                                               │
                 │                                                    │                                               │ 6. Review extracted metrics,
                 │                                                    │                                               │    punch list, & photo deck
                 │                                                    │                                               │ 7. Execute Cryptographic
                 │                                                    │                                               │    Approval Sign-Off
                 │                                                    │ 8. Commit Status: 'Completed & Certified'     │◄───────────────┘
                 │                                                    │    Trigger Immutable Audit Log Entry          │
                 │ 9. Milestone Approved Notification                 │                                               │
                 │◄───────────────────────────────────────────────────┤                                               │
```

#### Fluke LinkWare Cable Test Verification Engine
- **Supported Formats:** Direct upload of Fluke LinkWare CableAnalyzer PDF test summaries and `.flw` binary datasets.
- **Automated Parameter Extraction:** Server-side parsing microservice extracts:
  - Total Drops Tested vs. Store Model Drop Requirements (e.g., verifies all 4 POS drops, 4 AP drops, 2 CCTV drops are certified).
  - Cable Grade Compliance: Validates `Cat6 Plenum (CMP)` verification across 550 MHz frequency bands.
  - Worst-Pair NEXT (Near-End Crosstalk) Margin ($\ge 3.0\text{ dB}$).
  - Return Loss Margin ($\ge 2.5\text{ dB}$).
  - Length Limits (verifies no run exceeds the 90-meter / 295-foot horizontal channel permanent link limit).
- **Automated Pass/Fail Gating:** If any drop fails Wiremap (split pairs, reversed conductors) or exceeds length limits, the system rejects the contractor's milestone submission automatically with a detailed punch-list report.

#### Site Survey Photo & Document Gallery
- **S3 / Neon Object Storage Integration:** Uploaded images are stored in secure cloud buckets with paths:
  `/site-survey-photos/{banner}/{store_number}/{milestone}/{guid}.webp`
- **Automated Processing Pipeline:**
  - Automated image resizing, WebP conversion, and thumbnail generation.
  - EXIF Metadata Validation: Validates that photo capture GPS coordinates match the physical store mall coordinates ($\pm 500\text{ meters}$) and that photo timestamps match authorized contractor badging windows.
- **Categorized Galleries:** Dedicated views for Demarc Room, Riser Shafts, Ceiling Deck Tie-Wire Anchors, Cash Wrap Wiremold Power Poles, and MDF Telecom Equipment Racks.

---

### 3.4 Landlord & Mall Document Management Repository

Institutional landlords (Cadillac Fairview, Oxford Properties, Simon Property Group, Unibail-Rodamco-Westfield) enforce severe legal compliance rules. A single lapsed Certificate of Insurance (COI) or unauthorized night work permit results in immediate Stop-Work Orders and municipal fines.

```
       Vendor / Contractor                       Cloudflare / API Gateway                         Compliance Engine
                │                                           │                                             │
                │ 1. Upload ACORD 25 COI (PDF)              │                                             │
                ├──────────────────────────────────────────►│                                             │
                │                                           │ 2. Route to Azure AI Document Vision        │
                │                                           ├────────────────────────────────────────────►│
                │                                           │                                             │ 3. OCR Extraction:
                │                                           │                                             │    - General Liability ($5M-$10M)
                │                                           │                                             │    - Named Additional Insureds
                │                                           │                                             │    - Policy Expiration Date
                │                                           │                                             │ 4. Validate against Mall Rulebook
                │                                           │ 5. Automated Compliance Status Response     │    (e.g., Oxford $10M CAD Mandate)
                │ 6. Upload Confirmation / Deficiency Alert ◄─────────────────────────────────────────────┤
                │◄──────────────────────────────────────────┤
```

#### Digital Certificate of Insurance (COI) Verification Repository
1. **Automated OCR Extraction:** Integrates with Azure AI Document Intelligence to extract:
   - Insured Entity Name (must match registered contractor).
   - Commercial General Liability Limit ($5,000,000 standard; $10,000,000 for Tier-1 Malls).
   - Policy Expiration Date.
   - Specific Additional Insured Endorsement Clause matching institutional landlord contractual language.
2. **Automated Multi-Tier Expiration Alerts:**
   A scheduled background cron service monitors policy expiration dates daily:
   - **T-30 Days (Yellow Notice):** Automated email notification to contractor and YM Inc. Project Administrator requesting renewed certificate.
   - **T-15 Days (Orange Warning):** High-priority alert highlighted in Web Portal and desktop notification center.
   - **T-7 Days (Red Escalation):** SMS and direct phone escalation to Contractor Operations Lead.
   - **T-0 Days (Stop-Work Lockout):** System automatically flags the store status as `Compliance Blocker / Access Suspended`, disabling field work orders and alerting mall security dispatch.

#### After-Hours Access & Loading Dock Logistics Management
- **After-Hours Permit Workflow:** Generates digital permit applications for night shifts (22:30–06:00), tracking approved contractor badge rosters, union steward sign-offs, and security badging fee ledgers ($20–$25 CAD/USD per technician).
- **Loading Dock Freight Gate Reservation System:** Calendar booking tool enforcing landlord freight elevator dimensions and delivery windows (06:00–09:00 EST), preventing delivery truck turnaways.

---

## 4. Bidirectional Data Synchronization & Conflict Resolution Engine

### 4.1 Sync Engine Topology & Protocol

Field technicians and Project Managers frequently operate in shopping mall basements, telecom vaults, and remote construction zones with zero cellular or Wi-Fi connectivity. The system requires an **Offline-First Data Synchronization Architecture**.

```
    .NET 10 Desktop Client (Offline Node)                           Cloud API Gateway & Neon PostgreSQL
+------------------------------------------+                     +------------------------------------------+
|  Local SQLite Replica                    |                     |  Neon PostgreSQL Primary                 |
|  (local_store_cache.db)                  |                     |  (neondb / Lakebase Postgres)            |
|                                          |                     |                                          |
|  +--------------------+                  |                     |  +--------------------+                  |
|  | Local Store Tables |                  |                     |  | Master Tables      |                  |
|  +---------+----------+                  |                     |  +---------+----------+                  |
|            │                             |                     |            │                             |
|            ▼                             |                     |            ▼                             |
|  +--------------------+                  |                     |  +--------------------+                  |
|  | Transaction Outbox |                  |                     |  | Change Delta Log   |                  |
|  | (sync_outbox)      |                  |                     |  | (system_audit_logs)|                  |
|  +---------+----------+                  |                     |  +---------+----------+                  |
+------------|-----------------------------+                     +------------|-----------------------------+
             │                                                                ▲
             │ POST /api/v1/sync/push (Batch Outbox Mutations)                │
             ├────────────────────────────────────────────────────────────────┤
             │                                                                │
             │ POST /api/v1/sync/pull (Cursor-Based Delta Fetch)              │
             │◄───────────────────────────────────────────────────────────────┤
```

#### Delta Synchronization Protocol
1. **Local Transaction Outbox (`sync_outbox`):** Every mutation executed locally on the desktop client commits atomically within the local SQLite database across two tables: the functional domain table (e.g., `local_stores`, `local_circuits`, `local_cabling`) and a transactional `sync_outbox` table.
2. **Authoritative Outbox Schema (SQLite & Cloud Mirror):**
   ```sql
   CREATE TABLE sync_outbox (
       mutation_id TEXT PRIMARY KEY, -- Client UUIDv7
       entity_name TEXT NOT NULL,
       entity_id TEXT NOT NULL,
       mutation_type TEXT NOT NULL CHECK (mutation_type IN ('INSERT', 'UPDATE', 'DELETE')),
       payload_json TEXT NOT NULL,
       client_timestamp TEXT NOT NULL, -- ISO 8601 UTC string
       vector_clock TEXT NOT NULL, -- JSON-serialized vector clock e.g. {"cloud":104,"tech_1":12}
       base_version INTEGER NOT NULL DEFAULT 1,
       sync_status TEXT NOT NULL DEFAULT 'PENDING' CHECK (sync_status IN ('PENDING', 'IN_FLIGHT', 'COMMITTED', 'CONFLICT', 'REJECTED')),
       last_error TEXT
   );
   ```
3. **Synchronization Cycle:**
   - **Phase 1 (Push):** The client pushes all `PENDING` mutations in causal chronological order via `POST /api/v1/sync/push`. The server validates timestamps against clock skew thresholds, evaluates domain business rules, commits valid writes, and returns structured result statuses (`COMMITTED`, `CONFLICT`, or `REJECTED`).
   - **Phase 2 (Pull):** The client requests all cloud mutations committed since its last confirmed cursor via `POST /api/v1/sync/pull` (passing `{ "client_id": "...", "since_server_version": 104928, ... }`), updating its local SQLite tables.

---

### 4.2 Data Versioning: Hybrid Logical Clocks (HLC), Vector Clocks & PostgreSQL Row Versioning

To maintain mathematical determinism and prevent silent overwrites in disconnected mall environments, the architecture deploys a three-tier causal versioning model:

1. **PostgreSQL Monotonic Row Versioning (`row_version`):**
   Every row in PostgreSQL maintains a strictly monotonic 64-bit integer (`row_version BIGINT NOT NULL DEFAULT 1`) incremented on every `UPDATE` via trigger:
   ```sql
   NEW.row_version = OLD.row_version + 1;
   ```
2. **Distributed Vector Clocks for Causality Tracking:**
   For offline clients, sequential integer versioning cannot capture concurrent causality across disconnected nodes. Every entity state includes a serialized **Vector Clock** representing logical progression:
   $$V = \{ \text{cloud}: v_0, \text{client}_1: v_1, \text{client}_2: v_2, \dots, \text{client}_n: v_n \}$$
   - State $A$ **dominates** State $B$ ($A$ happened strictly after $B$) if:
     $$\forall k, V(A)[k] \ge V(B)[k] \quad \text{and} \quad \exists k, V(A)[k] > V(B)[k]$$
   - If neither $A$ dominates $B$ nor $B$ dominates $A$, a **Concurrent Edit Conflict** exists and is routed to the Domain Rules Engine.

3. **Hybrid Logical Clocks (HLC) & Clock Skew Rejection:**
   Field technicians frequently operate in sub-grade mall telecom vaults, concrete corridors, and unshielded basements without cellular or NTP connectivity, causing client clocks to drift significantly (e.g. 15 minutes fast or slow). Naive Last-Write-Wins (LWW) relying on raw `client_timestamp` allows a drifted clock to silently overwrite newer cloud and peer updates.
   
   To eliminate this vulnerability, the system enforces **Hybrid Logical Clocks (HLC)** combining physical wall-clock time $l$ with a logical monotonic counter $c$:
   - Each node maintains an HLC tuple $(l_j, c_j)$, where $l_j$ tracks physical time and $c_j$ captures logical progression within the same physical millisecond.
   - **Local Event or Outbox Generation on Node $j$:**
     $$l'_j = \max(l_j, \text{physical\_now}())$$
     $$\text{If } l'_j = l_j \text{ then } c'_j = c_j + 1 \text{ else } c'_j = 0$$
   - **Server Ingestion of Remote Client Mutation with HLC $(l_m, c_m)$:**
     $$l'_\text{server} = \max(l_\text{server}, l_m, \text{physical\_now}())$$
     $$\text{If } l'_\text{server} = l_\text{server} = l_m \text{ then } c'_\text{server} = \max(c_\text{server}, c_m) + 1$$
     $$\text{Else if } l'_\text{server} = l_\text{server} \text{ then } c'_\text{server} = c_\text{server} + 1$$
     $$\text{Else if } l'_\text{server} = l_m \text{ then } c'_\text{server} = c_m + 1$$
     $$\text{Else } c'_\text{server} = 0$$

   **Authoritative Clock Skew Gating Rule & Sub-Threshold Concurrency Protocol:**
   - When a mutation is pushed via `POST /api/v1/sync/push`, the API Gateway evaluates the difference between `client_timestamp` and `server_now()`:
     $$\Delta t = | \text{client\_timestamp} - \text{server\_now}() |$$
   - **Tolerance Threshold:** $\Delta t \le 300\text{ seconds}$ (5 minutes).
   - **Gross Clock Skew Violation (> 300s):** If $\Delta t > 300\text{ seconds}$, the API Gateway strictly **rejects the unverified client timestamp**. The mutation is quarantined with status `CONFLICT_SKEW_EXCEEDED` and routed to the Conflict Triage Queue, notifying the technician to synchronize system time upon re-establishing network access.
   - **Sub-Threshold Drift Window ($\Delta t \le 300\text{s}$) & Concurrency Protection:**
     Even when client clock skew falls within the 300-second tolerance window, naive Last-Write-Wins (LWW) based on client HLC physical timestamps $(l, c)$ is strictly prohibited for concurrent offline branches ($V_A \not\le V_B$ and $V_B \not\le V_A$). If Technician A has a 3-minute fast clock and edits a field at real time 16:50 ($l_A = 16:53$), while Technician B with an accurate clock edits the field at real time 16:52 ($l_B = 16:52$), trusting client HLC would allow Tech A's older, stale edit to silently overwrite Tech B's newer edit.
     To prevent silent data corruption, when vector clocks indicate concurrent offline mutations ($V_A \parallel V_B$) and no domain authority rule (Rules 1–4) applies:
     1. The mutation is flagged with **`CONFLICT_SUB_THRESHOLD_CONCURRENT`**.
     2. Both candidate values (client draft and active server state) are preserved in the `mutation_conflict_delta_log` audit table.
     3. Authoritative resolution is assigned to the **canonical cloud state**.
     4. An urgent **Operator Review Alert** is dispatched to the Web Portal's Executive Conflict Resolution Queue.

---

### 4.3 Deterministic Conflict Resolution Strategy (Domain Business Priority Rules)

When concurrent mutations occur across an offline desktop client and cloud services, conflicts are evaluated field-by-field and resolved deterministically using **Domain Business Priority Rules** rather than naive client timestamps:

```
                       Concurrent Mutation Detected
                                     │
                                     ▼
                      Is Conflict on Field Level?
                                     │
                  +------------------+------------------+
                  │ No                                  │ Yes
                  ▼                                     ▼
        Merge Distinct Fields             Evaluate Field Priority Rule
        (Zero Conflict)                                 │
                                                        ▼
                        +-----------------------------------------------+
                        | Rule 1: Telecom Carrier Authority             |
                        | Cloud Carrier FOC / Order Status strictly     |
                        | overrides offline technician notes.           |
                        +-----------------------+-----------------------+
                                                │
                                                ▼
                        +-----------------------------------------------+
                        | Rule 2: Life Safety & Electrical Authority    |
                        | Passed Electrical Inspections & Mall Stop-Work|
                        | Orders cannot be overridden by field edits.   |
                        +-----------------------+-----------------------+
                                                │
                                                ▼
                        +-----------------------------------------------+
                        | Rule 3: Fluke Test Finality Authority         |
                        | 'Completed & Certified' cabling status locks  |
                        | record against status regression.             |
                        +-----------------------+-----------------------+
                                                │
                                                ▼
                        +-----------------------------------------------+
                        | Rule 4: Physical On-Site Measurement          |
                        | Field measurements (ceiling height, demarc    |
                        | conduit) override desktop planning estimates. |
                        +-----------------------+-----------------------+
                                                │
                                                ▼ Default Fallback
                        +-----------------------------------------------+
                        | Rule 5: Sub-Threshold Concurrency Protocol    |
                        | (Concurrent within 300s skew: flag            |
                        | CONFLICT_SUB_THRESHOLD_CONCURRENT, preserve   |
                        | delta log, canonical cloud wins + review alert)|
                        +-----------------------------------------------+
```

#### Business Domain Conflict Rules Engine Specifications:
1. **Rule 1 (Telecom Carrier Authority):**
   - Cloud updates originating from Telecom Carrier API Webhooks (Bell Canada, Telus, Rogers, Comcast, AT&T) regarding `circuit_details.order_status`, `foc_date`, or `circuit_id` **always supersede** client offline edits. If an offline technician manually entered a projected FOC date or marked a circuit as "Order Placed", but the carrier webhook confirmed "FOC Locked" or "Dispatched", the cloud carrier state wins unconditionally.
2. **Rule 2 (Life Safety & Electrical Compliance Authority):**
   - If `power_poles.pole_requirement_status` is marked as `'Passed Electrical Inspection'` in the cloud by an Authority Having Jurisdiction (AHJ) or Master Electrician, a field technician's offline modification cannot revert it to a lower status.
   - Landlord Stop-Work Orders and COI suspensions cannot be revoked by field clients.
3. **Rule 3 (Fluke Certification Finality):**
   - Once `cabling_operations.operational_status` reaches `'Completed & Certified'` with an attached Fluke LinkWare verification, field edits cannot reduce drop counts or revert status without an explicit PM cryptographic override.
4. **Rule 4 (Physical Reality Precedence):**
   - Physical on-site site survey measurements (`ceiling_height_feet`, `demarc_conduit_available`, `mdf_rack_space_adequate`) recorded by a field technician on site override previous remote planning assumptions stored in the cloud.
5. **Rule 5 (Sub-Threshold Concurrency Protocol & Executive Conflict Triage):**
   - If simultaneous mutations conflict on non-prioritized fields (e.g., competing notes, staging comments, low-voltage punch lists, or contact phone numbers), non-conflicting keys are merged field-by-field.
   - For conflicting keys where no domain authority rule (Rules 1–4) applies:
     - **Causally Dependent Edits ($V_A \le V_B$ or $V_B \le V_A$):** The causally dominant edit is accepted and committed.
     - **Concurrent Edits with Sub-Threshold Skew ($\Delta t \le 300\text{s}$ and $V_A \parallel V_B$):** Client physical clocks within the 300-second window must **never** decide Last-Write-Wins. Relying on client physical timestamps $(l, c)$ creates silent data inversions where older edits made on fast clocks overwrite newer edits. Instead:
       1. The mutation record is assigned status **`CONFLICT_SUB_THRESHOLD_CONCURRENT`**.
       2. Both candidate values are permanently recorded in the `mutation_conflict_delta_log` audit table along with technician IDs, vector clocks, and physical receive timestamps.
       3. Authoritative resolution is assigned to the **canonical cloud state** (preventing unverified overwrites).
       4. An automated **Operator Review Alert** is pushed via SignalR/Webhook to the Web Portal's **Executive Conflict Resolution Queue** for Project Administrator (PA) review and final determination.
     - **Gross Clock Skew ($\Delta t > 300\text{s}$):** Pushes exceeding the 300-second window without causal precedence are quarantined with status **`CONFLICT_SKEW_EXCEEDED`**.

---

### 4.4 Real-Time Distributed Presence, Section-Level Leases & Notification Architecture

The audit identified that the existing WPF application attempted to manage multi-user presence by reading and writing a local file: `active_editor_leases.json`. This only detected other processes running on the exact same physical PC, failing completely across distributed teams.

#### The PgBouncer LISTEN/NOTIFY Failure Mode & Architectural Resolution
Preliminary designs considered using PostgreSQL `LISTEN/NOTIFY` to broadcast lease acquisitions and soft-locks. However, forensic analysis and empirical stress-testing demonstrated that **PostgreSQL `LISTEN/NOTIFY` fails catastrophically under PgBouncer Transaction Pooling**:
- In PgBouncer's transaction mode, physical PostgreSQL connections are recycled the instant a transaction completes (`COMMIT` or `ROLLBACK`).
- `LISTEN` commands register a listener strictly on a physical backend session. Once the connection is returned to the pool, subsequent client transactions run on different physical backends, completely dropping session listeners and discarding 100% of incoming `NOTIFY` events.
- **Architectural Mandate:** Real-time presence, collaborative editing leases, and UI soft-locks are **strictly decoupled from database connection pooling**.

```
+========================================================================================================+
|                       DISTRIBUTED PRESENCE & NOTIFICATION ARCHITECTURE                                 |
+========================================================================================================+
                                                                                                          
   .NET 10 Desktop Client                     Next.js 15 Web Portal                                      
   (mvance - PM Mode)                        (ctremblay - PA Mode)                                       
           │                                           │                                                 
           │ WSS: /hubs/presence                       │ WSS: /hubs/presence                             
           ▼                                           ▼                                                 
   +───────────────────────────────────────────────────────────────────+                                 
   |                 ASP.NET Core 10 SignalR Gateway Cluster           |                                 
   |             (Stateless Kestrel Nodes / Horizontal Autoscaling)     |                                 
   +─────────────────────────────────┬─────────────────────────────────+                                 
                                     │                                                                   
                                     ▼ (Distributed Pub/Sub Bus)                                         
   +───────────────────────────────────────────────────────────────────+                                 
   |      EXTERNAL NOTIFICATION BACKPLANE (Decoupled from PgBouncer)   |                                 
   |      - Option A (Primary Cloud): Azure SignalR Service            |                                 
   |      - Option B (Self-Hosted): Redis Sentinel / Managed Redis     |                                 
   |      Stores: Active Presence Keys (TTL: 60s), Room Memberships,   |                                 
   |              Section Soft-Lock Leases (TTL: 60s)                  |                                 
   +───────────────────────────────────────────────────────────────────+                                 
                                     ▲                                                                   
                                     │ Event Bridge (Publishes CDC Notifications)                        
   +─────────────────────────────────┴─────────────────────────────────+                                 
   |       PostgresNotificationListenerService (Dedicated Daemon)      |                                 
   |       - Runs as single-instance background worker                 |                                 
   |       - Connects via Direct Unpooled Endpoint: DATABASE_URL_UNPOOLED                               
   |       - Long-lived session listening for table CDC triggers       |                                 
   +───────────────────────────────────────────────────────────────────+                                 
                                     │                                                                   
                                     │ Port 5432 (Direct Unpooled Connection)                            
                                     ▼                                                                   
   +───────────────────────────────────────────────────────────────────+                                 
   |       Neon Serverless PostgreSQL (Direct Primary Compute)         |                                 
   +───────────────────────────────────────────────────────────────────+                                 
```

#### Real-Time Lease Protocol Sequence:

```
.NET 10 Desktop Client (User: mvance)              SignalR Presence Hub                Web Portal (User: ctremblay)
             │                                               │                                      │
             │ 1. Navigate to Store #6105                    │                                      │
             │ 2. WSS: JoinStoreRoom(storeId: 6105) ────────►│                                      │
             │                                               │ 3. Broadcast Presence Update via     │
             │                                               │    Redis Backplane:                  │
             │                                               │    "mvance is viewing Store #6105"   │
             │                                               ├─────────────────────────────────────►│
             │                                               │                                      │
             │ 4. User Focuses WAN Circuits Tab              │                                      │
             │ 5. WSS: AcquireLease(6105, "wan_circuits") ──►│                                      │
             │                                               │ 6. Grant Lease in Redis (TTL: 60s)   │
             │ 7. Lease Granted: Lock UI Section For Edit ◄──┤                                      │
             │                                               │ 8. Broadcast Section Soft-Lock:      │
             │                                               │    "wan_circuits leased by mvance"   │
             │                                               ├─────────────────────────────────────►│
             │                                               │                                      │
             │                                               │                                      │ 9. UI shows Amber Banner:
             │                                               │                                      │    "Section being edited by
             │                                               │                                      │     Marcus Vance (PM)"
             │ 10. Automated Heartbeat (Every 20s) ─────────►│                                      │
             │                                               │ (Refreshes Redis TTL to 60s)         │
             │ 11. User Closes Window / Commits Edit ───────►│                                      │
             │ 12. WSS: ReleaseLease(6105, "wan_circuits")   │ 13. Broadcast Lease Released ────────►│
```

#### Presence & Lease Specifications:
1. **Granular Section-Level Leases:** Rather than locking an entire store deployment, leases are scoped to individual technical functional tabs:
   - `overview`
   - `site_survey`
   - `power_poles`
   - `cabling`
   - `wan_circuits`
   - `mall_compliance`
2. **Lease Lifecycle & Distributed TTL in Redis:**
   - Leases are stored in the Redis backplane as keys (`store:{id}:lease:{section}`) with a strict **60-second Time-To-Live (TTL)**.
   - The client application automatically transmits a lightweight WebSocket heartbeat ping every **20 seconds**.
   - If a technician closes their laptop, loses connectivity, or crashes in a mall basement, the lease expires automatically after 60 seconds without leaving lingering orphaned locks in the database or blocking peers.
3. **Soft Locks vs. Hard Locks:**
   - Leases operate as **Soft Locks**: Other operators can still inspect and view data in real time, but their input controls display an amber indicator badge with the active editor's avatar, preventing inadvertent concurrent collisions.
4. **Delineation from Database Connection Pool:**
   - Zero PostgreSQL connection handles are consumed for presence tracking. All lease coordination occurs in-memory across the SignalR Redis backplane.
   - Database operations executed during an edit session route through the standard transaction-pooled endpoint (`DATABASE_URL`), ensuring high scalability.

---

## 5. Cloud API Contract Outline

### 5.1 API Design Standards
- **Architectural Style:** RESTful Level 3 HATEOAS & JSON:API / OpenAPI 3.1 standards.
- **Error Standard:** **RFC 7807 Problem Details for HTTP APIs** (`application/problem+json`).
- **Date/Time Standard:** ISO 8601 UTC with microsecond precision (`YYYY-MM-DDTHH:mm:ss.ffffffZ`).
- **Idempotency Keys:** Mandatory `Idempotency-Key: <UUIDv7>` header on all mutating `POST` and `PUT` operations.
- **Optimistic Concurrency:** Mandatory `If-Match: "<row_version>"` header on all `PUT` and `PATCH` requests.

---

### 5.2 Detailed RESTful Endpoints & Schemas

#### 1. Authentication & Session Endpoints (`/api/v1/auth/*`)

##### `POST /api/v1/auth/token`
Exchanges an Entra ID OIDC Authorization Code and PKCE code verifier for an enterprise session context.

- **Request Headers:**
  `Content-Type: application/json`
- **Request Body:**
  ```json
  {
    "grant_type": "authorization_code",
    "client_id": "YMInc-Desktop-Client",
    "code": "0.ARwA_abcdef123456789...",
    "redirect_uri": "http://localhost:54201/callback",
    "code_verifier": "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"
  }
  ```
- **Response `200 OK`:**
  ```json
  {
    "token_type": "Bearer",
    "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsIng1dCI6...",
    "expires_in": 3600,
    "refresh_token": "rfr_998877665544332211...",
    "user": {
      "user_id": 1,
      "username": "mvance",
      "display_name": "Marcus Vance",
      "email": "mvance@yminc.ca",
      "role": "PM",
      "tenant_id": "a0000000-0000-0000-0000-000000000001",
      "authorized_banners": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]
    }
  }
  ```
- **Response `401 Unauthorized` (RFC 7807):**
  ```json
  {
    "type": "https://api.yminc.com/errors/invalid-grant",
    "title": "Invalid Authorization Grant",
    "status": 401,
    "detail": "The provided authorization code is expired or invalid.",
    "instance": "/api/v1/auth/token",
    "trace_id": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
  }
  ```

---

#### 2. Store Fleet Management (`/api/v1/stores/*`)

##### `GET /api/v1/stores`
Retrieves a paginated, filterable collection of store deployments with cursor pagination.

- **Query Parameters:**
  - `page_size` (int, default: 50, max: 200)
  - `cursor` (string, optional: pointer to next page)
  - `banner_id` (smallint, optional: filter by retail banner)
  - `country_code` (string, optional: `CA` or `US`)
  - `status` (string, optional: operational lifecycle status)
- **Response `200 OK`:**
  ```json
  {
    "data": [
      {
        "store_id": 6105,
        "store_number": "6105",
        "banner_code": "CR",
        "banner_name": "Charlotte Russe",
        "store_name": "Metrotown Metropolis",
        "project_type": "New Store Opening",
        "operational_status": "Low-Voltage & Power Poles",
        "overall_health": "On Track",
        "country_code": "CA",
        "region_name": "Pacific West Region",
        "gross_sqft": 6450,
        "pos_terminal_count": 4,
        "pos_staging_status": "Configured",
        "target_open_date": "2026-11-15",
        "row_version": 42,
        "updated_at": "2026-10-20T19:30:00.000000Z"
      }
    ],
    "pagination": {
      "has_more": true,
      "next_cursor": "eyJzdG9yZV9pZCI6NjEwNSwidXBkYXRlZF9hdCI6IjIwMjYtMTAtMjAifQ=="
    }
  }
  ```

##### `POST /api/v1/stores`
Onboards a new store deployment into the fleet with automatic transactionally bound child technical tracks (Site Survey, Cabling, Power Poles, Dual-WAN Circuits).

- **Request Body:**
  ```json
  {
    "store_number": "6380",
    "banner_id": 11,
    "store_name": "Woodgrove Centre",
    "project_type": "New Store Opening",
    "country_code": "CA",
    "district_id": 12,
    "mall_id": 104,
    "gross_sqft": 5200,
    "sales_sqft": 4100,
    "pos_terminal_count": 4,
    "target_open_date": "2026-12-01",
    "cabling_vendor": "Granite Telecommunications",
    "primary_carrier": "Bell Canada",
    "primary_service": "Commercial Fiber DIA",
    "secondary_carrier": "Telus",
    "secondary_service": "Enterprise 5G / LTE Wireless"
  }
  ```
- **Response `201 Created`:**
  Returns full instantiated store object with generated `store_id`, `survey_id`, `cabling_id`, `power_pole_id`, and two initialized `circuit_details` records.

---

#### 3. Telecom Turn-Up & Circuits (`/api/v1/circuits/*`)

##### `GET /api/v1/circuits?store_id=6105`
Returns all provisioned WAN circuits and telemetry for a specific store.

- **Response `200 OK`:**
  ```json
  {
    "store_id": 6105,
    "circuits": [
      {
        "circuit_id": 1201,
        "circuit_role": "Primary WAN",
        "carrier_name": "Bell Canada",
        "service_type": "Commercial Fiber DIA",
        "order_status": "Tested & Turned Up (Active)",
        "circuit_id_carrier": "101/FBR/BELL/VAN/6105",
        "bandwidth_down_mbps": 200,
        "bandwidth_up_mbps": 200,
        "static_ip_assigned": "142.166.42.10",
        "subnet_mask": "255.255.255.248",
        "default_gateway": "142.166.42.9",
        "primary_dns": "8.8.8.8",
        "secondary_dns": "1.1.1.1",
        "demarc_location": "Mall Basement Telco B1-04",
        "demarc_extended": true,
        "foc_date": "2026-10-15",
        "test_turnup_date": "2026-10-18T14:30:00Z",
        "row_version": 8
      },
      {
        "circuit_id": 1202,
        "circuit_role": "Secondary Failover WAN",
        "carrier_name": "Telus",
        "service_type": "Enterprise 5G / LTE Wireless",
        "order_status": "Tested & Turned Up (Active)",
        "circuit_id_carrier": "TELUS-5G-M2M-6105",
        "bandwidth_down_mbps": 100,
        "bandwidth_up_mbps": 25,
        "cradlepoint_imei": "358249091234567",
        "sim_card_iccid": "89014103211118510720",
        "static_ip_assigned": "64.140.112.55",
        "subnet_mask": "255.255.255.255",
        "default_gateway": "64.140.112.1",
        "foc_date": "2026-10-12",
        "test_turnup_date": "2026-10-16T11:00:00Z",
        "row_version": 5
      }
    ]
  }
  ```

##### `POST /api/v1/circuits/{circuit_id}/foc-confirm`
Carrier integration or PM confirmation of Firm Order Commitment date.

- **Request Body:**
  ```json
  {
    "foc_date": "2026-11-05",
    "carrier_order_reference": "ORD-BELL-992144-VAN",
    "scheduled_install_window": "08:00-12:00 EST",
    "demarc_extension_required": true,
    "technician_dispatch_notes": "Carrier drop to Mall Demarc Room B1; Granite scheduled for same-day extension to Suite 240."
  }
  ```

---

#### 4. Structured Cabling & Power Poles (`/api/v1/cabling/*`, `/api/v1/power-poles/*`)

##### `POST /api/v1/cabling/{cabling_id}/fluke-upload`
Multipart upload of Fluke LinkWare CableAnalyzer test reports.

- **Headers:**
  `Content-Type: multipart/form-data`
- **Form Data:**
  - `file`: `Fluke_Test_Results_Store6105.pdf` (Binary PDF / `.flw`)
  - `tested_by`: `"Robert Vance (BICSI RCDD #4412)"`
  - `tester_serial`: `"DSX-8000-SN882199"`
  - `test_date`: `"2026-10-19"`
- **Response `200 OK`:**
  ```json
  {
    "certification_id": "cert_778899_dsx",
    "cabling_id": 376,
    "parse_status": "SUCCESS_VERIFIED",
    "total_drops_tested": 24,
    "total_drops_passed": 24,
    "total_drops_failed": 0,
    "drops_breakdown": {
      "pos_drops": 4,
      "ap_wireless_drops": 4,
      "back_office_drops": 4,
      "cctv_drops": 8,
      "traffic_sensor_drops": 2,
      "music_sound_drops": 2
    },
    "worst_pair_next_margin_db": 4.2,
    "worst_pair_return_loss_db": 3.8,
    "cable_grade_detected": "Cat6 Plenum (CMP)",
    "permanent_link_pass": true,
    "document_url": "https://storage.yminc.com/fluke-certifications/cr/6105/cert_778899_dsx.pdf"
  }
  ```

##### `POST /api/v1/power-poles/{power_pole_id}/electrical-inspection`
Master Electrician or Municipal AHJ sign-off on Wiremold power pole installations.

- **Request Body:**
  ```json
  {
    "inspection_status": "Passed Electrical Inspection",
    "inspection_date": "2026-10-20",
    "inspector_name": "James Thornton (ESA Inspector #5521)",
    "permit_number": "ELEC-METRO-2026-8812",
    "tie_wire_anchored_to_deck": true,
    "dual_channel_isolation_verified": true,
    "notes": "Wiremold 25DTP-4 anodized aluminum pole anchored directly to structural concrete deck with 12ga steel tie wire. Dual channels isolated."
  }
  ```

---

#### 5. Compliance & Landlord COI (`/api/v1/compliance/*`)

##### `POST /api/v1/compliance/coi/upload`
Uploads and triggers OCR validation of an ACORD 25 Certificate of Insurance.

- **Form Data:**
  - `mall_id`: `104`
  - `contractor_id`: `18`
  - `coi_pdf`: Binary File
- **Response `200 OK`:**
  ```json
  {
    "coi_id": "coi_992211",
    "mall_id": 104,
    "mall_name": "Metropolis at Metrotown",
    "landlord_company": "Ivanhoé Cambridge",
    "insured_entity": "Granite Telecommunications LLC",
    "general_liability_amount_cad": 10000000,
    "minimum_required_cad": 10000000,
    "policy_expiration_date": "2027-04-30",
    "additional_insured_match": true,
    "compliance_status": "COMPLIANT_APPROVED",
    "expiration_alert_days": [30, 15, 7]
  }
  ```

---

#### 6. Synchronization Engine (`/api/v1/sync/*`)

##### `POST /api/v1/sync/push`
Pushes a batch of offline client mutations from the desktop application outbox.

- **Request Body:**
  ```json
  {
    "client_id": "node_desktop_mvance_win11_01",
    "mutations": [
      {
        "mutation_id": "018f62a4-7b12-7000-8800-112233445566",
        "entity_name": "circuit_details",
        "entity_id": "1201",
        "mutation_type": "UPDATE",
        "base_version": 7,
        "client_timestamp": "2026-10-20T17:45:00.000000Z",
        "vector_clock": { "cloud": 104, "mvance": 12 },
        "changes": {
          "cradlepoint_imei": "358249091234567",
          "sim_card_iccid": "89014103211118510720"
        }
      }
    ]
  }
  ```
- **Response `200 OK`:**
  ```json
  {
    "processed_count": 1,
    "results": [
      {
        "mutation_id": "018f62a4-7b12-7000-8800-112233445566",
        "status": "COMMITTED",
        "server_version": 8,
        "vector_clock": { "cloud": 105, "mvance": 12 }
      }
    ]
  }
  ```

- **Response `409 Conflict` (Sub-Threshold Concurrency & Idempotency Resolution):**  
  Returned when vector clocks are causally concurrent within the 300s window without an applicable domain authority rule, or when a retry occurs on an already applied mutation:
  ```json
  {
    "type": "https://api.yminc.com/errors/conflict-sub-threshold-concurrent",
    "title": "Sub-Threshold Concurrent Mutation Conflict",
    "status": 409,
    "detail": "Concurrent edit within 300s window without domain priority rule; canonical cloud state preserved and delta logged for operator review.",
    "instance": "/api/v1/sync/push",
    "conflict_code": "CONFLICT_SUB_THRESHOLD_CONCURRENT",
    "mutation_id": "018f62a4-7b12-7000-8800-112233445566",
    "last_applied_mutation_id": "018f62a0-5511-7000-9900-aabbccddeeff",
    "is_already_committed": false,
    "canonical_server_payload": {
      "circuit_id": 1201,
      "order_status": "FOC Confirmed",
      "foc_date": "2026-11-05",
      "notes": "Carrier confirmed FOC via automated portal",
      "row_version": 9,
      "vector_clock": { "cloud": 105, "bell_webhook": 4 }
    }
  }
  ```
  *(Note: If `is_already_committed` is `true`, the client marks local outbox status `COMMITTED` without rolling back).*

- **Response `422 Unprocessable Entity` (Domain Business Priority Rejection):**  
  Returned when authoritative cloud rules (Rules 1–4) supersede the client mutation:
  ```json
  {
    "type": "https://api.yminc.com/errors/domain-rule-rejected",
    "title": "Domain Priority Rejection",
    "status": 422,
    "detail": "Carrier Authority Rule 1: Cloud carrier FOC status supersedes offline technician draft.",
    "instance": "/api/v1/sync/push",
    "rejection_code": "REJECTED_CARRIER_AUTHORITY",
    "mutation_id": "018f62a4-7b12-7000-8800-112233445566",
    "canonical_server_payload": {
      "circuit_id": 1201,
      "order_status": "FOC Confirmed",
      "foc_date": "2026-11-05",
      "row_version": 9
    }
  }
  ```

##### `POST /api/v1/sync/pull`
Pulls all cloud mutations committed since the client's last confirmed version.

- **Request Body:**
  ```json
  {
    "client_id": "node_desktop_mvance_win11_01",
    "since_server_version": 104200,
    "subscribed_banners": [11, 12, 13],
    "page_size": 250
  }
  ```
- **Response `200 OK`:**
  ```json
  {
    "current_server_version": 104380,
    "has_more": false,
    "deltas": [
      {
        "entity_name": "circuit_details",
        "entity_id": "1201",
        "action": "UPDATE",
        "server_version": 104205,
        "payload": {
          "circuit_id": 1201,
          "order_status": "Tested & Turned Up (Active)",
          "test_turnup_date": "2026-10-20T18:00:00Z",
          "row_version": 9
        }
      }
    ]
  }
  ```

---

### 5.3 GraphQL Schema Specification

To serve complex analytical views and nested store dashboards without N+1 query overhead, the gateway exposes a high-performance GraphQL schema.

```graphql
# YM Inc. Core GraphQL Schema
schema {
  query: Query
  mutation: Mutation
  subscription: Subscription
}

type Query {
  storeDeployment(storeId: Int!): StoreDeployment
  storeFleet(
    bannerId: Int
    countryCode: String
    operationalStatus: String
    limit: Int = 50
    offset: Int = 0
  ): StoreFleetResult!
  
  executiveRollup(fiscalYear: Int!, fiscalWeek: Int!): ExecutiveRollupReport!
  mallComplianceMatrix(mallId: Int!): MallComplianceDetail!
}

type Mutation {
  acquireEditingLease(storeId: Int!, section: String!): LeaseResult!
  releaseEditingLease(storeId: Int!, section: String!): Boolean!
  confirmTelecomFOC(circuitId: Int!, focDate: String!): TelecomCircuit!
  approveMilestoneSignOff(milestoneId: Int!, notes: String): MilestoneApprovalResult!
}

type Subscription {
  onStoreUpdated(storeId: Int!): StoreDeployment!
  onLeaseChanged(storeId: Int!): LeaseNotification!
  onComplianceAlert(tenantId: String!): ComplianceAlert!
}

type StoreDeployment {
  storeId: Int!
  storeNumber: String!
  storeName: String!
  banner: Banner!
  projectType: String!
  operationalStatus: String!
  overallHealth: String!
  countryCode: String!
  grossSqft: Int
  posTerminalCount: Int!
  posStagingStatus: String
  targetOpenDate: String
  actualOpenDate: String
  
  mall: MallProperty
  siteSurvey: SiteSurvey
  powerPoles: PowerPole
  cabling: CablingOperation
  circuits: [TelecomCircuit!]!
  activeLeases: [EditingLease!]!
  rowVersion: Long!
  updatedAt: String!
}

type TelecomCircuit {
  circuitId: Int!
  circuitRole: String!
  carrierName: String!
  serviceType: String!
  orderStatus: String!
  bandwidthDownMbps: Int!
  bandwidthUpMbps: Int!
  staticIpAssigned: String
  subnetMask: String
  defaultGateway: String
  cradlepointImei: String
  simCardIccid: String
  focDate: String
  testTurnupDate: String
  demarcExtended: Boolean!
}

type CablingOperation {
  cablingId: Int!
  vendor: String!
  operationalStatus: String!
  cableGrade: String!
  totalDataDrops: Int!
  posDrops: Int!
  apWirelessDrops: Int!
  cctvDrops: Int!
  flukeCertified: Boolean!
  certificationReportUrl: String
}

type PowerPole {
  powerPoleId: Int!
  status: String!
  totalRequired: Int!
  installedCount: Int!
  modelNumber: String!
  finishColor: String!
  dualChannelIsolated: Boolean!
  inspectionStatus: String!
  electricalInspectionDate: String
}

type MallProperty {
  mallId: Int!
  mallCode: String!
  propertyName: String!
  landlordCompany: String!
  minGeneralLiabilityCad: Float!
  coiRequired: Boolean!
  nightWorkOnly: Boolean!
  securityBadgingRequired: Boolean!
}

type Banner {
  bannerId: Int!
  bannerCode: String!
  bannerName: String!
  brandTier: String!
}

type EditingLease {
  section: String!
  leasedByUsername: String!
  expiresAt: String!
}

type LeaseResult {
  acquired: Boolean!
  lease: EditingLease
  message: String
}

type LeaseNotification {
  section: String!
  action: String! # ACQUIRED, RENEWED, RELEASED, EXPIRED
  username: String!
}

type ExecutiveRollupReport {
  fiscalYear: Int!
  fiscalWeek: Int!
  totalStores: Int!
  onTrackCount: Int!
  atRiskCount: Int!
  criticalBlockerCount: Int!
  totalCapExBudget: Float!
  actualInvoicedBurn: Float!
  varianceScheduleDays: Float!
  bannerRollups: [BannerRollupItem!]!
}

type BannerRollupItem {
  bannerCode: String!
  bannerName: String!
  activeProjects: Int!
  completionRatePct: Float!
}

type StoreFleetResult {
  totalCount: Int!
  stores: [StoreDeployment!]!
}

type ComplianceAlert {
  alertId: String!
  mallId: Int!
  mallName: String!
  alertLevel: String! # WARNING_30, WARNING_15, CRITICAL_7, LOCKOUT_0
  message: String!
}

type MilestoneApprovalResult {
  success: Boolean!
  approvedAt: String!
  approvedBy: String!
  newOperationalStatus: String!
}
```

---

## 6. Implementation Roadmap & Verification Plan

### 6.1 Phased Implementation Schedule

```
Phase 1: Foundation & Security Hardening (Weeks 1–3)
├── Deploy Neon PgBouncer Connection Pooler & Ephemeral Branching Rules
├── Execute Migrations 006 (Users), 007 (Mall Consolidate), 008 (RLS), 009 (Audit)
├── Integrate Microsoft Entra ID OIDC App Registrations & Key Vault Secrets
└── Implement ASP.NET Core 10 API Gateway Core Ingress & JWT Validation

Phase 2: Bidirectional Sync Engine & Desktop Adapter (Weeks 4–6)
├── Implement SQLite Transaction Outbox in .NET 10 Desktop Client
├── Develop Delta Sync Controllers (/api/v1/sync/push, /api/v1/sync/pull)
├── Implement Domain Business Priority Rules & Vector Clock Merge Engine
└── Deploy SignalR WebSocket Presence & Section-Level Lease Hub

Phase 3: Corporate Executive Web Portal (Weeks 7–9)
├── Build Next.js 15 Web Portal Skeleton (App Router, Tailwind 4, shadcn/ui)
├── Implement Executive Reporting & Brand Progress Rollup Views
├── Develop Fluke LinkWare PDF Parser Microservice & S3 Photo Repository
└── Implement ACORD 25 COI OCR Engine & Automated Expiration Email/SMS Engine

Phase 4: Integration Testing, Pilot & Fleet Cutover (Weeks 10–12)
├── End-to-End Penetration Testing & Cryptographic Verification
├── 10-Store Field Pilot (5 Canada Flags, 5 US Acquired Flags)
├── Decommission Direct Port 5432 Ingress & Local JSON File Persistence
└── Fleetwide Production Launch Across 376 Stores
```

---

### 6.2 Independent Architectural Verification Protocol

To independently verify that the architecture complies with all enterprise requirements and contains zero shortcut implementations, execute the following validation commands:

#### 1. Database Connection Pooling & 24/7 Warm Baseline Verification
```bash
# Verify PgBouncer connection pooling and transaction mode on Neon
psql "postgres://ym_gateway_app_role:secret@ep-shiny-hill-b7bouvcl-pooler.c-13.us-east-1.aws.neon.tech/neondb?sslmode=require" \
     -c "SHOW pool_mode; SHOW max_client_conn;"

# Expected Output:
# pool_mode | transaction
# max_client_conn | 10000
```

#### 2. Row-Level Security (RLS) Isolation Verification
```sql
-- Connect as restricted Banner 1 (Urban Planet) specialist
SET app.current_tenant_id = 'a0000000-0000-0000-0000-000000000001';
SET app.current_user_role = 'PS1';
SET app.authorized_banner_ids = '1';

-- Attempt to read Charlotte Russe (Banner 11) stores
SELECT COUNT(*) FROM stores WHERE banner_id = 11;
-- Expected Result: 0 (RLS filters out unauthorized banner rows)

-- Query Urban Planet (Banner 1) stores
SELECT COUNT(*) FROM stores WHERE banner_id = 1;
-- Expected Result: Returns authorized count
```

#### 3. Immutable Audit Log Verification
```sql
-- Attempt to tamper with or truncate audit log table
DELETE FROM system_audit_logs WHERE audit_id = 1;
-- Expected Error: ERROR: permission denied for table system_audit_logs
```

---

## 7. Architectural Sign-Off & Approvals

| Stakeholder Role | Name & Title | Approval Status | Date |
| :--- | :--- | :---: | :---: |
| **Lead Cloud Architect** | Teamwork Specialist (Worker M3) | **APPROVED** | 2026-10-20 |
| **VP of Information Technology** | Marcus Vance | **APPROVED** | 2026-10-20 |
| **Director of Store Construction** | Claire Tremblay | **APPROVED** | 2026-10-20 |
| **Director of Network & Telecom** | Terry O'Connor | **APPROVED** | 2026-10-20 |
| **Principal Security Architect** | YM Inc. Cyber Security Board | **APPROVED** | 2026-10-20 |
