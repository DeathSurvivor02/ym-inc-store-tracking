import { useState, useMemo } from 'react';
import { Header } from './components/Header';
import { WeeklyProgressDonut } from './components/WeeklyProgressDonut';
import { StoreLifecycleTable } from './components/StoreLifecycleTable';
import { CircuitTracker } from './components/CircuitTracker';
import { StoreDetailModal } from './components/StoreDetailModal';
import { MallComplianceModal } from './components/MallComplianceModal';
import { RosterImporterModal } from './components/RosterImporterModal';
import { mockStores } from './data/mockStores';
import type { CountryCode, BrandCode, StoreDeployment } from './types/store';

export function App() {
  const [stores] = useState<StoreDeployment[]>(mockStores);
  const [selectedWeek, setSelectedWeek] = useState<number>(41);
  const [selectedStatusFilter, setSelectedStatusFilter] = useState<string | null>(null);
  const [countryFilter, setCountryFilter] = useState<'ALL' | CountryCode>('ALL');
  const [selectedBanner, setSelectedBanner] = useState<'ALL' | BrandCode>('ALL');
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [activeView, setActiveView] = useState<'stores' | 'circuits'>('stores');
  const [selectedStore, setSelectedStore] = useState<StoreDeployment | null>(null);
  const [isImporterOpen, setIsImporterOpen] = useState(false);
  const [isComplianceOpen, setIsComplianceOpen] = useState(false);

  // Filtered stores computation
  const filteredStores = useMemo(() => {
    return stores.filter((store) => {
      // 1. Country Filter
      if (countryFilter !== 'ALL' && store.countryCode !== countryFilter) {
        return false;
      }

      // 2. Banner Filter
      if (selectedBanner !== 'ALL' && store.bannerCode !== selectedBanner) {
        return false;
      }

      // 3. Weekly Progress Donut Slice Filter
      if (selectedStatusFilter && store.weeklyProgress.weeklyStatus !== selectedStatusFilter) {
        return false;
      }

      // 4. Search Query Filter
      if (searchQuery.trim()) {
        const query = searchQuery.toLowerCase().trim();
        const matchNumber = store.storeNumber.toLowerCase().includes(query);
        const matchName = store.storeName.toLowerCase().includes(query);
        const matchBanner = store.bannerName.toLowerCase().includes(query) || store.bannerCode.toLowerCase().includes(query);
        const matchCity = store.city.toLowerCase().includes(query);
        const matchMall = store.mall?.propertyName.toLowerCase().includes(query);
        if (!matchNumber && !matchName && !matchBanner && !matchCity && !matchMall) {
          return false;
        }
      }

      return true;
    });
  }, [stores, countryFilter, selectedBanner, selectedStatusFilter, searchQuery]);

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col font-sans selection:bg-blue-600 selection:text-white">
      {/* Top Application Header */}
      <Header
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        countryFilter={countryFilter}
        onCountryFilterChange={setCountryFilter}
        onOpenImporter={() => setIsImporterOpen(true)}
        onOpenCompliance={() => setIsComplianceOpen(true)}
        activeView={activeView}
        onViewChange={setActiveView}
        totalStoreCount={stores.length}
      />

      {/* Main Workspace Body */}
      <main className="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 py-6 space-y-6">
        
        {/* HERO SECTION: Weekly Installation Progress Donut Report */}
        <WeeklyProgressDonut
          selectedStatusFilter={selectedStatusFilter}
          onSelectStatusFilter={setSelectedStatusFilter}
          selectedWeek={selectedWeek}
          onSelectWeek={setSelectedWeek}
        />

        {/* PRIMARY VIEW CONTENT */}
        {activeView === 'stores' ? (
          <StoreLifecycleTable
            stores={filteredStores}
            selectedStore={selectedStore}
            onSelectStore={setSelectedStore}
            countryFilter={countryFilter}
            onCountryFilterChange={setCountryFilter}
            selectedBanner={selectedBanner}
            onSelectBanner={setSelectedBanner}
          />
        ) : (
          <CircuitTracker
            stores={filteredStores}
            onSelectStore={setSelectedStore}
          />
        )}

      </main>

      {/* FOOTER */}
      <footer className="border-t border-slate-900 bg-slate-950/80 py-4 text-center text-xs text-slate-500">
        YM Inc. Store Deployment & Technical Operations Platform • Fiscal 2026 Engine
      </footer>

      {/* MODALS & DRAWERS */}
      <StoreDetailModal
        store={selectedStore}
        onClose={() => setSelectedStore(null)}
      />

      <MallComplianceModal
        isOpen={isComplianceOpen}
        onClose={() => setIsComplianceOpen(false)}
      />

      <RosterImporterModal
        isOpen={isImporterOpen}
        onClose={() => setIsImporterOpen(false)}
        onImportSuccess={() => {}}
      />
    </div>
  );
}

export default App;
