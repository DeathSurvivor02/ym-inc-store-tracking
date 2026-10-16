/**
 * End-to-end test verifying Neon PostgreSQL connection, schema, and queries
 */
const { Client } = require('pg');
require('dotenv').config();

const DATABASE_URL = process.env.DATABASE_URL;

if (!DATABASE_URL) {
  console.error('[FAIL] DATABASE_URL is not set in environment or .env');
  process.exit(1);
}

async function testConnection() {
  console.log('Testing connection to Neon PostgreSQL...');
  const isLocal = DATABASE_URL.includes('localhost') || DATABASE_URL.includes('127.0.0.1');
  const client = new Client({
    connectionString: DATABASE_URL,
    ssl: isLocal ? false : { rejectUnauthorized: false }
  });

  try {
    await client.connect();
    console.log('[PASS] Connected to Neon database successfully!');

    // 1. Verify schema tables
    const tableRes = await client.query(`
      SELECT table_name 
      FROM information_schema.tables 
      WHERE table_schema = 'public' 
      ORDER BY table_name;
    `);
    console.log(`[PASS] Schema contains ${tableRes.rows.length} public tables.`);

    // 2. Verify master banners
    const bannersRes = await client.query('SELECT banner_code, banner_name FROM banners ORDER BY banner_code;');
    console.log(`[PASS] Found ${bannersRes.rows.length} YM Inc. banners:`);
    bannersRes.rows.forEach(b => console.log(`       - ${b.banner_code}: ${b.banner_name}`));

    // 3. Verify sample stores
    const storesRes = await client.query(`
      SELECT s.store_number, s.store_name, b.banner_code, s.city, s.state_province, s.operational_status
      FROM stores s
      JOIN banners b ON s.banner_id = b.banner_id
      LIMIT 5;
    `);
    console.log(`[PASS] Sample Store Fleet Records:`);
    storesRes.rows.forEach(s => console.log(`       - Store #${s.store_number} (${s.banner_code}): ${s.store_name} [${s.city}, ${s.state_province}] Status: ${s.operational_status}`));

    // 4. Verify views
    const viewRes = await client.query('SELECT COUNT(*) FROM v_store_lifecycle_status;');
    console.log(`[PASS] Store Lifecycle Status View (v_store_lifecycle_status) is queryable (Rows: ${viewRes.rows[0].count}).`);
    const pieRes = await client.query('SELECT * FROM v_pie_fleet_health;');
    console.log(`[PASS] Executive Fleet Health Donut View (v_pie_fleet_health) is queryable (${pieRes.rows.length} categories).`);

    console.log('\n--> ALL END-TO-END NEON POSTGRESQL TESTS PASSED! <--');
  } catch (err) {
    console.error('[FAIL] Database test error:', err.message);
    process.exit(1);
  } finally {
    await client.end();
  }
}

testConnection();
