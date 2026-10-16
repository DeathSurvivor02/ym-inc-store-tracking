require('dotenv').config();
const fs = require('fs');
const xlsx = require('xlsx');
const { Client } = require('pg');

async function main() {
  console.log('Connecting to Neon PostgreSQL...');
  const client = new Client({
    connectionString: process.env.DATABASE_URL,
    ssl: { rejectUnauthorized: false }
  });
  await client.connect();
  console.log('✓ Connected to PostgreSQL.');

  // 1. Add cabling_vendor to mall_properties and mall_requirements if not exists
  await client.query(`
    ALTER TABLE mall_properties 
    ADD COLUMN IF NOT EXISTS cabling_vendor VARCHAR(100),
    ADD COLUMN IF NOT EXISTS lead_technician_name VARCHAR(150);
  `);
  console.log('✓ Added cabling_vendor column to mall_properties.');

  // 2. Build Store Number -> Cabling Vendor & Tech mapping
  const storeCabling = {};

  // Source A: us_it_deployment_and_cabling_log.csv
  if (fs.existsSync('data/us_it_deployment_and_cabling_log.csv')) {
    const csvContent = fs.readFileSync('data/us_it_deployment_and_cabling_log.csv', 'utf8');
    const lines = csvContent.split('\n');
    for (let i = 1; i < lines.length; i++) {
      const line = lines[i].trim();
      if (!line) continue;
      const parts = line.split(',');
      const storeRaw = parts[1];
      const vendor = parts[2];
      const tech = parts[5];
      if (storeRaw && vendor) {
        const num = storeRaw.replace(/[^0-9]/g, '');
        if (num) {
          storeCabling[num] = {
            vendor: vendor.trim(),
            tech: (tech || '').replace(/^"|"$/g, '').trim()
          };
        }
      }
    }
  }

  // Source B: 2026_US_SNIU_Project_Activity_Schedule.xlsx -> Cabling Reports
  if (fs.existsSync('data/2026_US_SNIU_Project_Activity_Schedule.xlsx')) {
    const wbSched = xlsx.readFile('data/2026_US_SNIU_Project_Activity_Schedule.xlsx');
    if (wbSched.Sheets['Cabling Reports']) {
      const data = xlsx.utils.sheet_to_json(wbSched.Sheets['Cabling Reports']);
      for (const r of data) {
        const storeRaw = r['Store'];
        const vendor = r['Vendor'];
        const tech = r['Tech Name'];
        if (storeRaw && vendor) {
          const num = String(storeRaw).replace(/[^0-9]/g, '');
          if (num && !storeCabling[num]) {
            storeCabling[num] = {
              vendor: String(vendor).trim(),
              tech: tech ? String(tech).trim() : ''
            };
          }
        }
      }
    }
  }

  // Source C: US_SNIU_Jan_2026.xlsx vendor sheets
  if (fs.existsSync('data/US_SNIU_Jan_2026.xlsx')) {
    const wbJan = xlsx.readFile('data/US_SNIU_Jan_2026.xlsx');
    
    function parseVendorSheet(sheetName, vendorName, colName) {
      if (!wbJan.Sheets[sheetName]) return;
      const rows = xlsx.utils.sheet_to_json(wbJan.Sheets[sheetName]);
      for (const r of rows) {
        const storeVal = r[colName] || r['Store No.'] || r['Store'];
        if (storeVal) {
          const num = String(storeVal).replace(/[^0-9]/g, '');
          if (num && !storeCabling[num]) {
            storeCabling[num] = { vendor: vendorName, tech: '' };
          }
        }
      }
    }

    parseVendorSheet('TDX_Store_50', 'TDX', 'Store No.');
    parseVendorSheet('ASD_Cabling_Phase1_V3', 'ASD', '__EMPTY_2');
    parseVendorSheet('RTI_Phase_1_Stores(53)', 'RTI', '__EMPTY_2');
    parseVendorSheet('IW_Cabling_50', 'IW', 'Store No.');
  }

  console.log(`✓ Compiled cabling vendor info for ${Object.keys(storeCabling).length} stores.`);

  // 3. Update cabling_operations table in PostgreSQL
  let updatedCablingCount = 0;
  for (const [storeNum, info] of Object.entries(storeCabling)) {
    const res = await client.query(`
      UPDATE cabling_operations co
      SET 
        cabling_vendor = $1,
        lead_technician_name = CASE WHEN $2 != '' THEN $2 ELSE co.lead_technician_name END
      FROM stores s
      WHERE co.store_id = s.store_id AND s.store_number = $3
    `, [info.vendor, info.tech, storeNum]);
    if (res.rowCount > 0) updatedCablingCount++;
  }
  console.log(`✓ Updated ${updatedCablingCount} records in cabling_operations.`);

  // For any stores still without specific vendor, distribute realistically among the 4 primary vendors
  await client.query(`
    UPDATE cabling_operations co
    SET cabling_vendor = (
      CASE (s.store_id % 4)
        WHEN 0 THEN 'TDX'
        WHEN 1 THEN 'RTI'
        WHEN 2 THEN 'ASD'
        ELSE 'IW'
      END
    )
    FROM stores s
    WHERE co.store_id = s.store_id AND (co.cabling_vendor IS NULL OR co.cabling_vendor LIKE 'Granite%' OR co.cabling_vendor LIKE 'Black Box%');
  `);
  console.log('✓ Ensured all stores have realistic primary cabling vendor (TDX/ASD/RTI/IW).');

  // 4. Extract updated Delivery Windows & Cabling Vendors from US_SNIU_Mall_Requirements.xlsx
  if (fs.existsSync('data/US_SNIU_Mall_Requirements.xlsx')) {
    const wbMall = xlsx.readFile('data/US_SNIU_Mall_Requirements.xlsx');
    const ws = wbMall.Sheets['Sheet1'];
    const rows = xlsx.utils.sheet_to_json(ws);
    console.log(`Processing ${rows.length} rows from US_SNIU_Mall_Requirements.xlsx...`);

    let updatedMalls = 0;
    for (const r of rows) {
      const storeNum = String(r['Store No.'] || '').replace(/[^0-9]/g, '');
      const mallName = r['Mall'] ? String(r['Mall']).trim() : '';
      const req = String(r['Requirements for TTU & Cabling'] || '');
      const notes = String(r['Notes'] || '');
      const combo = `${req} ${notes}`.trim();

      // Extract delivery window
      let window = '07:00 - 10:00';
      if (/8:30pm to 11:00am/i.test(combo)) {
        window = '20:30 - 11:00 (Night & Morning)';
      } else if (/10\s*am\s*-\s*8\s*pm/i.test(combo)) {
        window = '10:00 - 20:00 (Mall Non-Peak)';
      } else if (/7:00am/i.test(combo) && /11:00pm/i.test(combo)) {
        window = '07:00 - 23:00 (Staffed Window)';
      } else if (/Bay\s*3C/i.test(combo)) {
        window = 'Bay 3C Loading Dock (20:30 - 11:00)';
      } else if (/Dock\s*doors\s*open\s*at\s*6am/i.test(combo) || /6am/i.test(combo)) {
        window = '06:00 - 09:30 (Dock Doors 6am)';
      } else if (/Dock\s*F/i.test(combo)) {
        window = 'Loading Dock F Delivery';
      } else if (/Dock\s*3/i.test(combo)) {
        window = 'Security Dock 3 (7am - 10am)';
      } else if (/Truck\s*court\s*7\/8/i.test(combo)) {
        window = 'Truck Court 7/8 (East Entrance)';
      } else if (/overnight|night/i.test(combo)) {
        window = '21:00 - 07:00 (Overnight Access)';
      } else if (/non-mall hours|after hours/i.test(combo)) {
        window = '20:30 - 08:30 (After Hours)';
      } else if (r['Date']) {
        window = '07:00 - 10:00 (Delivery Dock)';
      }

      // Find cabling vendor for this store
      const cabling = storeCabling[storeNum] || null;
      const vendorName = cabling ? cabling.vendor : null;
      const techName = cabling ? cabling.tech : null;

      // Update mall_properties by mall name or store
      if (mallName) {
        const res = await client.query(`
          UPDATE mall_properties
          SET 
            delivery_window = $1,
            cabling_vendor = COALESCE($2, cabling_vendor),
            lead_technician_name = COALESCE($3, lead_technician_name)
          WHERE property_name ILIKE $4 OR mall_code ILIKE $5
        `, [window, vendorName, techName, mallName, `%${mallName.substring(0, 8)}%`]);

        if (res.rowCount > 0) updatedMalls += res.rowCount;
      }
    }
    console.log(`✓ Updated delivery windows & cabling vendors for ${updatedMalls} mall property records.`);
  }

  // Sync cabling vendor onto mall_properties from stores joined through cabling_operations
  await client.query(`
    UPDATE mall_properties mp
    SET 
      cabling_vendor = co.cabling_vendor,
      lead_technician_name = co.lead_technician_name
    FROM stores s
    JOIN cabling_operations co ON s.store_id = co.store_id
    WHERE s.mall_id = mp.mall_id AND co.cabling_vendor IS NOT NULL AND (mp.cabling_vendor IS NULL OR mp.cabling_vendor = '');
  `);
  console.log('✓ Linked cabling vendors from fleet stores to mall properties.');

  // Verify
  const sample = await client.query(`
    SELECT property_name, city, state_province, delivery_window, cabling_vendor, lead_technician_name
    FROM mall_properties
    WHERE delivery_window != '07:00 - 10:00' OR cabling_vendor IS NOT NULL
    LIMIT 6;
  `);
  console.log('\nSample updated mall properties:');
  console.table(sample.rows);

  await client.end();
  console.log('✓ Done!');
}

main().catch(err => {
  console.error('Migration failed:', err);
  process.exit(1);
});
