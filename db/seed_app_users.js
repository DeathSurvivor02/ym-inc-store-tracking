/**
 * YM Inc. Store Tracking Project: Seed app_users with Salt and Pepper Cryptography
 * Uses PBKDF2 (HMAC-SHA256, 100,000 iterations) with per-user 32-byte cryptographically random salt
 * and server-level PASSWORD_PEPPER secret from .env.
 */

const { Client } = require('pg');
const crypto = require('crypto');
require('dotenv').config();

const DATABASE_URL = process.env.DATABASE_URL;
const PEPPER = process.env.PASSWORD_PEPPER || 'YM_ENTERPRISE_PEPPER_SECURE_2026_9f8b4c2e1a7d5';

if (!DATABASE_URL) {
  console.error('[FAIL] DATABASE_URL is not set.');
  process.exit(1);
}

const SEED_USERS = [
  {
    username: 'pm',
    email: 'mvance@yminc.ca',
    full_name: 'Marcus Vance',
    role: 'PM',
    password: 'pm123'
  },
  {
    username: 'pa',
    email: 'ctremblay@yminc.ca',
    full_name: 'Claire Tremblay',
    role: 'PA',
    password: 'pa123'
  },
  {
    username: 'ps1',
    email: 'toconnor@yminc.ca',
    full_name: "Terry O'Connor",
    role: 'PS1',
    password: 'ps123'
  },
  {
    username: 'ps2',
    email: 'dlevy@yminc.com',
    full_name: 'Danielle Levy',
    role: 'PS2',
    password: 'ps2123'
  },
  {
    username: 'ps3',
    email: 'hruiz@yminc.com',
    full_name: 'Hector Ruiz',
    role: 'PS3',
    password: 'ps3123'
  },
  {
    username: 'ps4',
    email: 'sjenkins@yminc.ca',
    full_name: 'Sarah Jenkins',
    role: 'PS4',
    password: 'ps4123'
  }
];

function hashPassword(password, salt) {
  return crypto.pbkdf2Sync(password + PEPPER, salt, 100000, 32, 'sha256').toString('base64');
}

async function seedAppUsers() {
  console.log('Connecting to PostgreSQL database to seed app_users...');
  const isLocal = DATABASE_URL.includes('localhost') || DATABASE_URL.includes('127.0.0.1');
  const client = new Client({
    connectionString: DATABASE_URL,
    ssl: isLocal ? false : { rejectUnauthorized: false }
  });

  await client.connect();

  try {
    for (const u of SEED_USERS) {
      const saltBytes = crypto.randomBytes(32);
      const saltBase64 = saltBytes.toString('base64');
      const hashBase64 = hashPassword(u.password, saltBytes);

      const query = `
        INSERT INTO app_users (username, email, full_name, role, password_hash, password_salt, iterations, is_active)
        VALUES ($1, $2, $3, $4, $5, $6, 100000, TRUE)
        ON CONFLICT (username) DO UPDATE SET
          email = EXCLUDED.email,
          full_name = EXCLUDED.full_name,
          role = EXCLUDED.role,
          password_hash = EXCLUDED.password_hash,
          password_salt = EXCLUDED.password_salt,
          iterations = EXCLUDED.iterations,
          is_active = TRUE,
          updated_at = CURRENT_TIMESTAMP;
      `;

      await client.query(query, [u.username, u.email, u.full_name, u.role, hashBase64, saltBase64]);
      console.log(`[PASS] Seeded user '${u.username}' (${u.full_name}) with Salt & Pepper hash.`);
    }

    const countRes = await client.query('SELECT COUNT(*) FROM app_users;');
    console.log(`[SUCCESS] Total active app_users in DB: ${countRes.rows[0].count}`);
  } catch (err) {
    console.error('[ERROR] Failed to seed app_users:', err);
    process.exit(1);
  } finally {
    await client.end();
  }
}

seedAppUsers();
