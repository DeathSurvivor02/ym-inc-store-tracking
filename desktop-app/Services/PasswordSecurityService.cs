using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YMInc.StoreDeploymentApp.Services
{
    /// <summary>
    /// Enterprise Cryptographic Security Service implementing Salt and Pepper password hashing.
    /// - Salt: Cryptographically random 32-byte sequence generated per user and stored in the database.
    /// - Pepper: High-entropy system secret stored in environment configuration (.env), never in the database.
    /// - KDF: PBKDF2 (HMAC-SHA256, 100,000 iterations) with constant-time verification.
    /// </summary>
    public static class PasswordSecurityService
    {
        private const int DefaultIterations = 100_000;
        private const int SaltSizeBytes = 32;
        private const int HashSizeBytes = 32;
        private static readonly string FallbackPepper = "YM_ENTERPRISE_PEPPER_SECURE_2026_9f8b4c2e1a7d5";

        private static string? _cachedPepper;

        /// <summary>
        /// Retrieves the system pepper from the process environment or .env file.
        /// </summary>
        public static string GetPepper()
        {
            if (_cachedPepper != null)
                return _cachedPepper;

            string? pepper = Environment.GetEnvironmentVariable("PASSWORD_PEPPER");
            if (!string.IsNullOrWhiteSpace(pepper))
            {
                _cachedPepper = pepper.Trim('"', '\'', ' ');
                return _cachedPepper;
            }

            // Search up directory hierarchy for .env file
            try
            {
                string? currentDir = AppDomain.CurrentDomain.BaseDirectory;
                for (int i = 0; i < 6 && currentDir != null; i++)
                {
                    string envFile = Path.Combine(currentDir, ".env");
                    if (File.Exists(envFile))
                    {
                        foreach (var line in File.ReadAllLines(envFile))
                        {
                            var trimmed = line.Trim();
                            if (trimmed.StartsWith("PASSWORD_PEPPER="))
                            {
                                var val = trimmed.Substring("PASSWORD_PEPPER=".Length).Trim('"', '\'', ' ');
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    _cachedPepper = val;
                                    return _cachedPepper;
                                }
                            }
                        }
                    }
                    var parent = Directory.GetParent(currentDir);
                    currentDir = parent?.FullName;
                }
            }
            catch
            {
                // Fall back gracefully
            }

            _cachedPepper = FallbackPepper;
            return _cachedPepper;
        }

        /// <summary>
        /// Generates a new cryptographically random salt and hashes a plaintext password using Salt + Pepper.
        /// </summary>
        public static void HashNewPassword(
            string password, 
            out string saltBase64, 
            out string hashBase64, 
            int iterations = DefaultIterations)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            saltBase64 = Convert.ToBase64String(saltBytes);

            string pepper = GetPepper();
            byte[] passwordPepperBytes = Encoding.UTF8.GetBytes(password + pepper);

            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                passwordPepperBytes,
                saltBytes,
                iterations,
                HashAlgorithmName.SHA256,
                HashSizeBytes);

            hashBase64 = Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifies a candidate password against the stored salt and hash using Salt + Pepper in constant time.
        /// </summary>
        public static bool VerifyPassword(
            string candidatePassword, 
            string storedHashBase64, 
            string storedSaltBase64, 
            int iterations = DefaultIterations)
        {
            if (string.IsNullOrEmpty(candidatePassword) || 
                string.IsNullOrEmpty(storedHashBase64) || 
                string.IsNullOrEmpty(storedSaltBase64))
            {
                return false;
            }

            try
            {
                byte[] saltBytes = Convert.FromBase64String(storedSaltBase64);
                byte[] expectedHashBytes = Convert.FromBase64String(storedHashBase64);

                string pepper = GetPepper();
                byte[] candidatePasswordPepperBytes = Encoding.UTF8.GetBytes(candidatePassword + pepper);

                byte[] computedHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                    candidatePasswordPepperBytes,
                    saltBytes,
                    iterations <= 0 ? DefaultIterations : iterations,
                    HashAlgorithmName.SHA256,
                    expectedHashBytes.Length);

                return CryptographicOperations.FixedTimeEquals(computedHashBytes, expectedHashBytes);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates password complexity policy.
        /// </summary>
        public static bool ValidatePassword(string password, out string failureReason)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                failureReason = "Password cannot be empty or contain only whitespace.";
                return false;
            }

            if (password.Length < 6)
            {
                failureReason = "Password must be at least 6 characters in length.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }
    }
}
