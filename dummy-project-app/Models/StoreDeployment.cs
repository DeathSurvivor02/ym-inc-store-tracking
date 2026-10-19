using System;
using System.Collections.Generic;

namespace YMInc.StoreDeploymentApp.Models
{
    public class MallProperty
    {
        public int MallId { get; set; }
        public string MallCode { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string LandlordCompany { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string StateProvince { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = "CA";
        public bool CoiRequired { get; set; } = true;
        public decimal MinGeneralLiabilityCad { get; set; } = 5000000m;
        public string DeliveryWindow { get; set; } = "06:00 - 09:30";
        public bool NightWorkOnly { get; set; } = false; // After Hours Required
        public decimal BadgingFeePerTech { get; set; } = 25m;
        public string SpecialRules { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ManagementContactName { get; set; } = string.Empty;
        public string ManagementEmail { get; set; } = string.Empty;
        public string ManagementPhone { get; set; } = string.Empty;
        public string SecurityPhone { get; set; } = string.Empty;
        public string CablingVendor { get; set; } = string.Empty;
        public string? LeadTechnicianName { get; set; }
        public bool MallContacted { get; set; } = true;
        public string AssignedTeamMember { get; set; } = string.Empty;
        public string RequirementsCircuits { get; set; } = string.Empty;
        public string IntermediateSteps { get; set; } = string.Empty;

        public bool HasRequirements => !string.IsNullOrWhiteSpace(SpecialRules) || !string.IsNullOrWhiteSpace(RequirementsCircuits);
        public string ContactedDisplay
        {
            get => MallContacted ? "Contacted" : "Not Contacted";
            set => MallContacted = value != null && !value.Contains("Not", StringComparison.OrdinalIgnoreCase) && !value.Equals("No", StringComparison.OrdinalIgnoreCase);
        }
        public string RequirementsStatusDisplay
        {
            get => HasRequirements ? "Has Req." : (MallContacted ? "No Req. (Contacted)" : "Not Contacted");
            set
            {
                if (value != null && value.Contains("Has", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(SpecialRules))
                    SpecialRules = "Requirements verified";
            }
        }
        public string AfterHoursDisplay
        {
            get => NightWorkOnly ? "Yes" : "No";
            set => NightWorkOnly = value != null && (value.StartsWith("Y", StringComparison.OrdinalIgnoreCase) || value.Equals("True", StringComparison.OrdinalIgnoreCase));
        }
        public string CoiDisplay
        {
            get => CoiRequired ? "Yes" : "No";
            set => CoiRequired = value != null && (value.StartsWith("Y", StringComparison.OrdinalIgnoreCase) || value.Equals("True", StringComparison.OrdinalIgnoreCase));
        }
        public string ContactSummary => !string.IsNullOrEmpty(ManagementContactName) 
            ? $"{ManagementContactName} ({ManagementPhone})" 
            : (!string.IsNullOrEmpty(ManagementPhone) ? ManagementPhone : "N/A");
    }

    public class SiteSurvey
    {
        public int SurveyId { get; set; }
        public int StoreId { get; set; }
        public string SurveyVendor { get; set; } = string.Empty;
        public string SurveyorName { get; set; } = string.Empty;
        public string? SurveyorPhone { get; set; }
        public string ScheduledDate { get; set; } = string.Empty;
        public string? ActualDate { get; set; }
        public string Status { get; set; } = "Scheduled";
        public string CeilingType { get; set; } = string.Empty;
        public double CeilingHeightFeet { get; set; }
        public string DemarcLocation { get; set; } = string.Empty;
        public bool DemarcConduitAvailable { get; set; } = true;
        public string MdfRackLocation { get; set; } = "Back Office / Cash Wrap";
        public bool MdfRackSpaceAdequate { get; set; } = true;
        public int DedicatedCircuitsCount { get; set; } = 2;
        public string StructureNotes { get; set; } = string.Empty;
    }

    public class PowerPole
    {
        public int PowerPoleId { get; set; }
        public int StoreId { get; set; }
        public string Status { get; set; } = "Survey Required";
        public int TotalRequired { get; set; } = 1;
        public int InstalledCount { get; set; } = 0;
        public string Manufacturer { get; set; } = "Wiremold / Legrand";
        public string ModelNumber { get; set; } = "25DTP-4";
        public string FinishColor { get; set; } = "Anodized Aluminum";
        public string CeilingMountType { get; set; } = "T-Bar Grid Clamp with Structural Tie-Wire";
        public string? ElectricalContractor { get; set; }
        public int CashWrapFeedDrops { get; set; } = 4;
        public string? ScheduledInstallDate { get; set; }
        public string? ActualInstallDate { get; set; }
        public string? BlockerReason { get; set; }
    }

    public class CablingOperation
    {
        public int CablingId { get; set; }
        public int StoreId { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public string? LeadTechName { get; set; }
        public string Status { get; set; } = "Vendor Assigned";
        public string CableGrade { get; set; } = "Cat6 Plenum (CMP)";
        public int TotalDataDrops { get; set; }
        public int PosDrops { get; set; } = 4;
        public int ApDrops { get; set; } = 4;
        public int BackOfficeDrops { get; set; } = 4;
        public int CctvDrops { get; set; } = 8;
        public bool FlukeCertified { get; set; } = false;
        public string ScheduledStartDate { get; set; } = string.Empty;
        public string ScheduledCompletionDate { get; set; } = string.Empty;
        public string? ActualCompletionDate { get; set; }
    }

    public class TelecomCircuit
    {
        public int CircuitId { get; set; }
        public int StoreId { get; set; }
        public string Role { get; set; } = "Primary WAN"; // Primary WAN, Secondary Failover WAN
        public string CarrierName { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = "Order Placed";
        public int BandwidthDownMbps { get; set; }
        public int BandwidthUpMbps { get; set; }
        public string? FocDate { get; set; }
        public string? TurnupDate { get; set; }
        public bool DemarcExtended { get; set; }
        public string? JeopardyReason { get; set; }
    }

    public class StoreDeployment
    {
        public int StoreId { get; set; }
        public string StoreNumber { get; set; } = string.Empty;
        public string BannerCode { get; set; } = string.Empty;
        public string BannerName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string ProjectType { get; set; } = "Active Fleet";
        public string OperationalStatus { get; set; } = "Grand Open";
        public string OverallHealth { get; set; } = "On Track";
        public string CountryCode { get; set; } = "CA";
        public string Region { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string StateProvince { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public int GrossSqft { get; set; }
        public int SalesSqft { get; set; }
        public int PosTerminalCount { get; set; } = 4;
        public string PosStagingStatus { get; set; } = "Commissioned";
        public string? TargetOpenDate { get; set; }
        public string? OpeningDate { get; set; }

        public MallProperty? Mall { get; set; }
        public SiteSurvey? Survey { get; set; }
        public PowerPole? PowerPoles { get; set; }
        public CablingOperation? Cabling { get; set; }
        public List<TelecomCircuit> Circuits { get; set; } = new();

        public int WeeklyCompletionPct { get; set; } = 50;
        public string WeeklyStatus { get; set; } = "In Progress / On Schedule";
        public string WeeklySummaryNote { get; set; } = string.Empty;

        // Quick UI helpers (with setters for direct DataGrid cell editing)
        public string CountryFlag
        {
            get => CountryCode;
            set => CountryCode = (value ?? "CA").Trim().ToUpperInvariant();
        }
        public string MallName
        {
            get => Mall?.PropertyName ?? "Standalone / Street Front";
            set
            {
                Mall ??= new MallProperty { MallCode = $"M-{StoreNumber}", City = City, StateProvince = StateProvince, CountryCode = CountryCode };
                Mall.PropertyName = (value ?? "").Trim();
            }
        }
        public string CityState
        {
            get => $"{City}, {StateProvince}";
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                var parts = value.Split(',');
                City = parts[0].Trim();
                if (parts.Length > 1) StateProvince = parts[1].Trim();
            }
        }
        public string FullAddress => $"{Address}, {City}, {StateProvince} {PostalCode}";
        public string? PhoneNumber { get; set; }
        public string? PrimaryContactName { get; set; }
        public string? PrimaryContactEmail { get; set; }
        public string? PrimaryContactPhone { get; set; }
        public string StoreContactSummary => !string.IsNullOrEmpty(PrimaryContactName)
            ? $"{PrimaryContactName} ({PrimaryContactPhone ?? PhoneNumber ?? "Direct"})"
            : (!string.IsNullOrEmpty(PhoneNumber) ? PhoneNumber : "None Listed");
        private string? _powerPoleSummaryOverride;
        public string PowerPoleSummary
        {
            get => _powerPoleSummaryOverride ?? (PowerPoles != null ? $"{PowerPoles.InstalledCount}/{PowerPoles.TotalRequired} Installed" : "N/A");
            set
            {
                _powerPoleSummaryOverride = value;
                if (PowerPoles != null && !string.IsNullOrWhiteSpace(value))
                {
                    var slash = value.Split('/', ' ');
                    if (slash.Length >= 2 && int.TryParse(slash[0], out int inst) && int.TryParse(slash[1], out int tot))
                    {
                        PowerPoles.InstalledCount = inst;
                        PowerPoles.TotalRequired = tot;
                        _powerPoleSummaryOverride = null;
                    }
                }
            }
        }
        private string? _cablingSummaryOverride;
        public string CablingSummary
        {
            get => _cablingSummaryOverride ?? (Cabling != null ? $"{Cabling.TotalDataDrops} Drops ({(Cabling.FlukeCertified ? "Certified" : "Pending")})" : "N/A");
            set
            {
                _cablingSummaryOverride = value;
                if (Cabling != null && !string.IsNullOrWhiteSpace(value))
                {
                    var parts = value.Split(' ');
                    if (int.TryParse(parts[0], out int drops))
                    {
                        Cabling.TotalDataDrops = drops;
                    }
                    Cabling.FlukeCertified = value.Contains("Cert", StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        public string CablingVendorDisplay => Cabling != null && !string.IsNullOrEmpty(Cabling.Vendor) ? Cabling.Vendor : (!string.IsNullOrEmpty(Mall?.CablingVendor) ? Mall.CablingVendor : "Unassigned");
        private string? _circuitSummaryOverride;
        public string CircuitSummary
        {
            get => _circuitSummaryOverride ?? (Circuits.Count > 0 ? $"{Circuits.Count} Active" : "None");
            set => _circuitSummaryOverride = value;
        }

        // Quick-Inspector properties
        public string DeliveryOptionDisplay => Mall?.DeliveryWindow ?? "Standard Dock (08:00 - 17:00)";
        public string SiteSurveyStatus => Survey?.Status ?? "Scheduled";
        public string PowerPoleCondition => PowerPoles?.Status ?? "Survey Required";
        public int PowerPolesCount => PowerPoles?.TotalRequired ?? 1;
        public int CablingCat6Drops => Cabling?.TotalDataDrops ?? 16;
        public string PatchPanelType => Cabling?.CableGrade ?? "Cat6 Plenum";
        public string PrimaryCircuitVendor => Circuits.Count > 0 ? Circuits[0].CarrierName : "Pending Carrier";
        public string PrimaryCircuitStatus => Circuits.Count > 0 ? Circuits[0].OrderStatus : "Pending Order";
        public string TechnicalSummaryNote => WeeklySummaryNote;
    }
}
