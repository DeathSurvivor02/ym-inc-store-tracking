# CHANGELOG

All notable changes to the YM Inc. Store Tracking & Deployment Software will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [0.1.0-alpha] - 2026-10-21

### Added
- **Application Authentication & User Management**:
  - Migration `006_app_users_auth.sql` establishing `app_users`, `app_user_roles`, `app_user_sessions`, and `app_password_reset_tokens`.
  - Secure password hashing using PBKDF2 (SHA-512 with salt and 100,000 iterations via `PasswordSecurityService.cs`).
  - Account lockout protection, temporary password expiration, and forced password resets upon first login.
  - Dedicated interactive `ResetPasswordWindow.xaml` with security policy feedback and real-time validation.
  - Seed script `seed_app_users.js` with default accounts for Admin, Project Manager, IT Field Engineer, Operations, and Read-Only roles.
- **Enterprise Release & Versioning Infrastructure**:
  - GitHub Actions automated CI/CD workflow (`.github/workflows/release.yml`) for building, packaging, and publishing Windows executable artifacts upon version tag pushes (`v*.*.*`).
  - Automated version tagging helper script `scripts/tag_version.ps1` for easy iterative tagging and GitHub releases.
  - Comprehensive documentation in `docs/` detailing architecture, security protocols, and operational playbooks.

### Changed
- Standardized `desktop-app/YMInc.StoreDeploymentApp.csproj` to version `0.1.0-alpha`.
- Enhanced `AuthService.cs` and `DatabaseService.cs` to authenticate against Neon/PostgreSQL `app_users`.
- Updated migration runners (`migrate.js`, `apply_migrations.ps1`, `apply_migrations.sh`) to automatically include authentication tables and seeds.

---

## [0.0.4] - 2026-10-20
### Added
- Dynamic 4-5-4 retail fiscal calendar viewer and week selector.
- Tooltip enhancements and day card animations for scheduling.
- Dynamic theme engine with live state notifications.

## [0.0.3] - 2026-10-20
### Added
- Real-time presence service for multi-user coordination.
- Add-Store wizard with automated address verification and banner assignment.
- Activity History & Audit Logging table and listeners.

## [0.0.2] - 2026-10-19
### Added
- Standardized store IDs to YM Inc. 6-series (US stores in 6000s).
- US banner integration: Charlotte Russe (CR), rue21 (R21), and Mandee (M).
- Automated Excel roster importer and database sync tools.

## [0.0.1] - 2026-10-18
### Added
- Initial enterprise database schema, migrations, docker compose, and seeds.
- Desktop WPF application foundation with PostgreSQL/Neon database integration.
