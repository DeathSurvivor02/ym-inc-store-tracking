/**
 * Generates the complete NRF 4-5-4 Retail Fiscal Calendar for YM Inc.
 * Covers Fiscal Years 2025, 2026, and 2027 (1,000+ daily calendar records).
 */
const { Client } = require('pg');
require('dotenv').config();

const client = new Client({
  connectionString: process.env.DATABASE_URL,
  ssl: { rejectUnauthorized: false }
});

// Month structure in 4-5-4 quarters:
// Q1: M1 (4 wks), M2 (5 wks), M3 (4 wks)
// Q2: M4 (4 wks), M5 (5 wks), M6 (4 wks)
// Q3: M7 (4 wks), M8 (5 wks), M9 (4 wks)
// Q4: M10 (4 wks), M11 (5 wks), M12 (4 wks)
const MONTH_WEEKS = [4, 5, 4, 4, 5, 4, 4, 5, 4, 4, 5, 4];

const FISCAL_YEAR_STARTS = [
  { year: 2025, start: new Date('2025-02-02T00:00:00Z') },
  { year: 2026, start: new Date('2026-02-01T00:00:00Z') },
  { year: 2027, start: new Date('2027-01-31T00:00:00Z') }
];

function getHoliday(dateStr) {
  const [y, m, d] = dateStr.split('-').map(Number);
  if (m === 1 && d === 1) return "New Year's Day";
  if (m === 7 && d === 1) return "Canada Day";
  if (m === 7 && d === 4) return "Independence Day (US)";
  if (m === 10 && d === 31) return "Halloween";
  if (m === 11 && d === 11) return "Remembrance Day / Veterans Day";
  if (m === 12 && d === 24) return "Christmas Eve";
  if (m === 12 && d === 25) return "Christmas Day";
  if (m === 12 && d === 26) return "Boxing Day";
  if (m === 12 && d === 31) return "New Year's Eve";

  // Floating holidays
  if (dateStr === '2025-02-17' || dateStr === '2026-02-16' || dateStr === '2027-02-15') return "Family Day (CA) / Presidents Day (US)";
  if (dateStr === '2025-04-18' || dateStr === '2026-04-03' || dateStr === '2027-03-26') return "Good Friday";
  if (dateStr === '2025-05-19' || dateStr === '2026-05-18' || dateStr === '2027-05-24') return "Victoria Day (CA)";
  if (dateStr === '2025-05-26' || dateStr === '2026-05-25' || dateStr === '2027-05-31') return "Memorial Day (US)";
  if (dateStr === '2025-08-04' || dateStr === '2026-08-03' || dateStr === '2027-08-02') return "Civic Holiday (CA)";
  if (dateStr === '2025-09-01' || dateStr === '2026-09-07' || dateStr === '2027-09-06') return "Labor Day";
  if (dateStr === '2025-10-13' || dateStr === '2026-10-12' || dateStr === '2027-10-11') return "Thanksgiving (Canada)";
  if (dateStr === '2025-11-27' || dateStr === '2026-11-26' || dateStr === '2027-11-25') return "Thanksgiving (US)";
  if (dateStr === '2025-11-28' || dateStr === '2026-11-27' || dateStr === '2027-11-26') return "Black Friday";
  if (dateStr === '2025-12-01' || dateStr === '2026-11-30' || dateStr === '2027-11-29') return "Cyber Monday";

  return null;
}

async function populateFiscalCalendar() {
  await client.connect();
  console.log('Connected to Neon database.');
  console.log('Generating complete NRF 4-5-4 Retail Fiscal Calendar (2025 - 2027)...');

  let totalInserted = 0;

  for (const fy of FISCAL_YEAR_STARTS) {
    let currentDate = new Date(fy.start);
    let fiscalWeek = 1;

    for (let monthIdx = 0; monthIdx < 12; monthIdx++) {
      const fiscalMonth = monthIdx + 1;
      const fiscalQuarter = Math.floor(monthIdx / 3) + 1;
      const weeksInMonth = MONTH_WEEKS[monthIdx];

      for (let w = 0; w < weeksInMonth; w++) {
        for (let dayOfWeek = 1; dayOfWeek <= 7; dayOfWeek++) {
          const dateStr = currentDate.toISOString().split('T')[0];
          const holiday = getHoliday(dateStr);
          const isHoliday = holiday !== null;

          const query = `
            INSERT INTO fiscal_calendar (
              calendar_date, fiscal_year, fiscal_quarter, fiscal_month,
              fiscal_week, day_of_fiscal_week, is_holiday, holiday_name
            ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
            ON CONFLICT (calendar_date) DO UPDATE SET
              fiscal_year = EXCLUDED.fiscal_year,
              fiscal_quarter = EXCLUDED.fiscal_quarter,
              fiscal_month = EXCLUDED.fiscal_month,
              fiscal_week = EXCLUDED.fiscal_week,
              day_of_fiscal_week = EXCLUDED.day_of_fiscal_week,
              is_holiday = EXCLUDED.is_holiday,
              holiday_name = EXCLUDED.holiday_name;
          `;

          await client.query(query, [
            dateStr, fy.year, fiscalQuarter, fiscalMonth,
            fiscalWeek, dayOfWeek, isHoliday, holiday
          ]);

          totalInserted++;
          // Next day
          currentDate.setUTCDate(currentDate.getUTCDate() + 1);
        }
        fiscalWeek++;
      }
    }
  }

  console.log(`Successfully generated and loaded ${totalInserted} fiscal calendar days.`);

  const summary = await client.query(`
    SELECT fiscal_year, MIN(calendar_date) as start_date, MAX(calendar_date) as end_date, COUNT(*) as days, COUNT(DISTINCT fiscal_week) as weeks
    FROM fiscal_calendar
    GROUP BY fiscal_year
    ORDER BY fiscal_year;
  `);
  console.table(summary.rows);

  const holidays = await client.query(`
    SELECT calendar_date, holiday_name 
    FROM fiscal_calendar 
    WHERE is_holiday = TRUE AND fiscal_year = 2026
    ORDER BY calendar_date;
  `);
  console.log(`\nSample Retail Holidays in Fiscal 2026 (${holidays.rows.length} total):`);
  holidays.rows.slice(0, 8).forEach(h => console.log(`  - ${h.calendar_date.toISOString().split('T')[0]}: ${h.holiday_name}`));

  await client.end();
}

populateFiscalCalendar().catch(err => {
  console.error('[ERROR]', err);
  process.exit(1);
});
