import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiKeysApi } from '../../services/api';
import { ApiKey } from '../../types';
import { format } from 'date-fns';
import toast from 'react-hot-toast';

const ApiKeysPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [showCreate, setShowCreate] = useState(false);
  const [newKey, setNewKey] = useState<string | null>(null);
  const [form, setForm] = useState({ name: '', permissions: ['calls:read', 'calls:write'] });

  const { data: keys = [], isLoading } = useQuery<ApiKey[]>({
    queryKey: ['api-keys'],
    queryFn: apiKeysApi.getAll,
  });

  const createMutation = useMutation({
    mutationFn: apiKeysApi.create,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['api-keys'] });
      setShowCreate(false);
      setNewKey(data.key);
      toast.success('API key created');
    },
  });

  const revokeMutation = useMutation({
    mutationFn: apiKeysApi.revoke,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['api-keys'] });
      toast.success('API key revoked');
    },
  });

  const allPermissions = ['calls:read', 'calls:write', 'sip:manage', 'extensions:read', 'extensions:write', 'reports:read'];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">API Keys</h1>
          <p className="text-slate-400 text-sm mt-1">Manage API keys for external integrations</p>
        </div>
        <button onClick={() => setShowCreate(true)} className="btn-primary">+ Create API Key</button>
      </div>

      {/* API Keys table */}
      <div className="card p-0 overflow-hidden">
        <table className="w-full">
          <thead>
            <tr className="border-b border-slate-800">
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Name</th>
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Key Prefix</th>
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Status</th>
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Permissions</th>
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Last Used</th>
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Requests</th>
              <th className="text-left text-xs font-medium text-slate-500 uppercase px-4 py-3">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800">
            {isLoading ? (
              <tr><td colSpan={7} className="text-center py-8 text-slate-500">Loading...</td></tr>
            ) : keys.length === 0 ? (
              <tr><td colSpan={7} className="text-center py-8 text-slate-500">No API keys yet</td></tr>
            ) : keys.map(key => (
              <tr key={key.id} className="hover:bg-slate-800/50">
                <td className="px-4 py-3 text-sm font-medium text-slate-200">{key.name}</td>
                <td className="px-4 py-3 text-sm font-mono text-slate-400">{key.keyPrefix}...</td>
                <td className="px-4 py-3"><span className={key.status === 'Active' ? 'badge-green' : 'badge-red'}>{key.status}</span></td>
                <td className="px-4 py-3">
                  <div className="flex flex-wrap gap-1">
                    {key.permissions.slice(0, 2).map(p => <span key={p} className="badge-blue text-xs">{p}</span>)}
                    {key.permissions.length > 2 && <span className="badge-gray text-xs">+{key.permissions.length - 2}</span>}
                  </div>
                </td>
                <td className="px-4 py-3 text-sm text-slate-400">{key.lastUsedAt ? format(new Date(key.lastUsedAt), 'dd/MM HH:mm') : 'Never'}</td>
                <td className="px-4 py-3 text-sm text-slate-400">{key.requestCount.toLocaleString()}</td>
                <td className="px-4 py-3">
                  <button onClick={() => { if (window.confirm('Revoke this API key?')) revokeMutation.mutate(key.id); }} className="text-red-400 hover:text-red-300 text-xs">Revoke</button>
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
            <h3 className="text-white font-semibold text-lg mb-4">Create API Key</h3>
            <div className="space-y-4">
              <div>
                <label className="block text-sm text-slate-400 mb-1">Key Name</label>
                <input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} className="input" placeholder="e.g. CRM Integration" />
              </div>
              <div>
                <label className="block text-sm text-slate-400 mb-2">Permissions</label>
                <div className="space-y-2">
                  {allPermissions.map(perm => (
                    <label key={perm} className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={form.permissions.includes(perm)}
                        onChange={e => setForm(f => ({
                          ...f,
                          permissions: e.target.checked ? [...f.permissions, perm] : f.permissions.filter(p => p !== perm)
                        }))}
                        className="w-4 h-4"
                      />
                      <span className="text-sm text-slate-300 font-mono">{perm}</span>
                    </label>
                  ))}
                </div>
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={() => setShowCreate(false)} className="btn-secondary flex-1">Cancel</button>
              <button onClick={() => createMutation.mutate({ name: form.name, permissions: form.permissions })} disabled={!form.name || createMutation.isPending} className="btn-primary flex-1">
                {createMutation.isPending ? 'Creating...' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* New key display */}
      {newKey && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
          <div className="bg-slate-900 border border-yellow-600/30 rounded-2xl p-6 w-full max-w-lg">
            <h3 className="text-white font-semibold text-lg mb-2">🔑 Your New API Key</h3>
            <p className="text-yellow-400 text-sm mb-4">⚠️ This key will only be shown once. Copy and store it securely.</p>
            <div className="bg-slate-800 rounded-xl p-4 font-mono text-sm text-green-400 break-all select-all">{newKey}</div>
            <button onClick={() => { navigator.clipboard.writeText(newKey); toast.success('Copied!'); }} className="btn-secondary w-full mt-3">Copy to Clipboard</button>
            <button onClick={() => setNewKey(null)} className="btn-primary w-full mt-2">I've saved the key</button>
          </div>
        </div>
      )}
    </div>
  );
};

export default ApiKeysPage;
