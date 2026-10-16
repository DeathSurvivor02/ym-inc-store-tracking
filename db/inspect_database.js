/**
 * Inspect all tables, views, and row counts in the Neon PostgreSQL database
 */
const { Client } = require('pg');
require('dotenv').config();

const client = new Client({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }
});

async function inspect() {
  await client.connect();
  console.log('==================================================================');
  console.log('  YM Inc. Enterprise Database Inspector (Neon PostgreSQL)         ');
  console.log('==================================================================');

  // 1. Current Database info
  const dbInfo = await client.query('SELECT current_database(), current_user, version();');
  console.log(`Database: ${dbInfo.rows[0].current_database}`);
  console.log(`User:     ${dbInfo.rows[0].current_user}`);
  console.log(`Version:  ${dbInfo.rows[0].version.split(',')[0]}\n`);

  // 2. All Public Tables & Row Counts
  const tablesRes = await client.query(`
    SELECT table_name 
    FROM information_schema.tables 
    WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
    ORDER BY table_name;
  `);

  console.log(`--- BASE TABLES (${tablesRes.rows.length}) ---`);
  const tableData = [];
  for (const t of tablesRes.rows) {
    try {
      const c = await client.query(`SELECT COUNT(*) FROM "${t.table_name}";`);
      tableData.push({ Table: t.table_name, "Row Count": parseInt(c.rows[0].count) });
    } catch {
      tableData.push({ Table: t.table_name, "Row Count": 'N/A' });
    }
  }
  console.table(tableData);

  // 3. All SQL Views
  const viewsRes = await client.query(`
    SELECT table_name 
    FROM information_schema.tables 
    WHERE table_schema = 'public' AND table_type = 'VIEW'
    ORDER BY table_name;
  `);

  console.log(`\n--- ANALYTICAL SQL VIEWS (${viewsRes.rows.length}) ---`);
  const viewData = [];
  for (const v of viewsRes.rows) {
    try {
      const c = await client.query(`SELECT COUNT(*) FROM "${v.table_name}";`);
      viewData.push({ View: v.table_name, "Calculated Rows": parseInt(c.rows[0].count) });
    } catch {
      viewData.push({ View: v.table_name, "Calculated Rows": 'N/A' });
    }
  }
  console.table(viewData);

  await client.end();
}

inspect().catch(err => {
  console.error(err);
  process.exit(1);
});
