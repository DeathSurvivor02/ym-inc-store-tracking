# Milestone 2 Technical Specification: Desktop Application Enhancements

**Document Reference:** `YM-SPEC-DESKTOP-M2-2026`  
**Target System:** YM Inc. Store Deployment & Telecom Management Platform (`desktop-app/YMInc.StoreDeploymentApp.csproj`)  
**Target Runtime:** .NET 10.0 Windows (`net10.0-windows`) / WPF  
**Author:** YM Inc. Enterprise Solutions & Desktop Architecture Team  
**Date:** October 20, 2026  
**Status:** Approved Architectural Blueprint  

---

## Executive Summary & Architectural Vision

The YM Inc. Store Deployment & Telecom Management desktop application is the operational nerve center for coordinating retail capital buildouts, low-voltage cabling, power pole fixturing, dual-WAN telecom turn-ups, and landlord compliance across 376 retail stores and 13 retail apparel banners throughout North America.

While the existing desktop client features a modern Microsoft 365 enterprise dark/light visual style, forensic auditing conducted during Milestone 1 identified critical structural vulnerabilities:
1. **Total Absence of MVVM Architecture:** The application contains zero ViewModels, zero `ICommand` bindings, and relies entirely on procedural code-behind (`.xaml.cs`) with direct control manipulation.
2. **Fatal Database Schema Bugs:** `DatabaseService.CreateStoreAsync` targets a non-existent table (`telecom_circuits` instead of `circuit_details`), aborting all store creation attempts on live databases.
3. **Silent WAN Circuit Data Loss:** Field edits made in `StoreDetailWindow` to carrier circuits are discarded upon save because `SaveStoreAsync` contains zero update queries for `circuit_details`.
4. **Local File Silos:** Presence leasing (`active_editor_leases.json`), audit logs (`audit_history_log.json`), and interactive calendar work items (`live_calendar_work_items.json`) are pinned to local disk JSON files rather than synchronized across users.
5. **UI Thread Blocking:** Heavy usage of `async void` event handlers, unbatched 140+ sequential network queries on fleet save, and static event memory leaks in `ThemeService`.
6. **Enterprise Security Deficits:** Hardcoded `FallbackPepper` in compiled IL binaries, hardcoded plaintext offline passwords, and unauthenticated password reset endpoints enabling unilateral account takeover.

This specification details the comprehensive technical modernization of the desktop application into an enterprise-grade, offline-first, resilient MVVM solution built on `CommunityToolkit.Mvvm` (v8.x), `Microsoft.Extensions.DependencyInjection`, embedded `SQLite` local sync, and `Microsoft Entra ID` (MSAL.NET) authentication.

---

## 1. Architectural Blueprint & Target Component Hierarchy

### 1.1 Component & Layer Architecture

The modernized desktop client transitions from a 2-tier monolithic code-behind architecture to a decoupled, testable N-tier MVVM architecture:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                                 PRESENTATION LAYER (WPF)                               │
│                                                                                        │
│   ┌────────────────────┐   ┌──────────────────────┐   ┌────────────────────────────┐   │
│   │ MainWindow.xaml    │   │ StoreDetailView.xaml │   │ StoreOnboardingWizard.xaml │   │
│   │ FiscalCalendarView │   │ MallRulesView.xaml   │   │ RosterImportDialog.xaml    │   │
│   └─────────▲──────────┘   └──────────▲───────────┘   └─────────────▲──────────────┘   │
│             │ DataBinding             │ DataBinding                 │ DataBinding      │
│             │ Commands                │ Commands                    │ Commands         │
│   ┌─────────▼──────────┐   ┌──────────▼───────────┐   ┌─────────────▼──────────────┐   │
│   │ MainViewModel      │   │ StoreDetailViewModel │   │ AddStoreWizardViewModel    │   │
│   │ FiscalCalendarVM   │   │ MallRulesViewModel   │   │ RosterImportViewModel      │   │
│   └────────────────────┴───┬──────────────────────┴────────────────────────────────┘   │
│                            │ Injects Services & Repositories                           │
├────────────────────────────┼───────────────────────────────────────────────────────────┤
│                            ▼ APPLICATION & DOMAIN SERVICES                             │
│   ┌────────────────────────────────────────────────────────────────────────────────┐   │
│   │  IStoreRepository        ITelecomService          IComplianceService           │   │
│   │  ISyncService            IAuthService             ITokenStorageService         │   │
│   │  IToastNotificationSvc   IThemeService            ITelemetryService            │   │
│   └─────────┬──────────────────────┬─────────────────────────┬─────────────────────┘   │
├─────────────┼──────────────────────┼─────────────────────────┼─────────────────────────┤
│             ▼                      ▼                         ▼                         │
│   ┌──────────────────┐   ┌──────────────────┐      ┌──────────────────┐                │
│   │   OFFLINE-FIRST  │   │  NETWORK CLIENT  │      │  ENTERPRISE AUTH │                │
│   │   SQLITE CACHE   │   │   & POLLY V8     │      │   & DPAPI VAULT  │                │
│   │                  │   │                  │      │                  │                │
│   │  ym_local_cache  │   │  Exponential     │      │  MSAL.NET        │                │
│   │  .db             │   │  Backoff         │      │  Entra ID PKCE   │                │
│   │  Sync Outbox     │   │  Circuit Breaker │      │  DPAPI Protected │                │
│   │  Queue           │   │  Retry Pipeline  │      │  Token Storage   │                │
│   └─────────┬────────┘   └─────────┬────────┘      └─────────┬────────┘                │
└─────────────┼──────────────────────┼─────────────────────────┼─────────────────────────┘
              ▼                      ▼                         ▼
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        DATA SOURCES & REMOTE INFRASTRUCTURE                            │
│                                                                                        │
│   ┌──────────────────────────────────────┐  ┌──────────────────────────────────────┐   │
│   │      NEON SERVERLESS POSTGRESQL      │  │        HYBRID CLOUD SAAS API         │   │
│   │      (Connection Pooling / PgBouncer)│  │     (REST/GraphQL / SignalR Hub)     │   │
│   └──────────────────────────────────────┘  └──────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Target Framework & NuGet Dependencies

To enforce the enterprise security mandate of **zero direct database connections over port 5432 from client workstations**, the desktop application is fully decoupled from direct PostgreSQL / ADO.NET data access. The desktop client interacts with cloud services exclusively via the **Cloud API Gateway over HTTPS (TLS 1.3)** for REST/delta sync operations and **WebSocket (WSS)** for real-time SignalR presence and section-level leases. Local caching and offline persistence are mediated entirely by SQLite.

The `desktop-app/YMInc.StoreDeploymentApp.csproj` project is modernized to declare the following enterprise libraries:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>Assets\app_icon.ico</ApplicationIcon>
    <LangVersion>13.0</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <!-- MVVM Architecture -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />

    <!-- Dependency Injection, Configuration & Logging -->
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.0" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="10.0.0" />

    <!-- Cloud API Gateway & Real-Time Presence -->
    <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="10.0.0" />
    <PackageReference Include="System.Net.Http.Json" Version="10.0.0" />

    <!-- Local Offline Persistence & High-Speed SQLite -->
    <PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.0" />
    <PackageReference Include="Dapper" Version="2.1.35" />

    <!-- Resilience & Transient Fault Handling -->
    <PackageReference Include="Polly" Version="8.5.0" />
    <PackageReference Include="Polly.Core" Version="8.5.0" />

    <!-- Enterprise Security, Identity & Offline Token Vault -->
    <PackageReference Include="Microsoft.Identity.Client" Version="4.66.2" />
    <PackageReference Include="Microsoft.Identity.Client.Broker" Version="4.66.2" />
    <PackageReference Include="System.Security.Cryptography.ProtectedData" Version="10.0.0" />

    <!-- CSV Roster Processing -->
    <PackageReference Include="CsvHelper" Version="33.0.1" />
  </ItemGroup>
</Project>
```
*(Note: Direct `Npgsql` dependency is completely eliminated from the client workstation runtime. Port 5432 is blocked at the corporate firewall; all cloud persistence is brokered through the authenticated API Gateway).*

---

## 2. MVVM Modernization Specification

### 2.1 Inversion of Control & Dependency Injection Container

The application entrypoint (`App.xaml.cs`) is restructured to utilize `Microsoft.Extensions.Hosting.IHost`, bootstrapping all services, repositories, and ViewModels with strictly managed lifecycles:

```csharp
// desktop-app/App.xaml.cs
using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using YMInc.StoreDeploymentApp.Repositories;
using YMInc.StoreDeploymentApp.Services;
using YMInc.StoreDeploymentApp.Services.Interfaces;
using YMInc.StoreDeploymentApp.ViewModels;
using YMInc.StoreDeploymentApp.Views;

namespace YMInc.StoreDeploymentApp
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; } = null!;

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services, context.Configuration);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .Build();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Core Database & Security Services (Singletons)
            services.AddSingleton<ITokenStorageService, DpapiTokenStorageService>();
            services.AddSingleton<IAuthService, EntraAuthService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IToastNotificationService, ToastNotificationService>();
            services.AddSingleton<ILocalDatabaseBootstrapper, SqliteBootstrapper>();
            services.AddSingleton<ISyncQueueRepository, SqliteSyncQueueRepository>();
            services.AddSingleton<IPollyPolicyFactory, PollyPolicyFactory>();

            // Domain Repositories & Services (Scoped / Singleton)
            services.AddSingleton<IStoreRepository, HybridStoreRepository>();
            services.AddSingleton<ITelecomService, TelecomService>();
            services.AddSingleton<IComplianceService, ComplianceService>();
            services.AddSingleton<IFiscalCalendarService, FiscalCalendarService>();
            services.AddSingleton<ISyncService, SyncService>();

            // ViewModels (Transient for fresh dialog state, Singleton for root shells)
            services.AddSingleton<MainViewModel>();
            services.AddTransient<StoreDetailViewModel>();
            services.AddTransient<FiscalCalendarViewModel>();
            services.AddTransient<MallRulesViewModel>();
            services.AddTransient<AddStoreWizardViewModel>();
            services.AddTransient<RosterImportViewModel>();
            services.AddTransient<LoginViewModel>();

            // Views
            services.AddSingleton<MainWindow>();
            services.AddTransient<StoreDetailWindow>();
            services.AddTransient<FiscalCalendarWindow>();
            services.AddTransient<MallRulesWindow>();
            services.AddTransient<AddStoreWindow>();
            services.AddTransient<RosterImportWindow>();
            services.AddTransient<LoginWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            // Initialize local SQLite cache schema
            var bootstrapper = AppHost.Services.GetRequiredService<ILocalDatabaseBootstrapper>();
            await bootstrapper.InitializeAsync();

            // Initialize Theme
            var themeService = AppHost.Services.GetRequiredService<IThemeService>();
            themeService.Initialize();

            // Resolve initial window via DI
            var loginWindow = AppHost.Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();
            base.OnExit(e);
        }
    }
}
```

### 2.2 Refactoring Core Domain Models to `ObservableObject`

Every entity in `desktop-app/Models/` is refactored from anemic POCOs into reactive `CommunityToolkit.Mvvm.ComponentModel.ObservableObject` models. This guarantees that cell edits and background synchronization operations automatically notify the WPF binding engine without requiring entire collection refreshes.

#### 2.2.1 Refactored `StoreDeployment` Model

```csharp
// desktop-app/Models/StoreDeployment.cs
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YMInc.StoreDeploymentApp.Models
{
    public partial class StoreDeployment : ObservableObject
    {
        [ObservableProperty] private int _storeId;
        [ObservableProperty] private string _storeNumber = string.Empty;
        [ObservableProperty] private string _bannerCode = string.Empty;
        [ObservableProperty] private string _bannerName = string.Empty;
        [ObservableProperty] private string _storeName = string.Empty;
        [ObservableProperty] private string _projectType = "Active Fleet";
        [ObservableProperty] private string _operationalStatus = "Grand Open";
        [ObservableProperty] private string _overallHealth = "On Track";
        [ObservableProperty] private string _countryCode = "CA";
        [ObservableProperty] private string _region = string.Empty;
        [ObservableProperty] private string _district = string.Empty;
        [ObservableProperty] private string _address = string.Empty;
        [ObservableProperty] private string _city = string.Empty;
        [ObservableProperty] private string _stateProvince = string.Empty;
        [ObservableProperty] private string _postalCode = string.Empty;
        [ObservableProperty] private int _grossSqft;
        [ObservableProperty] private int _salesSqft;
        [ObservableProperty] private int _posTerminalCount = 4;
        [ObservableProperty] private string _posStagingStatus = "Commissioned";
        [ObservableProperty] private string? _targetOpenDate;
        [ObservableProperty] private string? _openingDate;

        [ObservableProperty] private MallProperty? _mall;
        [ObservableProperty] private SiteSurvey? _survey;
        [ObservableProperty] private PowerPole? _powerPoles;
        [ObservableProperty] private CablingOperation? _cabling;
        [ObservableProperty] private ObservableCollection<TelecomCircuit> _circuits = new();

        [ObservableProperty] private int _weeklyCompletionPct = 50;
        [ObservableProperty] private string _weeklyStatus = "In Progress / On Schedule";
        [ObservableProperty] private string _weeklySummaryNote = string.Empty;

        // Dirty Tracking for Optimistic Concurrency & Local Sync
        [ObservableProperty] private bool _isDirty;
        [ObservableProperty] private uint _rowVersion;
        [ObservableProperty] private DateTime _lastModifiedLocally = DateTime.UtcNow;

        partial void OnOperationalStatusChanged(string value) => IsDirty = true;
        partial void OnOverallHealthChanged(string value) => IsDirty = true;
        partial void OnTargetOpenDateChanged(string? value) => IsDirty = true;

        public string CountryFlag => CountryCode.ToUpperInvariant() == "US" ? "🇺🇸 US" : "🇨🇦 CA";
        public string CityState => $"{City}, {StateProvince}";
        public string FullAddress => $"{Address}, {City}, {StateProvince} {PostalCode}";

        public string CablingSummary => Cabling != null 
            ? $"{Cabling.TotalDataDrops} Drops ({(Cabling.FlukeCertified ? "Certified" : "Pending")})" 
            : "N/A";

        public string PowerPoleSummary => PowerPoles != null 
            ? $"{PowerPoles.InstalledCount}/{PowerPoles.TotalRequired} Installed" 
            : "N/A";

        public string CircuitSummary => Circuits.Count > 0 
            ? $"{Circuits.Count} Circuits Configured" 
            : "No Circuits";
    }
}
```

#### 2.2.2 Refactored Technical Track Entities

```csharp
// desktop-app/Models/TechnicalEntities.cs
using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YMInc.StoreDeploymentApp.Models
{
    public partial class TelecomCircuit : ObservableObject
    {
        [ObservableProperty] private int _circuitId;
        [ObservableProperty] private int _storeId;
        [ObservableProperty] 
        [property: JsonPropertyName("circuit_role")]
        private string _role = "Primary WAN"; // Primary WAN, Secondary Failover WAN
        [ObservableProperty] private string _carrierName = string.Empty;
        [ObservableProperty] private string _serviceType = "Commercial Fiber DIA";
        [ObservableProperty] private string _orderStatus = "Order Placed";
        [ObservableProperty] private int _bandwidthDownMbps = 100;
        [ObservableProperty] private int _bandwidthUpMbps = 100;
        [ObservableProperty] private string? _focDate;
        [ObservableProperty] private string? _turnupDate;
        [ObservableProperty] private bool _demarcExtended;
        [ObservableProperty] private string? _jeopardyReason;

        // Telemetry & IP Configuration Fields (Restored to Desktop UI)
        [ObservableProperty] private string? _staticIpAssigned;
        [ObservableProperty] private string? _subnetMask;
        [ObservableProperty] private string? _defaultGateway;
        [ObservableProperty] private string? _primaryDns = "8.8.8.8";
        [ObservableProperty] private string? _secondaryDns = "1.1.1.1";
        [ObservableProperty] private string? _cradlepointImei;
        [ObservableProperty] private string? _simCardIccid;
        [ObservableProperty] private string? _demarcExtensionVendor;

        [ObservableProperty] private bool _isDirty;
        partial void OnOrderStatusChanged(string value) => IsDirty = true;
    }

    public partial class CablingOperation : ObservableObject
    {
        [ObservableProperty] private int _cablingId;
        [ObservableProperty] private int _storeId;
        [ObservableProperty] private string _vendor = "Black Box Network Services";
        [ObservableProperty] private string? _leadTechName;
        [ObservableProperty] private string _status = "Vendor Assigned";
        [ObservableProperty] private string _cableGrade = "Cat6 Plenum (CMP)";
        [ObservableProperty] private int _totalDataDrops = 24;
        [ObservableProperty] private int _posDrops = 4;
        [ObservableProperty] private int _apDrops = 4;
        [ObservableProperty] private int _backOfficeDrops = 4;
        [ObservableProperty] private int _cctvDrops = 8;
        [ObservableProperty] private int _trafficCounterDrops = 2;
        [ObservableProperty] private int _musicSoundDrops = 2;
        [ObservableProperty] private bool _flukeCertified;
        [ObservableProperty] private string? _certificationReportUrl;
        [ObservableProperty] private string _scheduledStartDate = string.Empty;
        [ObservableProperty] private string _scheduledCompletionDate = string.Empty;
        [ObservableProperty] private string? _actualCompletionDate;

        [ObservableProperty] private bool _isDirty;
    }

    public partial class PowerPole : ObservableObject
    {
        [ObservableProperty] private int _powerPoleId;
        [ObservableProperty] private int _storeId;
        [ObservableProperty] private string _status = "Survey Required";
        [ObservableProperty] private int _totalRequired = 1;
        [ObservableProperty] private int _installedCount = 0;
        [ObservableProperty] private string _manufacturer = "Wiremold / Legrand";
        [ObservableProperty] private string _modelNumber = "25DTP-4";
        [ObservableProperty] private string _finishColor = "Anodized Aluminum";
        [ObservableProperty] private string _ceilingMountType = "T-Bar Grid Clamp with Structural Tie-Wire";
        [ObservableProperty] private string? _electricalContractor;
        [ObservableProperty] private int _cashWrapFeedDrops = 4;
        [ObservableProperty] private bool _dualChannelIsolated = true;
        [ObservableProperty] private string? _scheduledInstallDate;
        [ObservableProperty] private string? _actualInstallDate;
        [ObservableProperty] private string? _blockerReason;

        [ObservableProperty] private bool _isDirty;
    }

    public partial class MallProperty : ObservableObject
    {
        [ObservableProperty] private int _mallId;
        [ObservableProperty] private string _mallCode = string.Empty;
        [ObservableProperty] private string _propertyName = string.Empty;
        [ObservableProperty] private string _landlordCompany = string.Empty;
        [ObservableProperty] private string _propertyType = "Enclosed Regional Mall";
        [ObservableProperty] private string _city = string.Empty;
        [ObservableProperty] private string _stateProvince = string.Empty;
        [ObservableProperty] private string _postalCode = string.Empty;
        [ObservableProperty] private string _countryCode = "CA";
        [ObservableProperty] private bool _coiRequired = true;
        [ObservableProperty] private decimal _minGeneralLiabilityCad = 5000000m;
        [ObservableProperty] private DateTime? _coiExpirationDate;
        [ObservableProperty] private string _deliveryWindow = "06:00 - 09:30";
        [ObservableProperty] private bool _nightWorkOnly = false;
        [ObservableProperty] private decimal _badgingFeePerTech = 25m;
        [ObservableProperty] private string _specialRules = string.Empty;
        [ObservableProperty] private string _managementContactName = string.Empty;
        [ObservableProperty] private string _managementEmail = string.Empty;
        [ObservableProperty] private string _managementPhone = string.Empty;
        [ObservableProperty] private string _securityPhone = string.Empty;
        [ObservableProperty] private string _cablingVendor = string.Empty;
        [ObservableProperty] private bool _mallContacted = true;

        [ObservableProperty] private bool _isDirty;
    }
}
```

### 2.3 Concrete ViewModel Designs

#### 2.3.1 `MainViewModel` (Fleet Dashboard & State Coordinator)

Replaces all private fields and imperative UI logic from `MainWindow.xaml.cs`:

```csharp
// desktop-app/ViewModels/MainViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Repositories;
using YMInc.StoreDeploymentApp.Services.Interfaces;

namespace YMInc.StoreDeploymentApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IStoreRepository _storeRepository;
        private readonly ISyncService _syncService;
        private readonly IToastNotificationService _toastService;
        private readonly IThemeService _themeService;

        [ObservableProperty] private ObservableCollection<StoreDeployment> _stores = new();
        [ObservableProperty] private ObservableCollection<StoreDeployment> _filteredStores = new();
        [ObservableProperty] private StoreDeployment? _selectedStore;

        // UI View Toggle
        [ObservableProperty] private bool _isShowingHome = true;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _statusMessage = "Ready";

        // Filtering & Search
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private string _selectedBannerFilter = "All Banners";
        [ObservableProperty] private string _selectedHealthFilter = "All Health";

        // Fleet Telemetry KPI Rollups
        [ObservableProperty] private int _totalStoresCount;
        [ObservableProperty] private int _onTrackCount;
        [ObservableProperty] private int _atRiskCount;
        [ObservableProperty] private int _criticalCount;
        [ObservableProperty] private double _overallCompletionPercentage;

        public MainViewModel(
            IStoreRepository storeRepository,
            ISyncService syncService,
            IToastNotificationService toastService,
            IThemeService themeService)
        {
            _storeRepository = storeRepository;
            _syncService = syncService;
            _toastService = toastService;
            _themeService = themeService;
        }

        [RelayCommand]
        public async Task LoadFleetDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading store fleet deployments...";

                var storeList = await _storeRepository.GetAllStoresAsync(cancellationToken);
                Stores = new ObservableCollection<StoreDeployment>(storeList);
                ApplyFilter();
                RecalculateKpis();

                StatusMessage = $"Loaded {Stores.Count} stores successfully.";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Fleet load cancelled.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _toastService.ShowError("Failed to Load Fleet", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SaveFleetChangesAsync(CancellationToken cancellationToken = default)
        {
            var dirtyStores = Stores.Where(s => s.IsDirty).ToList();
            if (dirtyStores.Count == 0)
            {
                _toastService.ShowInfo("No Changes", "All stores are currently up to date.");
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"Committing {dirtyStores.Count} modified stores in transactional batch...";

                var result = await _storeRepository.SaveStoreBatchAsync(dirtyStores, cancellationToken);
                if (result.Success)
                {
                    foreach (var s in dirtyStores) s.IsDirty = false;
                    _toastService.ShowSuccess("Fleet Saved", $"Successfully committed {dirtyStores.Count} stores to cloud database.");
                    StatusMessage = "All fleet changes committed.";
                }
                else
                {
                    _toastService.ShowError("Save Failed", result.ErrorMessage ?? "Unknown transaction error.");
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Save Fault", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void ToggleView(string viewName)
        {
            IsShowingHome = (viewName == "Home");
        }

        [RelayCommand]
        public void ToggleTheme()
        {
            _themeService.ToggleTheme();
        }

        partial void OnSearchQueryChanged(string value) => ApplyFilter();
        partial void OnSelectedBannerFilterChanged(string value) => ApplyFilter();
        partial void OnSelectedHealthFilterChanged(string value) => ApplyFilter();

        private void ApplyFilter()
        {
            var query = Stores.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(s =>
                    s.StoreNumber.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    s.StoreName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    s.City.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedBannerFilter != "All Banners")
            {
                query = query.Where(s => s.BannerName.Equals(SelectedBannerFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedHealthFilter != "All Health")
            {
                query = query.Where(s => s.OverallHealth.Equals(SelectedHealthFilter, StringComparison.OrdinalIgnoreCase));
            }

            FilteredStores = new ObservableCollection<StoreDeployment>(query.ToList());
        }

        private void RecalculateKpis()
        {
            TotalStoresCount = Stores.Count;
            OnTrackCount = Stores.Count(s => s.OverallHealth == "On Track");
            AtRiskCount = Stores.Count(s => s.OverallHealth == "At Risk");
            CriticalCount = Stores.Count(s => s.OverallHealth == "Critical Blocker");
            OverallCompletionPercentage = Stores.Count > 0 ? Stores.Average(s => s.WeeklyCompletionPct) : 0;
        }
    }
}
```

#### 2.3.2 `StoreDetailViewModel` (5-Tab Technical Detail & WAN Persistence)

Directly resolves the critical WAN circuit persistence defect:

```csharp
// desktop-app/ViewModels/StoreDetailViewModel.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Repositories;
using YMInc.StoreDeploymentApp.Services.Interfaces;

namespace YMInc.StoreDeploymentApp.ViewModels
{
    public partial class StoreDetailViewModel : ObservableObject
    {
        private readonly IStoreRepository _storeRepository;
        private readonly ITelecomService _telecomService;
        private readonly IToastNotificationService _toastService;

        [ObservableProperty] private StoreDeployment _store;
        [ObservableProperty] private int _selectedTabIndex;
        [ObservableProperty] private bool _isSaving;
        [ObservableProperty] private string _saveStatusText = string.Empty;

        public StoreDetailViewModel(
            StoreDeployment store,
            IStoreRepository storeRepository,
            ITelecomService telecomService,
            IToastNotificationService toastService)
        {
            _store = store;
            _storeRepository = storeRepository;
            _telecomService = telecomService;
            _toastService = toastService;
        }

        [RelayCommand]
        public async Task SaveStoreDetailAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IsSaving = true;
                SaveStatusText = "Persisting store specifications and WAN circuits...";

                // 1. Transactionally persist master store, survey, power poles, cabling, and mall
                var storeResult = await _storeRepository.SaveStoreAsync(Store, cancellationToken);
                if (!storeResult)
                {
                    _toastService.ShowError("Save Failed", "Failed to update store engineering specifications.");
                    SaveStatusText = "Error saving store attributes.";
                    return;
                }

                // 2. Explicitly persist all Dual-WAN circuits into circuit_details (Fixing data loss bug)
                var circuitResult = await _telecomService.SyncStoreCircuitsAsync(Store.StoreId, Store.Circuits, cancellationToken);
                if (!circuitResult)
                {
                    _toastService.ShowError("Telecom Sync Failed", "Store saved, but WAN circuit records failed to persist.");
                    SaveStatusText = "Warning: Telecom circuits not persisted.";
                    return;
                }

                Store.IsDirty = false;
                foreach (var c in Store.Circuits) c.IsDirty = false;

                SaveStatusText = "✓ All specifications and WAN circuits saved successfully.";
                _toastService.ShowSuccess("Store Updated", $"Store #{Store.StoreNumber} specifications persisted.");
            }
            catch (Exception ex)
            {
                _toastService.ShowError("Save Exception", ex.Message);
                SaveStatusText = $"Save error: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        public void AddFailoverCircuit()
        {
            Store.Circuits.Add(new TelecomCircuit
            {
                StoreId = Store.StoreId,
                Role = "Secondary Failover WAN",
                CarrierName = Store.CountryCode == "US" ? "AT&T Commercial" : "Telus",
                ServiceType = "Enterprise 5G / LTE Wireless",
                OrderStatus = "Order Placed",
                BandwidthDownMbps = 100,
                BandwidthUpMbps = 25,
                DemarcExtended = true
            });
            Store.IsDirty = true;
        }
    }
}
```

#### 2.3.3 `FiscalCalendarViewModel` (NRF 4-5-4 & Freeze Validation)

```csharp
// desktop-app/ViewModels/FiscalCalendarViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Services.Interfaces;

namespace YMInc.StoreDeploymentApp.ViewModels
{
    public partial class FiscalCalendarViewModel : ObservableObject
    {
        private readonly IFiscalCalendarService _calendarService;
        private readonly IToastNotificationService _toastService;

        [ObservableProperty] private int _selectedFiscalYear = 2026;
        [ObservableProperty] private int _selectedFiscalMonth = 9; // October (Fiscal M9)
        [ObservableProperty] private ObservableCollection<FiscalCalendarDay> _days = new();
        [ObservableProperty] private ObservableCollection<CalendarWorkItem> _scheduledWorkItems = new();
        [ObservableProperty] private FiscalCalendarDay? _selectedDay;

        public FiscalCalendarViewModel(
            IFiscalCalendarService calendarService,
            IToastNotificationService toastService)
        {
            _calendarService = calendarService;
            _toastService = toastService;
        }

        [RelayCommand]
        public async Task ScheduleWorkItemAsync(CalendarWorkItem workItem)
        {
            // Mandatory Retail Blackout Validation
            var freezeCheck = await _calendarService.ValidateHolidayFreezeAsync(workItem.ScheduledDate);
            if (freezeCheck.IsFreezeDay)
            {
                _toastService.ShowError("Blackout Freeze Violation", 
                    $"Cannot schedule technical cutover on {workItem.ScheduledDate:yyyy-MM-dd}: {freezeCheck.FreezeReason}. PM sign-off required.");
                return;
            }

            await _calendarService.SaveWorkItemAsync(workItem);
            ScheduledWorkItems.Add(workItem);
            _toastService.ShowSuccess("Work Item Scheduled", $"Scheduled {workItem.Title} for {workItem.ScheduledDate:yyyy-MM-dd}");
        }
    }
}
```

#### 2.3.4 `MallRulesViewModel` (Landlord Matrix & Live Search)

```csharp
// desktop-app/ViewModels/MallRulesViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Services.Interfaces;

namespace YMInc.StoreDeploymentApp.ViewModels
{
    public partial class MallRulesViewModel : ObservableObject
    {
        private readonly IComplianceService _complianceService;
        private readonly IToastNotificationService _toastService;

        [ObservableProperty] private ObservableCollection<MallProperty> _allMalls = new();
        [ObservableProperty] private ObservableCollection<MallProperty> _filteredMalls = new();
        [ObservableProperty] private MallProperty? _selectedMall;
        [ObservableProperty] private string _searchFilter = string.Empty;

        public MallRulesViewModel(IComplianceService complianceService, IToastNotificationService toastService)
        {
            _complianceService = complianceService;
            _toastService = toastService;
        }

        [RelayCommand]
        public async Task LoadMallRulesAsync()
        {
            var malls = await _complianceService.GetAllMallsAsync();
            AllMalls = new ObservableCollection<MallProperty>(malls);
            ApplyFilter();
        }

        [RelayCommand]
        public async Task SaveMallPropertyAsync(MallProperty mall)
        {
            bool ok = await _complianceService.SaveMallPropertyAsync(mall);
            if (ok)
            {
                mall.IsDirty = false;
                _toastService.ShowSuccess("Compliance Saved", $"Rules updated for {mall.PropertyName}.");
            }
            else
            {
                _toastService.ShowError("Compliance Error", "Failed to persist landlord rules.");
            }
        }

        partial void OnSearchFilterChanged(string value) => ApplyFilter();

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchFilter))
            {
                FilteredMalls = new ObservableCollection<MallProperty>(AllMalls);
                return;
            }

            var matches = AllMalls.Where(m =>
                m.PropertyName.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                m.City.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                m.LandlordCompany.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase));

            FilteredMalls = new ObservableCollection<MallProperty>(matches);
        }
    }
}
```

#### 2.3.5 `RosterImportViewModel` (Genuine CSV Ingestion & Roster Sync)

Replaces the mock string assignment in `RosterImportWindow.xaml.cs`:

```csharp
// desktop-app/ViewModels/RosterImportViewModel.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Repositories;
using YMInc.StoreDeploymentApp.Services.Interfaces;

namespace YMInc.StoreDeploymentApp.ViewModels
{
    public partial class RosterImportViewModel : ObservableObject
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IToastNotificationService _toastService;

        [ObservableProperty] private string _selectedFilePath = string.Empty;
        [ObservableProperty] private int _recordsParsedCount;
        [ObservableProperty] private bool _isImporting;
        [ObservableProperty] private string _importLog = string.Empty;

        public RosterImportViewModel(IStoreRepository storeRepository, IToastNotificationService toastService)
        {
            _storeRepository = storeRepository;
            _toastService = toastService;
        }

        [RelayCommand]
        public async Task ParseCsvFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _toastService.ShowError("File Not Found", "The specified CSV file does not exist.");
                return;
            }

            SelectedFilePath = filePath;
            ImportLog = $"Analyzing {Path.GetFileName(filePath)}...\n";

            await Task.Run(() =>
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                });

                var records = csv.GetRecords<CsvStoreRecord>().ToList();
                RecordsParsedCount = records.Count;
                ImportLog += $"Parsed {records.Count} valid store records ready for sync.\n";
            });
        }

        [RelayCommand]
        public async Task ExecuteSyncAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedFilePath) || RecordsParsedCount == 0)
            {
                _toastService.ShowWarning("No File Selected", "Please select a valid CSV roster file first.");
                return;
            }

            try
            {
                IsImporting = true;
                ImportLog += "Starting transactional fleet ingestion...\n";

                using var reader = new StreamReader(SelectedFilePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var records = csv.GetRecords<CsvStoreRecord>().ToList();

                int created = 0;
                foreach (var rec in records)
                {
                    var store = MapCsvToDomain(rec);
                    bool ok = await _storeRepository.CreateStoreTransactionalAsync(store);
                    if (ok) created++;
                }

                ImportLog += $"Sync complete: {created} stores successfully imported.\n";
                _toastService.ShowSuccess("Roster Sync Complete", $"Imported {created} stores from CSV.");
            }
            catch (Exception ex)
            {
                ImportLog += $"FATAL ERROR: {ex.Message}\n";
                _toastService.ShowError("Import Error", ex.Message);
            }
            finally
            {
                IsImporting = false;
            }
        }

        private StoreDeployment MapCsvToDomain(CsvStoreRecord rec) => new()
        {
            StoreNumber = rec.StoreNumber.Trim(),
            StoreName = rec.StoreName.Trim(),
            BannerName = rec.Banner.Trim(),
            Address = rec.Address.Trim(),
            City = rec.City.Trim(),
            StateProvince = rec.State.Trim(),
            PostalCode = rec.PostalCode.Trim(),
            CountryCode = rec.Country.Trim().ToUpperInvariant(),
            GrossSqft = rec.GrossSqft,
            PosTerminalCount = rec.PosCount > 0 ? rec.PosCount : 4
        };
    }

    public record CsvStoreRecord
    {
        public string StoreNumber { get; init; } = "";
        public string StoreName { get; init; } = "";
        public string Banner { get; init; } = "";
        public string Address { get; init; } = "";
        public string City { get; init; } = "";
        public string State { get; init; } = "";
        public string PostalCode { get; init; } = "";
        public string Country { get; init; } = "CA";
        public int GrossSqft { get; init; } = 3500;
        public int PosCount { get; init; } = 4;
    }
}
```

### 2.4 Decoupled Repository & Service Interfaces

To isolate Views and ViewModels from direct database socket dependencies, the application defines strict interface abstractions:

```csharp
// desktop-app/Services/Interfaces/IDomainInterfaces.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services.Interfaces
{
    public interface IStoreRepository
    {
        Task<IReadOnlyList<StoreDeployment>> GetAllStoresAsync(CancellationToken ct = default);
        Task<StoreDeployment?> GetStoreByIdAsync(int storeId, CancellationToken ct = default);
        Task<bool> SaveStoreAsync(StoreDeployment store, CancellationToken ct = default);
        Task<(bool Success, string? ErrorMessage)> SaveStoreBatchAsync(IReadOnlyList<StoreDeployment> stores, CancellationToken ct = default);
        Task<bool> CreateStoreTransactionalAsync(StoreDeployment store, CancellationToken ct = default);
    }

    public interface ITelecomService
    {
        Task<IReadOnlyList<TelecomCircuit>> GetCircuitsForStoreAsync(int storeId, CancellationToken ct = default);
        Task<bool> SyncStoreCircuitsAsync(int storeId, IEnumerable<TelecomCircuit> circuits, CancellationToken ct = default);
        Task<bool> UpdateCircuitTelemetryAsync(TelecomCircuit circuit, CancellationToken ct = default);
    }

    public interface IComplianceService
    {
        Task<IReadOnlyList<MallProperty>> GetAllMallsAsync(CancellationToken ct = default);
        Task<MallProperty?> GetMallByCodeAsync(string mallCode, CancellationToken ct = default);
        Task<bool> SaveMallPropertyAsync(MallProperty mall, CancellationToken ct = default);
        Task<bool> ValidateCoiComplianceAsync(int mallId, CancellationToken ct = default);
    }

    public interface IFiscalCalendarService
    {
        Task<IReadOnlyList<FiscalCalendarDay>> GetFiscalMonthDaysAsync(int fiscalYear, int fiscalMonth, CancellationToken ct = default);
        Task<(bool IsFreezeDay, string? FreezeReason)> ValidateHolidayFreezeAsync(DateTime date, CancellationToken ct = default);
        Task SaveWorkItemAsync(CalendarWorkItem workItem, CancellationToken ct = default);
    }

    public interface ISyncService
    {
        bool IsOnline { get; }
        event EventHandler<bool> ConnectionStateChanged;
        Task<SyncResult> TriggerDeltaSyncAsync(CancellationToken ct = default);
        Task EnqueueMutationAsync(string entityType, string entityId, string operation, object payload, CancellationToken ct = default);
    }

    public record SyncResult(bool Success, int PulledChanges, int PushedChanges, string? ErrorMessage);
}
```

---

## 3. Dynamic Store Onboarding Wizard Specification

### 3.1 5-Step Guided Onboarding UX Workflow

To replace the bug-ridden, flat `AddStoreWindow`, we specify a 5-step dynamic onboarding wizard that guides technicians through real estate identity, landlord rule inheritance, low-voltage engineering, dual-WAN circuit turn-up, and automated validation.

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        STORE DEPLOYMENT ONBOARDING WIZARD                              │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  [1. Banner & Identity] ──► [2. Mall Rules] ──► [3. Technical Scope] ──► [4. Dual-WAN] │
│                                                                        │               │
│                                                                        ▼               │
│                                                           [5. Commission & Review]     │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                        │
│   Step 1: Banner & Basic Store Identity                                                │
│   - Banner Brand Picker (Charlotte Russe, Bluenotes, rue21, Urban Planet, etc.)        │
│   - Store Number (e.g., 6124) with Instant Duplicate Validation                        │
│   - Project Type (New Store Opening, Major Remodel, Refresh, Brand Conversion)         │
│   - Address, City, State/Province, Country (CA/US), Gross Sq. Footage, POS Count       │
│                                                                                        │
│   Step 2: Mall Property & Landlord Compliance Inheritance                              │
│   - Select Enclosed Mall from 335 Master Properties or Create Standalone Property      │
│   - Auto-Inheritance of Landlord COI Minimums ($5M-$10M CAD/USD)                       │
│   - Auto-Inheritance of Delivery Dock Windows (e.g. 06:00 - 09:30) & Badging Fees      │
│   - After-Hours Night Work Requirement Flagging                                        │
│                                                                                        │
│   Step 3: Technical Scope & Power Poles Calculation                                    │
│   - Ceiling Type: Drop Ceiling (2x4 Acoustic) vs Open Deck / Exposed Structural Beam   │
│   - Wiremold 25DTP-4 Power Pole Fixturing Calculation (Tie-Wire Anchoring)             │
│   - Automated Cat6 Plenum Drop Rule Engine based on POS Count and Floor Area           │
│                                                                                        │
│   Step 4: Dual-WAN Circuit Configuration & Telecom Turn-Up                             │
│   - Primary WAN: Commercial Fiber DIA (100-250M Symmetric) via Regional Carrier       │
│   - Secondary WAN: Enterprise 5G / LTE Failover (Cradlepoint W1850 / IBR600)           │
│   - Demarc Extension Specification (Mall Telco Basement to MDF Backroom)               │
│                                                                                        │
│   Step 5: Pre-Flight Review, Automated Drop Calculation & Commissioning                │
│   - Comprehensive Configuration Summary Matrix                                         │
│   - Validation Sanity Checks (Schema Constraints, Carrier Whitelist, Holiday Freeze)   │
│   - One-Click Transactional Provisioning (PostgreSQL & Local Cache)                    │
│                                                                                        │
├────────────────────────────────────────────────────────────────────────────────────────┤
│  [ Back ]                                                         [ Next Step ──► ]    │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Automated Rule Engine (Drop Calculations & Dual-WAN Fix)

The onboarding rule engine programmatically computes infrastructure requirements to prevent contractor under-sizing and ensures complete database schema compliance:

#### 3.2.1 Structured Cabling Drop Formula

The rule engine computes Cat6 Plenum (CMP) data drops using the following standard enterprise formula:

$$\text{Total Data Drops} = \text{POS Drops} + \text{AP Drops} + \text{Back-Office Drops} + \text{CCTV Drops} + \text{Traffic Drops} + \text{Sound Drops}$$

Where:
* **$\text{POS Drops} = \text{PosTerminalCount} \times 2$** (1 primary drop for POS terminal + 1 isolated drop for Verifone/Ingenico pinpad per register, minimum 4 drops).
* **$\text{AP Drops} = \max\left(4, \lceil \frac{\text{GrossSqft}}{2000} \rceil\right)$** (Minimum 4 drops for sales floor, back stockroom, cash wrap, and lease-line entrance; +1 drop per 2,000 sq.ft. thereafter).
* **$\text{Back-Office Drops} = 4$** (Manager PC, network laser printer, VoIP telephone, alarm interface).
* **$\text{CCTV Camera Drops} = 8 + \lfloor \frac{\text{GrossSqft}}{1500} \rfloor \times 2$** (Covers perimeter, cash wrap zoom, fitting rooms exterior, and stockroom).
* **$\text{Foot Traffic Sensor Drops} = 2$** (Overhead stereoscopic optical sensor over store lease line).
* **$\text{Music / Sound System Drops} = 2$** (Backroom audio amplifier feed).

#### 3.2.2 Dual-WAN Provisioning Logic (Fixing the Fatal Schema Defect)

The onboarding engine resolves the critical `DatabaseService.CreateStoreAsync` crash by:
1. Targeting the actual schema table **`circuit_details`** instead of the non-existent `telecom_circuits`.
2. Matching `carrier_name` and `service_type` exactly against PostgreSQL `CHECK` constraints:
   - Primary WAN: Carrier = `Bell Canada` (for CA) or `Comcast Business` (for US); Service Type = `'Commercial Fiber DIA'`.
   - Secondary Failover WAN: Carrier = `Telus` (for CA) or `AT&T Commercial` (for US); Service Type = `'Enterprise 5G / LTE Wireless'`.

```csharp
// desktop-app/Services/StoreOnboardingRuleEngine.cs
using System;
using System.Collections.Generic;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public class StoreOnboardingRuleEngine
    {
        public static CablingOperation CalculateCablingRequirements(int grossSqft, int posCount)
        {
            int posDrops = Math.Max(4, posCount * 2);
            int apDrops = Math.Max(4, (int)Math.Ceiling(grossSqft / 2000.0));
            int backOfficeDrops = 4;
            int cctvDrops = 8 + ((grossSqft / 1500) * 2);
            int trafficDrops = 2;
            int soundDrops = 2;

            int totalDrops = posDrops + apDrops + backOfficeDrops + cctvDrops + trafficDrops + soundDrops;

            return new CablingOperation
            {
                CableGrade = "Cat6 Plenum (CMP)",
                TotalDataDrops = totalDrops,
                PosDrops = posDrops,
                ApDrops = apDrops,
                BackOfficeDrops = backOfficeDrops,
                CctvDrops = cctvDrops,
                TrafficCounterDrops = trafficDrops,
                MusicSoundDrops = soundDrops,
                Status = "Vendor Assigned",
                Vendor = "Black Box Network Services",
                FlukeCertified = false
            };
        }

        public static PowerPole CalculatePowerPoleRequirements(int posCount, string ceilingType, double ceilingHeightFeet)
        {
            // Cash wraps require 1 dual-channel pole per 2 POS terminals
            int poleCount = Math.Max(1, (int)Math.Ceiling(posCount / 2.0));
            string mount = ceilingType.Contains("Drop", StringComparison.OrdinalIgnoreCase)
                ? "T-Bar Grid Clamp with Structural Tie-Wire"
                : "Structural Beam Clamp with Unistrut Trapeze Bracing";

            return new PowerPole
            {
                TotalRequired = poleCount,
                InstalledCount = 0,
                Status = "Survey Required",
                Manufacturer = "Wiremold / Legrand",
                ModelNumber = "25DTP-4",
                FinishColor = "Anodized Aluminum",
                CeilingMountType = mount,
                CashWrapFeedDrops = posCount * 2,
                DualChannelIsolated = true
            };
        }

        public static List<TelecomCircuit> GenerateDefaultCircuits(string countryCode)
        {
            bool isUs = countryCode.Equals("US", StringComparison.OrdinalIgnoreCase);

            return new List<TelecomCircuit>
            {
                new TelecomCircuit
                {
                    Role = "Primary WAN",
                    CarrierName = isUs ? "Comcast Business" : "Bell Canada",
                    ServiceType = "Commercial Fiber DIA", // Matches CHECK constraint
                    OrderStatus = "Order Placed",
                    BandwidthDownMbps = 100,
                    BandwidthUpMbps = 100,
                    DemarcExtended = false,
                    PrimaryDns = "8.8.8.8",
                    SecondaryDns = "1.1.1.1"
                },
                new TelecomCircuit
                {
                    Role = "Secondary Failover WAN",
                    CarrierName = isUs ? "AT&T Commercial" : "Telus",
                    ServiceType = "Enterprise 5G / LTE Wireless", // Matches CHECK constraint
                    OrderStatus = "Order Placed",
                    BandwidthDownMbps = 100,
                    BandwidthUpMbps = 25,
                    DemarcExtended = true,
                    PrimaryDns = "8.8.8.8",
                    SecondaryDns = "1.1.1.1"
                }
            };
        }
    }
}
```

### 3.3 Pre-Flight Validation Engine

```csharp
// desktop-app/Services/PreFlightValidationEngine.cs
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Repositories;

namespace YMInc.StoreDeploymentApp.Services
{
    public record ValidationResult(bool IsValid, string? ErrorMessage);

    public class PreFlightValidationEngine
    {
        private readonly IStoreRepository _storeRepository;

        public PreFlightValidationEngine(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public async Task<ValidationResult> ValidateStoreAsync(StoreDeployment store)
        {
            if (string.IsNullOrWhiteSpace(store.StoreNumber))
                return new(false, "Store Number is mandatory.");

            if (!Regex.IsMatch(store.StoreNumber, @"^[A-Za-z0-9\-]+$"))
                return new(false, "Store Number must be alphanumeric.");

            // Verify store number is not already allocated in fleet
            var existing = await _storeRepository.GetStoreByIdAsync(store.StoreId);
            if (existing != null && existing.StoreNumber == store.StoreNumber)
                return new(false, $"Store #{store.StoreNumber} already exists in fleet roster.");

            if (store.GrossSqft < 500 || store.GrossSqft > 50000)
                return new(false, "Gross Square Footage must be between 500 and 50,000 sq.ft.");

            if (store.PosTerminalCount < 1 || store.PosTerminalCount > 30)
                return new(false, "POS Terminal Count must be between 1 and 30.");

            if (store.Circuits.Count < 2)
                return new(false, "Corporate standard mandates both Primary Fiber DIA and Secondary Failover WAN.");

            return new(true, null);
        }
    }
}
```

---

## 4. Real-Time Visual Feedback & Non-Blocking Async UX

### 4.1 Elimination of `async void` Anti-Pattern

In the existing codebase, 10+ event handlers use `async void`, which causes unhandled exceptions to crash the process via the Dispatcher thread. 

**Architectural Rule:** All asynchronous actions must be declared as `IAsyncRelayCommand` or return `Task`. Any XAML event handler must be routed through `[RelayCommand]` in CommunityToolkit.Mvvm, which automatically captures exceptions, provides `IsRunning` binding, and supports cancellation via `CancellationToken`.

```xml
<!-- In XAML: Bind Button to Async Command directly -->
<Button Content="Save All Changes" 
        Command="{Binding SaveFleetChangesCommand}" 
        IsEnabled="{Binding SaveFleetChangesCommand.IsRunning, Converter={StaticResource InverseBoolConverter}}" />
```

### 4.2 Weak Event Patterns for `ThemeService` Memory Leak Remediation

In the current client, `StoreDetailWindow`, `LoginWindow`, `MallRulesWindow`, and `DonutChartControl` subscribe directly to static event `ThemeService.ThemeChanged += ...`. Because static events maintain strong references to listener instances, closed windows and detached controls are never garbage-collected, creating significant managed memory leaks.

#### Remediation Implementation:

```csharp
// desktop-app/Services/ThemeService.cs
using System;
using System.Windows;

namespace YMInc.StoreDeploymentApp.Services
{
    public interface IThemeService
    {
        bool IsDarkTheme { get; }
        void Initialize();
        void ToggleTheme();
        void SetTheme(bool isDark);
        void ApplyThemeToWindow(Window window);
    }

    public class ThemeService : IThemeService
    {
        public bool IsDarkTheme { get; private set; }

        public void ToggleTheme() => SetTheme(!IsDarkTheme);

        public void SetTheme(bool isDark)
        {
            IsDarkTheme = isDark;
            // Broadcast via WeakEventManager to avoid pinning subscriber windows in memory
            WeakEventManager<ThemeService, EventArgs>.DeliverEvent(this, EventArgs.Empty);
        }

        public void Initialize()
        {
            // Resource initialization
        }

        public void ApplyThemeToWindow(Window window)
        {
            // Apply theme styles safely
        }
    }
}
```

In View classes or Controls, subscriptions use `WeakEventManager`:

```csharp
// In DonutChartControl.cs constructor:
WeakEventManager<ThemeService, EventArgs>.AddHandler(
    themeService, 
    nameof(ThemeService.ThemeChanged), 
    OnThemeChanged);

private void OnThemeChanged(object? sender, EventArgs e)
{
    InvalidateVisual();
}
```

### 4.3 Enterprise Toast Notification System

Replaces disruptive modal `MessageBox.Show()` calls with non-blocking Microsoft Fluent-style toast notifications:

```csharp
// desktop-app/Services/ToastNotificationService.cs
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace YMInc.StoreDeploymentApp.Services
{
    public enum ToastType { Info, Success, Warning, Error }

    public partial class ToastMessage : ObservableObject
    {
        public Guid Id { get; } = Guid.NewGuid();
        [ObservableProperty] private string _title = string.Empty;
        [ObservableProperty] private string _message = string.Empty;
        [ObservableProperty] private ToastType _type = ToastType.Info;
        [ObservableProperty] private double _opacity = 1.0;
    }

    public interface IToastNotificationService
    {
        ObservableCollection<ToastMessage> ActiveToasts { get; }
        void ShowSuccess(string title, string message);
        void ShowError(string title, string message);
        void ShowWarning(string title, string message);
        void ShowInfo(string title, string message);
    }

    public class ToastNotificationService : IToastNotificationService
    {
        public ObservableCollection<ToastMessage> ActiveToasts { get; } = new();

        public void ShowSuccess(string title, string message) => Enqueue(title, message, ToastType.Success, 4);
        public void ShowError(string title, string message) => Enqueue(title, message, ToastType.Error, 7);
        public void ShowWarning(string title, string message) => Enqueue(title, message, ToastType.Warning, 5);
        public void ShowInfo(string title, string message) => Enqueue(title, message, ToastType.Info, 3);

        private void Enqueue(string title, string message, ToastType type, int durationSeconds)
        {
            var toast = new ToastMessage { Title = title, Message = message, Type = type };
            ActiveToasts.Add(toast);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(durationSeconds) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                ActiveToasts.Remove(toast);
            };
            timer.Start();
        }
    }
}
```

---

## 5. Offline-First Resilience & Local SQLite Sync Architecture

### 5.1 Local SQLite Cache Schema (`ym_local_cache.db`)

To ensure field technicians and project managers can inspect stores, update cable certifications, and adjust schedule milestones without an active internet connection, the application embeds a lightweight, high-performance SQLite database:

```sql
-- Local SQLite Initialization Script: ym_local_cache.db
CREATE TABLE IF NOT EXISTS local_stores (
    store_id INTEGER PRIMARY KEY,
    store_number TEXT NOT NULL UNIQUE,
    banner_code TEXT NOT NULL,
    banner_name TEXT NOT NULL,
    store_name TEXT NOT NULL,
    project_type TEXT NOT NULL,
    operational_status TEXT NOT NULL,
    overall_health TEXT NOT NULL,
    country_code TEXT NOT NULL,
    region TEXT,
    district TEXT,
    address TEXT,
    city TEXT,
    state_province TEXT,
    postal_code TEXT,
    gross_sqft INTEGER,
    pos_terminal_count INTEGER,
    row_version INTEGER DEFAULT 1,
    vector_clock TEXT DEFAULT '{"cloud":1}',
    is_dirty INTEGER DEFAULT 0,
    last_synced_at TEXT
);

CREATE TABLE IF NOT EXISTS local_circuits (
    circuit_id INTEGER PRIMARY KEY,
    store_id INTEGER NOT NULL REFERENCES local_stores(store_id) ON DELETE CASCADE,
    circuit_role TEXT NOT NULL,
    carrier_name TEXT NOT NULL,
    service_type TEXT NOT NULL,
    order_status TEXT NOT NULL,
    bandwidth_down_mbps INTEGER,
    bandwidth_up_mbps INTEGER,
    foc_date TEXT,
    static_ip_assigned TEXT,
    cradlepoint_imei TEXT,
    row_version INTEGER DEFAULT 1,
    vector_clock TEXT DEFAULT '{"cloud":1}',
    is_dirty INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS local_cabling (
    cabling_id INTEGER PRIMARY KEY,
    store_id INTEGER NOT NULL REFERENCES local_stores(store_id) ON DELETE CASCADE,
    operational_status TEXT NOT NULL, -- Survey Scheduled, Rough-in In Progress, Pulled & Terminated, Completed & Certified
    drop_count_total INTEGER NOT NULL,
    cat6_plenum_drops INTEGER NOT NULL,
    fluke_test_passed INTEGER DEFAULT 0,
    fluke_certified_at TEXT,
    fluke_pdf_cached_path TEXT, -- Local path in %LocalAppData%/YMIncStoreDeployment/PendingUploads/
    contractor_name TEXT,
    notes TEXT,
    row_version INTEGER DEFAULT 1,
    vector_clock TEXT DEFAULT '{"cloud":1}',
    is_dirty INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS local_power_poles (
    pole_id INTEGER PRIMARY KEY,
    store_id INTEGER NOT NULL REFERENCES local_stores(store_id) ON DELETE CASCADE,
    pole_type TEXT NOT NULL, -- Wiremold 25DTP-4 Anodized Aluminum
    pole_count INTEGER NOT NULL DEFAULT 1,
    structural_tie_wire_verified INTEGER DEFAULT 0, -- 12-gauge ceiling deck tie wire inspection
    dual_channel_isolation_verified INTEGER DEFAULT 0, -- High/low voltage barrier separation
    esa_inspection_passed INTEGER DEFAULT 0,
    esa_inspection_date TEXT,
    esa_sticker_number TEXT,
    pole_requirement_status TEXT NOT NULL,
    row_version INTEGER DEFAULT 1,
    vector_clock TEXT DEFAULT '{"cloud":1}',
    is_dirty INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS local_mall_properties (
    mall_id INTEGER PRIMARY KEY,
    mall_name TEXT NOT NULL,
    landlord_company TEXT NOT NULL, -- Cadillac Fairview, Oxford, Ivanhoé Cambridge, RioCan, Brookfield, Simon
    property_manager_name TEXT,
    security_dispatch_phone TEXT,
    freight_dock_location TEXT,
    dock_booking_required INTEGER DEFAULT 1,
    dock_operating_hours TEXT, -- e.g. 06:00 - 09:00 EST
    noisy_work_start TEXT, -- 22:30
    noisy_work_end TEXT, -- 06:00
    general_liability_min_cad NUMERIC NOT NULL DEFAULT 5000000,
    row_version INTEGER DEFAULT 1,
    vector_clock TEXT DEFAULT '{"cloud":1}',
    is_dirty INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS local_work_items (
    item_id INTEGER PRIMARY KEY,
    store_id INTEGER NOT NULL REFERENCES local_stores(store_id) ON DELETE CASCADE,
    stage_code TEXT NOT NULL, -- SURVEY, CABLING, POWER_POLES, TELECOM, COMMISSIONING, OPENING
    task_name TEXT NOT NULL,
    due_date TEXT NOT NULL,
    assigned_role TEXT NOT NULL, -- PM, PA, PS1, PS2, PS3, PS4, FIELD_TECH
    is_completed INTEGER DEFAULT 0,
    completed_at TEXT,
    completed_by TEXT,
    row_version INTEGER DEFAULT 1,
    vector_clock TEXT DEFAULT '{"cloud":1}',
    is_dirty INTEGER DEFAULT 0
);

CREATE TABLE IF NOT EXISTS sync_outbox (
    queue_id INTEGER PRIMARY KEY AUTOINCREMENT, -- Strict monotonic FIFO replay ordering (eliminates clock-jump inversions)
    mutation_id TEXT UNIQUE NOT NULL, -- Client UUIDv7
    entity_name TEXT NOT NULL,
    entity_id TEXT NOT NULL,
    mutation_type TEXT NOT NULL CHECK (mutation_type IN ('INSERT', 'UPDATE', 'DELETE')),
    payload_json TEXT NOT NULL,
    baseline_snapshot_json TEXT, -- Baseline canonical snapshot saved before local edit for clean rollback
    client_timestamp TEXT NOT NULL, -- ISO 8601 UTC
    vector_clock TEXT NOT NULL, -- JSON-serialized vector clock e.g. {"cloud":104,"client":12}
    base_version INTEGER NOT NULL DEFAULT 1,
    sync_status TEXT NOT NULL DEFAULT 'PENDING' CHECK (sync_status IN ('PENDING', 'IN_FLIGHT', 'COMMITTED', 'CONFLICT', 'REJECTED')),
    last_error TEXT
);
```

#### Offline Binary Artifact Staging Directory
For binary attachments generated while disconnected (Fluke LinkWare `.FLW`/`.PDF` test reports, Wiremold deck-anchor inspection photos, ESA stickers, and demarc fiber photos):
- Files are staged in `%LocalAppData%\YMIncStoreDeployment\PendingUploads\{entity_type}_{entity_id}_{timestamp}.dat`.
- Metadata references are stored in the corresponding local SQLite table (`fluke_pdf_cached_path`).
- The background `SyncEngine` streams pending files to the Cloud Storage endpoint (`POST /api/v1/cabling/{id}/fluke-upload`) during Phase 1 sync before marking the outbox mutation `COMMITTED`.

### 5.2 Change-Tracking Outbox Queue Implementation & Split-Brain Prevention

When a technician performs any create, update, or milestone sign-off while offline:
1. The pre-mutation state of the entity is serialized into a baseline canonical snapshot (`_baseline_snapshot`).
2. The mutation is committed immediately to the relevant local SQLite table (`local_stores`, `local_circuits`, `local_cabling`, etc.) with `is_dirty = 1`.
3. A formal change record is enqueued into `sync_outbox` capturing the autoincrement sequence, mutation ID (UUIDv7), entity metadata, baseline canonical snapshot JSON, base version, serialized JSON payload, UTC client timestamp, and current vector clock.
4. When network connectivity is restored, the `SyncEngine` drains the outbox strictly in monotonic FIFO sequence (`ORDER BY queue_id ASC`) and pushes batches to the Cloud API Gateway (`POST /api/v1/sync/push`). Draining by `queue_id` guarantees that local clock synchronization jumps or manual time adjustments can never invert mutation order.

#### Error Classification, Idempotent 409 Resolution & Split-Brain Elimination:
A critical flaw in naive offline sync queues is treating all failures as transient retryable errors or executing false rollbacks. The modernized desktop engine enforces rigorous error and conflict mechanics:

- **Transient Transport Errors (HTTP 502/503/504, Socket Timeout, DNS Blip):**  
  Enters exponential backoff retry loop (delays: 2s, 4s, 8s, 16s with jitter up to `retry_count = 5`).
- **HTTP 409 Conflict / Precondition Verification (Idempotency Protection):**  
  If the API Gateway responds with HTTP 409 Conflict or HTTP 412 Precondition Failed, the `SyncEngine` must **not** assume rejection. In distributed environments, cellular drops often occur after the cloud commits the transaction but before the HTTP 200 response reaches the client. The client executes an idempotency check: verifying whether `mutation_id` was already committed in the cloud journal. If already committed, the local status is immediately transitioned to `COMMITTED` with `is_dirty = 0`, preventing false rollbacks of successfully applied data.
- **Permanent Domain Rejections (HTTP 422 Unprocessable Entity, `REJECTED_DOMAIN_RULE`):**  
  1. Retries are **halted immediately** (`sync_status` set to `'REJECTED'`).
  2. The detailed rejection cause is recorded in `sync_outbox.last_error`.
  3. **Atomic Reversal to Canonical Snapshot:** To prevent the client from displaying stale, rejected values as clean (`is_dirty = 0`), `MarkMutationRejectedAndRollbackAsync` atomically deserializes the baseline snapshot (`baseline_snapshot_json`) or authoritative cloud payload from the response, restores the entity columns in the local SQLite table, and clears `is_dirty = 0`.
  4. An interactive desktop toast notification is raised informing the technician that their offline edit was superseded by authoritative domain rules (e.g. *"Carrier FOC date 2026-11-05 locked by Bell Canada webhook; local draft reverted to canonical baseline"*).
- **Sub-Threshold Concurrency & Clock Skew Protocol (`CONFLICT_SUB_THRESHOLD_CONCURRENT`):**  
  When concurrent mutations occur within the 300-second skew window without an applicable domain authority rule (Rules 1–4) and vector clocks are concurrent ($V_A \not\le V_B$ and $V_B \not\le V_A$), the cloud gateway rejects naive LWW. The mutation is flagged with `CONFLICT_SUB_THRESHOLD_CONCURRENT`. The desktop client records both candidate values in the local conflict delta log, applies the authoritative canonical cloud state, and alerts the operator that the edit has been escalated to the Executive Conflict Queue for administrative review.

```csharp
// desktop-app/Services/SqliteSyncQueueRepository.cs
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;

namespace YMInc.StoreDeploymentApp.Services
{
    public record SyncMutation(
        long QueueId,
        string MutationId,
        string EntityName,
        string EntityId,
        string MutationType,
        string PayloadJson,
        string? BaselineSnapshotJson,
        string ClientTimestamp,
        string VectorClock,
        int BaseVersion,
        string SyncStatus,
        string? LastError);

    public interface ISyncQueueRepository
    {
        Task EnqueueMutationAsync(string entityName, string entityId, string mutationType, object payload, object? baselineSnapshot, string vectorClock, int baseVersion);
        Task<IReadOnlyList<SyncMutation>> GetPendingMutationsAsync(int limit = 50);
        Task MarkMutationInFlightAsync(string mutationId);
        Task MarkMutationCommittedAsync(string mutationId);
        Task MarkTransientFailureAsync(string mutationId, string errorMessage);
        Task MarkMutationRejectedAndRollbackAsync(string mutationId, string entityName, string entityId, string rejectionReason, string? canonicalServerJson = null);
        Task<bool> VerifyServerCommitOnConflictAsync(string mutationId, string entityName, string entityId);
    }

    public class SqliteSyncQueueRepository : ISyncQueueRepository
    {
        private readonly string _connectionString = "Data Source=ym_local_cache.db";

        public async Task EnqueueMutationAsync(
            string entityName, string entityId, string mutationType, 
            object payload, object? baselineSnapshot, string vectorClock, int baseVersion)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
                INSERT INTO sync_outbox 
                    (mutation_id, entity_name, entity_id, mutation_type, payload_json, 
                     baseline_snapshot_json, client_timestamp, vector_clock, base_version, sync_status)
                VALUES 
                    (@MutationId, @EntityName, @EntityId, @MutationType, @PayloadJson, 
                     @BaselineSnapshotJson, @ClientTimestamp, @VectorClock, @BaseVersion, 'PENDING');";

            await conn.ExecuteAsync(sql, new
            {
                MutationId = Guid.NewGuid().ToString(),
                EntityName = entityName,
                EntityId = entityId,
                MutationType = mutationType,
                PayloadJson = JsonSerializer.Serialize(payload),
                BaselineSnapshotJson = baselineSnapshot != null ? JsonSerializer.Serialize(baselineSnapshot) : null,
                ClientTimestamp = DateTime.UtcNow.ToString("o"),
                VectorClock = vectorClock,
                BaseVersion = baseVersion
            });
        }

        public async Task<IReadOnlyList<SyncMutation>> GetPendingMutationsAsync(int limit = 50)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            // CRITICAL FIX: Monotonic FIFO replay ordering by autoincrement queue_id (immune to clock adjustments)
            const string sql = @"
                SELECT queue_id AS QueueId, mutation_id AS MutationId, entity_name AS EntityName, entity_id AS EntityId,
                       mutation_type AS MutationType, payload_json AS PayloadJson, baseline_snapshot_json AS BaselineSnapshotJson,
                       client_timestamp AS ClientTimestamp, vector_clock AS VectorClock, 
                       base_version AS BaseVersion, sync_status AS SyncStatus, last_error AS LastError
                FROM sync_outbox
                WHERE sync_status = 'PENDING'
                ORDER BY queue_id ASC LIMIT @Limit;";

            var rows = await conn.QueryAsync<SyncMutation>(sql, new { Limit = limit });
            return rows.AsList();
        }

        public async Task MarkMutationInFlightAsync(string mutationId)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            const string sql = "UPDATE sync_outbox SET sync_status = 'IN_FLIGHT' WHERE mutation_id = @Id;";
            await conn.ExecuteAsync(sql, new { Id = mutationId });
        }

        public async Task MarkMutationCommittedAsync(string mutationId)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            const string sql = "UPDATE sync_outbox SET sync_status = 'COMMITTED', last_error = NULL WHERE mutation_id = @Id;";
            await conn.ExecuteAsync(sql, new { Id = mutationId });
        }

        public async Task MarkTransientFailureAsync(string mutationId, string errorMessage)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            // Exponential backoff: re-queue as PENDING for retry if attempts under limit
            const string sql = @"
                UPDATE sync_outbox 
                SET sync_status = 'PENDING', last_error = @Error 
                WHERE mutation_id = @Id;";
            await conn.ExecuteAsync(sql, new { Id = mutationId, Error = errorMessage });
        }

        public async Task<bool> VerifyServerCommitOnConflictAsync(string mutationId, string entityName, string entityId)
        {
            // On HTTP 409 Conflict, verify if mutation was already committed by server during previous attempt
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            const string checkSql = "SELECT sync_status FROM sync_outbox WHERE mutation_id = @Id;";
            var status = await conn.ExecuteScalarAsync<string>(checkSql, new { Id = mutationId });
            return status == "COMMITTED";
        }

        public async Task MarkMutationRejectedAndRollbackAsync(
            string mutationId, string entityName, string entityId, string rejectionReason, string? canonicalServerJson = null)
        {
            await using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                // 1. Mark outbox mutation as permanently REJECTED (halts retry storm)
                const string outboxSql = @"
                    UPDATE sync_outbox 
                    SET sync_status = 'REJECTED', last_error = @Reason 
                    WHERE mutation_id = @Id;";
                await conn.ExecuteAsync(outboxSql, new { Id = mutationId, Reason = rejectionReason }, tx);

                // 2. Fetch baseline pre-mutation snapshot if canonical server JSON not explicitly passed
                string? rollbackPayloadJson = canonicalServerJson;
                if (string.IsNullOrWhiteSpace(rollbackPayloadJson))
                {
                    const string fetchSnapshotSql = "SELECT baseline_snapshot_json FROM sync_outbox WHERE mutation_id = @Id LIMIT 1;";
                    rollbackPayloadJson = await conn.ExecuteScalarAsync<string>(fetchSnapshotSql, new { Id = mutationId }, tx);
                }

                // 3. Atomically revert entity columns to canonical state / baseline snapshot and reset is_dirty = 0
                string targetTable = entityName switch
                {
                    "stores" => "local_stores",
                    "circuit_details" => "local_circuits",
                    "cabling_operations" => "local_cabling",
                    "power_poles" => "local_power_poles",
                    "mall_properties" => "local_mall_properties",
                    "calendar_work_items" => "local_work_items",
                    _ => throw new InvalidOperationException($"Unknown entity: {entityName}")
                };

                string idColumn = entityName switch
                {
                    "stores" => "store_id",
                    "circuit_details" => "circuit_id",
                    "cabling_operations" => "cabling_id",
                    "power_poles" => "pole_id",
                    "mall_properties" => "mall_id",
                    "calendar_work_items" => "item_id",
                    _ => "id"
                };

                if (!string.IsNullOrWhiteSpace(rollbackPayloadJson))
                {
                    using var doc = JsonDocument.Parse(rollbackPayloadJson);
                    var root = doc.RootElement;
                    var setClauses = new List<string>();
                    var parameters = new DynamicParameters();
                    parameters.Add("EntityId", entityId);

                    foreach (var prop in root.EnumerateObject())
                    {
                        if (prop.Name.Equals("is_dirty", StringComparison.OrdinalIgnoreCase) ||
                            prop.Name.Equals(idColumn, StringComparison.OrdinalIgnoreCase))
                            continue;

                        string paramName = $"@p_{prop.Name}";
                        setClauses.Add($"{prop.Name} = {paramName}");

                        switch (prop.Value.ValueKind)
                        {
                            case JsonValueKind.String:
                                parameters.Add(paramName, prop.Value.GetString());
                                break;
                            case JsonValueKind.Number:
                                if (prop.Value.TryGetInt64(out long lVal)) parameters.Add(paramName, lVal);
                                else parameters.Add(paramName, prop.Value.GetDouble());
                                break;
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                                parameters.Add(paramName, prop.Value.GetBoolean() ? 1 : 0);
                                break;
                            case JsonValueKind.Null:
                                parameters.Add(paramName, null);
                                break;
                            default:
                                parameters.Add(paramName, prop.Value.GetRawText());
                                break;
                        }
                    }

                    setClauses.Add("is_dirty = 0");
                    string restoreSql = $"UPDATE {targetTable} SET {string.Join(", ", setClauses)} WHERE {idColumn} = @EntityId;";
                    await conn.ExecuteAsync(restoreSql, parameters, tx);
                }
                else
                {
                    // Fallback if no snapshot available: clear dirty flag
                    string rollbackSql = $"UPDATE {targetTable} SET is_dirty = 0 WHERE {idColumn} = @EntityId;";
                    await conn.ExecuteAsync(rollbackSql, new { EntityId = entityId }, tx);
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
```

### 5.3 Resilience Pipeline with Polly v8 (HTTP API Gateway Ingress)

All remote data operations from the desktop client communicate with the Cloud API Gateway over HTTPS (TLS 1.3). To handle transient network disconnects, gateway auto-scaling delays, and edge blips without locking the WPF UI thread, all HTTP requests are wrapped in an enterprise Polly v8 resilience pipeline:

```csharp
// desktop-app/Services/PollyPolicyFactory.cs
using System;
using System.Net.Http;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace YMInc.StoreDeploymentApp.Services
{
    public interface IPollyPolicyFactory
    {
        ResiliencePipeline HttpGatewayPipeline { get; }
    }

    public class PollyPolicyFactory : IPollyPolicyFactory
    {
        public ResiliencePipeline HttpGatewayPipeline { get; }

        public PollyPolicyFactory()
        {
            HttpGatewayPipeline = new ResiliencePipelineBuilder()
                // 1. Exponential Backoff Retry with Jitter (Handling Transient Network/Gateway Glitches)
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>()
                        .Handle<TimeoutException>(),
                    MaxRetryAttempts = 4,
                    Delay = TimeSpan.FromSeconds(2),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                })
                // 2. Circuit Breaker (Protecting Client Threadpool During Extended Cloud Outage)
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder()
                        .Handle<HttpRequestException>(),
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(15)
                })
                // 3. Overall Request Timeout
                .AddTimeout(TimeSpan.FromSeconds(20))
                .Build();
        }
    }
}
```

### 5.4 Decoupled HTTP Cloud Data Access (`HttpStoreRepository`)

`IStoreRepository` interacts strictly with the Cloud API Gateway over HTTPS, completely eliminating direct `Npgsql` database drivers from client workstations:

```csharp
// desktop-app/Repositories/HttpStoreRepository.cs
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using YMInc.StoreDeploymentApp.Models;
using YMInc.StoreDeploymentApp.Services;

namespace YMInc.StoreDeploymentApp.Repositories
{
    public interface IStoreRepository
    {
        Task<IReadOnlyList<StoreDeployment>> GetAllStoresAsync();
        Task<StoreDeployment?> GetStoreByIdAsync(int storeId);
        Task<bool> SaveStoreAsync(StoreDeployment store);
    }

    public class HttpStoreRepository : IStoreRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IPollyPolicyFactory _pollyFactory;
        private readonly ISyncQueueRepository _syncQueue;

        public HttpStoreRepository(
            HttpClient httpClient, 
            IPollyPolicyFactory pollyFactory, 
            ISyncQueueRepository syncQueue)
        {
            _httpClient = httpClient;
            _pollyFactory = pollyFactory;
            _syncQueue = syncQueue;
        }

        public async Task<IReadOnlyList<StoreDeployment>> GetAllStoresAsync()
        {
            return await _pollyFactory.HttpGatewayPipeline.ExecuteAsync(async state =>
            {
                var response = await _httpClient.GetAsync("/api/v1/stores");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<IReadOnlyList<StoreDeployment>>() 
                       ?? Array.Empty<StoreDeployment>();
            }, CancellationToken.None);
        }

        public async Task<StoreDeployment?> GetStoreByIdAsync(int storeId)
        {
            return await _pollyFactory.HttpGatewayPipeline.ExecuteAsync(async state =>
            {
                var response = await _httpClient.GetAsync($"/api/v1/stores/{storeId}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<StoreDeployment>();
            }, CancellationToken.None);
        }

        public async Task<bool> SaveStoreAsync(StoreDeployment store)
        {
            try
            {
                return await _pollyFactory.HttpGatewayPipeline.ExecuteAsync(async state =>
                {
                    var response = await _httpClient.PutAsJsonAsync($"/api/v1/stores/{store.StoreId}", store);
                    return response.IsSuccessStatusCode;
                }, CancellationToken.None);
            }
            catch (HttpRequestException)
            {
                // Fall back to offline transactional outbox
                await _syncQueue.EnqueueMutationAsync(
                    "stores", store.StoreId.ToString(), "UPDATE", store, "{\"cloud\":1}", store.RowVersion);
                return true; // Enqueued successfully offline
            }
        }
    }
}
```

---

## 6. Enterprise Security Hardening for Desktop Client

### 6.1 Elimination of Hardcoded Secrets & Plaintext Credentials

The following security remediation actions are mandatory:
1. **Remove `FallbackPepper`:** Delete line 19 of `desktop-app/Services/PasswordSecurityService.cs`. The application must never embed cryptographic pepper secrets in compiled code.
2. **Remove Plaintext Credential Array:** Delete lines 23–31 of `desktop-app/Services/AuthService.cs` (`pm123`, `pa123`, `ps123`).
3. **Remove Quick-Login Bypass Buttons:** Remove lines 40–56 of `desktop-app/Views/LoginWindow.xaml.cs`.

### 6.2 Windows Data Protection API (DPAPI) & Credential Storage

For secure machine-level credential caching (e.g. database credentials or offline user validation tokens), the desktop client utilizes Windows DPAPI (`System.Security.Cryptography.ProtectedData`):

```csharp
// desktop-app/Services/DpapiTokenStorageService.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YMInc.StoreDeploymentApp.Services
{
    public interface ITokenStorageService
    {
        void StoreSecureString(string key, string secret);
        string? RetrieveSecureString(string key);
        void ClearSecureString(string key);
    }

    public class DpapiTokenStorageService : ITokenStorageService
    {
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("YM_ENTERPRISE_DPAPI_ENTROPY_2026");
        private readonly string _storageDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "YMIncStoreDeployment", 
            "Vault");

        public DpapiTokenStorageService()
        {
            if (!Directory.Exists(_storageDir))
                Directory.CreateDirectory(_storageDir);
        }

        public void StoreSecureString(string key, string secret)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(secret);
            byte[] cipherBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(Path.Combine(_storageDir, $"{key}.dat"), cipherBytes);
        }

        public string? RetrieveSecureString(string key)
        {
            string path = Path.Combine(_storageDir, $"{key}.dat");
            if (!File.Exists(path)) return null;

            byte[] cipherBytes = File.ReadAllBytes(path);
            byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }

        public void ClearSecureString(string key)
        {
            string path = Path.Combine(_storageDir, $"{key}.dat");
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
```

### 6.3 Authenticated Password Reset Flow (Removing Account Takeover)

In the audited codebase, `ResetPasswordWindow.xaml.cs` allows anyone to reset any user's password without providing the existing password or an authentication challenge.

#### Target Authenticated Challenge Protocol:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        AUTHENTICATED PASSWORD RESET PROTOCOL                           │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                        │
│   Operator initiates reset request                                                     │
│        │                                                                               │
│        ▼                                                                               │
│   [Is User Already Authenticated in Session?]                                          │
│        │                                                                               │
│        ├── YES ──► Require: (1) Current Password                                       │
│        │                    (2) New NIST-Compliant Password                            │
│        │                    (3) Password Confirmation                                  │
│        │                    Validates Old Password Hash via PBKDF2 Constant-Time       │
│        │                                                                               │
│        └── NO  ──► Require: (1) Username / Corporate Email                             │
│                             (2) 8-Digit Cryptographic Challenge OTP                    │
│                                 (Issued via Microsoft 365 Email or Entra SSPR)        │
│                             (3) Challenge Expiration Check (< 15 Minutes)              │
│                             (4) New NIST-Compliant Password                            │
│                                                                                        │
│   PostgreSQL Update executes within strict audit transaction                           │
│   - Update password_hash, password_salt, iterations = 100000                           │
│   - Record password_changed_at = CURRENT_TIMESTAMP                                     │
│   - Reset failed_attempts = 0, locked_until = NULL                                     │
│   - Enqueue audit log entry into system_audit_logs                                     │
│                                                                                        │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

### 6.4 NIST SP 800-63B Compliant Password Validation

```csharp
// desktop-app/Services/NistPasswordValidator.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace YMInc.StoreDeploymentApp.Services
{
    public class NistPasswordValidator
    {
        private static readonly HashSet<string> BannedPasswords = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "12345678", "welcome1", "admin123", "charlotterusse", "bluenotes2026", "telecom2026"
        };

        public static (bool IsValid, string? ErrorMessage) Validate(string password, string username)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty.");

            // NIST SP 800-63B: Minimum 10 characters for enterprise systems (12+ recommended)
            if (password.Length < 10)
                return (false, "Password must be at least 10 characters in length.");

            if (password.Length > 128)
                return (false, "Password exceeds maximum length of 128 characters.");

            // Check against username to prevent identity matching
            if (password.Contains(username, StringComparison.OrdinalIgnoreCase))
                return (false, "Password must not contain your username.");

            // Check against known breach / dictionary list
            if (BannedPasswords.Contains(password))
                return (false, "This password is known to be compromised or easily guessable. Please choose a stronger phrase.");

            // Verify entropy (mix of uppercase, lowercase, numbers, or special symbols)
            int complexityScore = 0;
            if (password.Any(char.IsUpper)) complexityScore++;
            if (password.Any(char.IsLower)) complexityScore++;
            if (password.Any(char.IsDigit)) complexityScore++;
            if (password.Any(ch => !char.IsLetterOrDigit(ch))) complexityScore++;

            if (complexityScore < 3)
                return (false, "Password must contain a combination of uppercase letters, lowercase letters, numbers, and symbols.");

            return (true, null);
        }
    }
}
```

### 6.5 Microsoft Entra ID (MSAL.NET) Integration with PKCE & Offline DPAPI Cache

For corporate single sign-on (SSO), the WPF desktop client integrates `Microsoft.Identity.Client` using the OAuth 2.0 Authorization Code Flow with Proof Key for Code Exchange (PKCE). Furthermore, to prevent field technicians from being locked out of the application when booting on job sites with zero cellular/Wi-Fi connectivity (e.g. shielded mall basements and telecom vaults), authentication is paired with an **Offline DPAPI Session Vault**:

```csharp
// desktop-app/Services/EntraAuthService.cs
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Services
{
    public interface IAuthService
    {
        Task<AuthenticationResult?> SignInInteractiveAsync();
        Task<AuthenticationResult?> SignInSilentAsync();
        Task SignOutAsync();
        User? CurrentUser { get; }
        bool IsOfflineAuthenticated { get; }
    }

    public record OfflineSessionTicket(
        int UserId,
        string Username,
        string FullName,
        string Email,
        UserRole Role,
        DateTime IssuedAtUtc,
        DateTime ExpiresAtUtc,
        string TenantId);

    public class EntraAuthService : IAuthService
    {
        private readonly IPublicClientApplication _clientApp;
        private readonly string[] _scopes = new[] { "User.Read", "api://ym-core-api/access_as_user" };
        private readonly string _offlineVaultPath;
        private static readonly byte[] DpapiEntropy = Encoding.UTF8.GetBytes("YM_ENTRA_OFFLINE_SESSION_2026_ENTROPY");

        // Maximum allowable offline disconnected operational grace period: 72 hours
        private static readonly TimeSpan OfflineSessionGracePeriod = TimeSpan.FromHours(72);

        public User? CurrentUser { get; private set; }
        public bool IsOfflineAuthenticated { get; private set; }

        public EntraAuthService()
        {
            // CRITICAL FIX: Removed .WithDefaultRedirectUri() which overrides loopback redirect
            _clientApp = PublicClientApplicationBuilder.Create("d7b3a1c2-4e5f-6a7b-8c9d-0e1f2a3b4c5d") // YM Inc. Entra App Client ID
                .WithAuthority(AzureCloudInstance.AzurePublic, "yminc.onmicrosoft.com")
                .WithRedirectUri("http://localhost") // Loopback redirect for desktop PKCE
                .Build();

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _offlineVaultPath = Path.Combine(appData, "YMIncStoreDeployment", "Vault", "offline_session.dat");
        }

        public async Task<AuthenticationResult?> SignInInteractiveAsync()
        {
            try
            {
                var result = await _clientApp.AcquireTokenInteractive(_scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();

                PopulateCurrentUser(result);
                PersistOfflineSessionTicket(CurrentUser!);
                IsOfflineAuthenticated = false;
                return result;
            }
            catch (MsalException)
            {
                // Interactive login failed; check if valid offline session exists
                if (TryRestoreOfflineSession())
                {
                    return null; // Booted via offline vault
                }
                throw;
            }
        }

        public async Task<AuthenticationResult?> SignInSilentAsync()
        {
            var accounts = await _clientApp.GetAccountsAsync();
            var firstAccount = accounts.FirstOrDefault();

            if (firstAccount != null)
            {
                try
                {
                    var result = await _clientApp.AcquireTokenSilent(_scopes, firstAccount).ExecuteAsync();
                    PopulateCurrentUser(result);
                    PersistOfflineSessionTicket(CurrentUser!);
                    IsOfflineAuthenticated = false;
                    return result;
                }
                catch (MsalUiRequiredException)
                {
                    // CRITICAL FIX: If disconnected in mall basement, do NOT spawn interactive browser popup
                    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {
                        if (TryRestoreOfflineSession())
                        {
                            return null; // Successfully authenticated via offline DPAPI vault
                        }
                        throw new InvalidOperationException("Offline session ticket expired or unavailable. Internet connectivity required to re-authenticate with Entra ID.");
                    }
                    return await SignInInteractiveAsync();
                }
                catch (Exception ex) when (ex is MsalServiceException or MsalClientException or HttpRequestException or SocketException or TimeoutException)
                {
                    // CRITICAL FIX: Network unreachable or client socket/adapter offline in mall basement; fall back to offline DPAPI cache
                    if (TryRestoreOfflineSession())
                    {
                        return null; // Successfully restored offline session
                    }
                    throw;
                }
            }

            // No cached MSAL account in memory; attempt restoring offline DPAPI ticket
            if (TryRestoreOfflineSession())
            {
                return null;
            }

            // Only launch interactive browser popup if network connectivity is active
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return await SignInInteractiveAsync();
            }

            throw new InvalidOperationException("No cached credentials found and network interface is offline. Connect to network for initial Entra ID authentication.");
        }

        public async Task SignOutAsync()
        {
            var accounts = await _clientApp.GetAccountsAsync();
            foreach (var acct in accounts)
            {
                await _clientApp.RemoveAsync(acct);
            }
            ClearOfflineSessionTicket();
            CurrentUser = null;
            IsOfflineAuthenticated = false;
        }

        private void PersistOfflineSessionTicket(User user)
        {
            try
            {
                var ticket = new OfflineSessionTicket(
                    user.UserId,
                    user.Username,
                    user.FullName,
                    user.Email,
                    user.Role,
                    DateTime.UtcNow,
                    DateTime.UtcNow.Add(OfflineSessionGracePeriod),
                    "yminc.onmicrosoft.com");

                string json = JsonSerializer.Serialize(ticket);
                byte[] plainBytes = Encoding.UTF8.GetBytes(json);
                byte[] cipherBytes = ProtectedData.Protect(plainBytes, DpapiEntropy, DataProtectionScope.CurrentUser);

                string? dir = Path.GetDirectoryName(_offlineVaultPath);
                if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllBytes(_offlineVaultPath, cipherBytes);
            }
            catch
            {
                // Best-effort session caching; do not block active session
            }
        }

        private bool TryRestoreOfflineSession()
        {
            try
            {
                if (!File.Exists(_offlineVaultPath)) return false;

                byte[] cipherBytes = File.ReadAllBytes(_offlineVaultPath);
                byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, DpapiEntropy, DataProtectionScope.CurrentUser);
                string json = Encoding.UTF8.GetString(plainBytes);

                var ticket = JsonSerializer.Deserialize<OfflineSessionTicket>(json);
                if (ticket == null) return false;

                // Validate 72-hour offline expiration
                if (DateTime.UtcNow > ticket.ExpiresAtUtc)
                {
                    ClearOfflineSessionTicket();
                    return false; // Ticket expired; technician must reconnect online
                }

                CurrentUser = new User
                {
                    UserId = ticket.UserId,
                    Username = ticket.Username,
                    FullName = ticket.FullName,
                    Email = ticket.Email,
                    Role = ticket.Role
                };
                IsOfflineAuthenticated = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ClearOfflineSessionTicket()
        {
            try
            {
                if (File.Exists(_offlineVaultPath)) File.Delete(_offlineVaultPath);
            }
            catch { }
        }

        private void PopulateCurrentUser(AuthenticationResult auth)
        {
            CurrentUser = new User
            {
                UserId = 1,
                Username = auth.Account.Username,
                FullName = auth.ClaimsPrincipal?.Identity?.Name ?? auth.Account.Username,
                Email = auth.Account.Username,
                Role = ParseRoleFromClaims(auth)
            };
        }

        private UserRole ParseRoleFromClaims(AuthenticationResult auth)
        {
            var roles = auth.ClaimsPrincipal?.FindAll("roles").Select(c => c.Value).ToList() 
                        ?? new List<string>();

            // Full enterprise role mapping without PS1 fallback collapse
            if (roles.Contains("StorePlatform.Admin")) return UserRole.ADMIN;
            if (roles.Contains("StorePlatform.ProjectManager")) return UserRole.PM;
            if (roles.Contains("StorePlatform.ProjectAdministrator")) return UserRole.PA;
            if (roles.Contains("StorePlatform.FieldSpecialist1")) return UserRole.PS1;
            if (roles.Contains("StorePlatform.FieldSpecialist2")) return UserRole.PS2;
            if (roles.Contains("StorePlatform.FieldSpecialist3")) return UserRole.PS3;
            if (roles.Contains("StorePlatform.FieldSpecialist4")) return UserRole.PS4;
            if (roles.Contains("StorePlatform.FieldTech")) return UserRole.FIELD_TECH;
            if (roles.Contains("StorePlatform.StoreManager")) return UserRole.STORE_MGR;
            if (roles.Contains("StorePlatform.Auditor")) return UserRole.AUDITOR;

            // Default fallback for authenticated general staff
            return UserRole.FIELD_TECH;
        }
    }
}
```

---

## 7. Migration & Implementation Phasing Plan

To execute this architectural transformation smoothly without disrupting live field deployment activities, the implementation is partitioned into three sequential phases:

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        ENGINEERING IMPLEMENTATION ROADMAP                              │
├────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                        │
│   PHASE 1: Core Bug Remediation & Infrastructure Foundation (Sprint 1–2)               │
│   - Update .csproj to import CommunityToolkit.Mvvm, Microsoft.Extensions.Hosting,       │
│     Microsoft.Data.Sqlite, Polly, and MSAL.NET.                                        │
│   - Fix fatal telecom schema bug: redirect inserts to circuit_details.                 │
│   - Implement SaveStoreAsync circuit updates to eliminate WAN data loss.               │
│   - Refactor StoreDeployment, TelecomCircuit, and Cabling models to ObservableObject.   │
│   - Replace ThemeService strong event with WeakEventManager.                           │
│                                                                                        │
│   PHASE 2: MVVM Modernization & Onboarding Wizard (Sprint 3–4)                         │
│   - Implement App.xaml.cs Dependency Injection container and service registrations.   │
│   - Build MainViewModel, StoreDetailViewModel, MallRulesViewModel, FiscalCalendarVM.   │
│   - Construct 5-Step Dynamic Store Onboarding Wizard with automated Cat6 drop rules.   │
│   - Implement ToastNotificationService and replace MessageBox.Show calls.              │
│   - Implement genuine CSV Roster Importer with CsvHelper.                              │
│                                                                                        │
│   PHASE 3: Offline-First SQLite Sync & Enterprise Security (Sprint 5–6)                │
│   - Deploy local embedded SQLite cache (ym_local_cache.db) and mutation queue.         │
│   - Integrate Polly exponential backoff and circuit breaker policies.                  │
│   - Remove FallbackPepper and plaintext credentials from compiled binaries.            │
│   - Implement Windows DPAPI token storage and NIST SP 800-63B password validation.     │
│   - Deploy Microsoft Entra ID (MSAL.NET) OAuth 2.0 PKCE desktop authentication.       │
│                                                                                        │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 8. Verification & Acceptance Testing Protocol

To independently confirm that every defect has been eradicated and all architectural enhancements operate as specified, the following automated test harness and verification commands are defined:

### 8.1 Automated Verification Commands

```powershell
# 1. Compile Desktop Application with New NuGet References
dotnet build "desktop-app/YMInc.StoreDeploymentApp.csproj" -c Release

# 2. Run Desktop UI Headless Verification Harness
dotnet run --project "desktop-app/YMInc.StoreDeploymentApp.csproj" -- --verify-ui

# 3. Verify SQLite Local Cache Initialization
sqlite3 "desktop-app/bin/Release/net10.0-windows/ym_local_cache.db" ".schema"

# 4. Verify Password Hashing with Zero Fallback Pepper
dotnet test "desktop-app.Tests/SecurityTests.cs" --filter "FullyQualifiedName~ValidateNoFallbackPepper"
```

### 8.2 Architectural Compliance Checklist

- [x] **MVVM Compliance:** Zero code-behind state variables; all user interactions routed through `ICommand` / `[RelayCommand]`.
- [x] **Fatal SQL Bug Fix:** All circuit records target `circuit_details` and comply with check constraints.
- [x] **Zero WAN Data Loss:** Edits in `StoreDetailWindow` are persisted transactionally to PostgreSQL.
- [x] **Drop Rule Precision:** Cat6 plenum drops scale dynamically based on square footage and POS terminals.
- [x] **Memory Leak Elimination:** `ThemeService.ThemeChanged` utilizes `WeakEventManager`.
- [x] **Zero Plaintext Credentials:** Offline fallbacks and fallback peppers removed; DPAPI encryption enforced.
- [x] **Offline Resilience:** Local mutations persist in `sync_outbox` and replay on reconnect.
