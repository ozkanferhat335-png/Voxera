import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { callsApi } from '../../services/api';
import { CallLog, PagedResult } from '../../types';
import { format } from 'date-fns';
import toast from 'react-hot-toast';

const directionBadge = (d: string) => {
  const map: Record<string, string> = { Inbound: 'badge-green', Outbound: 'badge-blue', Internal: 'badge-gray' };
  return <span className={map[d] ?? 'badge-gray'}>{d}</span>;
};

const statusBadge = (s: string) => {
  const map: Record<string, string> = { Completed: 'badge-green', Missed: 'badge-red', Active: 'badge-blue', Failed: 'badge-red', Busy: 'badge-yellow' };
  return <span className={map[s] ?? 'badge-gray'}>{s}</span>;
};

const formatDuration = (seconds?: number) => {
  if (!seconds) return '-';
  const m = Math.floor(seconds / 60);
  const s = seconds % 60;
  return `${m}:${s.toString().padStart(2, '0')}`;
};

const CallsPage: React.FC = () => {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [direction, setDirection] = useState('');
  const [status, setStatus] = useState('');
  const [showDialer, setShowDialer] = useState(false);
  const [dialFrom, setDialFrom] = useState('');
  const [dialTo, setDialTo] = useState('');

  const { data, isLoading, refetch } = useQuery<PagedResult<CallLog>>({
    queryKey: ['calls', page, search, direction, status],
    queryFn: () => callsApi.getCallLogs({ page, pageSize: 20, search: search || undefined, direction: direction || undefined, status: status || undefined }),
  });

  const handleOriginate = async () => {
    if (!dialFrom || !dialTo) return;
    try {
      await callsApi.originate(dialFrom, dialTo);
      toast.success(`Calling ${dialTo}...`);
      setShowDialer(false);
      setDialFrom('');
      setDialTo('');
    } catch {
      toast.error('Failed to initiate call');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">Call Logs</h1>
          <p className="text-slate-400 text-sm mt-1">View and manage all call records</p>
        </div>
        <button onClick={() => setShowDialer(true)} className="btn-primary flex items-center gap-2">
          <span>📞</span> New Call
        </button>
      </div>

      {/* Filters */}
      <div className="card">
        <div className="flex flex-wrap gap-3">
          <input
            value={search}
            onChange={e => { setSearch(e.target.value); setPage(1); }}
            className="input flex-1 min-w-48"
            placeholder="Search by number..."
          />
          <select value={direction} onChange={e => { setDirection(e.target.value); setPage(1); }} className="input w-40">
            <option value="">All Directions</option>
            <option value="Inbound">Inbound</option>
            <option value="Outbound">Outbound</option>
            <option value="Internal">Internal</option>
          </select>
          <select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }} className="input w-40">
            <option value="">All Statuses</option>
            <option value="Completed">Completed</option>
            <option value="Missed">Missed</option>
            <option value="Active">Active</option>
            <option value="Failed">Failed</option>
          </select>
          <button onClick={() => refetch()} className="btn-secondary">Refresh</button>
        </div>
      </div>

      {/* Table */}
      <div className="card p-0 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-800">
                <th className="text-left text-xs font-medium text-slate-500 uppercase tracking-wider px-4 py-3">Caller</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase tracking-wider px-4 py-3">Callee</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase tracking-wider px-4 py-3">Direction</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase tracking-wider px-4 py-3">Status</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase tracking-wider px-4 py-3">Duration</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase tracking-wider px-4 py-3">Started</th>
                <th className="text-left text-xs font-medium text-slate-500 uppercase tracking-wider px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800">
              {isLoading ? (
                <tr><td colSpan={7} className="text-center py-12 text-slate-500">Loading...</td></tr>
              ) : data?.items?.length === 0 ? (
                <tr><td colSpan={7} className="text-center py-12 text-slate-500">No call logs found</td></tr>
              ) : data?.items?.map(call => (
                <tr key={call.id} className="hover:bg-slate-800/50 transition-colors">
                  <td className="px-4 py-3">
                    <div className="text-sm font-medium text-slate-200">{call.callerNumber ?? '-'}</div>
                    {call.callerName && <div className="text-xs text-slate-500">{call.callerName}</div>}
                  </td>
                  <td className="px-4 py-3 text-sm text-slate-300">{call.calleeNumber ?? '-'}</td>
                  <td className="px-4 py-3">{directionBadge(call.direction)}</td>
                  <td className="px-4 py-3">{statusBadge(call.status)}</td>
                  <td className="px-4 py-3 text-sm text-slate-300 font-mono">{formatDuration(call.durationSeconds)}</td>
                  <td className="px-4 py-3 text-sm text-slate-400">
                    {format(new Date(call.startedAt), 'dd/MM HH:mm')}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      {call.isRecorded && (
                        <button className="text-blue-400 hover:text-blue-300 text-xs" title="Download recording">▶ Recording</button>
                      )}
                      {call.aiSummary && (
                        <button className="text-purple-400 hover:text-purple-300 text-xs" title="AI Summary">AI</button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t border-slate-800">
            <span className="text-sm text-slate-500">
              Showing {((page - 1) * 20) + 1}–{Math.min(page * 20, data.totalCount)} of {data.totalCount}
            </span>
            <div className="flex gap-2">
              <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1} className="btn-secondary text-sm py-1 px-3 disabled:opacity-40">← Prev</button>
              <button onClick={() => setPage(p => Math.min(data.totalPages, p + 1))} disabled={page === data.totalPages} className="btn-secondary text-sm py-1 px-3 disabled:opacity-40">Next →</button>
            </div>
          </div>
        )}
      </div>

      {/* Dialer modal */}
      {showDialer && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-2xl p-6 w-full max-w-sm">
            <h3 className="text-white font-semibold text-lg mb-4">Make a Call</h3>
            <div className="space-y-3">
              <div>
                <label className="block text-sm text-slate-400 mb-1">From Extension</label>
                <input value={dialFrom} onChange={e => setDialFrom(e.target.value)} className="input" placeholder="e.g. 101" />
              </div>
              <div>
                <label className="block text-sm text-slate-400 mb-1">To Number</label>
                <input value={dialTo} onChange={e => setDialTo(e.target.value)} className="input" placeholder="e.g. 102 or 05001234567" />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={() => setShowDialer(false)} className="btn-secondary flex-1">Cancel</button>
              <button onClick={handleOriginate} className="btn-primary flex-1">Call</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CallsPage;
