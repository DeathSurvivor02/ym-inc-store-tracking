import type { 
  StoreDeployment, 
  CountryCode, 
  BrandCode, 
  OperationalStatus,
  HealthStatus
} from '../types/store';
import { 
  MapPin, 
  Zap, 
  Server, 
  Cable, 
  ExternalLink,
  Filter
} from 'lucide-react';

interface StoreLifecycleTableProps {
  stores: StoreDeployment[];
  selectedStore: StoreDeployment | null;
  onSelectStore: (store: StoreDeployment) => void;
  countryFilter: 'ALL' | CountryCode;
  onCountryFilterChange: (c: 'ALL' | CountryCode) => void;
  selectedBanner: 'ALL' | BrandCode;
  onSelectBanner: (b: 'ALL' | BrandCode) => void;
}

export const StoreLifecycleTable: React.FC<StoreLifecycleTableProps> = ({
  stores,
  selectedStore,
  onSelectStore,
  countryFilter,
  onCountryFilterChange,
  selectedBanner,
  onSelectBanner,
}) => {
  const bannersList: { code: BrandCode; name: string }[] = [
    { code: 'BN', name: 'Bluenotes' },
    { code: 'CR', name: 'Charlotte Russe' },
    { code: 'UP', name: 'Urban Planet' },
    { code: 'R21', name: 'rue21' },
    { code: 'M', name: 'Mandee' },
    { code: 'SS', name: 'Suzy Shier' },
    { code: 'LC', name: 'Le Château' },
    { code: 'F21', name: 'Forever 21' },
    { code: 'UK', name: 'Urban Kids' },
    { code: 'W49', name: 'West 49' },
  ];

  const getHealthBadge = (health: HealthStatus) => {
    switch (health) {
      case 'On Track':
        return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/30">On Track</span>;
      case 'At Risk':
        return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-500/10 text-amber-400 border border-amber-500/30">At Risk</span>;
      case 'Critical Blocker':
        return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-500/10 text-rose-400 border border-rose-500/30">Critical Blocker</span>;
      case 'Complete':
        return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-500/10 text-blue-400 border border-blue-500/30">Complete</span>;
    }
  };

  const getStageBadge = (status: OperationalStatus) => {
    return (
      <span className="text-xs font-medium px-2 py-0.5 rounded bg-slate-800 text-slate-300 border border-slate-700">
        {status}
      </span>
    );
  };

  return (
    <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden shadow-xl mt-6">
      
      {/* Control Bar: Filters & Counters */}
      <div className="p-4 bg-slate-900/90 border-b border-slate-800 flex flex-wrap items-center justify-between gap-4">
        
        {/* Country Filter Pill Group */}
        <div className="flex items-center gap-2">
          <span className="text-xs text-slate-400 flex items-center gap-1 font-medium">
            <Filter className="w-3.5 h-3.5" /> Fleet:
          </span>
          <div className="flex bg-slate-950 p-1 rounded-lg border border-slate-800 text-xs">
            <button
              onClick={() => onCountryFilterChange('ALL')}
              className={`px-2.5 py-1 rounded font-medium transition-all cursor-pointer ${
                countryFilter === 'ALL'
                  ? 'bg-blue-600 text-white'
                  : 'text-slate-400 hover:text-slate-200'
              }`}
            >
              All ({stores.length})
            </button>
            <button
              onClick={() => onCountryFilterChange('CA')}
              className={`px-2.5 py-1 rounded font-medium transition-all flex items-center gap-1 cursor-pointer ${
                countryFilter === 'CA'
                  ? 'bg-blue-600 text-white'
                  : 'text-slate-400 hover:text-slate-200'
              }`}
            >
              🇨🇦 Canada (6xxx)
            </button>
            <button
              onClick={() => onCountryFilterChange('US')}
              className={`px-2.5 py-1 rounded font-medium transition-all flex items-center gap-1 cursor-pointer ${
                countryFilter === 'US'
                  ? 'bg-blue-600 text-white'
                  : 'text-slate-400 hover:text-slate-200'
              }`}
            >
              🇺🇸 USA (6000s)
            </button>
          </div>
        </div>

        {/* Banner Filter Dropdown */}
        <div className="flex items-center gap-2 text-xs">
          <span className="text-slate-400 font-medium">Banner:</span>
          <select
            value={selectedBanner}
            onChange={(e) => onSelectBanner(e.target.value as 'ALL' | BrandCode)}
            className="bg-slate-950 border border-slate-700 text-slate-200 rounded-lg px-2.5 py-1 text-xs focus:ring-1 focus:ring-blue-500 focus:outline-none cursor-pointer"
          >
            <option value="ALL">All Banners</option>
            {bannersList.map((b) => (
              <option key={b.code} value={b.code}>
                {b.name} ({b.code})
              </option>
            ))}
          </select>
        </div>

      </div>

      {/* Stores Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="border-b border-slate-800 bg-slate-950/60 text-[11px] font-semibold text-slate-400 uppercase tracking-wider">
              <th className="py-3 px-4">Store & Banner</th>
              <th className="py-3 px-4">Mall Property / Location</th>
              <th className="py-3 px-4">Project Type</th>
              <th className="py-3 px-4">Deployment Stage</th>
              <th className="py-3 px-4">Health Status</th>
              <th className="py-3 px-4">Infrastructure Specs</th>
              <th className="py-3 px-4">Target Open</th>
              <th className="py-3 px-4 text-right">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/60 text-xs">
            {stores.length === 0 ? (
              <tr>
                <td colSpan={8} className="py-12 text-center text-slate-500">
                  No stores match the active filter criteria.
                </td>
              </tr>
            ) : (
              stores.map((store) => {
                const isSelected = selectedStore?.storeId === store.storeId;
                return (
                  <tr
                    key={store.storeId}
                    onClick={() => onSelectStore(store)}
                    className={`cursor-pointer transition-colors ${
                      isSelected
                        ? 'bg-blue-950/30 hover:bg-blue-950/40'
                        : 'hover:bg-slate-800/40'
                    }`}
                  >
                    {/* Store # & Banner */}
                    <td className="py-3 px-4">
                      <div className="flex items-center gap-2">
                        <span className="font-mono font-bold text-white text-sm">
                          #{store.storeNumber}
                        </span>
                        <span className="text-[10px] font-semibold px-1.5 py-0.5 rounded bg-slate-800 text-blue-400 border border-slate-700">
                          {store.bannerCode}
                        </span>
                        <span className="text-[10px] text-slate-400">
                          {store.countryCode === 'CA' ? '🇨🇦' : '🇺🇸'}
                        </span>
                      </div>
                      <div className="text-slate-300 font-medium mt-0.5">
                        {store.storeName}
                      </div>
                    </td>

                    {/* Mall Property & Location */}
                    <td className="py-3 px-4">
                      <div className="text-slate-200 font-medium">
                        {store.mall ? store.mall.propertyName : 'Street / Strip Plaza'}
                      </div>
                      <div className="text-[11px] text-slate-400 flex items-center gap-1 mt-0.5">
                        <MapPin className="w-3 h-3 text-slate-500" />
                        {store.city}, {store.stateProvince}
                      </div>
                      {store.mall?.unionLaborRequired && (
                        <div className="text-[10px] text-amber-400 font-medium flex items-center gap-1 mt-0.5">
                          ⚠️ Union Labor Mandated
                        </div>
                      )}
                    </td>

                    {/* Project Type */}
                    <td className="py-3 px-4">
                      <span className="text-slate-300 font-medium">
                        {store.projectType}
                      </span>
                      <div className="text-[11px] text-slate-400">
                        {store.salesSqft?.toLocaleString()} sq ft sales
                      </div>
                    </td>

                    {/* Stage */}
                    <td className="py-3 px-4">
                      {getStageBadge(store.operationalStatus)}
                      <div className="text-[10px] text-slate-400 mt-1">
                        POS: {store.posStagingStatus} ({store.posTerminalCount} lanes)
                      </div>
                    </td>

                    {/* Health Status */}
                    <td className="py-3 px-4">
                      {getHealthBadge(store.overallHealth)}
                    </td>

                    {/* Technical Infrastructure Snapshot */}
                    <td className="py-3 px-4">
                      <div className="flex flex-col gap-1 text-[11px]">
                        {/* Power Poles */}
                        {store.powerPoles && (
                          <div className="flex items-center gap-1.5">
                            <Zap className="w-3 h-3 text-amber-400" />
                            <span className="text-slate-300">
                              Wiremold 25DTP-4: <strong>{store.powerPoles.installedCount}/{store.powerPoles.totalRequired}</strong>
                            </span>
                          </div>
                        )}
                        {/* Cabling */}
                        {store.cabling && (
                          <div className="flex items-center gap-1.5">
                            <Cable className="w-3 h-3 text-blue-400" />
                            <span className="text-slate-300">
                              Cat6 Drops: <strong>{store.cabling.totalDataDrops}</strong> (
                              {store.cabling.flukeCertified ? (
                                <span className="text-emerald-400 font-semibold">Fluke Pass</span>
                              ) : (
                                <span className="text-amber-400">Pending Cert</span>
                              )}
                              )
                            </span>
                          </div>
                        )}
                        {/* Circuits */}
                        {store.circuits && store.circuits.length > 0 && (
                          <div className="flex items-center gap-1.5">
                            <Server className="w-3 h-3 text-emerald-400" />
                            <span className="text-slate-300">
                              WAN: <strong>{store.circuits.length} Circuits</strong> (
                              {store.circuits[0].orderStatus.includes('Turned Up') ? (
                                <span className="text-emerald-400 font-semibold">Active</span>
                              ) : (
                                <span className="text-blue-400">FOC Confirmed</span>
                              )}
                              )
                            </span>
                          </div>
                        )}
                      </div>
                    </td>

                    {/* Target Open */}
                    <td className="py-3 px-4">
                      <div className="text-slate-200 font-mono font-medium">
                        {store.targetOpenDate || store.openingDate}
                      </div>
                      {store.targetOpenDate && (
                        <div className="text-[10px] text-blue-400 font-medium">
                          Scheduled Turn-over
                        </div>
                      )}
                    </td>

                    {/* Action Button */}
                    <td className="py-3 px-4 text-right">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          onSelectStore(store);
                        }}
                        className="px-2.5 py-1 rounded bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-medium border border-slate-700 inline-flex items-center gap-1 transition-all cursor-pointer"
                      >
                        <span>Details</span>
                        <ExternalLink className="w-3 h-3" />
                      </button>
                    </td>

                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Footer Info */}
      <div className="p-3 bg-slate-950/80 border-t border-slate-800/80 flex items-center justify-between text-xs text-slate-400">
        <div>
          Showing <strong className="text-white">{stores.length}</strong> storefronts
        </div>
        <div className="flex items-center gap-4 text-[11px]">
          <span className="flex items-center gap-1">
            <span className="w-2 h-2 rounded-full bg-emerald-500" /> On Track
          </span>
          <span className="flex items-center gap-1">
            <span className="w-2 h-2 rounded-full bg-amber-500" /> At Risk
          </span>
          <span className="flex items-center gap-1">
            <span className="w-2 h-2 rounded-full bg-rose-500" /> Critical Blocker
          </span>
        </div>
      </div>

    </div>
  );
};
