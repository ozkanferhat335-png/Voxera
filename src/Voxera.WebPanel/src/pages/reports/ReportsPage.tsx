import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { reportsApi } from '../../services/api';
import { format } from 'date-fns';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';

const ReportsPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'daily' | 'operators' | 'missed'>('daily');
  const [date, setDate] = useState(format(new Date(), 'yyyy-MM-dd'));

  const { data: dailyReport } = useQuery({
    queryKey: ['report-daily', date],
    queryFn: () => reportsApi.getDaily(date),
    enabled: activeTab === 'daily',
  });

  const { data: operatorReport = [] } = useQuery({
    queryKey: ['report-operators'],
    queryFn: () => reportsApi.getOperatorPerformance(),
    enabled: activeTab === 'operators',
  });

  const { data: missedCalls = [] } = useQuery({
    queryKey: ['report-missed'],
    queryFn: () => reportsApi.getMissedCalls(),
    enabled: activeTab === 'missed',
  });

  const pieData = dailyReport ? [
    { name: 'Answered', value: dailyReport.answeredCalls, color: '#22c55e' },
    { name: 'Missed', value: dailyReport.missedCalls, color: '#ef4444' },
  ] : [];

  const directionData = dailyReport ? [
    { name: 'Inbound', value: dailyReport.inboundCalls, color: '#3b82f6' },
    { name: 'Outbound', value: dailyReport.outboundCalls, color: '#8b5cf6' },
    { name: 'Internal', value: dailyReport.internalCalls, color: '#64748b' },
  ] : [];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-white">Reports</h1>
        <p className="text-slate-400 text-sm mt-1">Analyze call center performance</p>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 bg-slate-900 border border-slate-800 rounded-xl p-1 w-fit">
        {(['daily', 'operators', 'missed'] as const).map(tab => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors capitalize ${activeTab === tab ? 'bg-blue-600 text-white' : 'text-slate-400 hover:text-slate-200'}`}
          >
            {tab === 'daily' ? 'Daily Report' : tab === 'operators' ? 'Operator Performance' : 'Missed Calls'}
          </button>
        ))}
      </div>

      {/* Daily Report */}
      {activeTab === 'daily' && (
        <div className="space-y-6">
          <div className="flex items-center gap-4">
            <input type="date" value={date} onChange={e => setDate(e.target.value)} className="input w-48" />
          </div>

          {dailyReport && (
            <>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {[
                  { label: 'Total Calls', value: dailyReport.totalCalls, color: 'text-blue-400' },
                  { label: 'Answered', value: dailyReport.answeredCalls, color: 'text-green-400' },
                  { label: 'Missed', value: dailyReport.missedCalls, color: 'text-red-400' },
                  { label: 'Recorded', value: dailyReport.recordedCalls, color: 'text-purple-400' },
                ].map(stat => (
                  <div key={stat.label} className="card text-center">
                    <p className={`text-3xl font-bold ${stat.color}`}>{stat.value}</p>
                    <p className="text-slate-400 text-sm mt-1">{stat.label}</p>
                  </div>
                ))}
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="card">
                  <h3 className="text-white font-semibold mb-4">Answer Rate</h3>
                  <ResponsiveContainer width="100%" height={200}>
                    <PieChart>
                      <Pie data={pieData} cx="50%" cy="50%" innerRadius={60} outerRadius={80} dataKey="value">
                        {pieData.map((entry, i) => <Cell key={i} fill={entry.color} />)}
                      </Pie>
                      <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px', color: '#f1f5f9' }} />
                    </PieChart>
                  </ResponsiveContainer>
                  <div className="flex justify-center gap-4 mt-2">
                    {pieData.map(d => (
                      <div key={d.name} className="flex items-center gap-1.5 text-sm">
                        <div className="w-3 h-3 rounded-full" style={{ backgroundColor: d.color }} />
                        <span className="text-slate-400">{d.name}: {d.value}</span>
                      </div>
                    ))}
                  </div>
                </div>

                <div className="card">
                  <h3 className="text-white font-semibold mb-4">Call Direction</h3>
                  <ResponsiveContainer width="100%" height={200}>
                    <BarChart data={directionData}>
                      <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" />
                      <XAxis dataKey="name" tick={{ fill: '#64748b', fontSize: 12 }} />
                      <YAxis tick={{ fill: '#64748b', fontSize: 12 }} />
                      <Tooltip contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px', color: '#f1f5f9' }} />
                      <Bar dataKey="value" radius={[4, 4, 0, 0]}>
                        {directionData.map((entry, i) => <Cell key={i} fill={entry.color} />)}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              </div>
            </>
          )}
        </div>
      )}

      {/* Operator Performance */}
      {activeTab === 'operators' && (
        <div className="card p-0 overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-800">
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Operator</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Extension</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Total Calls</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Answered</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Missed</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Avg Duration</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800">
              {(operatorReport as any[]).map((op: any, i: number) => (
                <tr key={i} className="hover:bg-slate-800/50">
                  <td className="px-4 py-3 text-sm text-slate-200">{op.operatorName}</td>
                  <td className="px-4 py-3 text-sm font-mono text-slate-400">{op.extensionNumber}</td>
                  <td className="px-4 py-3 text-sm text-slate-300">{op.totalCalls}</td>
                  <td className="px-4 py-3 text-sm text-green-400">{op.answeredCalls}</td>
                  <td className="px-4 py-3 text-sm text-red-400">{op.missedCalls}</td>
                  <td className="px-4 py-3 text-sm text-slate-400">{Math.round(op.averageDuration)}s</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Missed Calls */}
      {activeTab === 'missed' && (
        <div className="card p-0 overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-800">
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Caller</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Time</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Ring Duration</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800">
              {(missedCalls as any[]).map((call: any, i: number) => (
                <tr key={i} className="hover:bg-slate-800/50">
                  <td className="px-4 py-3">
                    <div className="text-sm font-mono text-slate-200">{call.callerNumber}</div>
                    {call.callerName && <div className="text-xs text-slate-500">{call.callerName}</div>}
                  </td>
                  <td className="px-4 py-3 text-sm text-slate-400">{format(new Date(call.startedAt), 'dd/MM/yyyy HH:mm')}</td>
                  <td className="px-4 py-3 text-sm text-slate-400">{call.ringDurationSeconds}s</td>
                </tr>
              ))}
              {(missedCalls as any[]).length === 0 && (
                <tr><td colSpan={3} className="text-center py-8 text-slate-500">No missed calls</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default ReportsPage;
