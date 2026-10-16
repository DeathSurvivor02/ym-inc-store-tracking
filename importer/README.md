# YM Inc. Store Roster Importer

Automated data pipeline utility designed to ingest official YM Inc. store rosters from Excel (`.xlsx`) or CSV spreadsheets directly into the PostgreSQL database.

---

## 🏬 YM Inc. Store ID Numbering Convention

This importer adheres to YM Inc.'s official ERP store numbering architecture:
- **US Located Stores (Charlotte Russe, rue21, Mandee):** All store numbers in the **`6000s`** series (`6001`, `6002`, `6010`, `6025`, etc.).
- **Canadian Stores (Urban Planet, Bluenotes, Suzy Shier, Le Château, etc.):** 4-digit codes starting with **`6`** (`6101`, `6142`, `6302`, `6405`, etc.).

---

## 📥 How to Import an Excel Sheet

### 1. Place Your Excel File
When you download the store roster spreadsheet from YM Inc., save it inside this directory or anywhere on your machine (e.g. `importer/ym_store_roster_2026.xlsx`).

### 2. Run the Importer
```bash
node importer/import_store_roster.js importer/ym_store_roster_2026.xlsx
```
*(Or for CSV files: `node importer/import_store_roster.js importer/sample_store_roster.csv`)*

---

## 📋 Recognized Spreadsheet Columns

The importer features fuzzy header matching and accepts common column name variants:

| Field | Recognized Header Names in Excel / CSV | Description |
| :--- | :--- | :--- |
| **Store Number** | `store_number`, `store_no`, `store_id`, `store #`, `unit` | Official YM Inc. Store ID (`6000s` for US, `6xxx` for CA) |
| **Banner** | `banner_code`, `banner`, `brand`, `chain`, `concept` | `CR`, `R21`, `M`, `UP`, `BN`, `SS`, `LC`, `F21`, etc. |
| **Store Name** | `store_name`, `name`, `location_name`, `store_location` | e.g. "Charlotte Russe Garden State Plaza" |
| **Mall Property** | `mall_code`, `mall`, `property`, `shopping_centre` | e.g. "MALL-GSP" |
| **Address** | `address_line1`, `address`, `street_address` | Physical street address |
| **City** | `city`, `municipality` | e.g. "Paramus", "Toronto" |
| **State / Province**| `state_province`, `state`, `prov`, `province`, `st` | e.g. `NJ`, `NY`, `ON`, `TX`, `CA` |
| **Postal / Zip** | `postal_code`, `zip`, `zip_code`, `postal` | e.g. `07652`, `M6A 2T9` |
| **Square Footage**| `gross_sqft`, `gross_sq_ft`, `total_sqft`, `sqft` | Leased gross floor area |
| **Selling Sq Ft** | `sales_sqft`, `selling_sqft`, `sales_sq_ft` | Retail sales floor area |
| **Lifecycle Status**| `operational_status`, `status`, `lifecycle_status` | `Grand Open`, `Site Survey`, `Low-Voltage`, etc. |
| **Target Open Date**| `target_open_date`, `opening_date`, `open_date` | `YYYY-MM-DD` |
