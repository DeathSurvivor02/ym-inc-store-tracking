namespace YMInc.StoreDeploymentApp.Models
{
    public enum UserRole
    {
        PM,     // Project Manager (Lead Deployment)
        PA,     // Project Administrator (Compliance, Badging, Ingestion)
        PS1,    // Project Specialist 1
        PS2,    // Project Specialist 2
        PS3,    // Project Specialist 3
        PS4     // Project Specialist 4
    }

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public string RoleDisplayName => Role switch
        {
            UserRole.PM => "PM — Project Manager (Lead)",
            UserRole.PA => "PA — Project Administrator",
            UserRole.PS1 => "PS 1 — Project Specialist",
            UserRole.PS2 => "PS 2 — Project Specialist",
            UserRole.PS3 => "PS 3 — Project Specialist",
            UserRole.PS4 => "PS 4 — Project Specialist",
            _ => Role.ToString()
        };

        public string RoleBadgeColor => Role switch
        {
            UserRole.PM => "#3B82F6",  // Blue
            UserRole.PA => "#8B5CF6",  // Purple
            _ => "#10B981"             // Emerald for PS 1..4
        };

        // Internal team permissions
        public bool CanApproveMilestones => Role is UserRole.PM;
        public bool CanImportRoster => Role is UserRole.PM or UserRole.PA;
        public bool CanManageCompliance => Role is UserRole.PM or UserRole.PA;
        public bool CanUpdateInfrastructure => Role is UserRole.PM or UserRole.PS1 or UserRole.PS2 or UserRole.PS3 or UserRole.PS4;
        public bool CanUpdateCircuits => Role is UserRole.PM or UserRole.PS1 or UserRole.PS2 or UserRole.PS3 or UserRole.PS4;
    }
}
