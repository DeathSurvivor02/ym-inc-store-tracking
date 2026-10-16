/**
 * YM Inc. Store Tracking Project: Excel & CSV Store Roster Importer
 * File: importer/import_store_roster.js
 * 
 * Usage:
 *   node importer/import_store_roster.js <path-to-excel-or-csv-file>
 * 
 * Features:
 *   - Parses both .xlsx and .csv store rosters downloaded from YM Inc. ERP.
 *   - Automatically detects 6000s series store IDs as US locations.
 *   - Fuzzy header resolution for common spreadsheet columns:
 *     (Store #, Store ID, Banner, Location Name, Mall, Address, City, State/Prov, Zip, SqFt).
 *   - Idempotently upserts records into PostgreSQL `stores` and `store_attributes`.
 */

const fs = require('fs');
const path = require('path');
const { Client } = require('pg');

// Optional XLSX library check
let xlsx = null;
try {
  xlsx = require('xlsx');
} catch (e) {
  // If xlsx isn't installed yet, fall back to native CSV parser
}

function parseCSV(content) {
  const lines = content.split(/\r?\n/).filter(line => line.trim().length > 0);
  if (lines.length < 2) return [];
  const headers = lines[0].split(',').map(h => h.trim().replace(/^["']|["']$/g, ''));
  const rows = [];
  for (let i = 1; i < lines.length; i++) {
    const values = lines[i].split(',').map(v => v.trim().replace(/^["']|["']$/g, ''));
    const row = {};
    headers.forEach((h, idx) => {
      row[h] = values[idx] || '';
    });
    rows.push(row);
  }
  return rows;
}

function normalizeKey(str) {
  return str.toLowerCase().replace(/[^a-z0-9]/g, '');
}

function findValue(row, possibleKeys) {
  const normalizedRow = {};
  for (const k in row) {
    normalizedRow[normalizeKey(k)] = row[k];
  }
  for (const candidate of possibleKeys) {
    const val = normalizedRow[normalizeKey(candidate)];
    if (val !== undefined && val !== null && val !== '') {
      return String(val).trim();
    }
  }
  return null;
}

async function main() {
  const filePath = process.argv[2] || path.join(__dirname, 'sample_store_roster.csv');

  if (!fs.existsSync(filePath)) {
    console.error(`[ERROR] File not found: ${filePath}`);
    process.exit(1);
  }

  console.log(`==================================================================`);
  console.log(`  YM Inc. Store Tracking - Automated Roster Ingestion Tool       `);
  console.log(`==================================================================`);
  console.log(`Reading file: ${filePath}`);

  let rows = [];
  const ext = path.extname(filePath).toLowerCase();

  if (ext === '.xlsx' || ext === '.xls') {
    if (!xlsx) {
      console.error(`[ERROR] To import Excel (.xlsx) files, run: npm install xlsx pg`);
      process.exit(1);
    }
    const workbook = xlsx.readFile(filePath);
    const sheetName = workbook.SheetNames[0];
    rows = xlsx.utils.sheet_to_json(workbook.Sheets[sheetName]);
  } else {
    const fileContent = fs.readFileSync(filePath, 'utf8');
    rows = parseCSV(fileContent);
  }

  console.log(`Discovered ${rows.length} store records in spreadsheet.`);

  // Connect to PostgreSQL
  const client = new Client({
    connectionString: process.env.DATABASE_URL || 'postgresql://ym_admin:ym_secure_password_2026@localhost:5432/ym_store_tracking'
  });

  try {
    await client.connect();
    console.log(`Connected to PostgreSQL database: ym_store_tracking`);

    // Cache existing banners
    const bannersRes = await client.query('SELECT banner_id, banner_code, banner_name FROM banners');
    const bannerMap = new Map();
    bannersRes.rows.forEach(b => {
      bannerMap.set(b.banner_code.toUpperCase(), b.banner_id);
      bannerMap.set(b.banner_name.toUpperCase(), b.banner_id);
    });

    let importedCount = 0;

    for (const row of rows) {
      const storeNum = findValue(row, ['store_number', 'store_no', 'store_id', 'store', 'store#', 'unit']);
      if (!storeNum) continue;

      const rawBanner = findValue(row, ['banner_code', 'banner', 'brand', 'chain', 'concept']) || 'UP';
      const bannerId = bannerMap.get(rawBanner.toUpperCase()) || 1; // Default to Urban Planet if unknown

      const storeName = findValue(row, ['store_name', 'name', 'location_name', 'store_location']) || `Store #${storeNum}`;
      const address = findValue(row, ['address_line1', 'address', 'street_address']) || 'Address Pending';
      const city = findValue(row, ['city', 'municipality']) || 'Unknown City';
      const stateProv = findValue(row, ['state_province', 'state', 'prov', 'province', 'st']) || 'ON';
      const postalCode = findValue(row, ['postal_code', 'zip', 'zip_code', 'postal']) || '00000';

      // Auto-classify US stores: YM stores starting with 6000s or US state codes
      let countryCode = findValue(row, ['country_code', 'country']) || '';
      if (!countryCode) {
        if (/^60\d{2}$/.test(storeNum) || ['NJ', 'NY', 'PA', 'IL', 'FL', 'TX', 'CA'].includes(stateProv.toUpperCase())) {
          countryCode = 'US';
        } else {
          countryCode = 'CA';
        }
      }

      const grossSqft = parseInt(findValue(row, ['gross_sqft', 'gross_sq_ft', 'total_sqft', 'sqft']) || '8000', 10);
      const salesSqft = parseInt(findValue(row, ['sales_sqft', 'selling_sqft', 'sales_sq_ft']) || '6800', 10);
      const operationalStatus = findValue(row, ['operational_status', 'status', 'lifecycle_status']) || 'Grand Open';
      const overallHealth = findValue(row, ['overall_health', 'health']) || 'On Track';
      const targetOpenDate = findValue(row, ['target_open_date', 'opening_date', 'open_date']) || '2026-11-01';

      // Assign default district based on region
      const defaultDistrictId = countryCode === 'US' ? 12 : 1;

      // Upsert into stores
      const upsertStoreQuery = `
        INSERT INTO stores (
          store_number, banner_id, district_id, store_name, address_line1,
          city, state_province, postal_code, country_code, gross_sqft, sales_sqft,
          operational_status, overall_health, opening_date, target_open_date
        ) VALUES (
          $1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15
        )
        ON CONFLICT (store_number) DO UPDATE SET
          banner_id = EXCLUDED.banner_id,
          store_name = EXCLUDED.store_name,
          address_line1 = EXCLUDED.address_line1,
          city = EXCLUDED.city,
          state_province = EXCLUDED.state_province,
          postal_code = EXCLUDED.postal_code,
          country_code = EXCLUDED.country_code,
          gross_sqft = EXCLUDED.gross_sqft,
          sales_sqft = EXCLUDED.sales_sqft,
          operational_status = EXCLUDED.operational_status,
          overall_health = EXCLUDED.overall_health
        RETURNING store_id;
      `;

      const storeRes = await client.query(upsertStoreQuery, [
        storeNum, bannerId, defaultDistrictId, storeName, address,
        city, stateProv, postalCode, countryCode, grossSqft, salesSqft,
        operationalStatus, overallHealth, targetOpenDate, targetOpenDate
      ]);

      const storeId = storeRes.rows[0].store_id;

      // Upsert into store_attributes
      const upsertAttrQuery = `
        INSERT INTO store_attributes (store_id, real_estate_format, gross_sq_ft, selling_sq_ft)
        VALUES ($1, 'Enclosed Mall', $2, $3)
        ON CONFLICT (store_id) DO UPDATE SET
          gross_sq_ft = EXCLUDED.gross_sq_ft,
          selling_sq_ft = EXCLUDED.selling_sq_ft;
      `;
      await client.query(upsertAttrQuery, [storeId, grossSqft, salesSqft]);

      importedCount++;
      const tag = countryCode === 'US' ? '[US 6000s]' : '[CA 6xxx] ';
      console.log(`  ✓ Synced ${tag} Store #${storeNum}: ${storeName} (${city}, ${stateProv})`);
    }

    console.log(`==================================================================`);
    console.log(`  Import complete! Successfully upserted ${importedCount} stores. `);
    console.log(`==================================================================`);

  } catch (err) {
    console.error(`[DATABASE ERROR]`, err);
  } finally {
    await client.end();
  }
}

if (require.main === module) {
  main();
}

module.exports = { parseCSV, findValue, normalizeKey };
