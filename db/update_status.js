const { Client } = require('pg');
require('dotenv').config();

const client = new Client({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }
});

async function run() {
  await client.connect();
  console.log('Connected to Neon database.');

  console.log('Altering constraint stores_operational_status_check...');
  await client.query('ALTER TABLE stores DROP CONSTRAINT IF EXISTS stores_operational_status_check;');
  await client.query(`
    ALTER TABLE stores ADD CONSTRAINT stores_operational_status_check CHECK (operational_status IN (
      'Planning', 'Lease Execution', 'Site Survey', 'Permitting & Design',
      'GC Construction', 'Low-Voltage & Power Poles', 'IT Staging & Telecom',
      'Merchandising', 'Pre-Opening QA', 'Open', 'Closed', 'Grand Open', 'Temporary Closed', 
      'Renovating', 'Decommissioned'
    ));
  `);

  await client.query("ALTER TABLE stores ALTER COLUMN operational_status SET DEFAULT 'Open';");

  const u1 = await client.query("UPDATE stores SET operational_status = 'Open' WHERE operational_status IN ('Grand Open', 'Open');");
  const u2 = await client.query("UPDATE stores SET operational_status = 'Closed' WHERE operational_status IN ('Temporary Closed', 'Decommissioned', 'Closed');");

  console.log(`Updated to Open: ${u1.rowCount} rows`);
  console.log(`Updated to Closed: ${u2.rowCount} rows`);

  const check = await client.query('SELECT operational_status, count(*) FROM stores GROUP BY operational_status ORDER BY count DESC;');
  console.table(check.rows);

  await client.end();
}

run().catch(err => {
  console.error(err);
  process.exit(1);
});
