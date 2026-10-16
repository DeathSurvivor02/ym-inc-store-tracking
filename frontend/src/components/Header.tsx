import { Building2, Search, UploadCloud, ShieldAlert, Radio } from 'lucide-react';
import type { CountryCode } from '../types/store';

interface HeaderProps {
  searchQuery: string;
  onSearchChange: (q: string) => void;
  countryFilter: 'ALL' | CountryCode;
  onCountryFilterChange: (c: 'ALL' | CountryCode) => void;
  onOpenImporter: () => void;
  onOpenCompliance: () => void;
  activeView: 'stores' | 'circuits';
  onViewChange: (v: 'stores' | 'circuits') => void;
  totalStoreCount: number;
}

export const Header: React.FC<HeaderProps> = ({
  searchQuery,
  onSearchChange,
  onOpenImporter,
  onOpenCompliance,
  activeView,
  onViewChange,
}) => {
  return (
    <header className="bg-slate-900 border-b border-slate-800 sticky top-0 z-30 shadow-md">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16 gap-4">
          
          {/* Logo & Platform Name */}
          <div className="flex items-center gap-3 shrink-0">
            <div className="h-10 w-10 rounded-xl bg-gradient-to-tr from-blue-600 to-indigo-600 flex items-center justify-center shadow-lg shadow-blue-500/20 text-white font-black text-xl tracking-wider">
              YM
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h1 className="text-base font-bold text-white tracking-tight">
                  YM Inc. Store Deployment Hub
                </h1>
                <span className="text-[10px] uppercase font-semibold bg-blue-900/60 text-blue-300 border border-blue-700/50 px-2 py-0.5 rounded">
                  Track A Infrastructure
                </span>
              </div>
              <p className="text-[11px] text-slate-400">
                NSO, Remodels, Power Poles, Cat6 Cabling & Telecom Readiness
              </p>
            </div>
          </div>

          {/* Search Bar */}
          <div className="flex-1 max-w-md hidden md:block">
            <div className="relative">
              <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => onSearchChange(e.target.value)}
                placeholder="Search store # (e.g. 6142, 6004), banner, mall, city..."
                className="w-full bg-slate-950/80 border border-slate-700 rounded-lg pl-9 pr-4 py-1.5 text-xs text-white placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
              />
              {searchQuery && (
                <button
                  onClick={() => onSearchChange('')}
                  className="absolute right-2.5 top-1/2 -translate-y-1/2 text-xs text-slate-400 hover:text-white cursor-pointer"
                >
                  ✕
                </button>
              )}
            </div>
          </div>

          {/* Right Navigation & Action Buttons */}
          <div className="flex items-center gap-2.5">
            {/* View Switcher: Store Deployment vs Telecom WAN */}
            <div className="flex bg-slate-950 p-1 rounded-lg border border-slate-800 text-xs">
              <button
                onClick={() => onViewChange('stores')}
                className={`px-3 py-1.5 rounded-md font-medium transition-all flex items-center gap-1.5 cursor-pointer ${
                  activeView === 'stores'
                    ? 'bg-blue-600 text-white shadow-sm'
                    : 'text-slate-400 hover:text-slate-200'
                }`}
              >
                <Building2 className="w-3.5 h-3.5" />
                <span className="hidden sm:inline">Store Rollout</span>
              </button>
              <button
                onClick={() => onViewChange('circuits')}
                className={`px-3 py-1.5 rounded-md font-medium transition-all flex items-center gap-1.5 cursor-pointer ${
                  activeView === 'circuits'
                    ? 'bg-blue-600 text-white shadow-sm'
                    : 'text-slate-400 hover:text-slate-200'
                }`}
              >
                <Radio className="w-3.5 h-3.5" />
                <span className="hidden sm:inline">Telecom WAN</span>
              </button>
            </div>

            {/* Landlord Rules Button */}
            <button
              onClick={onOpenCompliance}
              className="px-3 py-1.5 bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-medium rounded-lg border border-slate-700 flex items-center gap-1.5 transition-all cursor-pointer"
              title="Mall Landlord Rules (Union, COI, Badging)"
            >
              <ShieldAlert className="w-3.5 h-3.5 text-amber-400" />
              <span className="hidden sm:inline">Mall Rules</span>
            </button>

            {/* Import Roster Button */}
            <button
              onClick={onOpenImporter}
              className="px-3 py-1.5 bg-blue-600 hover:bg-blue-500 text-white text-xs font-semibold rounded-lg shadow-sm flex items-center gap-1.5 transition-all cursor-pointer"
            >
              <UploadCloud className="w-3.5 h-3.5" />
              <span className="hidden sm:inline">Import Roster</span>
            </button>
          </div>

        </div>
      </div>
    </header>
  );
};
