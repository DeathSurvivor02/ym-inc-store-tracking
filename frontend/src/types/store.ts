export type CountryCode = 'CA' | 'US';

export type BrandCode = 'UP' | 'BN' | 'SS' | 'LC' | 'SRN' | 'F21' | 'UK' | 'W49' | 'STN' | 'UB' | 'CR' | 'R21' | 'M';

export type ProjectType = 
  | 'Active Fleet'
  | 'New Store Opening'
  | 'Major Remodel'
  | 'Minor Refresh'
  | 'Brand Conversion'
  | 'Relocation'
  | 'Store Closure';

export type OperationalStatus = 
  | 'Planning'
  | 'Lease Execution'
  | 'Site Survey'
  | 'Permitting & Design'
  | 'GC Construction'
  | 'Low-Voltage & Power Poles'
  | 'IT Staging & Telecom'
  | 'Merchandising'
  | 'Pre-Opening QA'
  | 'Grand Open'
  | 'Temporary Closed'
  | 'Renovating'
  | 'Decommissioned';

export type HealthStatus = 'On Track' | 'At Risk' | 'Critical Blocker' | 'Complete';

export interface Banner {
  bannerId: number;
  bannerCode: BrandCode;
  bannerName: string;
  brandTier: string;
  targetDemographic: string;
}

export interface MallProperty {
  mallId: number;
  mallCode: string;
  propertyName: string;
  landlordCompany: string;
  propertyType: string;
  city: string;
  stateProvince: string;
  countryCode: CountryCode;
  unionLaborRequired: boolean;
  unionLocalDesignation?: string;
  coiRequired: boolean;
  minGeneralLiabilityCad: number;
  deliveryWindow: string;
  nightWorkOnly: boolean;
  badgingFeePerTech: number;
}

export interface SiteSurvey {
  surveyVendor: string;
  surveyorName: string;
  surveyorPhone?: string;
  scheduledDate: string;
  actualDate?: string;
  status: 'Scheduled' | 'In Progress' | 'Completed & Approved' | 'Conditional Pass' | 'Action Required / Failed';
  ceilingType: string;
  ceilingHeightFeet: number;
  demarcLocation: string;
  demarcConduitAvailable: boolean;
  mdfRackLocation: string;
  mdfRackSpaceAdequate: boolean;
  dedicatedCircuitsCount: number;
  photoUrl?: string;
}

export interface PowerPole {
  status: 
    | 'Not Required (Wall / Floor Feed)'
    | 'Survey Required'
    | 'Engineering Design'
    | 'Equipment Ordered'
    | 'Delivered On Site'
    | 'Installed & Anchored'
    | 'Passed Electrical Inspection'
    | 'Deficiency Hold';
  totalRequired: number;
  installedCount: number;
  manufacturer: string;
  modelNumber: string;
  finishColor: 'Anodized Aluminum' | 'Matte Black' | 'Custom White' | 'Industrial Grey';
  ceilingMountType: string;
  electricalContractor?: string;
  cashWrapFeedDrops: number;
  scheduledInstallDate?: string;
  actualInstallDate?: string;
  electricalInspectionDate?: string;
  blockerReason?: string;
}

export interface CablingOperation {
  vendor: string;
  leadTechName?: string;
  status: 
    | 'Vendor Assigned'
    | 'Pathway & J-Hooks Installed'
    | 'Cable Pull In Progress'
    | 'Termination & Patch Panels'
    | 'Fluke Testing & Certification'
    | 'Punch List Remediation'
    | 'Completed & Certified'
    | 'Delayed / Access Blocked';
  cableGrade: 'Cat6 Riser (CMR)' | 'Cat6 Plenum (CMP)' | 'Cat6A Plenum (CMP)';
  totalDataDrops: number;
  posDrops: number;
  apDrops: number;
  backOfficeDrops: number;
  cctvDrops: number;
  flukeCertified: boolean;
  scheduledStartDate: string;
  scheduledCompletionDate: string;
  actualCompletionDate?: string;
  punchListItems?: string;
}

export interface TelecomCircuit {
  circuitId: number;
  role: 'Primary WAN' | 'Secondary Failover WAN' | 'Out-of-Band Mgt';
  carrierName: string;
  serviceType: string;
  orderStatus: 
    | 'Order Placed'
    | 'Site Survey / Engineering'
    | 'Permit Pending'
    | 'FOC Confirmed'
    | 'Dispatched / In Construction'
    | 'Demarc Extension Required'
    | 'Tested & Turned Up (Active)'
    | 'Carrier Delay / Jeopardy';
  bandwidthDownMbps: number;
  bandwidthUpMbps: number;
  focDate?: string;
  turnupDate?: string;
  demarcExtended: boolean;
  jeopardyReason?: string;
}

export interface StoreDeployment {
  storeId: number;
  storeNumber: string; // 6xxx for CA, 6000s for US
  bannerCode: BrandCode;
  bannerName: string;
  storeName: string;
  projectType: ProjectType;
  operationalStatus: OperationalStatus;
  overallHealth: HealthStatus;
  countryCode: CountryCode;
  region: string;
  district: string;
  address: string;
  city: string;
  stateProvince: string;
  postalCode: string;
  grossSqft: number;
  salesSqft: number;
  posTerminalCount: number;
  posStagingStatus: 'Pending' | 'Hardware Staged' | 'Configured' | 'Shipped' | 'On Site' | 'Commissioned';
  targetOpenDate?: string;
  openingDate?: string;
  
  mall?: MallProperty;
  survey?: SiteSurvey;
  powerPoles?: PowerPole;
  cabling?: CablingOperation;
  circuits: TelecomCircuit[];
  
  // Weekly progress log for selected week
  weeklyProgress: {
    fiscalYear: number;
    fiscalWeek: number;
    completionPct: number;
    weeklyStatus: 'Completed & Certified' | 'In Progress / On Schedule' | 'Scheduled / Pending' | 'Blocked / In Jeopardy';
    summaryNote: string;
  };
}

export interface WeeklyProgressReport {
  fiscalYear: number;
  fiscalWeek: number;
  weekLabel: string;
  dateRange: string;
  totalActiveProjects: number;
  completedThisWeek: number;
  inProgress: number;
  scheduledPending: number;
  blockedInJeopardy: number;
  overallProgressPct: number;
  milestones: {
    name: string;
    completed: number;
    total: number;
    pct: number;
  }[];
}
