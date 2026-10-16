const fs = require('fs');
const xlsx = require('xlsx');
const { Client } = require('pg');
require('dotenv').config();

async function importVerifiedSurveys() {
    const client = new Client({ connectionString: process.env.DATABASE_URL });
    await client.connect();
    console.log('Connected to Neon PostgreSQL.');

    // 1. Load Stores from DB
    const dbStoresRes = await client.query('SELECT store_id, store_number, banner_id FROM stores;');
    const storeMap = new Map();
    for (const row of dbStoresRes.rows) {
        storeMap.set(row.store_number.trim(), row.store_id);
        const digits = row.store_number.replace(/\D/g, '');
        if (digits) storeMap.set(digits, row.store_id);
    }
    console.log(`Loaded ${dbStoresRes.rows.length} stores from database.`);

    // 2. Load Cabling Log for contractor / technician assignment
    const cablingLogMap = new Map();
    if (fs.existsSync('data/us_it_deployment_and_cabling_log.csv')) {
        const lines = fs.readFileSync('data/us_it_deployment_and_cabling_log.csv', 'utf8').split('\n');
        for (let i = 1; i < lines.length; i++) {
            const line = lines[i].trim();
            if (!line) continue;
            // Parse CSV columns: Date,Store,Vendor,Status,Details,Tech Name,Circuit Vendor,Calendar Source
            const parts = line.split(',');
            if (parts.length >= 6) {
                const date = parts[0].trim();
                const storeStr = parts[1].trim(); // e.g. "CR 6362" or "6362"
                const vendor = parts[2].trim();
                const status = parts[3].trim();
                const tech = parts[5].trim();
                const digits = storeStr.replace(/\D/g, '');
                if (digits) {
                    cablingLogMap.set(digits, { date, vendor, status, tech });
                }
            }
        }
    }
    console.log(`Loaded ${cablingLogMap.size} contractor assignments from us_it_deployment_and_cabling_log.csv.`);

    // 3. Read Site survey sheet from US_SNIU_Jan_2026.xlsx
    console.log('Reading Site survey sheet from data/US_SNIU_Jan_2026.xlsx...');
    const wb = xlsx.readFile('data/US_SNIU_Jan_2026.xlsx');
    const ws = wb.Sheets['Site survey'];
    const surveyRows = xlsx.utils.sheet_to_json(ws);
    console.log(`Found ${surveyRows.length} verified site survey entries in Excel.`);

    // Clean existing dummy site surveys
    await client.query('DELETE FROM site_surveys;');
    console.log('Cleared old unverified site surveys.');

    let insertedSurveys = 0;
    const handledStoreIds = new Set();

    for (const r of surveyRows) {
        const storeNumRaw = String(r['1. Store Number'] || '').trim();
        const storeDigits = storeNumRaw.replace(/\D/g, '');
        const storeId = storeMap.get(storeDigits) || storeMap.get(storeNumRaw);

        if (!storeId || handledStoreIds.has(storeId)) continue;
        handledStoreIds.add(storeId);

        // Ceiling Type
        const rawCeiling = r["7 a). If 'Yes' to Question 7, what type of ceiling does your store have?"] ||
                           r["i) Ceiling Type in the Backroom"] ||
                           r["iii) Ceiling Type in the Sales floor"] ||
                           'Tiled Ceiling';
        let ceilingType = rawCeiling.trim();

        // Ceiling Height
        const rawHeight = String(r["8 a). If 'Yes' to Question 8, what is the approximate height? (Unit of measure in feet)"] ||
                                 r["i) Backroom"] ||
                                 r["iv) Area immediately above cash desk (cash wrap)"] ||
                                 '12-14ft').trim();
        let heightFeet = 14.0;
        if (rawHeight.includes('10-15')) heightFeet = 12.5;
        else if (rawHeight.includes('15-20')) heightFeet = 17.5;
        else if (rawHeight.includes('20-25')) heightFeet = 22.5;
        else if (rawHeight.includes('25+')) heightFeet = 26.0;
        else if (rawHeight.includes('8-10')) heightFeet = 9.0;
        else {
            const parsed = parseFloat(rawHeight.replace(/[^0-9.]/g, ''));
            if (!isNaN(parsed) && parsed > 5 && parsed < 40) heightFeet = parsed;
        }

        // Status & Dates
        const flowStatus = String(r['Flow Status'] || 'COMPLETED').trim().toUpperCase();
        const surveyStatus = flowStatus === 'COMPLETED' ? 'Completed & Approved' : 'Submitted (Review Pending)';
        const submissionDateRaw = r['Submission Date'];
        let actualDate = null;
        if (submissionDateRaw) {
            const parsedDate = new Date(submissionDateRaw);
            if (!isNaN(parsedDate.getTime())) {
                actualDate = parsedDate.toISOString().split('T')[0];
            }
        }

        // Demarc Location
        const demarcInBackroom = String(r["14. Does your store have a demarcation in backroom?"] || '').trim().toLowerCase() === 'yes';
        const demarcLocation = demarcInBackroom ? 'Store Backroom (Verified)' : 'Mall Common Demarc Riser';

        // MDF Rack
        const hasRack = String(r["12. Does your store have a network rack? (Metal cabinet roughly the size of a bar fridge; see the images below)."] || '').trim().toLowerCase() === 'yes';
        const rackLocationRaw = r["12 a). If 'Yes' to Question 12, please indicate its location."];
        const mdfRackLocation = hasRack 
            ? (rackLocationRaw ? `${rackLocationRaw.trim()} Rack` : 'Store Backroom Rack') 
            : 'No Network Rack On-Site';

        // Structure Notes
        const dropPole = r["9. Is there an existing drop pole located near the cash desk (cash wrap) (see examples below)."] || 'No';
        const ladder = r["11. Does your store have a ladder?"] || 'No';
        const ladderHeight = r["11 a). If 'Yes' to Question 11, how high can it extend? Please refer to any measurement stickers on the ladder for details."] || '';
        const stingray = r["15. Where is the Stingray device (amplifier for the store's music) located?"] || 'Manager’s Office';
        const washroom = r["17. Are the washroom/s located inside or outside of the store?"] || 'Inside';
        const washroomProcess = r["17 a). If the washroom is located outside of the store, please indicate the process for access after-hours."] || '';

        const structureNotes = `Verified Survey Submission (${actualDate || '2026'}). Drop Pole: ${dropPole}; Network Rack: ${hasRack ? 'Present' : 'None'}; Music/Stingray: ${stingray}; Ladder: ${ladder} ${ladderHeight}; Washroom: ${washroom} ${washroomProcess}`.trim();

        // Vendor / Technician assignment from cabling log
        const assignment = cablingLogMap.get(storeDigits) || {};
        const surveyVendor = assignment.vendor ? `${assignment.vendor} Field Services` : 'Store Manager Survey (Field Verified)';
        const surveyorName = assignment.tech ? assignment.tech.split(' ')[0] + ' ' + (assignment.tech.split(' ')[1] || '') : 'Store Operations Team';
        const surveyorPhone = assignment.tech && assignment.tech.match(/\d{3}[-.]?\d{3}[-.]?\d{4}/) ? assignment.tech.match(/\d{3}[-.]?\d{3}[-.]?\d{4}/)[0] : null;

        await client.query(`
            INSERT INTO site_surveys (
                store_id, survey_vendor, surveyor_name, surveyor_phone, 
                scheduled_date, actual_survey_date, survey_status,
                ceiling_type, ceiling_height_feet, ceiling_structure_notes,
                demarc_location, demarc_conduit_available,
                mdf_rack_location, mdf_rack_space_adequate, dedicated_circuits_count
            ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15);
        `, [
            storeId,
            surveyVendor,
            surveyorName,
            surveyorPhone,
            assignment.date || actualDate || '2026-05-01',
            actualDate || '2026-05-01',
            surveyStatus,
            `${ceilingType} (${rawHeight})`,
            heightFeet,
            structureNotes,
            demarcLocation,
            demarcInBackroom,
            mdfRackLocation,
            hasRack,
            hasRack ? 2 : 1
        ]);

        insertedSurveys++;

        // Also update power_poles table with real drop pole verification
        const existingPole = String(dropPole).toLowerCase() === 'yes';
        await client.query(`
            UPDATE power_poles
            SET 
                pole_requirement_status = $1,
                total_poles_required = $2,
                poles_installed = $3,
                electrical_contractor = $4,
                blocker_reason = $5
            WHERE store_id = $6;
        `, [
            existingPole ? 'Existing Pole Verified Present' : 'New Drop Pole Installation Required',
            existingPole ? 1 : 1,
            existingPole ? 1 : 0,
            assignment.vendor ? `${assignment.vendor} Electrical Services` : 'Pending Contractor Dispatch',
            existingPole ? null : 'Cash wrap requires Wiremold drop pole drop from deck.',
            storeId
        ]);
    }

    console.log(`Successfully imported ${insertedSurveys} verified store site surveys and updated power pole specs.`);

    // 4. For any store without a survey submitted in Excel, mark as explicitly Unverified / Pending
    for (const [storeNum, storeId] of storeMap.entries()) {
        if (!handledStoreIds.has(storeId)) {
            handledStoreIds.add(storeId);
            const digits = storeNum.replace(/\D/g, '');
            const assignment = cablingLogMap.get(digits) || {};
            await client.query(`
                INSERT INTO site_surveys (
                    store_id, survey_vendor, surveyor_name, surveyor_phone, 
                    scheduled_date, actual_survey_date, survey_status,
                    ceiling_type, ceiling_height_feet, ceiling_structure_notes,
                    demarc_location, demarc_conduit_available,
                    mdf_rack_location, mdf_rack_space_adequate, dedicated_circuits_count
                ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14, $15);
            `, [
                storeId,
                assignment.vendor ? `${assignment.vendor} Field Dispatch` : 'Pending Field Dispatch',
                assignment.tech || 'Unassigned / Not In Log',
                null,
                assignment.date || '2026-10-28',
                null,
                'Pending Field Survey',
                'Unverified (Pending Survey)',
                0.0,
                'No site survey submitted by store manager yet. Pending contractor dispatch.',
                'Unverified Demarcation',
                false,
                'Pending Verification',
                false,
                0
            ]);

            await client.query(`
                UPDATE power_poles
                SET 
                    pole_requirement_status = 'Pending Site Survey Verification',
                    poles_installed = 0,
                    electrical_contractor = $1,
                    blocker_reason = 'Pending physical store inspection'
                WHERE store_id = $2;
            `, [
                assignment.vendor ? `${assignment.vendor} Electrical Services` : 'Unassigned',
                storeId
            ]);
        }
    }

    console.log('All stores now have verified site surveys or explicit Pending verification records.');
    await client.end();
}

importVerifiedSurveys().catch(console.error);
