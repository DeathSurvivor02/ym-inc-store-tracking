import { useState } from 'react';
import type { StoreDeployment } from '../types/store';
import { 
  X, 
  Building2, 
  MapPin, 
  Zap, 
  Cable, 
  Radio, 
  ShieldAlert, 
  AlertTriangle, 
  FileText,
  Layers
} from 'lucide-react';

interface StoreDetailModalProps {
  store: StoreDeployment | null;
  onClose: () => void;
}

export const StoreDetailModal: React.FC<StoreDetailModalProps> = ({ store, onClose }) => {
  const [activeTab, setActiveTab] = useState<'survey' | 'powerPoles' | 'cabling' | 'telecom' | 'mall'>('survey');

  if (!store) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-200">
      <div 
        className="bg-slate-900 border border-slate-700/80 rounded-2xl w-full max-w-4xl max-h-[90vh] flex flex-col shadow-2xl overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Modal Header */}
        <div className="p-6 bg-slate-950/80 border-b border-slate-800 flex items-start justify-between">
          <div className="flex items-start gap-4">
            <div className="w-12 h-12 rounded-xl bg-blue-600/20 border border-blue-500/40 flex items-center justify-center text-blue-400 font-bold text-lg shrink-0">
              {store.bannerCode}
            </div>
            <div>
              <div className="flex items-center gap-2 flex-wrap">
                <span className="font-mono text-xl font-extrabold text-white">
                  Store #{store.storeNumber}
                </span>
                <span className="text-xs font-semibold px-2 py-0.5 rounded bg-blue-900/60 text-blue-300 border border-blue-700/50">
                  {store.bannerName}
                </span>
                <span className="text-xs text-slate-400">
                  {store.countryCode === 'CA' ? '🇨🇦 Canada' : '🇺🇸 United States'}
                </span>
                <span className={`text-xs px-2.5 py-0.5 rounded-full font-semibold ${
                  store.overallHealth === 'On Track' ? 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30' :
                  store.overallHealth === 'At Risk' ? 'bg-amber-500/20 text-amber-400 border border-amber-500/30' :
                  'bg-rose-500/20 text-rose-400 border border-rose-500/30'
                }`}>
                  {store.overallHealth}
                </span>
              </div>
              <h2 className="text-base font-semibold text-slate-200 mt-1">
                {store.storeName}
              </h2>
              <p className="text-xs text-slate-400 flex items-center gap-1 mt-0.5">
                <MapPin className="w-3.5 h-3.5 text-slate-500" />
                {store.address}, {store.city}, {store.stateProvince} {store.postalCode}
              </p>
            </div>
          </div>

          <button
            onClick={onClose}
            className="p-1.5 rounded-lg bg-slate-800 text-slate-400 hover:text-white hover:bg-slate-700 transition-all cursor-pointer"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Technical Specification Tabs */}
        <div className="flex border-b border-slate-800 bg-slate-950/40 px-6 gap-2 text-xs overflow-x-auto">
          <button
            onClick={() => setActiveTab('survey')}
            className={`py-3 px-3 font-semibold border-b-2 flex items-center gap-1.5 transition-all shrink-0 cursor-pointer ${
              activeTab === 'survey'
                ? 'border-blue-500 text-blue-400'
                : 'border-transparent text-slate-400 hover:text-slate-200'
            }`}
          >
            <FileText className="w-4 h-4" /> Site Survey
          </button>
          <button
            onClick={() => setActiveTab('powerPoles')}
            className={`py-3 px-3 font-semibold border-b-2 flex items-center gap-1.5 transition-all shrink-0 cursor-pointer ${
              activeTab === 'powerPoles'
                ? 'border-amber-500 text-amber-400'
                : 'border-transparent text-slate-400 hover:text-slate-200'
            }`}
          >
            <Zap className="w-4 h-4" /> Wiremold Power Poles
          </button>
          <button
            onClick={() => setActiveTab('cabling')}
            className={`py-3 px-3 font-semibold border-b-2 flex items-center gap-1.5 transition-all shrink-0 cursor-pointer ${
              activeTab === 'cabling'
                ? 'border-sky-500 text-sky-400'
                : 'border-transparent text-slate-400 hover:text-slate-200'
            }`}
          >
            <Cable className="w-4 h-4" /> Cat6 Cabling & Drops
          </button>
          <button
            onClick={() => setActiveTab('telecom')}
            className={`py-3 px-3 font-semibold border-b-2 flex items-center gap-1.5 transition-all shrink-0 cursor-pointer ${
              activeTab === 'telecom'
                ? 'border-purple-500 text-purple-400'
                : 'border-transparent text-slate-400 hover:text-slate-200'
            }`}
          >
            <Radio className="w-4 h-4" /> Telecom Circuits & WAN
          </button>
          <button
            onClick={() => setActiveTab('mall')}
            className={`py-3 px-3 font-semibold border-b-2 flex items-center gap-1.5 transition-all shrink-0 cursor-pointer ${
              activeTab === 'mall'
                ? 'border-emerald-500 text-emerald-400'
                : 'border-transparent text-slate-400 hover:text-slate-200'
            }`}
          >
            <ShieldAlert className="w-4 h-4" /> Mall Landlord Rules
          </button>
        </div>

        {/* Tab Content Body */}
        <div className="p-6 overflow-y-auto flex-1 space-y-6 text-xs">
          
          {/* TAB 1: SITE SURVEY */}
          {activeTab === 'survey' && (
            <div className="space-y-4">
              {store.survey ? (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="bg-slate-950 p-4 rounded-xl border border-slate-800 space-y-3">
                    <h4 className="text-sm font-bold text-white flex items-center gap-2">
                      <Layers className="w-4 h-4 text-blue-400" /> Ceiling & Structural Specifications
                    </h4>
                    <div className="space-y-2 text-slate-300">
                      <div>
                        <span className="text-slate-500">Ceiling Type:</span>{' '}
                        <strong className="text-white">{store.survey.ceilingType}</strong>
                      </div>
                      <div>
                        <span className="text-slate-500">Clear Ceiling Height:</span>{' '}
                        <strong className="text-white font-mono">{store.survey.ceilingHeightFeet} ft</strong>
                      </div>
                      <div>
                        <span className="text-slate-500">Survey Status:</span>{' '}
                        <span className="text-emerald-400 font-semibold">{store.survey.status}</span>
                      </div>
                      <div>
                        <span className="text-slate-500">Surveyor:</span>{' '}
                        <strong className="text-white">{store.survey.surveyorName}</strong> ({store.survey.surveyVendor})
                      </div>
                      <div>
                        <span className="text-slate-500">Completed Date:</span>{' '}
                        <span className="font-mono text-slate-200">{store.survey.actualDate || store.survey.scheduledDate}</span>
                      </div>
                    </div>
                  </div>

                  <div className="bg-slate-950 p-4 rounded-xl border border-slate-800 space-y-3">
                    <h4 className="text-sm font-bold text-white flex items-center gap-2">
                      <Building2 className="w-4 h-4 text-emerald-400" /> Demarc & MDF Infrastructure
                    </h4>
                    <div className="space-y-2 text-slate-300">
                      <div>
                        <span className="text-slate-500">Demarc Location:</span>{' '}
                        <strong className="text-white">{store.survey.demarcLocation}</strong>
                      </div>
                      <div>
                        <span className="text-slate-500">Demarc Conduit Path:</span>{' '}
                        <span className={store.survey.demarcConduitAvailable ? 'text-emerald-400 font-semibold' : 'text-rose-400 font-semibold'}>
                          {store.survey.demarcConduitAvailable ? '✓ Conduit Clear & Available' : '✗ Blocked / Missing'}
                        </span>
                      </div>
                      <div>
                        <span className="text-slate-500">MDF Rack Location:</span>{' '}
                        <strong className="text-white">{store.survey.mdfRackLocation}</strong>
                      </div>
                      <div>
                        <span className="text-slate-500">MDF Rack Space:</span>{' '}
                        <span className="text-emerald-400 font-semibold">✓ Adequate</span>
                      </div>
                      <div>
                        <span className="text-slate-500">Dedicated Rack Circuits:</span>{' '}
                        <strong className="text-white font-mono">{store.survey.dedicatedCircuitsCount} Dedicated 120V 20A feeds</strong>
                      </div>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="p-8 text-center text-slate-500 bg-slate-950 rounded-xl border border-slate-800">
                  No site survey data recorded yet for this store.
                </div>
              )}
            </div>
          )}

          {/* TAB 2: POWER POLES */}
          {activeTab === 'powerPoles' && (
            <div className="space-y-4">
              {store.powerPoles ? (
                <div className="bg-slate-950 p-5 rounded-xl border border-slate-800 space-y-4">
                  <div className="flex items-center justify-between pb-3 border-b border-slate-800">
                    <div>
                      <h4 className="text-sm font-bold text-white">
                        Wiremold / Legrand Model {store.powerPoles.modelNumber}
                      </h4>
                      <p className="text-slate-400 text-[11px]">
                        Dual-channel isolated power poles feeding cash wrap POS registers.
                      </p>
                    </div>
                    <span className="px-3 py-1 rounded-full font-semibold text-amber-400 bg-amber-500/10 border border-amber-500/30">
                      {store.powerPoles.status}
                    </span>
                  </div>

                  <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                    <div className="bg-slate-900 p-3 rounded-lg border border-slate-800">
                      <span className="text-slate-500 text-[10px] uppercase font-bold">Total Required</span>
                      <p className="text-xl font-bold font-mono text-white mt-0.5">{store.powerPoles.totalRequired}</p>
                    </div>
                    <div className="bg-slate-900 p-3 rounded-lg border border-slate-800">
                      <span className="text-slate-500 text-[10px] uppercase font-bold">Installed & Anchored</span>
                      <p className="text-xl font-bold font-mono text-emerald-400 mt-0.5">{store.powerPoles.installedCount}</p>
                    </div>
                    <div className="bg-slate-900 p-3 rounded-lg border border-slate-800">
                      <span className="text-slate-500 text-[10px] uppercase font-bold">Finish Color</span>
                      <p className="text-sm font-bold text-slate-200 mt-1">{store.powerPoles.finishColor}</p>
                    </div>
                    <div className="bg-slate-900 p-3 rounded-lg border border-slate-800">
                      <span className="text-slate-500 text-[10px] uppercase font-bold">Cash Wrap Feed Drops</span>
                      <p className="text-xl font-bold font-mono text-blue-400 mt-0.5">{store.powerPoles.cashWrapFeedDrops}</p>
                    </div>
                  </div>

                  <div className="space-y-2 pt-2 text-slate-300">
                    <div>
                      <span className="text-slate-500">Ceiling Mount Type:</span>{' '}
                      <strong className="text-white">{store.powerPoles.ceilingMountType}</strong>
                    </div>
                    {store.powerPoles.electricalContractor && (
                      <div>
                        <span className="text-slate-500">Electrical Contractor:</span>{' '}
                        <strong className="text-white">{store.powerPoles.electricalContractor}</strong>
                      </div>
                    )}
                    {store.powerPoles.scheduledInstallDate && (
                      <div>
                        <span className="text-slate-500">Scheduled Install:</span>{' '}
                        <span className="font-mono text-slate-200">{store.powerPoles.scheduledInstallDate}</span>
                      </div>
                    )}
                    {store.powerPoles.blockerReason && (
                      <div className="p-3 bg-rose-950/40 border border-rose-900/60 rounded-lg text-rose-300 font-medium flex items-start gap-2">
                        <AlertTriangle className="w-4 h-4 text-rose-400 shrink-0 mt-0.5" />
                        <div>
                          <strong>Blocker Alert:</strong> {store.powerPoles.blockerReason}
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              ) : (
                <div className="p-8 text-center text-slate-500 bg-slate-950 rounded-xl border border-slate-800">
                  No power pole requirements recorded.
                </div>
              )}
            </div>
          )}

          {/* TAB 3: CABLING OPERATIONS */}
          {activeTab === 'cabling' && (
            <div className="space-y-4">
              {store.cabling ? (
                <div className="bg-slate-950 p-5 rounded-xl border border-slate-800 space-y-4">
                  <div className="flex items-center justify-between pb-3 border-b border-slate-800">
                    <div>
                      <h4 className="text-sm font-bold text-white">
                        Structured Low-Voltage Cabling ({store.cabling.cableGrade})
                      </h4>
                      <p className="text-slate-400 text-[11px]">
                        Vendor: <strong className="text-slate-200">{store.cabling.vendor}</strong> | Lead Tech: {store.cabling.leadTechName || 'TBD'}
                      </p>
                    </div>
                    <span className="px-3 py-1 rounded-full font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/30">
                      {store.cabling.status}
                    </span>
                  </div>

                  <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
                    <div className="bg-slate-900 p-2.5 rounded-lg border border-slate-800 text-center">
                      <span className="text-[10px] text-slate-500 uppercase font-bold">Total Drops</span>
                      <p className="text-lg font-bold font-mono text-white">{store.cabling.totalDataDrops}</p>
                    </div>
                    <div className="bg-slate-900 p-2.5 rounded-lg border border-slate-800 text-center">
                      <span className="text-[10px] text-slate-500 uppercase font-bold">POS Drops</span>
                      <p className="text-lg font-bold font-mono text-emerald-400">{store.cabling.posDrops}</p>
                    </div>
                    <div className="bg-slate-900 p-2.5 rounded-lg border border-slate-800 text-center">
                      <span className="text-[10px] text-slate-500 uppercase font-bold">AP Wireless</span>
                      <p className="text-lg font-bold font-mono text-sky-400">{store.cabling.apDrops}</p>
                    </div>
                    <div className="bg-slate-900 p-2.5 rounded-lg border border-slate-800 text-center">
                      <span className="text-[10px] text-slate-500 uppercase font-bold">CCTV Cameras</span>
                      <p className="text-lg font-bold font-mono text-purple-400">{store.cabling.cctvDrops}</p>
                    </div>
                    <div className="bg-slate-900 p-2.5 rounded-lg border border-slate-800 text-center">
                      <span className="text-[10px] text-slate-500 uppercase font-bold">Fluke Certified</span>
                      <p className="text-xs font-bold mt-1">
                        {store.cabling.flukeCertified ? (
                          <span className="text-emerald-400">✓ Certified</span>
                        ) : (
                          <span className="text-amber-400">Pending</span>
                        )}
                      </p>
                    </div>
                  </div>

                  <div className="text-slate-300 space-y-1 pt-2">
                    <div>
                      <span className="text-slate-500">Scheduled Schedule:</span>{' '}
                      <span className="font-mono text-slate-200">
                        {store.cabling.scheduledStartDate} → {store.cabling.scheduledCompletionDate}
                      </span>
                    </div>
                    {store.cabling.actualCompletionDate && (
                      <div>
                        <span className="text-slate-500">Actual Completion Date:</span>{' '}
                        <span className="font-mono text-emerald-400">{store.cabling.actualCompletionDate}</span>
                      </div>
                    )}
                  </div>
                </div>
              ) : (
                <div className="p-8 text-center text-slate-500 bg-slate-950 rounded-xl border border-slate-800">
                  No cabling operations recorded.
                </div>
              )}
            </div>
          )}

          {/* TAB 4: TELECOM WAN CIRCUITS */}
          {activeTab === 'telecom' && (
            <div className="space-y-4">
              <div className="space-y-3">
                {store.circuits.map((c, i) => (
                  <div key={i} className="bg-slate-950 p-4 rounded-xl border border-slate-800 space-y-2">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <span className={`px-2 py-0.5 rounded text-xs font-semibold ${
                          c.role === 'Primary WAN' ? 'bg-blue-900/60 text-blue-300' : 'bg-purple-900/60 text-purple-300'
                        }`}>
                          {c.role}
                        </span>
                        <strong className="text-white text-sm">{c.carrierName}</strong>
                        <span className="text-slate-400">({c.serviceType})</span>
                      </div>
                      <span className={`px-2.5 py-0.5 rounded-full text-xs font-semibold ${
                        c.orderStatus.includes('Active') || c.orderStatus.includes('Turned Up')
                          ? 'bg-emerald-500/20 text-emerald-400'
                          : 'bg-blue-500/20 text-blue-400'
                      }`}>
                        {c.orderStatus}
                      </span>
                    </div>

                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 pt-2 text-slate-300 text-[11px]">
                      <div>
                        <span className="text-slate-500">Bandwidth:</span>{' '}
                        <strong className="text-emerald-400 font-mono">{c.bandwidthDownMbps}</strong> /{' '}
                        <strong className="text-blue-400 font-mono">{c.bandwidthUpMbps}</strong> Mbps
                      </div>
                      <div>
                        <span className="text-slate-500">Demarc Extended:</span>{' '}
                        <strong className={c.demarcExtended ? 'text-emerald-400' : 'text-amber-400'}>
                          {c.demarcExtended ? '✓ Yes' : '✗ Required'}
                        </strong>
                      </div>
                      {c.focDate && (
                        <div>
                          <span className="text-slate-500">FOC Date:</span>{' '}
                          <span className="font-mono text-slate-200">{c.focDate}</span>
                        </div>
                      )}
                      {c.turnupDate && (
                        <div>
                          <span className="text-slate-500">Turn-Up Date:</span>{' '}
                          <span className="font-mono text-emerald-400">{c.turnupDate}</span>
                        </div>
                      )}
                    </div>

                    {c.jeopardyReason && (
                      <div className="p-2.5 bg-rose-950/40 border border-rose-900/60 rounded-lg text-rose-300 text-xs mt-2">
                        <strong>Jeopardy Reason:</strong> {c.jeopardyReason}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* TAB 5: MALL LANDLORD RULES */}
          {activeTab === 'mall' && (
            <div className="space-y-4">
              {store.mall ? (
                <div className="bg-slate-950 p-5 rounded-xl border border-slate-800 space-y-4">
                  <div className="flex items-center justify-between pb-3 border-b border-slate-800">
                    <div>
                      <h4 className="text-sm font-bold text-white">{store.mall.propertyName}</h4>
                      <p className="text-slate-400 text-[11px]">
                        Landlord Company: <strong className="text-slate-200">{store.mall.landlordCompany}</strong>
                      </p>
                    </div>
                    <span className="text-xs px-2.5 py-0.5 rounded bg-slate-800 text-slate-300 font-mono">
                      {store.mall.mallCode}
                    </span>
                  </div>

                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="bg-slate-900 p-3.5 rounded-xl border border-slate-800 space-y-2">
                      <span className="text-slate-400 font-bold uppercase text-[10px]">Labor & Union Rules</span>
                      <div className="text-slate-200">
                        <span className="text-slate-500">Union Labor Required:</span>{' '}
                        {store.mall.unionLaborRequired ? (
                          <span className="text-rose-400 font-bold">YES (Mandatory Union Trades)</span>
                        ) : (
                          <span className="text-emerald-400 font-bold">No (Open Shop Allowed)</span>
                        )}
                      </div>
                      {store.mall.unionLocalDesignation && (
                        <div>
                          <span className="text-slate-500">Designated Locals:</span>{' '}
                          <strong className="text-white">{store.mall.unionLocalDesignation}</strong>
                        </div>
                      )}
                      <div>
                        <span className="text-slate-500">Night Work Only:</span>{' '}
                        {store.mall.nightWorkOnly ? (
                          <span className="text-amber-400 font-bold">Yes (Mall off-hours only)</span>
                        ) : (
                          <span className="text-slate-300">Standard business / day allowed</span>
                        )}
                      </div>
                    </div>

                    <div className="bg-slate-900 p-3.5 rounded-xl border border-slate-800 space-y-2">
                      <span className="text-slate-400 font-bold uppercase text-[10px]">Insurance & Security</span>
                      <div>
                        <span className="text-slate-500">COI Required:</span>{' '}
                        <span className="text-white font-semibold">Yes (Mandatory before site entry)</span>
                      </div>
                      <div>
                        <span className="text-slate-500">Min. General Liability:</span>{' '}
                        <strong className="text-emerald-400 font-mono">
                          ${(store.mall.minGeneralLiabilityCad / 1000000).toFixed(0)}M CAD
                        </strong>
                      </div>
                      <div>
                        <span className="text-slate-500">Delivery Window:</span>{' '}
                        <strong className="text-white font-mono">{store.mall.deliveryWindow}</strong>
                      </div>
                      <div>
                        <span className="text-slate-500">Badging Fee:</span>{' '}
                        <strong className="text-amber-400 font-mono">${store.mall.badgingFeePerTech} CAD / tech</strong>
                      </div>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="p-8 text-center text-slate-500 bg-slate-950 rounded-xl border border-slate-800">
                  Street front or open strip location — no mall landlord restrictions.
                </div>
              )}
            </div>
          )}

        </div>

        {/* Modal Footer */}
        <div className="p-4 bg-slate-950 border-t border-slate-800 flex items-center justify-between text-xs">
          <span className="text-slate-500">
            Last updated via YM Inc. Enterprise Sync
          </span>
          <button
            onClick={onClose}
            className="px-4 py-1.5 bg-slate-800 hover:bg-slate-700 text-white rounded-lg transition-all font-medium cursor-pointer"
          >
            Close
          </button>
        </div>

      </div>
    </div>
  );
};
