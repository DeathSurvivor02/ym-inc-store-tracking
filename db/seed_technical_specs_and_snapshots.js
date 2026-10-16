const { Pool } = require('pg');
require('dotenv').config();

const pool = new Pool({ connectionString: process.env.DATABASE_URL });

async function seedTechnicalSpecs() {
  const client = await pool.connect();
  try {
    await client.query('BEGIN');

    console.log('Fetching all stores...');
    const storesRes = await client.query(`
      SELECT store_id, store_number, banner_id, operational_status, overall_health, 
             country_code, gross_sqft, sales_sqft, target_open_date, opening_date
      FROM stores
      ORDER BY store_id;
    `);
    const stores = storesRes.rows;
    console.log(`Found ${stores.length} stores.`);

    const existingSurveys = new Set((await client.query('SELECT store_id FROM site_surveys')).rows.map(r => r.store_id));
    const existingPoles = new Set((await client.query('SELECT store_id FROM power_poles')).rows.map(r => r.store_id));
    const existingCabling = new Set((await client.query('SELECT store_id FROM cabling_operations')).rows.map(r => r.store_id));
    const existingCircuits = new Set((await client.query('SELECT store_id FROM circuit_details')).rows.map(r => r.store_id));

    let surveysInserted = 0;
    let polesInserted = 0;
    let cablingInserted = 0;
    let circuitsInserted = 0;

    for (const store of stores) {
      const isOpen = store.operational_status === 'Open' || store.operational_status === 'Grand Open';
      const isTelecomStaging = store.operational_status === 'IT Staging & Telecom';
      const isAtRisk = store.overall_health === 'At Risk' || store.overall_health === 'Critical Blocker';
      const isUS = store.country_code === 'US';
      const sqft = store.gross_sqft || 6000;
      const poleCount = sqft > 12000 ? 4 : (sqft > 8000 ? 3 : 2);
      const dataDrops = sqft > 12000 ? 36 : (sqft > 8000 ? 28 : 24);

      // 1. Site Surveys
      if (!existingSurveys.has(store.store_id)) {
        const vendor = isUS ? 'National Retail Surveyors LLC' : 'Apex Telecom Surveyors Canada';
        const status = 'Completed & Approved';
        const ceiling = sqft > 10000 ? 'Open Plenum / Exposed Deck' : 'Drop Ceiling (2x2 Acoustic Tiles)';
        const height = sqft > 10000 ? 16.5 : 13.0;

        await client.query(`
          INSERT INTO site_surveys (
            store_id, survey_vendor, surveyor_name, surveyor_phone, scheduled_date, 
            actual_survey_date, survey_status, ceiling_type, ceiling_height_feet, 
            demarc_location, demarc_conduit_available, mdf_rack_location, 
            mdf_rack_space_adequate, power_outlet_ready_for_rack, dedicated_circuits_count, 
            existing_cabling_reuse_viable, wireless_ap_mounting_viable, photo_repository_url
          ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16, $17, $18);
        `, [
          store.store_id,
          vendor,
          isUS ? 'Dave Miller' : 'Jean-Luc Tremblay',
          isUS ? '555-214-8831' : '416-555-0198',
          '2026-08-15',
          '2026-08-16',
          status,
          ceiling,
          height,
          'Mall Demarc Riser Closet B',
          true,
          'Back Office Cash Wrap IDF',
          true,
          true,
          2,
          false,
          true,
          'https://photos.yminc.com/surveys/' + store.store_number
        ]);
        surveysInserted++;
      }

      // 2. Power Poles
      if (!existingPoles.has(store.store_id)) {
        let poleStatus = 'Passed Electrical Inspection';
        let installed = poleCount;
        let blocker = null;

        if (!isOpen) {
          if (isTelecomStaging) {
            poleStatus = 'Passed Electrical Inspection';
            installed = poleCount;
          } else if (isAtRisk) {
            poleStatus = 'Deficiency Hold';
            installed = 0;
            blocker = 'Landlord requires union certified electrician for tie-wire ceiling anchor';
          } else {
            poleStatus = 'Installed & Anchored';
            installed = Math.max(0, poleCount - 1);
          }
        }

        await client.query(`
          INSERT INTO power_poles (
            store_id, pole_requirement_status, total_poles_required, poles_installed,
            pole_manufacturer, pole_model_number, finish_color, ceiling_mount_type,
            electrical_contractor, electrician_permit_number, dual_channel_isolated,
            cash_wrap_feed_drops, scheduled_install_date, actual_install_date, blocker_reason
          ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15);
        `, [
          store.store_id,
          poleStatus,
          poleCount,
          installed,
          'Wiremold / Legrand',
          '25DTP-4',
          'Anodized Aluminum',
          'T-Bar Grid Clamp with Structural Tie-Wire',
          isUS ? 'Commercial Electric Solutions' : 'Guild Electric Ltd.',
          'ELEC-PERMIT-' + store.store_number,
          true,
          4,
          '2026-09-10',
          isOpen || isTelecomStaging ? '2026-09-18' : null,
          blocker
        ]);
        polesInserted++;
      }

      // 3. Cabling Operations
      if (!existingCabling.has(store.store_id)) {
        let cablingStatus = 'Completed & Certified';
        let flukeCertified = true;

        if (!isOpen) {
          if (isTelecomStaging) {
            cablingStatus = 'Completed & Certified';
            flukeCertified = true;
          } else if (isAtRisk) {
            cablingStatus = 'Delayed / Access Blocked';
            flukeCertified = false;
          } else {
            cablingStatus = 'Cable Pull In Progress';
            flukeCertified = false;
          }
        }

        await client.query(`
          INSERT INTO cabling_operations (
            store_id, cabling_vendor, lead_technician_name, operational_status,
            cable_grade, total_data_drops, pos_drops, ap_wireless_drops, back_office_drops,
            cctv_camera_drops, patch_panel_type, racks_installed_count, fluke_test_certified,
            scheduled_start_date, scheduled_completion_date, actual_completion_date
          ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15, $16);
        `, [
          store.store_id,
          isUS ? 'Granite Telecommunications' : 'Black Box Network Services',
          isUS ? 'Hector Ruiz' : 'Terry O\'Connor',
          cablingStatus,
          'Cat6 Plenum (CMP)',
          dataDrops,
          6,
          4,
          4,
          Math.max(6, dataDrops - 14),
          '24-Port Cat6 RJ45 Unloaded Keystone',
          1,
          flukeCertified,
          '2026-09-20',
          '2026-09-28',
          flukeCertified ? '2026-09-27' : null
        ]);
        cablingInserted++;
      }

      // 4. Circuit Details (Primary WAN + Secondary WAN)
      if (!existingCircuits.has(store.store_id)) {
        const primaryCarrier = isUS ? 'Comcast Business' : 'Bell Canada';
        const secondaryCarrier = isUS ? 'AT&T Commercial' : 'Telus';
        const primaryStatus = isOpen || isTelecomStaging ? 'Tested & Turned Up (Active)' : (isAtRisk ? 'Carrier Delay / Jeopardy' : 'Dispatched / In Construction');
        const secondaryStatus = isOpen || isTelecomStaging ? 'Tested & Turned Up (Active)' : 'Tested & Turned Up (Active)';

        // Primary DIA Fiber
        await client.query(`
          INSERT INTO circuit_details (
            store_id, circuit_role, carrier_name, service_type, order_status,
            bandwidth_down_mbps, bandwidth_up_mbps, demarc_extended, primary_dns, secondary_dns
          ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10);
        `, [
          store.store_id,
          'Primary WAN',
          primaryCarrier,
          'Commercial Fiber DIA',
          primaryStatus,
          200,
          200,
          true,
          '8.8.8.8',
          '1.1.1.1'
        ]);

        // Secondary Failover Wireless LTE/5G
        await client.query(`
          INSERT INTO circuit_details (
            store_id, circuit_role, carrier_name, service_type, order_status,
            bandwidth_down_mbps, bandwidth_up_mbps, demarc_extended, primary_dns, secondary_dns
          ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10);
        `, [
          store.store_id,
          'Secondary Failover WAN',
          secondaryCarrier,
          'Enterprise 5G / LTE Wireless',
          secondaryStatus,
          100,
          25,
          true,
          '8.8.8.8',
          '1.1.1.1'
        ]);

        circuitsInserted += 2;
      }
    }

    // 5. Populate weekly_store_snapshots for Week 41 and Week 42
    await client.query('DELETE FROM weekly_store_snapshots WHERE fiscal_year = 2026 AND fiscal_week IN (41, 42);');
    console.log('Inserting weekly_store_snapshots for Week 41 and Week 42...');

    for (const store of stores) {
      const isOpen = store.operational_status === 'Open' || store.operational_status === 'Grand Open';
      const isTelecomStaging = store.operational_status === 'IT Staging & Telecom';
      const isAtRisk = store.overall_health === 'At Risk' || store.overall_health === 'Critical Blocker';

      // Week 41 snapshot
      let statusWk41 = 'Completed & Certified';
      let healthWk41 = 'On Track';
      if (!isOpen) {
        if (isAtRisk) {
          statusWk41 = 'Blocked / In Jeopardy';
          healthWk41 = 'At Risk';
        } else if (isTelecomStaging) {
          statusWk41 = 'In Progress / On Schedule';
        } else {
          statusWk41 = 'In Progress / On Schedule';
        }
      }

      await client.query(`
        INSERT INTO weekly_store_snapshots (
          store_id, snapshot_date, fiscal_year, fiscal_week, operational_status, overall_health,
          site_survey_status, power_pole_status, cabling_status, primary_circuit_status, pos_readiness_status,
          primary_blocker_category, days_to_target_opening, reported_by
        ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14);
      `, [
        store.store_id,
        '2026-10-17',
        2026,
        41,
        store.operational_status,
        healthWk41,
        'Completed & Approved',
        isOpen ? 'Passed Electrical Inspection' : 'Installed & Anchored',
        isOpen ? 'Completed & Certified' : 'Cable Pull In Progress',
        isOpen ? 'Tested & Turned Up (Active)' : 'Dispatched / In Construction',
        isOpen ? 'Commissioned' : 'Configured',
        isAtRisk ? 'Union Labor Dispute' : 'None',
        isOpen ? 0 : 30,
        'Automated Deployment Engine'
      ]);

      // Week 42 snapshot (progress advances)
      let statusWk42 = statusWk41;
      let healthWk42 = healthWk41;

      await client.query(`
        INSERT INTO weekly_store_snapshots (
          store_id, snapshot_date, fiscal_year, fiscal_week, operational_status, overall_health,
          site_survey_status, power_pole_status, cabling_status, primary_circuit_status, pos_readiness_status,
          primary_blocker_category, days_to_target_opening, reported_by
        ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14);
      `, [
        store.store_id,
        '2026-10-24',
        2026,
        42,
        store.operational_status,
        healthWk42,
        'Completed & Approved',
        isOpen ? 'Passed Electrical Inspection' : 'Passed Electrical Inspection',
        isOpen ? 'Completed & Certified' : 'Completed & Certified',
        isOpen ? 'Tested & Turned Up (Active)' : 'Tested & Turned Up (Active)',
        isOpen ? 'Commissioned' : 'Commissioned',
        isAtRisk ? 'Union Labor Dispute' : 'None',
        isOpen ? 0 : 23,
        'Automated Deployment Engine'
      ]);
    }

    await client.query('COMMIT');
    console.log(`Success! Inserted:
      - Surveys: ${surveysInserted}
      - Power Poles: ${polesInserted}
      - Cabling Ops: ${cablingInserted}
      - Circuits: ${circuitsInserted}
      - Snapshots for 376 stores in Wk 41 & Wk 42 (${stores.length * 2} snapshots).
    `);
  } catch (err) {
    await client.query('ROLLBACK');
    console.error('Error seeding technical specs:', err);
  } finally {
    client.release();
    await pool.end();
  }
}

seedTechnicalSpecs();
