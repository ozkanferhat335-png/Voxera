import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../store/authStore';
import { useCallStore } from '../store/callStore';
import toast from 'react-hot-toast';

const WS_URL = process.env.REACT_APP_WS_URL || 'http://localhost:5000';

let callHubConnection: signalR.HubConnection | null = null;
let dashboardHubConnection: signalR.HubConnection | null = null;

export const connectCallHub = async () => {
  const token = useAuthStore.getState().accessToken;
  if (!token) return;

  callHubConnection = new signalR.HubConnectionBuilder()
    .withUrl(`${WS_URL}/hubs/calls`, {
      accessTokenFactory: () => token,
      transport: signalR.HttpTransportType.WebSockets,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  // Incoming call event
  callHubConnection.on('IncomingCall', (data: any) => {
    useCallStore.getState().setIncomingCall({
      callId: data.callId,
      callerNumber: data.callerNumber,
      callerName: data.callerName,
      startedAt: new Date(),
    });
    // Play ringtone
    playRingtone();
    toast.custom((t) => (
      <div className={`${t.visible ? 'animate-enter' : 'animate-leave'} max-w-md w-full bg-slate-800 shadow-lg rounded-lg pointer-events-auto flex ring-1 ring-black ring-opacity-5`}>
        <div className="flex-1 w-0 p-4">
          <div className="flex items-start">
            <div className="ml-3 flex-1">
              <p className="text-sm font-medium text-white">Incoming Call</p>
              <p className="mt-1 text-sm text-slate-400">{data.callerNumber}</p>
            </div>
          </div>
        </div>
      </div>
    ), { duration: 30000 });
  });

  // Call ended event
  callHubConnection.on('CallEnded', (data: any) => {
    useCallStore.getState().removeActiveCall(data.callId);
    useCallStore.getState().setIncomingCall(null);
  });

  // Agent status changed
  callHubConnection.on('AgentStatusChanged', (data: any) => {
    console.log('Agent status changed:', data);
  });

  try {
    await callHubConnection.start();
    console.log('CallHub connected');
  } catch (err) {
    console.error('CallHub connection failed:', err);
  }
};

export const connectDashboardHub = async () => {
  const token = useAuthStore.getState().accessToken;
  if (!token) return;

  dashboardHubConnection = new signalR.HubConnectionBuilder()
    .withUrl(`${WS_URL}/hubs/dashboard`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .build();

  dashboardHubConnection.on('StatsUpdated', (data: any) => {
    console.log('Dashboard stats updated:', data);
  });

  try {
    await dashboardHubConnection.start();
    console.log('DashboardHub connected');
  } catch (err) {
    console.error('DashboardHub connection failed:', err);
  }
};

export const updateAgentStatus = async (status: string) => {
  if (callHubConnection?.state === signalR.HubConnectionState.Connected) {
    await callHubConnection.invoke('UpdateAgentStatus', status);
  }
};

export const disconnectHubs = async () => {
  await callHubConnection?.stop();
  await dashboardHubConnection?.stop();
};

let ringtoneAudio: HTMLAudioElement | null = null;
const playRingtone = () => {
  if (!ringtoneAudio) {
    ringtoneAudio = new Audio('/sounds/ringtone.mp3');
    ringtoneAudio.loop = true;
  }
  ringtoneAudio.play().catch(() => {});
};

export const stopRingtone = () => {
  ringtoneAudio?.pause();
  if (ringtoneAudio) ringtoneAudio.currentTime = 0;
};
