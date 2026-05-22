import React, { useState } from 'react';
import { useCallStore } from '../../store/callStore';
import { sipApi } from '../../services/api';
import { updateAgentStatus } from '../../services/signalr';

const statuses = [
  { value: 'Available', label: 'Available', color: 'bg-green-500' },
  { value: 'Busy', label: 'Busy', color: 'bg-red-500' },
  { value: 'Away', label: 'Away', color: 'bg-yellow-500' },
  { value: 'DoNotDisturb', label: 'Do Not Disturb', color: 'bg-red-700' },
  { value: 'WrapUp', label: 'Wrap Up', color: 'bg-orange-500' },
  { value: 'Offline', label: 'Offline', color: 'bg-slate-500' },
];

const AgentStatusSelector: React.FC = () => {
  const { agentStatus, setAgentStatus } = useCallStore();
  const [open, setOpen] = useState(false);

  const current = statuses.find(s => s.value === agentStatus) ?? statuses[5];

  const handleSelect = async (status: string) => {
    setAgentStatus(status);
    setOpen(false);
    await updateAgentStatus(status);
  };

  return (
    <div className="relative">
      <button
        onClick={() => setOpen(!open)}
        className="flex items-center gap-2 bg-slate-800 hover:bg-slate-700 border border-slate-700 rounded-lg px-3 py-1.5 text-sm text-slate-200 transition-colors"
      >
        <div className={`w-2 h-2 rounded-full ${current.color}`} />
        <span>{current.label}</span>
        <span className="text-slate-500">▾</span>
      </button>

      {open && (
        <div className="absolute right-0 top-full mt-1 w-48 bg-slate-800 border border-slate-700 rounded-xl shadow-xl z-50 overflow-hidden">
          {statuses.map((s) => (
            <button
              key={s.value}
              onClick={() => handleSelect(s.value)}
              className={`w-full flex items-center gap-3 px-4 py-2.5 text-sm text-left hover:bg-slate-700 transition-colors ${agentStatus === s.value ? 'text-blue-400' : 'text-slate-300'}`}
            >
              <div className={`w-2 h-2 rounded-full ${s.color}`} />
              {s.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};

export default AgentStatusSelector;
