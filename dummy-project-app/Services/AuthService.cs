using System;
using System.Collections.Generic;
using System.Linq;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public class AuthService
    {
        private static readonly List<User> InternalTeamUsers = new()
        {
            new User
            {
                UserId = 1,
                Username = "pm",
                FullName = "Marcus Vance",
                Email = "mvance@yminc.ca",
                PasswordHash = "pm123",
                Role = UserRole.PM
            },
            new User
            {
                UserId = 2,
                Username = "pa",
                FullName = "Claire Tremblay",
                Email = "ctremblay@yminc.ca",
                PasswordHash = "pa123",
                Role = UserRole.PA
            },
            new User
            {
                UserId = 3,
                Username = "ps1",
                FullName = "Terry O'Connor",
                Email = "toconnor@yminc.ca",
                PasswordHash = "ps123",
                Role = UserRole.PS1
            },
            new User
            {
                UserId = 4,
                Username = "ps2",
                FullName = "Danielle Levy",
                Email = "dlevy@yminc.com",
                PasswordHash = "ps2123",
                Role = UserRole.PS2
            },
            new User
            {
                UserId = 5,
                Username = "ps3",
                FullName = "Hector Ruiz",
                Email = "hruiz@yminc.com",
                PasswordHash = "ps3123",
                Role = UserRole.PS3
            },
            new User
            {
                UserId = 6,
                Username = "ps4",
                FullName = "Sarah Jenkins",
                Email = "sjenkins@yminc.ca",
                PasswordHash = "ps4123",
                Role = UserRole.PS4
            }
        };

        public static User? CurrentUser { get; private set; }

        public static (bool Success, string Message, User? User) Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Please enter both username and password.", null);
            }

            var user = InternalTeamUsers.FirstOrDefault(u => 
                u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase) && 
                u.PasswordHash == password);

            if (user == null)
            {
                return (false, "Invalid credentials. Please verify your team user ID and password.", null);
            }

            CurrentUser = user;
            return (true, "Authentication successful.", user);
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static IReadOnlyList<User> GetInternalTeam() => InternalTeamUsers.AsReadOnly();
    }
}
