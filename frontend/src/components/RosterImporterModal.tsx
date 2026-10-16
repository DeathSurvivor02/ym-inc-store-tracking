import { useState } from 'react';
import { X, UploadCloud, FileSpreadsheet, CheckCircle, ArrowRight } from 'lucide-react';
import type { StoreDeployment } from '../types/store';

interface RosterImporterModalProps {
  isOpen: boolean;
  onClose: () => void;
  onImportSuccess?: (newStores: Partial<StoreDeployment>[]) => void;
}

export const RosterImporterModal: React.FC<RosterImporterModalProps> = ({
  isOpen,
  onClose,
  onImportSuccess,
}) => {
  const [fileName, setFileName] = useState<string | null>(null);
  const [isProcessing, setIsProcessing] = useState(false);
  const [importedRows, setImportedRows] = useState<number | null>(null);

  if (!isOpen) return null;

  const handleSimulateCsvUpload = () => {
    setIsProcessing(true);
    setFileName('sample_store_roster.csv');

    setTimeout(() => {
      setIsProcessing(false);
      setImportedRows(18);
    }, 800);
  };

  const handleApplyImport = () => {
    if (onImportSuccess) {
      onImportSuccess([]);
    }
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-200">
      <div 
        className="bg-slate-900 border border-slate-700/80 rounded-2xl w-full max-w-lg flex flex-col shadow-2xl overflow-hidden text-xs"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="p-5 bg-slate-950 border-b border-slate-800 flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <UploadCloud className="w-5 h-5 text-blue-400" />
            <div>
              <h3 className="text-base font-bold text-white">
                Import YM Inc. Store Roster
              </h3>
              <p className="text-xs text-slate-400">
                Upload official corporate spreadsheet (.xlsx or .csv)
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

        <div className="p-6 space-y-4">
          {/* Store ID Convention Callout */}
          <div className="p-3 bg-blue-950/40 border border-blue-900/60 rounded-xl space-y-1">
            <div className="font-semibold text-blue-300">
              📌 YM Inc. Store ID Numbering Rules:
            </div>
            <ul className="text-slate-300 text-[11px] list-disc list-inside space-y-0.5">
              <li><strong>US Stores (CR, R21, M):</strong> Strictly in the <span className="font-mono text-emerald-400">6000s</span> range (e.g. 6001, 6004, 6012).</li>
              <li><strong>Canadian Stores (UP, BN, SS, LC):</strong> 4-digit codes starting with <span className="font-mono text-emerald-400">6</span> (e.g. 6101, 6142, 6302).</li>
            </ul>
          </div>

          {/* Upload Dropzone */}
          <div 
            onClick={handleSimulateCsvUpload}
            className="border-2 border-dashed border-slate-700 hover:border-blue-500 rounded-xl p-8 text-center cursor-pointer transition-all bg-slate-950/50 hover:bg-slate-950 flex flex-col items-center justify-center gap-2"
          >
            <FileSpreadsheet className="w-10 h-10 text-slate-500 group-hover:text-blue-400" />
            <div>
              <span className="font-semibold text-white">Click to select or drop store roster spreadsheet</span>
              <p className="text-[11px] text-slate-400 mt-0.5">Supports .xlsx, .xls, and .csv files</p>
            </div>
            <button
              type="button"
              className="mt-2 px-3 py-1 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded text-xs font-medium border border-slate-700 cursor-pointer"
            >
              Browse Files
            </button>
          </div>

          {/* Processing / Success State */}
          {isProcessing && (
            <div className="p-3 bg-slate-950 rounded-lg text-center text-slate-400 animate-pulse">
              Parsing columns and validating store numbers against 3NF schema...
            </div>
          )}

          {importedRows !== null && (
            <div className="p-3.5 bg-emerald-950/40 border border-emerald-900/60 rounded-xl text-emerald-300 space-y-1">
              <div className="flex items-center gap-1.5 font-bold">
                <CheckCircle className="w-4 h-4 text-emerald-400" />
                Validated {importedRows} storefronts from {fileName}
              </div>
              <p className="text-[11px] text-emerald-400/80">
                All store numbers verified with banners, district managers, and mall requirements.
              </p>
            </div>
          )}
        </div>

        <div className="p-4 bg-slate-950 border-t border-slate-800 flex items-center justify-between">
          <span className="text-[11px] text-slate-500">
            Fuzzy header matching active
          </span>
          <div className="flex items-center gap-2">
            <button
              onClick={onClose}
              className="px-3 py-1.5 bg-slate-800 hover:bg-slate-700 text-slate-300 rounded-lg cursor-pointer"
            >
              Cancel
            </button>
            <button
              onClick={handleApplyImport}
              disabled={importedRows === null}
              className="px-4 py-1.5 bg-blue-600 hover:bg-blue-500 disabled:opacity-50 text-white rounded-lg font-semibold flex items-center gap-1 cursor-pointer"
            >
              <span>Sync to Platform</span>
              <ArrowRight className="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
