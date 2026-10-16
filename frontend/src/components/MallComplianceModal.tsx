import { X, ShieldAlert } from 'lucide-react';
import { mockMallProperties } from '../data/mockStores';

interface MallComplianceModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export const MallComplianceModal: React.FC<MallComplianceModalProps> = ({ isOpen, onClose }) => {
  if (!isOpen) return null;

  const malls = Object.values(mockMallProperties);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-200">
      <div 
        className="bg-slate-900 border border-slate-700/80 rounded-2xl w-full max-w-4xl max-h-[85vh] flex flex-col shadow-2xl overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="p-5 bg-slate-950 border-b border-slate-800 flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <ShieldAlert className="w-5 h-5 text-amber-400" />
            <div>
              <h3 className="text-base font-bold text-white">
                Mall Landlord Compliance & Technical Rules Directory
              </h3>
              <p className="text-xs text-slate-400">
                Union labor requirements, insurance COI limits, technician badging fees, and delivery windows.
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg bg-slate-800 text-slate-400 hover:text-white hover:bg-slate-700 cursor-pointer"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-5 overflow-y-auto space-y-4 text-xs">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {malls.map((mall) => (
              <div 
                key={mall.mallId}
                className="bg-slate-950 border border-slate-800 rounded-xl p-4 space-y-3 hover:border-slate-700 transition-all"
              >
                <div className="flex items-start justify-between">
                  <div>
                    <h4 className="text-sm font-bold text-white">{mall.propertyName}</h4>
                    <span className="text-[11px] text-slate-400">
                      Landlord: <strong className="text-slate-300">{mall.landlordCompany}</strong> ({mall.countryCode === 'CA' ? '🇨🇦 Canada' : '🇺🇸 USA'})
                    </span>
                  </div>
                  <span className="font-mono text-[10px] px-2 py-0.5 rounded bg-slate-900 text-slate-400 border border-slate-800">
                    {mall.mallCode}
                  </span>
                </div>

                <div className="space-y-1.5 text-slate-300 border-t border-slate-900 pt-2">
                  <div className="flex items-center justify-between">
                    <span className="text-slate-500">Union Labor:</span>
                    {mall.unionLaborRequired ? (
                      <span className="text-rose-400 font-bold bg-rose-950/40 px-2 py-0.5 rounded border border-rose-900/50">
                        Mandatory Union
                      </span>
                    ) : (
                      <span className="text-emerald-400 font-medium">Open Shop Permitted</span>
                    )}
                  </div>

                  {mall.unionLocalDesignation && (
                    <div className="text-[11px] text-slate-400">
                      <span className="text-slate-500">Designation:</span> {mall.unionLocalDesignation}
                    </div>
                  )}

                  <div className="flex items-center justify-between">
                    <span className="text-slate-500">COI General Liability:</span>
                    <strong className="text-emerald-400 font-mono">
                      ${(mall.minGeneralLiabilityCad / 1000000).toFixed(0)}M CAD
                    </strong>
                  </div>

                  <div className="flex items-center justify-between">
                    <span className="text-slate-500">Delivery Window:</span>
                    <span className="font-mono text-slate-200">{mall.deliveryWindow}</span>
                  </div>

                  <div className="flex items-center justify-between">
                    <span className="text-slate-500">Night Work Only:</span>
                    <span className={mall.nightWorkOnly ? 'text-amber-400 font-semibold' : 'text-slate-300'}>
                      {mall.nightWorkOnly ? 'Yes (Mall off-hours)' : 'No'}
                    </span>
                  </div>

                  <div className="flex items-center justify-between">
                    <span className="text-slate-500">Tech Badging Fee:</span>
                    <span className="font-mono text-slate-200">${mall.badgingFeePerTech} / badge</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="p-4 bg-slate-950 border-t border-slate-800 flex justify-end">
          <button
            onClick={onClose}
            className="px-4 py-1.5 bg-slate-800 hover:bg-slate-700 text-white rounded-lg text-xs font-semibold cursor-pointer"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
};
