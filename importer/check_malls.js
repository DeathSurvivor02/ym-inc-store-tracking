require('dotenv').config();
const { Client } = require('pg');

async function checkStoreMalls() {
  const client = new Client({
    connectionString: process.env.DATABASE_URL,
    ssl: { rejectUnauthorized: false }
  });
  await client.connect();

  const totalStores = await client.query('SELECT count(*) FROM stores');
  const linkedStores = await client.query('SELECT count(*) FROM stores WHERE mall_id IS NOT NULL');
  console.log(`Total stores: ${totalStores.rows[0].count}, Linked with mall_id: ${linkedStores.rows[0].count}`);

  const sampleUnlinked = await client.query('SELECT store_id, store_number, store_name, city, state_province, mall_id FROM stores WHERE mall_id IS NULL LIMIT 5');
  console.log('Sample unlinked stores:', sampleUnlinked.rows);

  await client.end();
}

checkStoreMalls().catch(console.error);
