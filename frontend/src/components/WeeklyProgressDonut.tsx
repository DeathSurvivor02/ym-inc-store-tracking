import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts';
import type { WeeklyProgressReport } from '../types/store';
import { mockWeeklyReports } from '../data/mockStores';
import { Zap } from 'lucide-react';

interface WeeklyProgressDonutProps {
  selectedStatusFilter: string | null;
  onSelectStatusFilter: (status: string | null) => void;
  selectedWeek: number;
  onSelectWeek: (week: number) => void;
}

const STATUS_COLORS: Record<string, string> = {
  'Completed & Certified': '#10b981', // Emerald-500
  'In Progress / On Schedule': '#0ea5e9', // Sky-500
  'Scheduled / Pending': '#f59e0b', // Amber-500
  'Blocked / In Jeopardy': '#f43f5e', // Rose-500
};

export const WeeklyProgressDonut: React.FC<WeeklyProgressDonutProps> = ({
  selectedStatusFilter,
  onSelectStatusFilter,
  selectedWeek,
  onSelectWeek,
}) => {
  const report: WeeklyProgressReport = mockWeeklyReports[selectedWeek] || mockWeeklyReports[41];

  const chartData = [
    { name: 'Completed & Certified', value: report.completedThisWeek, color: STATUS_COLORS['Completed & Certified'] },
    { name: 'In Progress / On Schedule', value: report.inProgress, color: STATUS_COLORS['In Progress / On Schedule'] },
    { name: 'Scheduled / Pending', value: report.scheduledPending, color: STATUS_COLORS['Scheduled / Pending'] },
    { name: 'Blocked / In Jeopardy', value: report.blockedInJeopardy, color: STATUS_COLORS['Blocked / In Jeopardy'] },
  ];

  return (
    <div className="bg-slate-900/90 border border-slate-800 rounded-xl p-5 shadow-2xl backdrop-blur-sm">
      {/* Top Header Bar */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-4 border-b border-slate-800/80">
        <div>
          <div className="flex items-center gap-2">
            <span className="flex h-2.5 w-2.5 rounded-full bg-emerald-400 animate-pulse" />
            <h2 className="text-lg font-bold tracking-tight text-white flex items-center gap-2">
              Weekly Installation Progress Report
            </h2>
            <span className="bg-blue-500/10 text-blue-400 text-xs px-2.5 py-0.5 rounded-full border border-blue-500/30 font-mono font-medium">
              4-5-4 Retail Calendar
            </span>
          </div>
          <p className="text-xs text-slate-400 mt-1">
            Tracking hardware installations, low-voltage cabling, power poles, and circuit turn-ups across active NSO & Remodels.
          </p>
        </div>

        {/* Fiscal Week Selector Buttons */}
        <div className="flex items-center bg-slate-950 p-1 rounded-lg border border-slate-800 text-xs">
          <button
            onClick={() => onSelectWeek(41)}
            className={`px-3 py-1.5 rounded-md font-medium transition-all ${
              selectedWeek === 41
                ? 'bg-blue-600 text-white shadow-sm'
                : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            Fiscal Wk 41 (Oct 11-17)
          </button>
          <button
            onClick={() => onSelectWeek(42)}
            className={`px-3 py-1.5 rounded-md font-medium transition-all ${
              selectedWeek === 42
                ? 'bg-blue-600 text-white shadow-sm'
                : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            Fiscal Wk 42 (Oct 18-24)
          </button>
        </div>
      </div>

      {/* Main Grid: Donut + Stat Cards + Milestone Bars */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 mt-5 items-center">
        
        {/* Left: Donut Chart with Centered KPI */}
        <div className="lg:col-span-5 flex flex-col items-center justify-center relative">
          <div className="w-full h-64 relative flex items-center justify-center">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={chartData}
                  cx="50%"
                  cy="50%"
                  innerRadius={72}
                  outerRadius={98}
                  paddingAngle={4}
                  dataKey="value"
                  onClick={(data) => {
                    const name = data?.name ? String(data.name) : null;
                    if (selectedStatusFilter === name) {
                      onSelectStatusFilter(null);
                    } else {
                      onSelectStatusFilter(name);
                    }
                  }}
                  cursor="pointer"
                >
                  {chartData.map((entry) => {
                    const isSelected = selectedStatusFilter === entry.name;
                    return (
                      <Cell
                        key={`cell-${entry.name}`}
                        fill={entry.color}
                        stroke={isSelected ? '#ffffff' : '#0f172a'}
                        strokeWidth={isSelected ? 3 : 2}
                        className="transition-all duration-200 hover:opacity-90"
                      />
                    );
                  })}
                </Pie>
                <Tooltip
                  content={({ active, payload }) => {
                    if (active && payload && payload.length) {
                      const data = payload[0];
                      const pct = Math.round(((data.value as number) / report.totalActiveProjects) * 100);
                      return (
                        <div className="bg-slate-950 border border-slate-700 px-3 py-2 rounded-lg shadow-xl text-xs">
                          <p className="font-semibold text-white">{data.name}</p>
                          <p className="text-slate-300">
                            <span className="font-mono text-emerald-400 font-bold">{data.value}</span> stores ({pct}%)
                          </p>
                          <p className="text-[10px] text-slate-500 mt-0.5">Click to filter store table below</p>
                        </div>
                      );
                    }
                    return null;
                  }}
                />
              </PieChart>
            </ResponsiveContainer>

            {/* Centered Donut Callout */}
            <div className="absolute inset-0 flex flex-col items-center justify-center pointer-events-none">
              <span className="text-3xl font-extrabold text-white tracking-tight">
                {report.overallProgressPct}%
              </span>
              <span className="text-[11px] font-semibold uppercase tracking-wider text-slate-400">
                Weekly Target
              </span>
              <span className="text-[10px] text-emerald-400 font-medium flex items-center gap-0.5 mt-0.5">
                <Zap className="w-3 h-3 inline" /> +12% this week
              </span>
            </div>
          </div>

          {selectedStatusFilter && (
            <div className="mt-2 flex items-center gap-2">
              <span className="text-xs text-slate-400">Filtered by:</span>
              <span className="text-xs font-semibold px-2 py-0.5 rounded bg-slate-800 text-blue-400 border border-slate-700">
                {selectedStatusFilter}
              </span>
              <button
                onClick={() => onSelectStatusFilter(null)}
                className="text-xs text-rose-400 hover:underline cursor-pointer"
              >
                Clear filter
              </button>
            </div>
          )}
        </div>

        {/* Center: Interactive Category Slices Breakdown */}
        <div className="lg:col-span-3 flex flex-col gap-2.5">
          {chartData.map((item) => {
            const isSelected = selectedStatusFilter === item.name;
            const pct = Math.round((item.value / report.totalActiveProjects) * 100);
            return (
              <button
                key={item.name}
                onClick={() => onSelectStatusFilter(isSelected ? null : item.name)}
                className={`flex items-center justify-between p-2.5 rounded-lg border text-left transition-all cursor-pointer ${
                  isSelected
                    ? 'bg-slate-800 border-white/40 shadow-md ring-1 ring-white/20'
                    : 'bg-slate-950/60 border-slate-800/80 hover:bg-slate-800/50 hover:border-slate-700'
                }`}
              >
                <div className="flex items-center gap-2.5">
                  <span
                    className="w-3 h-3 rounded-full shrink-0"
                    style={{ backgroundColor: item.color }}
                  />
                  <div>
                    <div className="text-xs font-medium text-slate-200 leading-tight">
                      {item.name}
                    </div>
                    <div className="text-[10px] text-slate-400">
                      {item.value} of {report.totalActiveProjects} store projects
                    </div>
                  </div>
                </div>
                <div className="text-right">
                  <span className="text-sm font-bold text-white font-mono">{pct}%</span>
                </div>
              </button>
            );
          })}
        </div>

        {/* Right: Technical Infrastructure Milestones Completion Progress */}
        <div className="lg:col-span-4 bg-slate-950/80 rounded-xl p-4 border border-slate-800/90 flex flex-col gap-3">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-200 uppercase tracking-wider">
              Installation Milestones Burn-up
            </span>
            <span className="text-[11px] text-slate-400">Total Fleet (24)</span>
          </div>

          <div className="flex flex-col gap-2.5">
            {report.milestones.map((m) => (
              <div key={m.name} className="flex flex-col gap-1">
                <div className="flex items-center justify-between text-xs">
                  <span className="text-slate-300 font-medium">{m.name}</span>
                  <span className="text-slate-400 font-mono text-[11px]">
                    <strong className="text-white">{m.completed}</strong> / {m.total} ({m.pct}%)
                  </span>
                </div>
                <div className="w-full bg-slate-800/90 h-2 rounded-full overflow-hidden">
                  <div
                    className={`h-full rounded-full transition-all duration-500 ${
                      m.pct === 100
                        ? 'bg-emerald-500'
                        : m.pct >= 75
                        ? 'bg-blue-500'
                        : m.pct >= 50
                        ? 'bg-amber-500'
                        : 'bg-rose-500'
                    }`}
                    style={{ width: `${m.pct}%` }}
                  />
                </div>
              </div>
            ))}
          </div>

          <div className="pt-2 border-t border-slate-800/80 text-[11px] text-slate-400 flex items-center justify-between">
            <span>Overall Fleet Readiness</span>
            <span className="text-emerald-400 font-bold">{report.overallProgressPct}% On-Schedule</span>
          </div>
        </div>

      </div>
    </div>
  );
};
