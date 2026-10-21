const { Client } = require('pg');
require('dotenv').config();

async function main() {
  const client = new Client({ connectionString: process.env.DATABASE_URL, ssl: { rejectUnauthorized: false } });
  await client.connect();

  const storeRes = await client.query("SELECT store_id, store_number, store_name FROM stores WHERE store_number = '6001'");
  console.log('Store:', storeRes.rows);

  const surveyRes = await client.query("SELECT * FROM site_surveys WHERE store_id = $1", [storeRes.rows[0].store_id]);
  console.log('Surveys count:', surveyRes.rows.length);
  if (surveyRes.rows.length > 0) {
    console.log('Survey fields:', Object.keys(surveyRes.rows[0]));
    console.log('Survey survey_status constraint or values:', surveyRes.rows[0].survey_status);
  }

  const presenceRes = await client.query("SELECT * FROM app_user_presence");
  console.log('Presence rows count:', presenceRes.rows.length);
  console.log('Presence rows:', presenceRes.rows);

  await client.end();
}

main().catch(console.error);
