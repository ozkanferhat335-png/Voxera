import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { dashboardApi } from '../../services/api';
import { DashboardStats } from '../../types';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar } from 'recharts';

const StatCard: React.FC<{ title: string; value: string | number; subtitle?: string; color?: string; icon: string }> = ({ title, value, subtitle, color = 'blue', icon }) => {
  const colors: Record<string, string> = {
    blue: 'from-blue-600/20 to-blue-600/5 border-blue-600/30 text-blue-400',
    green: 'from-green-600/20 to-green-600/5 border-green-600/30 text-green-400',
    red: 'from-red-600/20 to-red-600/5 border-red-600/30 text-red-400',
    yellow: 'from-yellow-600/20 to-yellow-600/5 border-yellow-600/30 text-yellow-400',
    purple: 'from-purple-600/20 to-purple-600/5 border-purple-600/30 text-purple-400',
  };

  return (
    <div className={`bg-gradient-to-br ${colors[color]} border rounded-xl p-5`}>
      <div className="flex items-start justify-between">
        <div>
          <p className="text-slate-400 text-sm font-medium">{title}</p>
          <p className="text-3xl font-bold text-white mt-1">{value}</p>
          {subtitle && <p className="text-slate-500 text-xs mt-1">{subtitle}</p>}
        </div>
        <span className="text-2xl">{icon}</span>
      </div>
    </div>
  );
};

const DashboardPage: React.FC = () => {
  const { data: stats, isLoading } = useQuery<DashboardStats>({
    queryKey: ['dashboard-stats'],
    queryFn: dashboardApi.getStats,
    refetchInterval: 30000,
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-slate-400 flex items-center gap-3">
          <svg className="animate-spin h-5 w-5" viewBox="0 0 24 24" fill="none">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/>
          </svg>
          Loading dashboard...
        </div>
      </div>
    );
  }

  const chartData = stats?.hourlyStats?.map(h => ({
    hour: `${h.hour}:00`,
    total: h.totalCalls,
    answered: h.answeredCalls,
    missed: h.missedCalls,
  })) ?? [];

  const formatDuration = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = Math.floor(seconds % 60);
    return `${m}m ${s}s`;
  };

  return (
    <div className="space-y-6">
      {/* Page header */}
      <div>
        <h1 className="text-2xl font-bold text-white">Dashboard</h1>
        <p className="text-slate-400 text-sm mt-1">Real-time overview of your call center</p>
      </div>

      {/* Stats grid */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard title="Total Calls Today" value={stats?.totalCallsToday ?? 0} icon="📞" color="blue" />
        <StatCard title="Active Calls" value={stats?.activeCalls ?? 0} subtitle="Right now" icon="🔴" color="red" />
        <StatCard title="Missed Calls" value={stats?.missedCallsToday ?? 0} subtitle="Today" icon="📵" color="yellow" />
        <StatCard title="Avg Duration" value={formatDuration(stats?.averageCallDuration ?? 0)} icon="⏱" color="purple" />
      </div>

      {/* Agent stats */}
      <div className="grid grid-cols-3 gap-4">
        <StatCard title="Total Agents" value={stats?.totalAgents ?? 0} icon="👥" color="blue" />
        <StatCard title="Available" value={stats?.availableAgents ?? 0} icon="✅" color="green" />
        <StatCard title="Busy" value={stats?.busyAgents ?? 0} icon="🔴" color="red" />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Hourly call volume */}
        <div className="card">
          <h3 className="text-white font-semibold mb-4">Hourly Call Volume</h3>
          <ResponsiveContainer width="100%" height={220}>
            <AreaChart data={chartData}>
              <defs>
                <linearGradient id="totalGrad" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3}/>
                  <stop offset="95%" stopColor="#3b82f6" stopOpacity={0}/>
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" />
              <XAxis dataKey="hour" tick={{ fill: '#64748b', fontSize: 11 }} />
              <YAxis tick={{ fill: '#64748b', fontSize: 11 }} />
              <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px', color: '#f1f5f9' }} />
              <Area type="monotone" dataKey="total" stroke="#3b82f6" fill="url(#totalGrad)" strokeWidth={2} name="Total" />
              <Area type="monotone" dataKey="answered" stroke="#22c55e" fill="none" strokeWidth={2} name="Answered" />
              <Area type="monotone" dataKey="missed" stroke="#ef4444" fill="none" strokeWidth={2} name="Missed" />
            </AreaChart>
          </ResponsiveContainer>
        </div>

        {/* Call distribution */}
        <div className="card">
          <h3 className="text-white font-semibold mb-4">Call Distribution</h3>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={chartData.filter(d => d.total > 0).slice(-12)}>
              <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" />
              <XAxis dataKey="hour" tick={{ fill: '#64748b', fontSize: 11 }} />
              <YAxis tick={{ fill: '#64748b', fontSize: 11 }} />
              <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px', color: '#f1f5f9' }} />
              <Bar dataKey="answered" fill="#22c55e" radius={[4, 4, 0, 0]} name="Answered" />
              <Bar dataKey="missed" fill="#ef4444" radius={[4, 4, 0, 0]} name="Missed" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Quick actions */}
      <div className="card">
        <h3 className="text-white font-semibold mb-4">Quick Actions</h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
          {[
            { label: 'New Extension', icon: '☎', href: '/extensions' },
            { label: 'View Call Logs', icon: '📋', href: '/calls' },
            { label: 'IVR Setup', icon: '🎛', href: '/ivr' },
            { label: 'Reports', icon: '📊', href: '/reports' },
          ].map(action => (
            <a
              key={action.label}
              href={action.href}
              className="flex flex-col items-center gap-2 p-4 bg-slate-800 hover:bg-slate-700 rounded-xl transition-colors cursor-pointer"
            >
              <span className="text-2xl">{action.icon}</span>
              <span className="text-slate-300 text-sm font-medium">{action.label}</span>
            </a>
          ))}
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;
