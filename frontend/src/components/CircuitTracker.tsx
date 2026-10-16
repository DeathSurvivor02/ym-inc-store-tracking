import type { StoreDeployment } from '../types/store';
import { Radio, CheckCircle, Clock, AlertTriangle, ShieldCheck } from 'lucide-react';

interface CircuitTrackerProps {
  stores: StoreDeployment[];
  onSelectStore: (store: StoreDeployment) => void;
}

export const CircuitTracker: React.FC<CircuitTrackerProps> = ({ stores, onSelectStore }) => {
  // Flatten all circuits with store reference
  const allCircuits = stores.flatMap((store) =>
    store.circuits.map((circuit) => ({
      ...circuit,
      storeNumber: store.storeNumber,
      storeName: store.storeName,
      bannerCode: store.bannerCode,
      countryCode: store.countryCode,
      city: store.city,
      stateProvince: store.stateProvince,
      parentStore: store,
    }))
  );

  const getStatusBadge = (status: string) => {
    if (status.includes('Active') || status.includes('Turned Up')) {
      return (
        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/30">
          <CheckCircle className="w-3 h-3" /> Turned Up (Active)
        </span>
      );
    }
    if (status.includes('FOC Confirmed')) {
      return (
        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-500/10 text-blue-400 border border-blue-500/30">
          <Clock className="w-3 h-3" /> FOC Confirmed
        </span>
      );
    }
    if (status.includes('Jeopardy') || status.includes('Delay')) {
      return (
        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-500/10 text-rose-400 border border-rose-500/30">
          <AlertTriangle className="w-3 h-3" /> Carrier Jeopardy
        </span>
      );
    }
    return (
      <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-amber-500/10 text-amber-400 border border-amber-500/30">
        {status}
      </span>
    );
  };

  return (
    <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden shadow-xl mt-6">
      {/* Top Banner */}
      <div className="p-4 bg-slate-900/90 border-b border-slate-800 flex items-center justify-between">
        <div>
          <h3 className="text-base font-bold text-white flex items-center gap-2">
            <Radio className="w-4 h-4 text-blue-400" />
            Dual-WAN Telecom Circuit & Demarc Readiness Matrix
          </h3>
          <p className="text-xs text-slate-400 mt-0.5">
            Tracking Primary DIA Fiber, Secondary LTE Failover, Carrier FOC dates, and Demarc extensions.
          </p>
        </div>
        <div className="text-xs font-medium bg-slate-950 px-3 py-1.5 rounded-lg border border-slate-800 text-slate-300">
          Total Circuits Tracked: <strong className="text-white">{allCircuits.length}</strong>
        </div>
      </div>

      {/* Circuits Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="border-b border-slate-800 bg-slate-950/60 text-[11px] font-semibold text-slate-400 uppercase tracking-wider">
              <th className="py-3 px-4">Store Location</th>
              <th className="py-3 px-4">Circuit Role</th>
              <th className="py-3 px-4">Carrier & Service</th>
              <th className="py-3 px-4">Bandwidth</th>
              <th className="py-3 px-4">Order / Turn-Up Status</th>
              <th className="py-3 px-4">Demarc Extension</th>
              <th className="py-3 px-4">Key Dates</th>
              <th className="py-3 px-4">Jeopardy / Notes</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/60 text-xs">
            {allCircuits.map((circuit, idx) => (
              <tr
                key={idx}
                onClick={() => onSelectStore(circuit.parentStore)}
                className="hover:bg-slate-800/40 cursor-pointer transition-colors"
              >
                <td className="py-3 px-4">
                  <div className="flex items-center gap-1.5">
                    <span className="font-mono font-bold text-white">#{circuit.storeNumber}</span>
                    <span className="text-[10px] px-1.5 py-0.2 rounded bg-slate-800 text-blue-400 font-semibold border border-slate-700">
                      {circuit.bannerCode}
                    </span>
                    <span className="text-xs">{circuit.countryCode === 'CA' ? '🇨🇦' : '🇺🇸'}</span>
                  </div>
                  <div className="text-slate-300 font-medium">{circuit.storeName}</div>
                  <div className="text-[11px] text-slate-500">{circuit.city}, {circuit.stateProvince}</div>
                </td>

                <td className="py-3 px-4">
                  <span
                    className={`text-xs font-semibold px-2 py-0.5 rounded ${
                      circuit.role === 'Primary WAN'
                        ? 'bg-blue-900/40 text-blue-300 border border-blue-700/50'
                        : 'bg-purple-900/40 text-purple-300 border border-purple-700/50'
                    }`}
                  >
                    {circuit.role}
                  </span>
                </td>

                <td className="py-3 px-4">
                  <div className="text-slate-200 font-semibold">{circuit.carrierName}</div>
                  <div className="text-[11px] text-slate-400">{circuit.serviceType}</div>
                </td>

                <td className="py-3 px-4 font-mono text-slate-300">
                  <span className="text-emerald-400 font-bold">{circuit.bandwidthDownMbps}</span> ↓ /{' '}
                  <span className="text-blue-400 font-bold">{circuit.bandwidthUpMbps}</span> ↑ Mbps
                </td>

                <td className="py-3 px-4">{getStatusBadge(circuit.orderStatus)}</td>

                <td className="py-3 px-4">
                  {circuit.demarcExtended ? (
                    <span className="inline-flex items-center gap-1 text-emerald-400 text-xs font-medium">
                      <ShieldCheck className="w-3.5 h-3.5" /> Completed
                    </span>
                  ) : (
                    <span className="inline-flex items-center gap-1 text-amber-400 text-xs font-medium">
                      <Clock className="w-3.5 h-3.5" /> Required
                    </span>
                  )}
                </td>

                <td className="py-3 px-4 font-mono text-xs">
                  {circuit.focDate && (
                    <div className="text-slate-300">
                      <span className="text-slate-500 text-[10px]">FOC: </span>{circuit.focDate}
                    </div>
                  )}
                  {circuit.turnupDate && (
                    <div className="text-emerald-400">
                      <span className="text-slate-500 text-[10px]">Turn-up: </span>{circuit.turnupDate}
                    </div>
                  )}
                </td>

                <td className="py-3 px-4">
                  {circuit.jeopardyReason ? (
                    <span className="text-rose-400 font-medium text-xs flex items-center gap-1">
                      ⚠️ {circuit.jeopardyReason}
                    </span>
                  ) : (
                    <span className="text-slate-500 text-xs">—</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
