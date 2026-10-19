const { Client } = require('pg');
require('dotenv').config();

async function syncMallContactedAndReqs() {
    const client = new Client({ connectionString: process.env.DATABASE_URL });
    await client.connect();
    console.log('Connected to Neon PostgreSQL.');

    await client.query(`
        ALTER TABLE mall_properties ADD COLUMN IF NOT EXISTS mall_contacted BOOLEAN DEFAULT false;
        ALTER TABLE mall_properties ADD COLUMN IF NOT EXISTS has_requirements BOOLEAN DEFAULT false;
        ALTER TABLE mall_properties ADD COLUMN IF NOT EXISTS assigned_team_member VARCHAR(100) DEFAULT '';
        ALTER TABLE mall_properties ADD COLUMN IF NOT EXISTS requirements_circuits TEXT DEFAULT '';
        ALTER TABLE mall_properties ADD COLUMN IF NOT EXISTS intermediate_steps TEXT DEFAULT '';
    `);

    // Sync from mall_requirements
    await client.query(`
        UPDATE mall_properties mp
        SET 
            mall_contacted = COALESCE(mr.mall_contacted, false),
            assigned_team_member = COALESCE(mr.assigned_team_member, ''),
            special_cabling_rules = COALESCE(mr.requirements_ttu_cabling, ''),
            requirements_circuits = COALESCE(mr.requirements_circuits, ''),
            intermediate_steps = TRIM(BOTH ' | ' FROM COALESCE(mr.intermediate_steps, '') || CASE WHEN COALESCE(mr.notes, '') <> '' THEN ' | ' || mr.notes ELSE '' END),
            has_requirements = (COALESCE(TRIM(mr.requirements_ttu_cabling), '') <> '' OR COALESCE(TRIM(mr.requirements_circuits), '') <> '')
        FROM (
            SELECT DISTINCT ON (mall_id) 
                mall_id, mall_contacted, assigned_team_member, 
                requirements_ttu_cabling, requirements_circuits, intermediate_steps, notes
            FROM mall_requirements
            WHERE mall_id IS NOT NULL
            ORDER BY mall_id, (COALESCE(TRIM(requirements_ttu_cabling), '') <> '') DESC, mall_contacted DESC
        ) mr
        WHERE mp.mall_id = mr.mall_id;
    `);

    // For Canadian malls not in US mall_requirements
    await client.query(`
        UPDATE mall_properties
        SET 
            mall_contacted = true,
            has_requirements = (COALESCE(TRIM(special_cabling_rules), '') <> '')
        WHERE country_code = 'CA';
    `);

    const stats = await client.query(`
        SELECT 
            COUNT(*) as total,
            COUNT(*) FILTER (WHERE mall_contacted = true) as contacted,
            COUNT(*) FILTER (WHERE mall_contacted = false) as not_contacted,
            COUNT(*) FILTER (WHERE has_requirements = true) as has_reqs,
            COUNT(*) FILTER (WHERE has_requirements = false) as no_reqs,
            COUNT(*) FILTER (WHERE mall_contacted = true AND has_requirements = false) as contacted_no_reqs,
            COUNT(*) FILTER (WHERE mall_contacted = false AND has_requirements = false) as not_contacted_no_reqs
        FROM mall_properties;
    `);
    console.log('Updated mall_properties stats:', stats.rows[0]);
    await client.end();
}

syncMallContactedAndReqs().catch(console.error);
