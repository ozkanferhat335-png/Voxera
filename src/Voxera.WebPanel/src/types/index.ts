export interface User {
  userId: string;
  fullName: string;
  email: string;
  role: string;
  companyId: string;
  companyName: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  userId: string;
  fullName: string;
  email: string;
  role: string;
  companyId: string;
  companyName: string;
}

export interface Extension {
  id: string;
  number: string;
  displayName: string;
  type: string;
  status: string;
  voicemailEnabled: boolean;
  recordCalls: boolean;
  doNotDisturb: boolean;
  forwardTo?: string;
  agentStatus?: string;
}

export interface SipAccount {
  id: string;
  username: string;
  domain: string;
  status: string;
  agentStatus: string;
}

export interface CallLog {
  id: string;
  callId: string;
  callerNumber?: string;
  callerName?: string;
  calleeNumber?: string;
  direction: string;
  status: string;
  startedAt: string;
  answeredAt?: string;
  endedAt?: string;
  durationSeconds?: number;
  isRecorded: boolean;
  recordingPath?: string;
  aiSummary?: string;
  sentiment?: string;
}

export interface DashboardStats {
  totalCallsToday: number;
  activeCalls: number;
  missedCallsToday: number;
  totalAgents: number;
  availableAgents: number;
  busyAgents: number;
  averageCallDuration: number;
  totalExtensions: number;
  hourlyStats: HourlyCallStat[];
}

export interface HourlyCallStat {
  hour: number;
  totalCalls: number;
  answeredCalls: number;
  missedCalls: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiKey {
  id: string;
  name: string;
  keyPrefix: string;
  status: string;
  permissions: string[];
  expiresAt?: string;
  lastUsedAt?: string;
  requestCount: number;
}

export interface IvrMenu {
  id: string;
  name: string;
  greetingText?: string;
  isActive: boolean;
  options: IvrOption[];
}

export interface IvrOption {
  id: string;
  digit: string;
  description: string;
  actionType: string;
  actionTarget?: string;
}

export interface CallQueue {
  id: string;
  name: string;
  extension: string;
  strategy: string;
  isActive: boolean;
  agents: QueueAgent[];
}

export interface QueueAgent {
  id: string;
  extensionId: string;
  priority: number;
  isActive: boolean;
}

export interface ActiveCall {
  callId: string;
  callerNumber: string;
  calleeNumber: string;
  state: string;
  durationSeconds: number;
}
