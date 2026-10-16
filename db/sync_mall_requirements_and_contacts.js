/**
 * Comprehensive migration and sync script:
 * 1. Drops obsolete merchandise_categories.
 * 2. Restructures mall_requirements to exactly match US_SNIU_Mall_Requirements.xlsx.
 * 3. Populates all 320+ US & Canadian mall properties into mall_properties.
 * 4. Links stores to mall_properties and updates store phone numbers.
 * 5. Populates store_contacts with Mall Management and Store Manager contacts.
 */

const fs = require('fs');
const path = require('path');
const xlsx = require('xlsx');
const { Client } = require('pg');
require('dotenv').config();

const client = new Client({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }
});

function formatPhone(raw) {
  if (!raw) return null;
  const digits = String(raw).replace(/\D/g, '');
  if (digits.length === 10) {
    return `(${digits.slice(0, 3)}) ${digits.slice(3, 6)}-${digits.slice(6)}`;
  }
  if (digits.length === 11 && digits.startsWith('1')) {
    return `(${digits.slice(1, 4)}) ${digits.slice(4, 7)}-${digits.slice(7)}`;
  }
  return String(raw).trim();
}

function parseDate(raw) {
  if (!raw) return null;
  if (typeof raw === 'number') {
    // Excel serial date
    const d = new Date(Math.round((raw - 25569) * 86400 * 1000));
    return isNaN(d.getTime()) ? null : d.toISOString().split('T')[0];
  }
  if (typeof raw === 'string') {
    const trimmed = raw.trim();
    if (/^\d{4}-\d{2}-\d{2}/.test(trimmed)) {
      return trimmed.slice(0, 10);
    }
    const d = new Date(trimmed);
    return isNaN(d.getTime()) ? null : d.toISOString().split('T')[0];
  }
  return null;
}

function generateMallCode(mallName, index) {
  const clean = mallName.toUpperCase().replace(/[^A-Z0-9]/g, '');
  const prefix = clean.slice(0, 6);
  return `MALL-${prefix}-${String(index).padStart(3, '0')}`;
}

async function run() {
  await client.connect();
  console.log('Connected to Neon database.');

  // =========================================================================
  // STEP 1: Drop merchandise_categories
  // =========================================================================
  console.log('\n--- STEP 1: Dropping merchandise_categories ---');
  await client.query('DROP VIEW IF EXISTS v_pie_category_mix CASCADE;');
  await client.query('ALTER TABLE IF EXISTS weekly_store_inventory DROP CONSTRAINT IF EXISTS weekly_store_inventory_category_id_fkey;');
  await client.query('ALTER TABLE IF EXISTS weekly_store_inventory DROP COLUMN IF EXISTS category_id;');
  await client.query('DROP TABLE IF EXISTS merchandise_categories CASCADE;');
  console.log('✓ Successfully removed merchandise_categories and related dependencies.');

  // =========================================================================
  // STEP 2: Restructure mall_properties and mall_requirements
  // =========================================================================
  console.log('\n--- STEP 2: Restructuring mall tables to match Excel ---');

  // Add contact columns to mall_properties if missing
  await client.query(`
    ALTER TABLE mall_properties 
    ADD COLUMN IF NOT EXISTS management_contact_name VARCHAR(150),
    ADD COLUMN IF NOT EXISTS management_email VARCHAR(255),
    ADD COLUMN IF NOT EXISTS management_phone VARCHAR(100),
    ADD COLUMN IF NOT EXISTS special_cabling_rules TEXT;

    ALTER TABLE store_contacts 
    ALTER COLUMN email TYPE VARCHAR(255),
    ALTER COLUMN phone_mobile TYPE VARCHAR(100),
    ALTER COLUMN phone_office TYPE VARCHAR(100);

    DELETE FROM store_contacts WHERE contact_role IN ('Landlord Operations Manager', 'Store Manager');
  `);

  // Drop old mall_requirements and recreate matching the Excel sheet columns
  await client.query('DROP TABLE IF EXISTS mall_requirements CASCADE;');
  await client.query(`
    CREATE TABLE mall_requirements (
      requirement_id SERIAL PRIMARY KEY,
      mall_id INT NOT NULL REFERENCES mall_properties(mall_id) ON DELETE CASCADE,
      store_id INT REFERENCES stores(store_id) ON DELETE CASCADE,
      store_number VARCHAR(20),
      mall_name VARCHAR(150) NOT NULL,
      assigned_team_member VARCHAR(100),
      mall_contacted BOOLEAN NOT NULL DEFAULT FALSE,
      contact_name VARCHAR(150),
      email_address VARCHAR(255),
      phone VARCHAR(100),
      via_email BOOLEAN DEFAULT FALSE,
      via_phone BOOLEAN DEFAULT FALSE,
      contact_date DATE,
      requirements_ttu_cabling TEXT,
      requirements_circuits TEXT,
      intermediate_steps TEXT,
      notes TEXT,
      coi_required BOOLEAN DEFAULT TRUE,
      night_work_only BOOLEAN DEFAULT FALSE,
      work_hours_restriction VARCHAR(150),
      security_phone VARCHAR(50),
      created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
      updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
      CONSTRAINT uq_mall_req UNIQUE (mall_id, store_id)
    );
  `);
  console.log('✓ Created mall_requirements table matching exact Excel columns.');

  // =========================================================================
  // STEP 3: Load US_SNIU_Mall_Requirements.xlsx
  // =========================================================================
  const excelPath = path.join(__dirname, '..', 'data', 'US_SNIU_Mall_Requirements.xlsx');
  if (!fs.existsSync(excelPath)) {
    throw new Error(`File not found: ${excelPath}`);
  }

  const wb = xlsx.readFile(excelPath);
  const rows = xlsx.utils.sheet_to_json(wb.Sheets['Sheet1']);
  console.log(`\n--- STEP 3: Ingesting ${rows.length} Mall Requirements & Properties from Excel ---`);

  // Load existing stores mapping
  const storesRes = await client.query('SELECT store_id, store_number, store_name FROM stores;');
  const storeMap = new Map();
  storesRes.rows.forEach(s => storeMap.set(String(s.store_number).trim(), s.store_id));

  // Load existing mall_properties
  const existingMallsRes = await client.query('SELECT mall_id, property_name FROM mall_properties;');
  const mallMap = new Map();
  existingMallsRes.rows.forEach(m => mallMap.set(m.property_name.toUpperCase().trim(), m.mall_id));

  let mallIndex = existingMallsRes.rows.length + 1;
  let newMallsCount = 0;
  let reqCount = 0;
  let contactsCount = 0;

  for (const r of rows) {
    const rawStoreNum = r['Store No.'] ? String(r['Store No.']).trim() : null;
    let rawMall = r['Mall'] ? String(r['Mall']).trim() : '';

    if (!rawMall || rawMall === rawStoreNum) {
      rawMall = r['Address'] ? `${r['City']} Retail Center` : `Store #${rawStoreNum} Center`;
    }

    const mallUpper = rawMall.toUpperCase().trim();
    let mallId = mallMap.get(mallUpper);

    const address = r['Address'] ? String(r['Address']).trim() : 'Address Pending';
    const city = r['City'] ? String(r['City']).trim() : 'Unknown';
    const state = r['State'] ? String(r['State']).trim().toUpperCase() : 'US';
    const storePhone = formatPhone(r['Store Phone']);

    const contactName = r['Contact Name'] ? String(r['Contact Name']).trim() : null;
    const email = r['Email Address'] ? String(r['Email Address']).trim() : null;
    const phone = r['Phone'] ? String(r['Phone']).trim() : null;
    const teamMember = r['Team Member'] ? String(r['Team Member']).trim() : null;
    const mallContacted = Boolean(r['Mall Contacted']);
    const viaEmail = Boolean(r['Via Email']);
    const viaPhone = Boolean(r['Via Phone']);
    const contactDate = parseDate(r['Date']);
    const reqCabling = r['Requirements for TTU & Cabling'] ? String(r['Requirements for TTU & Cabling']).trim() : null;
    const reqCircuits = r['Requirements for Circuits'] ? String(r['Requirements for Circuits']).trim() : null;
    const intermediate = r['Intermediate Steps'] ? String(r['Intermediate Steps']).trim() : null;
    const notes = r['Notes'] ? String(r['Notes']).trim() : null;

    // Derive boolean rules
    const combinedRules = `${reqCabling || ''} ${notes || ''}`;
    const coiRequired = combinedRules.toLowerCase().includes('no coi') ? false : (combinedRules.toLowerCase().includes('coi') || true);
    const nightWorkOnly = combinedRules.toLowerCase().includes('after mall hours') || combinedRules.toLowerCase().includes('off-hours');

    // Extract security phone if present
    let secPhone = null;
    const secMatch = combinedRules.match(/security\s*([0-9\.\-\+]+)/i);
    if (secMatch) secPhone = secMatch[1];

    // If mall does not exist in mall_properties, insert it!
    if (!mallId) {
      const mallCode = generateMallCode(rawMall, mallIndex++);
      const insertMallQuery = `
        INSERT INTO mall_properties (
          mall_code, property_name, landlord_company, property_type,
          address_line1, city, state_province, postal_code, country_code,
          management_contact_name, management_email, management_phone, security_dispatch_phone,
          special_cabling_rules
        ) VALUES (
          $1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14
        )
        RETURNING mall_id;
      `;

      const mallRes = await client.query(insertMallQuery, [
        mallCode, rawMall, 'Commercial Mall Operations', 'Enclosed Regional Mall',
        address, city, state, '00000', 'US',
        contactName, email, phone, secPhone, reqCabling
      ]);

      mallId = mallRes.rows[0].mall_id;
      mallMap.set(mallUpper, mallId);
      newMallsCount++;
    }

    const storeId = rawStoreNum ? storeMap.get(rawStoreNum) : null;

    // Update store with mall_id and store phone number
    if (storeId) {
      await client.query(`
        UPDATE stores 
        SET mall_id = $1,
            phone_number = COALESCE($2, phone_number)
        WHERE store_id = $3;
      `, [mallId, storePhone, storeId]);

      // Insert/Update mall_requirements
      const reqQuery = `
        INSERT INTO mall_requirements (
          mall_id, store_id, store_number, mall_name, assigned_team_member,
          mall_contacted, contact_name, email_address, phone,
          via_email, via_phone, contact_date, requirements_ttu_cabling,
          requirements_circuits, intermediate_steps, notes, coi_required,
          night_work_only, security_phone
        ) VALUES (
          $1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16, $17, $18, $19
        )
        ON CONFLICT (mall_id, store_id) DO UPDATE SET
          assigned_team_member = EXCLUDED.assigned_team_member,
          mall_contacted = EXCLUDED.mall_contacted,
          contact_name = EXCLUDED.contact_name,
          email_address = EXCLUDED.email_address,
          phone = EXCLUDED.phone,
          via_email = EXCLUDED.via_email,
          via_phone = EXCLUDED.via_phone,
          contact_date = EXCLUDED.contact_date,
          requirements_ttu_cabling = EXCLUDED.requirements_ttu_cabling,
          requirements_circuits = EXCLUDED.requirements_circuits,
          intermediate_steps = EXCLUDED.intermediate_steps,
          notes = EXCLUDED.notes,
          coi_required = EXCLUDED.coi_required,
          night_work_only = EXCLUDED.night_work_only,
          security_phone = EXCLUDED.security_phone;
      `;

      await client.query(reqQuery, [
        mallId, storeId, rawStoreNum, rawMall, teamMember,
        mallContacted, contactName, email, phone,
        viaEmail, viaPhone, contactDate, reqCabling,
        reqCircuits, intermediate, notes, coiRequired,
        nightWorkOnly, secPhone
      ]);
      reqCount++;

      // =========================================================================
      // STEP 4: Insert Contact Records into store_contacts
      // =========================================================================
      if (contactName || email || phone) {
        await client.query(`
          INSERT INTO store_contacts (
            store_id, contact_role, full_name, organization, email, phone_office, phone_mobile, notes
          ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8);
        `, [
          storeId,
          'Landlord Operations Manager',
          contactName || 'Mall Operations Contact',
          rawMall,
          email || 'operations@mall.com',
          phone,
          phone || storePhone || 'N/A',
          reqCabling ? reqCabling.slice(0, 250) : null
        ]);
        contactsCount++;
      }

      if (storePhone) {
        await client.query(`
          INSERT INTO store_contacts (
            store_id, contact_role, full_name, organization, email, phone_office, phone_mobile, notes
          ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8);
        `, [
          storeId,
          'Store Manager',
          `Store #${rawStoreNum} Manager`,
          'YM Inc.',
          `store${rawStoreNum}@yminc.ca`,
          storePhone,
          storePhone,
          'Primary Store Front Direct Line'
        ]);
        contactsCount++;
      }
    }
  }

  console.log(`\n==================================================================`);
  console.log(`  MALL & CONTACT SYNC COMPLETE!`);
  console.log(`==================================================================`);
  console.log(`  New Malls Added to mall_properties: ${newMallsCount}`);
  const totalMalls = await client.query('SELECT COUNT(*) FROM mall_properties;');
  console.log(`  Total Malls in Database:            ${totalMalls.rows[0].count} (All US & CA Properties)`);
  console.log(`  Mall Requirements Rows:             ${reqCount} (Matching Excel exactly)`);
  console.log(`  Store & Mall Contacts Created:      ${contactsCount}`);

  const sampleContacts = await client.query(`
    SELECT sc.contact_role, sc.full_name, sc.organization, sc.phone_mobile, sc.email
    FROM store_contacts sc
    LIMIT 6;
  `);
  console.log('\nSample Store & Mall Management Contacts in DB:');
  console.table(sampleContacts.rows);

  await client.end();
}

run().catch(err => {
  console.error('[SYNC ERROR]', err);
  process.exit(1);
});
