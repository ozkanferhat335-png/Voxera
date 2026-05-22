import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { extensionsApi } from '../../services/api';
import { Extension } from '../../types';
import toast from 'react-hot-toast';

const agentStatusColors: Record<string, string> = {
  Available: 'bg-green-500',
  Busy: 'bg-red-500',
  Away: 'bg-yellow-500',
  DoNotDisturb: 'bg-red-700',
  Offline: 'bg-slate-500',
};

const ExtensionsPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ number: '', displayName: '', type: 'User' });

  const { data: extensions = [], isLoading } = useQuery<Extension[]>({
    queryKey: ['extensions'],
    queryFn: extensionsApi.getAll,
  });

  const createMutation = useMutation({
    mutationFn: extensionsApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['extensions'] });
      setShowCreate(false);
      setForm({ number: '', displayName: '', type: 'User' });
      toast.success('Extension created successfully');
    },
    onError: (err: any) => toast.error(err.response?.data?.error ?? 'Failed to create extension'),
  });

  const deleteMutation = useMutation({
    mutationFn: extensionsApi.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['extensions'] });
      toast.success('Extension deleted');
    },
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">Extensions</h1>
          <p className="text-slate-400 text-sm mt-1">Manage internal phone extensions</p>
        </div>
        <button onClick={() => setShowCreate(true)} className="btn-primary">+ New Extension</button>
      </div>

      {/* Extensions grid */}
      {isLoading ? (
        <div className="text-center py-12 text-slate-500">Loading extensions...</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {extensions.map(ext => (
            <div key={ext.id} className="card hover:border-slate-700 transition-colors">
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-3">
                  <div className="relative">
                    <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl flex items-center justify-center text-white font-bold">
                      {ext.number}
                    </div>
                    {ext.agentStatus && (
                      <div className={`absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full border-2 border-slate-900 ${agentStatusColors[ext.agentStatus] ?? 'bg-slate-500'}`} />
                    )}
                  </div>
                  <div>
                    <p className="text-white font-semibold">{ext.displayName}</p>
                    <p className="text-slate-500 text-xs">{ext.type}</p>
                  </div>
                </div>
                <span className={ext.status === 'Active' ? 'badge-green' : 'badge-gray'}>{ext.status}</span>
              </div>

              <div className="space-y-1.5 text-xs text-slate-500">
                <div className="flex items-center justify-between">
                  <span>Voicemail</span>
                  <span className={ext.voicemailEnabled ? 'text-green-400' : 'text-slate-600'}>{ext.voicemailEnabled ? 'Enabled' : 'Disabled'}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span>Recording</span>
                  <span className={ext.recordCalls ? 'text-green-400' : 'text-slate-600'}>{ext.recordCalls ? 'On' : 'Off'}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span>DND</span>
                  <span className={ext.doNotDisturb ? 'text-red-400' : 'text-slate-600'}>{ext.doNotDisturb ? 'Active' : 'Off'}</span>
                </div>
                {ext.forwardTo && (
                  <div className="flex items-center justify-between">
                    <span>Forward to</span>
                    <span className="text-blue-400">{ext.forwardTo}</span>
                  </div>
                )}
              </div>

              <div className="flex gap-2 mt-4 pt-3 border-t border-slate-800">
                <button className="btn-secondary text-xs py-1 flex-1">Edit</button>
                <button
                  onClick={() => { if (window.confirm('Delete this extension?')) deleteMutation.mutate(ext.id); }}
                  className="btn-danger text-xs py-1 flex-1"
                >Delete</button>
              </div>
            </div>
          ))}

          {extensions.length === 0 && (
            <div className="col-span-3 text-center py-16 text-slate-500">
              <p className="text-4xl mb-3">☎</p>
              <p className="font-medium">No extensions yet</p>
              <p className="text-sm mt-1">Create your first extension to get started</p>
            </div>
          )}
        </div>
      )}

      {/* Create modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-2xl p-6 w-full max-w-md">
            <h3 className="text-white font-semibold text-lg mb-4">Create Extension</h3>
            <div className="space-y-4">
              <div>
                <label className="block text-sm text-slate-400 mb-1">Extension Number</label>
                <input value={form.number} onChange={e => setForm(f => ({ ...f, number: e.target.value }))} className="input" placeholder="e.g. 101" />
              </div>
              <div>
                <label className="block text-sm text-slate-400 mb-1">Display Name</label>
                <input value={form.displayName} onChange={e => setForm(f => ({ ...f, displayName: e.target.value }))} className="input" placeholder="e.g. John Doe" />
              </div>
              <div>
                <label className="block text-sm text-slate-400 mb-1">Type</label>
                <select value={form.type} onChange={e => setForm(f => ({ ...f, type: e.target.value }))} className="input">
                  <option value="User">User</option>
                  <option value="Queue">Queue</option>
                  <option value="IVR">IVR</option>
                  <option value="Conference">Conference</option>
                </select>
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={() => setShowCreate(false)} className="btn-secondary flex-1">Cancel</button>
              <button onClick={() => createMutation.mutate(form)} disabled={createMutation.isPending} className="btn-primary flex-1">
                {createMutation.isPending ? 'Creating...' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ExtensionsPage;
