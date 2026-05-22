import { create } from 'zustand';
import { ActiveCall } from '../types';

interface IncomingCall {
  callId: string;
  callerNumber: string;
  callerName?: string;
  startedAt: Date;
}

interface CallState {
  activeCalls: ActiveCall[];
  incomingCall: IncomingCall | null;
  agentStatus: string;
  setActiveCalls: (calls: ActiveCall[]) => void;
  addActiveCall: (call: ActiveCall) => void;
  removeActiveCall: (callId: string) => void;
  setIncomingCall: (call: IncomingCall | null) => void;
  setAgentStatus: (status: string) => void;
}

export const useCallStore = create<CallState>((set) => ({
  activeCalls: [],
  incomingCall: null,
  agentStatus: 'Offline',
  setActiveCalls: (calls) => set({ activeCalls: calls }),
  addActiveCall: (call) => set((state) => ({ activeCalls: [...state.activeCalls, call] })),
  removeActiveCall: (callId) => set((state) => ({ activeCalls: state.activeCalls.filter(c => c.callId !== callId) })),
  setIncomingCall: (call) => set({ incomingCall: call }),
  setAgentStatus: (status) => set({ agentStatus: status }),
}));
