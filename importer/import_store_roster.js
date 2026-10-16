/**
 * YM Inc. Store Tracking Project: Excel & CSV Store Roster Importer
 * File: importer/import_store_roster.js
 * 
 * Usage:
 *   node importer/import_store_roster.js <path-to-excel-or-csv-file>
 */

const fs = require('fs');
const path = require('path');
const { Client } = require('pg');
require('dotenv').config();

// Optional XLSX library check
let xlsx = null;
try {
  xlsx = require('xlsx');
} catch (e) {
  // Fall back to native CSV parser
}

function parseCSV(content) {
  const rows = [];
  let currentRow = [];
  let currentField = '';
  let insideQuote = false;

  for (let i = 0; i < content.length; i++) {
    const char = content[i];
    const nextChar = content[i + 1];

    if (char === '"') {
      if (insideQuote && nextChar === '"') {
        currentField += '"';
        i++; // skip escaped quote
      } else {
        insideQuote = !insideQuote;
      }
    } else if (char === ',' && !insideQuote) {
      currentRow.push(currentField.trim());
      currentField = '';
    } else if ((char === '\r' || char === '\n') && !insideQuote) {
      if (char === '\r' && nextChar === '\n') {
        i++;
      }
      currentRow.push(currentField.trim());
      if (currentRow.some(f => f.length > 0)) {
        rows.push(currentRow);
      }
      currentRow = [];
      currentField = '';
    } else {
      currentField += char;
    }
  }
  if (currentField.length > 0 || currentRow.length > 0) {
    currentRow.push(currentField.trim());
    if (currentRow.some(f => f.length > 0)) {
      rows.push(currentRow);
    }
  }

  if (rows.length < 2) return [];
  const headers = rows[0].map(h => h.trim().replace(/^["']|["']$/g, ''));
  return rows.slice(1).map(r => {
    const obj = {};
    headers.forEach((h, idx) => {
      obj[h] = r[idx] ? r[idx].replace(/^["']|["']$/g, '').trim() : '';
    });
    return obj;
  });
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

const US_STATES = new Set([
  'AL', 'AK', 'AZ', 'AR', 'CA', 'CO', 'CT', 'DE', 'FL', 'GA',
  'HI', 'ID', 'IL', 'IN', 'IA', 'KS', 'KY', 'LA', 'ME', 'MD',
  'MA', 'MI', 'MN', 'MS', 'MO', 'MT', 'NE', 'NV', 'NH', 'NJ',
  'NM', 'NY', 'NC', 'ND', 'OH', 'OK', 'OR', 'PA', 'RI', 'SC',
  'SD', 'TN', 'TX', 'UT', 'VT', 'VA', 'WA', 'WV', 'WI', 'WY', 'DC', 'PR'
]);

function mapDistrict(stateProv, countryCode) {
  const s = (stateProv || '').toUpperCase();
  if (countryCode === 'CA') {
    if (['ON'].includes(s)) return 1;
    if (['QC'].includes(s)) return 6;
    if (['AB', 'SK', 'MB'].includes(s)) return 8;
    if (['BC'].includes(s)) return 10;
    if (['NS', 'NB', 'NL', 'PE'].includes(s)) return 11;
    return 1;
  }
  // US Districts
  if (['NY', 'NJ', 'CT'].includes(s)) return 12; // DIST-NYNJ
  if (['PA', 'DE', 'MD', 'VA', 'WV'].includes(s)) return 13; // DIST-PAPH
  if (['IL', 'WI', 'MI', 'IN', 'OH', 'MN', 'IA', 'MO', 'KY'].includes(s)) return 14; // DIST-CHI
  if (['FL', 'GA', 'SC', 'NC', 'AL', 'TN', 'MS'].includes(s)) return 15; // DIST-FLS
  if (['TX', 'LA', 'OK', 'AR'].includes(s)) return 16; // DIST-HOU
  if (['CA', 'WA', 'OR', 'NV', 'AZ', 'UT', 'CO', 'NM', 'ID', 'MT', 'WY'].includes(s)) return 18; // DIST-CALA
  return 12; // Default US
}

function normalizeStatus(rawStatus) {
  if (!rawStatus) return 'Open';
  const upper = rawStatus.toUpperCase();
  if (upper.includes('CLOSED') || upper.includes('DECOMM')) return 'Closed';
  if (upper.includes('OPEN') || upper === 'ACTIVE' || upper.includes('S-P-')) return 'Open';
  if (upper.includes('NP') || upper.includes('CABLING') || upper.includes('STAGING')) return 'Low-Voltage & Power Poles';
  if (upper.includes('CONSTRUCT') || upper.includes('GC')) return 'GC Construction';
  if (upper.includes('SURVEY')) return 'Site Survey';
  return 'Open';
}

function resolveBanner(rawBanner, bannerMap) {
  if (!rawBanner) return 1;
  const upper = rawBanner.toUpperCase().trim();
  if (bannerMap.has(upper)) return bannerMap.get(upper);

  // Common aliases
  if (upper.includes('RUE') || upper === 'RUE21' || upper === 'R21') return bannerMap.get('R21') || bannerMap.get('RUE21') || 12;
  if (upper.includes('CHARLOTTE') || upper === 'CR') return bannerMap.get('CR') || bannerMap.get('CHARLOTTE RUSSE') || 11;
  if (upper.includes('MANDEE') || upper === 'M') return bannerMap.get('M') || bannerMap.get('MANDEE') || 13;
  if (upper.includes('PLANET') || upper === 'UP') return bannerMap.get('UP') || bannerMap.get('URBAN PLANET') || 1;
  if (upper.includes('BLUENOTES') || upper === 'BN') return bannerMap.get('BN') || bannerMap.get('BLUENOTES') || 2;
  if (upper.includes('SUZY') || upper === 'SS') return bannerMap.get('SS') || bannerMap.get('SUZY SHIER') || 3;
  if (upper.includes('CHÂTEAU') || upper.includes('CHATEAU') || upper === 'LC') return bannerMap.get('LC') || bannerMap.get('LE CHÂTEAU') || 4;
  if (upper.includes('FOREVER') || upper === 'F21') return bannerMap.get('F21') || bannerMap.get('FOREVER 21') || 6;
  if (upper.includes('WEST') || upper === 'W49') return bannerMap.get('W49') || bannerMap.get('WEST 49') || 8;
  if (upper.includes('SIRENS') || upper === 'SRN') return bannerMap.get('SRN') || bannerMap.get('SIRENS') || 5;

  return 1;
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
  const connStr = process.env.DATABASE_URL || 'postgresql://ym_admin:ym_secure_password_2026@localhost:5432/ym_store_tracking';
  const isLocal = connStr.includes('localhost') || connStr.includes('127.0.0.1');
  const client = new Client({
    connectionString: connStr,
    ssl: isLocal ? false : { rejectUnauthorized: false }
  });

  try {
    await client.connect();
    console.log(`Connected to PostgreSQL database: ym_store_tracking`);

    // Cache existing banners
    const bannersRes = await client.query('SELECT banner_id, banner_code, banner_name FROM banners');
    const bannerMap = new Map();
    const bannerNameMap = new Map();
    bannersRes.rows.forEach(b => {
      bannerMap.set(b.banner_code.toUpperCase(), b.banner_id);
      bannerMap.set(b.banner_name.toUpperCase(), b.banner_id);
      bannerNameMap.set(b.banner_id, b.banner_name);
    });

    let importedCount = 0;

    for (const row of rows) {
      const storeNum = findValue(row, ['store_no', 'store_number', 'store_id', 'store', 'store#', 'unit']);
      if (!storeNum) continue;

      const rawBanner = findValue(row, ['banner', 'banner_code', 'brand', 'chain', 'concept']) || 'UP';
      const bannerId = resolveBanner(rawBanner, bannerMap);
      const bannerName = bannerNameMap.get(bannerId) || 'YM Store';

      const mall = findValue(row, ['mall', 'mall_name', 'shopping_center', 'property']) || '';
      let storeName = findValue(row, ['store_name', 'name', 'location_name', 'store_location']);
      if (!storeName || storeName === `Store #${storeNum}` || storeName.includes(' - ')) {
        storeName = bannerName;
      }

      const address = findValue(row, ['address', 'address_line1', 'street_address']) || 'Address Pending';
      const city = findValue(row, ['city', 'municipality']) || 'Unknown City';
      const stateProv = (findValue(row, ['state', 'state_province', 'prov', 'province', 'st']) || 'ON').toUpperCase();
      const postalCode = findValue(row, ['zip', 'postal_code', 'zip_code', 'postal']) || '00000';
      const phone = findValue(row, ['store_phone', 'phone', 'phone_number']) || null;

      // Auto-classify Country
      let countryCode = findValue(row, ['country_code', 'country']) || '';
      if (!countryCode) {
        countryCode = US_STATES.has(stateProv) ? 'US' : 'CA';
      }

      const grossSqft = parseInt(findValue(row, ['gross_sqft', 'gross_sq_ft', 'total_sqft', 'sqft']) || '7500', 10);
      const salesSqft = parseInt(findValue(row, ['sales_sqft', 'selling_sqft', 'sales_sq_ft']) || '6200', 10);

      const rawStatus = findValue(row, ['status', 'operational_status', 'lifecycle_status']);
      const operationalStatus = normalizeStatus(rawStatus);
      const overallHealth = findValue(row, ['overall_health', 'health']) || 'On Track';
      const targetOpenDate = findValue(row, ['date', 'target_open_date', 'opening_date', 'open_date']) || '2026-11-01';

      const districtId = mapDistrict(stateProv, countryCode);

      // Upsert into stores
      const upsertStoreQuery = `
        INSERT INTO stores (
          store_number, banner_id, district_id, store_name, address_line1,
          city, state_province, postal_code, country_code, phone_number, gross_sqft, sales_sqft,
          operational_status, overall_health, opening_date, target_open_date
        ) VALUES (
          $1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16
        )
        ON CONFLICT (store_number) DO UPDATE SET
          banner_id = EXCLUDED.banner_id,
          district_id = EXCLUDED.district_id,
          store_name = EXCLUDED.store_name,
          address_line1 = EXCLUDED.address_line1,
          city = EXCLUDED.city,
          state_province = EXCLUDED.state_province,
          postal_code = EXCLUDED.postal_code,
          country_code = EXCLUDED.country_code,
          phone_number = COALESCE(EXCLUDED.phone_number, stores.phone_number),
          gross_sqft = EXCLUDED.gross_sqft,
          sales_sqft = EXCLUDED.sales_sqft,
          operational_status = EXCLUDED.operational_status,
          overall_health = EXCLUDED.overall_health
        RETURNING store_id;
      `;

      const storeRes = await client.query(upsertStoreQuery, [
        storeNum, bannerId, districtId, storeName, address,
        city, stateProv, postalCode, countryCode, phone, grossSqft, salesSqft,
        operationalStatus, overallHealth, targetOpenDate, targetOpenDate
      ]);

      const storeId = storeRes.rows[0].store_id;

      // Upsert into store_attributes
      const realEstateFormat = mall ? 'Enclosed Mall' : 'Power Centre / Big Box';
      const upsertAttrQuery = `
        INSERT INTO store_attributes (
          store_id, real_estate_format, selling_sq_ft, gross_sq_ft
        ) VALUES ($1, $2, $3, $4)
        ON CONFLICT (store_id) DO UPDATE SET
          real_estate_format = EXCLUDED.real_estate_format,
          selling_sq_ft = EXCLUDED.selling_sq_ft,
          gross_sq_ft = EXCLUDED.gross_sq_ft;
      `;
      await client.query(upsertAttrQuery, [
        storeId, realEstateFormat, salesSqft, grossSqft
      ]);

      importedCount++;
      if (importedCount <= 10 || importedCount % 50 === 0 || importedCount === rows.length) {
        console.log(`  ✓ Synced [${countryCode}] Store #${storeNum}: ${storeName} (${city}, ${stateProv}) [${operationalStatus}]`);
      }
    }

    console.log(`==================================================================`);
    console.log(`  Import complete! Successfully imported/updated ${importedCount} stores. `);
    console.log(`==================================================================`);

    const totalStores = await client.query('SELECT COUNT(*) FROM stores;');
    const usStores = await client.query("SELECT COUNT(*) FROM stores WHERE country_code = 'US';");
    const caStores = await client.query("SELECT COUNT(*) FROM stores WHERE country_code = 'CA';");
    console.log(`Total Stores in Database: ${totalStores.rows[0].count} (US: ${usStores.rows[0].count}, CA: ${caStores.rows[0].count})`);

  } catch (err) {
    console.error('[DATABASE ERROR]', err);
  } finally {
    await client.end();
  }
}

main();
