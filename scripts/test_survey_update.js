const { Client } = require('pg');
require('dotenv').config();

async function main() {
  const client = new Client({ connectionString: process.env.DATABASE_URL, ssl: { rejectUnauthorized: false } });
  await client.connect();

  try {
    const res = await client.query(`
      UPDATE site_surveys
      SET 
          ceiling_type = $1,
          ceiling_height_feet = $2,
          survey_status = $3,
          demarc_location = $4,
          demarc_conduit_available = $5,
          mdf_rack_location = $6,
          surveyor_name = $7,
          ceiling_structure_notes = $8
      WHERE store_id = $9 RETURNING *;
    `, ['Unverified (Pending Survey)', 0, 'Pending Field Survey', 'Unverified Demarcation', false, 'Pending Verification', 'Unassigned / Not In Log (Pending Field Dispatch)', 'No site survey submitted by store manager yet. Pending contractor dispatch. bkjbkjbjk', 12]);

    console.log('Update result rowCount:', res.rowCount);
  } catch (err) {
    console.error('Update error:', err);
  } finally {
    await client.end();
  }
}

main();
