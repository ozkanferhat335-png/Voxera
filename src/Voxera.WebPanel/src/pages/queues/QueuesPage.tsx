import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { queuesApi } from '../../services/api';
import { CallQueue } from '../../types';
import toast from 'react-hot-toast';

const QueuesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ name: '', extension: '' });

  const { data: queues = [], isLoading } = useQuery<CallQueue[]>({
    queryKey: ['queues'],
    queryFn: queuesApi.getAll,
  });

  const createMutation = useMutation({
    mutationFn: queuesApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['queues'] });
      setShowCreate(false);
      setForm({ name: '', extension: '' });
      toast.success('Queue created');
    },
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">Call Queues</h1>
          <p className="text-slate-400 text-sm mt-1">Manage call distribution queues</p>
        </div>
        <button onClick={() => setShowCreate(true)} className="btn-primary">+ New Queue</button>
      </div>

      {isLoading ? (
        <div className="text-center py-12 text-slate-500">Loading...</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {queues.map(queue => (
            <div key={queue.id} className="card">
              <div className="flex items-start justify-between mb-3">
                <div>
                  <h3 className="text-white font-semibold">{queue.name}</h3>
                  <p className="text-slate-500 text-sm">Extension: <span className="font-mono text-blue-400">{queue.extension}</span></p>
                </div>
                <div className="flex items-center gap-2">
                  <span className="badge-gray">{queue.strategy}</span>
                  <span className={queue.isActive ? 'badge-green' : 'badge-gray'}>{queue.isActive ? 'Active' : 'Inactive'}</span>
                </div>
              </div>

              <div className="flex items-center gap-2 text-sm text-slate-400">
                <span>👥 {queue.agents?.length ?? 0} agents</span>
              </div>

              {queue.agents?.length > 0 && (
                <div className="mt-3 space-y-1">
                  {queue.agents.slice(0, 3).map(agent => (
                    <div key={agent.id} className="flex items-center justify-between text-xs text-slate-500">
                      <span>Agent {agent.extensionId.slice(0, 8)}...</span>
                      <span>Priority: {agent.priority}</span>
                    </div>
                  ))}
                </div>
              )}

              <div className="flex gap-2 mt-4 pt-3 border-t border-slate-800">
                <button className="btn-secondary text-xs py-1 flex-1">Manage Agents</button>
                <button className="btn-secondary text-xs py-1 flex-1">Settings</button>
              </div>
            </div>
          ))}

          {queues.length === 0 && (
            <div className="col-span-2 text-center py-16 text-slate-500">
              <p className="text-4xl mb-3">📋</p>
              <p className="font-medium">No call queues configured</p>
              <p className="text-sm mt-1">Create a queue to distribute calls among agents</p>
            </div>
          )}
        </div>
      )}

      {showCreate && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-2xl p-6 w-full max-w-md">
            <h3 className="text-white font-semibold text-lg mb-4">Create Call Queue</h3>
            <div className="space-y-4">
              <div>
                <label className="block text-sm text-slate-400 mb-1">Queue Name</label>
                <input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} className="input" placeholder="e.g. Support Queue" />
              </div>
              <div>
                <label className="block text-sm text-slate-400 mb-1">Extension Number</label>
                <input value={form.extension} onChange={e => setForm(f => ({ ...f, extension: e.target.value }))} className="input" placeholder="e.g. 9001" />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={() => setShowCreate(false)} className="btn-secondary flex-1">Cancel</button>
              <button onClick={() => createMutation.mutate(form)} disabled={!form.name || !form.extension || createMutation.isPending} className="btn-primary flex-1">
                {createMutation.isPending ? 'Creating...' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default QueuesPage;
