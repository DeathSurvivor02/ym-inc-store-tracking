require('dotenv').config();
const { Client } = require('pg');

async function syncMallProperties() {
  const client = new Client({
    connectionString: process.env.DATABASE_URL,
    ssl: { rejectUnauthorized: false }
  });
  await client.connect();
  console.log('Connected to Neon PostgreSQL.');

  // 1. Add columns to mall_properties if they do not exist
  await client.query(`
    ALTER TABLE mall_properties 
    ADD COLUMN IF NOT EXISTS coi_required BOOLEAN DEFAULT TRUE,
    ADD COLUMN IF NOT EXISTS min_general_liability_cad DECIMAL(12,2) DEFAULT 5000000.00,
    ADD COLUMN IF NOT EXISTS delivery_window VARCHAR(100) DEFAULT '07:00 - 10:00',
    ADD COLUMN IF NOT EXISTS night_work_only BOOLEAN DEFAULT FALSE,
    ADD COLUMN IF NOT EXISTS badging_fee_per_tech DECIMAL(8,2) DEFAULT 25.00;
  `);
  console.log('✓ Added columns to mall_properties.');

  // 2. Populate mall_properties from mall_requirements
  const updateRes = await client.query(`
    UPDATE mall_properties mp
    SET 
      coi_required = COALESCE(mr.coi_required, mp.coi_required, true),
      night_work_only = COALESCE(mr.night_work_only, mp.night_work_only, false),
      delivery_window = CASE 
        WHEN mr.work_hours_restriction IS NOT NULL AND mr.work_hours_restriction != '' THEN mr.work_hours_restriction
        WHEN mr.night_work_only = true THEN '20:30 - 08:00 (After Hours)'
        ELSE COALESCE(mp.delivery_window, '07:00 - 10:00')
      END,
      special_cabling_rules = CASE 
        WHEN mr.requirements_ttu_cabling IS NOT NULL AND mr.requirements_ttu_cabling != '' THEN mr.requirements_ttu_cabling
        ELSE mp.special_cabling_rules
      END,
      management_contact_name = CASE 
        WHEN mr.contact_name IS NOT NULL AND mr.contact_name != '' THEN mr.contact_name
        ELSE mp.management_contact_name
      END,
      management_email = CASE 
        WHEN mr.email_address IS NOT NULL AND mr.email_address != '' THEN mr.email_address
        ELSE mp.management_email
      END,
      management_phone = CASE 
        WHEN mr.phone IS NOT NULL AND mr.phone != '' THEN mr.phone
        ELSE mp.management_phone
      END,
      security_dispatch_phone = CASE 
        WHEN mr.security_phone IS NOT NULL AND mr.security_phone != '' THEN mr.security_phone
        ELSE mp.security_dispatch_phone
      END
    FROM (
      SELECT DISTINCT ON (mall_id) *
      FROM mall_requirements
      WHERE mall_id IS NOT NULL
      ORDER BY mall_id, requirement_id
    ) mr
    WHERE mp.mall_id = mr.mall_id;
  `);
  console.log(`✓ Synchronized ${updateRes.rowCount} mall properties with requirements data.`);

  // 3. Verify count
  const verify = await client.query(`
    SELECT count(*) as total,
           count(management_contact_name) filter (where management_contact_name is not null and management_contact_name != '') as with_contact,
           count(special_cabling_rules) filter (where special_cabling_rules is not null and special_cabling_rules != '') as with_rules
    FROM mall_properties;
  `);
  console.log('Verification stats:', verify.rows[0]);

  await client.end();
}

syncMallProperties().catch(err => {
  console.error('Migration error:', err);
  process.exit(1);
});
