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
        public string CountryCode { get; set; } = "CA";
        public bool UnionLaborRequired { get; set; }
        public string? UnionLocalDesignation { get; set; }
        public bool CoiRequired { get; set; } = true;
        public decimal MinGeneralLiabilityCad { get; set; } = 5000000m;
        public string DeliveryWindow { get; set; } = "06:00 - 09:30";
        public bool NightWorkOnly { get; set; }
        public decimal BadgingFeePerTech { get; set; } = 25m;
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
        public string StoreNumber { get; set; } = string.Empty; // 6xxx for CA, 6000s for US
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

        // Quick UI helpers
        public string CountryFlag => CountryCode == "CA" ? "🇨🇦" : "🇺🇸";
        public string PowerPoleSummary => PowerPoles != null ? $"{PowerPoles.InstalledCount}/{PowerPoles.TotalRequired} Installed" : "N/A";
        public string CablingSummary => Cabling != null ? $"{Cabling.TotalDataDrops} Drops ({(Cabling.FlukeCertified ? "Fluke Pass" : "Pending")})" : "N/A";
        public string CircuitSummary => Circuits.Count > 0 ? $"{Circuits.Count} WAN Circuits" : "None";
    }
}
