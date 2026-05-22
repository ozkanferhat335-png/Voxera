import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { sipApi, extensionsApi } from '../../services/api';
import { SipAccount, Extension } from '../../types';
import toast from 'react-hot-toast';

const statusColors: Record<string, string> = {
  Available: 'badge-green',
  Busy: 'badge-red',
  Away: 'badge-yellow',
  Offline: 'badge-gray',
  DoNotDisturb: 'badge-red',
};

const SipAccountsPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [showCreate, setShowCreate] = useState(false);
  const [newKey, setNewKey] = useState<{ username: string; domain: string; password: string } | null>(null);
  const [form, setForm] = useState({ extensionId: '', enableWebRtc: false });

  const { data: accounts = [], isLoading } = useQuery<SipAccount[]>({
    queryKey: ['sip-accounts'],
    queryFn: sipApi.getAll,
  });

  const { data: extensions = [] } = useQuery<Extension[]>({
    queryKey: ['extensions'],
    queryFn: extensionsApi.getAll,
  });

  const createMutation = useMutation({
    mutationFn: sipApi.create,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['sip-accounts'] });
      setShowCreate(false);
      if (data.plainPassword) {
        setNewKey({ username: data.username, domain: data.domain, password: data.plainPassword });
      }
      toast.success('SIP account created');
    },
    onError: (err: any) => toast.error(err.response?.data?.error ?? 'Failed to create SIP account'),
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">SIP Accounts</h1>
          <p className="text-slate-400 text-sm mt-1">Manage SIP credentials for softphones</p>
        </div>
        <button onClick={() => setShowCreate(true)} className="btn-primary">+ New SIP Account</button>
      </div>

      {/* Accounts table */}
      <div className="card p-0 overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-slate-800">
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Username</th>
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Domain</th>
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Status</th>
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Agent Status</th>
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800">
            {isLoading ? (
              <tr><td colSpan={5} className="text-center py-8 text-slate-500">Loading...</td></tr>
            ) : accounts.length === 0 ? (
              <tr><td colSpan={5} className="text-center py-8 text-slate-500">No SIP accounts found</td></tr>
            ) : accounts.map(acc => (
              <tr key={acc.id} className="hover:bg-slate-800/50">
                <td className="px-4 py-3 text-sm font-mono text-slate-200">{acc.username}</td>
                <td className="px-4 py-3 text-sm text-slate-400">{acc.domain}</td>
                <td className="px-4 py-3"><span className={acc.status === 'Active' ? 'badge-green' : 'badge-gray'}>{acc.status}</span></td>
                <td className="px-4 py-3"><span className={statusColors[acc.agentStatus] ?? 'badge-gray'}>{acc.agentStatus}</span></td>
                <td className="px-4 py-3">
                  <button className="text-red-400 hover:text-red-300 text-xs">Deactivate</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Create modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-2xl p-6 w-full max-w-md">
            <h3 className="text-white font-semibold text-lg mb-4">Create SIP Account</h3>
            <div className="space-y-4">
              <div>
                <label className="block text-sm text-slate-400 mb-1">Extension</label>
                <select value={form.extensionId} onChange={e => setForm(f => ({ ...f, extensionId: e.target.value }))} className="input">
                  <option value="">Select extension...</option>
                  {extensions.map(ext => (
                    <option key={ext.id} value={ext.id}>{ext.number} - {ext.displayName}</option>
                  ))}
                </select>
              </div>
              <div className="flex items-center gap-3">
                <input type="checkbox" id="webrtc" checked={form.enableWebRtc} onChange={e => setForm(f => ({ ...f, enableWebRtc: e.target.checked }))} className="w-4 h-4" />
                <label htmlFor="webrtc" className="text-sm text-slate-300">Enable WebRTC (browser-based calls)</label>
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={() => setShowCreate(false)} className="btn-secondary flex-1">Cancel</button>
              <button onClick={() => createMutation.mutate({ extensionId: form.extensionId, enableWebRtc: form.enableWebRtc })} disabled={!form.extensionId || createMutation.isPending} className="btn-primary flex-1">
                {createMutation.isPending ? 'Creating...' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* New key display */}
      {newKey && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
          <div className="bg-slate-900 border border-green-600/30 rounded-2xl p-6 w-full max-w-md">
            <h3 className="text-white font-semibold text-lg mb-2">SIP Account Created</h3>
            <p className="text-slate-400 text-sm mb-4">Save these credentials. The password will not be shown again.</p>
            <div className="bg-slate-800 rounded-xl p-4 space-y-2 font-mono text-sm">
              <div><span className="text-slate-500">Username: </span><span className="text-green-400">{newKey.username}</span></div>
              <div><span className="text-slate-500">Domain: </span><span className="text-blue-400">{newKey.domain}</span></div>
              <div><span className="text-slate-500">Password: </span><span className="text-yellow-400">{newKey.password}</span></div>
            </div>
            <button onClick={() => setNewKey(null)} className="btn-primary w-full mt-4">I've saved the credentials</button>
          </div>
        </div>
      )}
    </div>
  );
};

export default SipAccountsPage;
