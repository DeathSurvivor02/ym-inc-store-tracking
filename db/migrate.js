/**
 * YM Inc. Store Tracking Project: Universal PostgreSQL Migration & Seed Runner
 * Usage:
 *   node db/migrate.js [--seeds] [--schema-only]
 */

const fs = require('fs');
const path = require('path');
const { Client } = require('pg');
require('dotenv').config();

const DATABASE_URL = process.env.DATABASE_URL;

if (!DATABASE_URL) {
  console.error('[ERROR] DATABASE_URL is not set in .env. Please configure your database connection string.');
  process.exit(1);
}

const includeSeeds = process.argv.includes('--seeds') || process.argv.includes('-s');

async function run() {
  console.log('==================================================================');
  console.log('  YM Inc. Store Tracking Project: Database Migration Engine       ');
  console.log('==================================================================');

  // Obfuscate credentials when printing
  const sanitizedUrl = DATABASE_URL.replace(/:\/\/([^:]+):([^@]+)@/, '://$1:****@');
  console.log(`Connecting to: ${sanitizedUrl}`);

  // Use SSL if connecting to remote host (non-localhost)
  const isLocal = DATABASE_URL.includes('localhost') || DATABASE_URL.includes('127.0.0.1');
  const clientConfig = {
    connectionString: DATABASE_URL,
    ssl: isLocal ? false : { rejectUnauthorized: false }
  };

  const client = new Client(clientConfig);

  try {
    await client.connect();
    console.log('[OK] Successfully connected to PostgreSQL instance.');

    const migrationFiles = [
      '001_management_hierarchy.sql',
      '002_stores_and_lifecycle.sql',
      '003_analytics_and_facts.sql',
      '004_indexes.sql',
      '005_views.sql',
      '006_app_users_auth.sql'
    ];

    for (const file of migrationFiles) {
      const fullPath = path.join(__dirname, 'migrations', file);
      if (!fs.existsSync(fullPath)) {
        throw new Error(`Migration file not found: ${fullPath}`);
      }
      console.log(`\n--> Applying migration: ${file}...`);
      const sql = fs.readFileSync(fullPath, 'utf8');
      await client.query(sql);
      console.log(`    [PASS] ${file} executed successfully.`);
    }

    if (includeSeeds) {
      const seedFile = path.join(__dirname, 'seeds', '01_master_seeds.sql');
      if (fs.existsSync(seedFile)) {
        console.log(`\n--> Seeding database with master records: 01_master_seeds.sql...`);
        const seedSql = fs.readFileSync(seedFile, 'utf8');
        await client.query(seedSql);
        console.log(`    [PASS] Master seeds loaded successfully.`);
      }
    }

    // Quick verification query
    console.log('\n==================================================================');
    console.log('  Verification Summary:');
    console.log('==================================================================');
    try {
      const banners = await client.query('SELECT COUNT(*) FROM banners;');
      const stores = await client.query('SELECT COUNT(*) FROM stores;');
      const regions = await client.query('SELECT COUNT(*) FROM regions;');
      const districts = await client.query('SELECT COUNT(*) FROM districts;');
      console.log(`  Banners in DB:   ${banners.rows[0].count}`);
      console.log(`  Regions in DB:   ${regions.rows[0].count}`);
      console.log(`  Districts in DB: ${districts.rows[0].count}`);
      console.log(`  Stores in DB:    ${stores.rows[0].count}`);
    } catch (err) {
      console.log(`  (Could not fetch row counts: ${err.message})`);
    }

    console.log('\n[SUCCESS] All database migrations completed successfully!\n');
  } catch (err) {
    console.error('\n[ERROR] Migration failed:', err.message);
    if (err.position) {
      console.error(`Position: ${err.position}`);
    }
    process.exit(1);
  } finally {
    await client.end();
  }
}

run();
