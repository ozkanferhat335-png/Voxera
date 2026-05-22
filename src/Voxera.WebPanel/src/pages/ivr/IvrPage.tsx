import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ivrApi } from '../../services/api';
import { IvrMenu } from '../../types';
import toast from 'react-hot-toast';

const IvrPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ name: '', greetingText: '' });

  const { data: menus = [], isLoading } = useQuery<IvrMenu[]>({
    queryKey: ['ivr-menus'],
    queryFn: ivrApi.getAll,
  });

  const createMutation = useMutation({
    mutationFn: ivrApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ivr-menus'] });
      setShowCreate(false);
      setForm({ name: '', greetingText: '' });
      toast.success('IVR menu created');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: ivrApi.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ivr-menus'] });
      toast.success('IVR menu deleted');
    },
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-white">IVR Menus</h1>
          <p className="text-slate-400 text-sm mt-1">Configure interactive voice response menus</p>
        </div>
        <button onClick={() => setShowCreate(true)} className="btn-primary">+ New IVR Menu</button>
      </div>

      {isLoading ? (
        <div className="text-center py-12 text-slate-500">Loading...</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {menus.map(menu => (
            <div key={menu.id} className="card">
              <div className="flex items-start justify-between mb-3">
                <div>
                  <h3 className="text-white font-semibold">{menu.name}</h3>
                  {menu.greetingText && <p className="text-slate-400 text-sm mt-1 line-clamp-2">{menu.greetingText}</p>}
                </div>
                <span className={menu.isActive ? 'badge-green' : 'badge-gray'}>{menu.isActive ? 'Active' : 'Inactive'}</span>
              </div>

              {menu.options?.length > 0 && (
                <div className="space-y-1.5 mt-3">
                  <p className="text-xs text-slate-500 font-medium uppercase tracking-wider">Options</p>
                  {menu.options.map(opt => (
                    <div key={opt.id} className="flex items-center gap-3 text-sm">
                      <span className="w-6 h-6 bg-slate-800 rounded-lg flex items-center justify-center text-blue-400 font-mono font-bold text-xs">{opt.digit}</span>
                      <span className="text-slate-300">{opt.description}</span>
                      <span className="text-slate-500 text-xs ml-auto">{opt.actionType}</span>
                    </div>
                  ))}
                </div>
              )}

              <div className="flex gap-2 mt-4 pt-3 border-t border-slate-800">
                <button className="btn-secondary text-xs py-1 flex-1">Edit Options</button>
                <button onClick={() => deleteMutation.mutate(menu.id)} className="btn-danger text-xs py-1 flex-1">Delete</button>
              </div>
            </div>
          ))}

          {menus.length === 0 && (
            <div className="col-span-2 text-center py-16 text-slate-500">
              <p className="text-4xl mb-3">🎛</p>
              <p className="font-medium">No IVR menus configured</p>
              <p className="text-sm mt-1">Create an IVR menu to handle incoming calls automatically</p>
            </div>
          )}
        </div>
      )}

      {showCreate && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-2xl p-6 w-full max-w-md">
            <h3 className="text-white font-semibold text-lg mb-4">Create IVR Menu</h3>
            <div className="space-y-4">
              <div>
                <label className="block text-sm text-slate-400 mb-1">Menu Name</label>
                <input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} className="input" placeholder="e.g. Main Menu" />
              </div>
              <div>
                <label className="block text-sm text-slate-400 mb-1">Greeting Text (TTS)</label>
                <textarea value={form.greetingText} onChange={e => setForm(f => ({ ...f, greetingText: e.target.value }))} className="input h-24 resize-none" placeholder="Welcome to Voxera. Press 1 for sales, press 2 for support..." />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={() => setShowCreate(false)} className="btn-secondary flex-1">Cancel</button>
              <button onClick={() => createMutation.mutate(form)} disabled={!form.name || createMutation.isPending} className="btn-primary flex-1">
                {createMutation.isPending ? 'Creating...' : 'Create'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default IvrPage;
