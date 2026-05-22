import React, { useEffect, useState } from 'react';
import { useCallStore } from '../../store/callStore';
import { callsApi } from '../../services/api';
import { stopRingtone } from '../../services/signalr';
import toast from 'react-hot-toast';

interface Props {
  call: { callId: string; callerNumber: string; callerName?: string; startedAt: Date };
}

const IncomingCallPopup: React.FC<Props> = ({ call }) => {
  const { setIncomingCall } = useCallStore();
  const [elapsed, setElapsed] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => setElapsed(e => e + 1), 1000);
    return () => clearInterval(interval);
  }, []);

  const handleAnswer = async () => {
    stopRingtone();
    setIncomingCall(null);
    toast.success('Call answered');
  };

  const handleReject = async () => {
    try {
      await callsApi.hangup(call.callId);
    } catch {}
    stopRingtone();
    setIncomingCall(null);
    toast('Call rejected', { icon: '📵' });
  };

  return (
    <div className="fixed bottom-6 right-6 z-50 w-80">
      <div className="bg-slate-800 border border-slate-700 rounded-2xl shadow-2xl overflow-hidden">
        {/* Header */}
        <div className="bg-gradient-to-r from-blue-600 to-indigo-600 px-4 py-3">
          <div className="flex items-center gap-2">
            <div className="w-2 h-2 bg-white rounded-full call-ringing" />
            <span className="text-white text-sm font-medium">Incoming Call</span>
            <span className="ml-auto text-white/70 text-xs">{elapsed}s</span>
          </div>
        </div>

        {/* Caller info */}
        <div className="px-4 py-4 flex items-center gap-3">
          <div className="w-12 h-12 bg-gradient-to-br from-blue-500 to-purple-600 rounded-full flex items-center justify-center text-white font-bold text-lg flex-shrink-0">
            {call.callerName?.charAt(0) ?? call.callerNumber.charAt(0)}
          </div>
          <div>
            {call.callerName && <p className="text-white font-semibold">{call.callerName}</p>}
            <p className="text-slate-300 font-mono text-sm">{call.callerNumber}</p>
          </div>
        </div>

        {/* Actions */}
        <div className="px-4 pb-4 flex gap-3">
          <button
            onClick={handleReject}
            className="flex-1 bg-red-600 hover:bg-red-700 text-white font-medium py-2.5 rounded-xl transition-colors flex items-center justify-center gap-2"
          >
            <span>✕</span> Reject
          </button>
          <button
            onClick={handleAnswer}
            className="flex-1 bg-green-600 hover:bg-green-700 text-white font-medium py-2.5 rounded-xl transition-colors flex items-center justify-center gap-2"
          >
            <span>✓</span> Answer
          </button>
        </div>
      </div>
    </div>
  );
};

export default IncomingCallPopup;
