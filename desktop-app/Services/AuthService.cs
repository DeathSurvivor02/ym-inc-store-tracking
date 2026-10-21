using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public class AuthService
    {
        private static readonly DatabaseService _dbService = new();

        // Local offline fallback team users (pre-hashed with Salt & Pepper)
        private static readonly List<User> InternalTeamUsers = new();

        static AuthService()
        {
            InitializeOfflineTeam();
        }

        private static void InitializeOfflineTeam()
        {
            var rawUsers = new[]
            {
                (1, "pm", "Marcus Vance", "mvance@yminc.ca", "pm123", UserRole.PM),
                (2, "pa", "Claire Tremblay", "ctremblay@yminc.ca", "pa123", UserRole.PA),
                (3, "ps1", "Terry O'Connor", "toconnor@yminc.ca", "ps123", UserRole.PS1),
                (4, "ps2", "Danielle Levy", "dlevy@yminc.com", "ps2123", UserRole.PS2),
                (5, "ps3", "Hector Ruiz", "hruiz@yminc.com", "ps3123", UserRole.PS3),
                (6, "ps4", "Sarah Jenkins", "sjenkins@yminc.ca", "ps4123", UserRole.PS4)
            };

            foreach (var (id, username, name, email, pass, role) in rawUsers)
            {
                PasswordSecurityService.HashNewPassword(pass, out string salt, out string hash);
                InternalTeamUsers.Add(new User
                {
                    UserId = id,
                    Username = username,
                    FullName = name,
                    Email = email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Iterations = 100_000,
                    IsActive = true,
                    Role = role
                });
            }
        }

        public static User? CurrentUser { get; private set; }

        /// <summary>
        /// Asynchronously authenticates a user using database-backed Salt and Pepper verification.
        /// Falls back to secure local cache if PostgreSQL database is unreachable.
        /// </summary>
        public static async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Please enter both username and password.", null);
            }

            string cleanUsername = username.Trim();

            // 1. Attempt database-backed Salt & Pepper authentication
            try
            {
                var dbUser = await _dbService.GetUserByUsernameOrEmailAsync(cleanUsername);
                if (dbUser != null)
                {
                    if (!dbUser.IsActive)
                    {
                        return (false, "This account is inactive or disabled. Please contact your administrator.", null);
                    }

                    bool passwordValid = PasswordSecurityService.VerifyPassword(
                        password, 
                        dbUser.PasswordHash, 
                        dbUser.PasswordSalt, 
                        dbUser.Iterations);

                    if (passwordValid)
                    {
                        _ = _dbService.RecordUserLoginAsync(dbUser.UserId);
                        CurrentUser = dbUser;
                        return (true, "Authentication successful (Verified via Neon PostgreSQL).", dbUser);
                    }
                    else
                    {
                        _ = _dbService.RecordFailedLoginAttemptAsync(cleanUsername);
                        return (false, "Invalid credentials. Please verify your username and password.", null);
                    }
                }
            }
            catch
            {
                // Database query failed; fall through to offline cache
            }

            // 2. Offline fallback authentication using Salt & Pepper
            var localUser = InternalTeamUsers.FirstOrDefault(u =>
                u.Username.Equals(cleanUsername, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Equals(cleanUsername, StringComparison.OrdinalIgnoreCase));

            if (localUser != null)
            {
                bool localValid = PasswordSecurityService.VerifyPassword(
                    password,
                    localUser.PasswordHash,
                    localUser.PasswordSalt,
                    localUser.Iterations);

                if (localValid)
                {
                    CurrentUser = localUser;
                    return (true, "Authentication successful (Offline Mode).", localUser);
                }
            }

            return (false, "Invalid credentials. Please verify your team user ID and password.", null);
        }

        /// <summary>
        /// Synchronous wrapper for LoginAsync for legacy call-sites.
        /// </summary>
        public static (bool Success, string Message, User? User) Login(string username, string password)
        {
            try
            {
                return Task.Run(() => LoginAsync(username, password)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                return (false, $"Authentication error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Resets a user's password in the database using Salt and Pepper cryptography.
        /// </summary>
        public static async Task<(bool Success, string Message)> ResetPasswordAsync(string usernameOrEmail, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
            {
                return (false, "Please enter your username or registered corporate email.");
            }

            if (!PasswordSecurityService.ValidatePassword(newPassword, out var validationError))
            {
                return (false, validationError);
            }

            // 1. Attempt database password reset
            try
            {
                var result = await _dbService.ResetUserPasswordAsync(usernameOrEmail, newPassword);
                if (result.Success)
                {
                    // Update local offline cache in case offline mode is used later
                    UpdateLocalPasswordCache(usernameOrEmail, newPassword);
                    return result;
                }
            }
            catch (Exception ex)
            {
                // If DB failed, attempt offline cache if DB is offline
                if (UpdateLocalPasswordCache(usernameOrEmail, newPassword))
                {
                    return (true, "Password updated in local cache (Offline Mode).");
                }
                return (false, $"Failed to reset password: {ex.Message}");
            }

            // If DB returned not found, check offline cache
            if (UpdateLocalPasswordCache(usernameOrEmail, newPassword))
            {
                return (true, "Password reset successfully in local cache.");
            }

            return (false, "No account was found matching the username or email provided.");
        }

        private static bool UpdateLocalPasswordCache(string usernameOrEmail, string newPassword)
        {
            var user = InternalTeamUsers.FirstOrDefault(u =>
                u.Username.Equals(usernameOrEmail.Trim(), StringComparison.OrdinalIgnoreCase) ||
                u.Email.Equals(usernameOrEmail.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                PasswordSecurityService.HashNewPassword(newPassword, out string salt, out string hash);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
                return true;
            }

            return false;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static IReadOnlyList<User> GetInternalTeam() => InternalTeamUsers.AsReadOnly();
    }
}
