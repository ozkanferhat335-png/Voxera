import axios, { AxiosInstance } from 'axios';
import { useAuthStore } from '../store/authStore';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

const api: AxiosInstance = axios.create({
  baseURL: `${API_URL}/api/v1`,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30000,
});

// Request interceptor - attach JWT
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Response interceptor - handle 401 and token refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const refreshToken = useAuthStore.getState().refreshToken;
        const response = await axios.post(`${API_URL}/api/v1/auth/refresh`, { refreshToken });
        const { access_token, refresh_token } = response.data;
        useAuthStore.getState().setTokens(access_token, refresh_token);
        originalRequest.headers.Authorization = `Bearer ${access_token}`;
        return api(originalRequest);
      } catch {
        useAuthStore.getState().logout();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

// Auth
export const authApi = {
  login: (email: string, password: string) =>
    api.post('/auth/login', { email, password }).then(r => r.data),
  register: (data: { companyName: string; firstName: string; lastName: string; email: string; password: string }) =>
    api.post('/auth/register', data).then(r => r.data),
  me: () => api.get('/auth/me').then(r => r.data),
  logout: () => api.post('/auth/logout'),
};

// Dashboard
export const dashboardApi = {
  getStats: () => api.get('/dashboard/stats').then(r => r.data),
  getActiveCalls: () => api.get('/dashboard/active-calls').then(r => r.data),
};

// Calls
export const callsApi = {
  getCallLogs: (params?: Record<string, any>) =>
    api.get('/calls', { params }).then(r => r.data),
  originate: (fromExtension: string, toNumber: string) =>
    api.post('/calls/originate', { fromExtension, toNumber }).then(r => r.data),
  hangup: (callId: string) =>
    api.post(`/calls/${callId}/hangup`).then(r => r.data),
  transfer: (callId: string, destination: string) =>
    api.post(`/calls/${callId}/transfer`, { destination }).then(r => r.data),
  hold: (callId: string, hold: boolean) =>
    api.post(`/calls/${callId}/hold`, { hold }).then(r => r.data),
  getRecording: (callId: string) =>
    api.get(`/calls/${callId}/recording`).then(r => r.data),
};

// Extensions
export const extensionsApi = {
  getAll: () => api.get('/extensions').then(r => r.data),
  getById: (id: string) => api.get(`/extensions/${id}`).then(r => r.data),
  create: (data: { number: string; displayName: string; type?: string }) =>
    api.post('/extensions', data).then(r => r.data),
  update: (id: string, data: Record<string, any>) =>
    api.put(`/extensions/${id}`, data).then(r => r.data),
  delete: (id: string) => api.delete(`/extensions/${id}`),
};

// SIP Accounts
export const sipApi = {
  getAll: () => api.get('/sip-accounts').then(r => r.data),
  create: (data: { extensionId: string; password?: string; enableWebRtc?: boolean }) =>
    api.post('/sip-accounts', data).then(r => r.data),
  updateStatus: (id: string, status: string) =>
    api.patch(`/sip-accounts/${id}/status`, { status }).then(r => r.data),
  delete: (id: string) => api.delete(`/sip-accounts/${id}`),
};

// API Keys
export const apiKeysApi = {
  getAll: () => api.get('/api-keys').then(r => r.data),
  create: (data: { name: string; permissions?: string[]; expiresAt?: string }) =>
    api.post('/api-keys', data).then(r => r.data),
  revoke: (id: string) => api.delete(`/api-keys/${id}`),
};

// IVR
export const ivrApi = {
  getAll: () => api.get('/ivr').then(r => r.data),
  create: (data: { name: string; greetingText?: string }) =>
    api.post('/ivr', data).then(r => r.data),
  addOption: (menuId: string, data: { digit: string; description: string; actionType: string; actionTarget?: string }) =>
    api.post(`/ivr/${menuId}/options`, data).then(r => r.data),
  delete: (menuId: string) => api.delete(`/ivr/${menuId}`),
};

// Queues
export const queuesApi = {
  getAll: () => api.get('/queues').then(r => r.data),
  create: (data: { name: string; extension: string }) =>
    api.post('/queues', data).then(r => r.data),
  addAgent: (queueId: string, data: { extensionId: string; priority?: number }) =>
    api.post(`/queues/${queueId}/agents`, data).then(r => r.data),
  removeAgent: (queueId: string, agentId: string) =>
    api.delete(`/queues/${queueId}/agents/${agentId}`),
};

// Reports
export const reportsApi = {
  getDaily: (date?: string) => api.get('/reports/daily', { params: { date } }).then(r => r.data),
  getOperatorPerformance: (from?: string, to?: string) =>
    api.get('/reports/operator-performance', { params: { from, to } }).then(r => r.data),
  getMissedCalls: (from?: string, to?: string) =>
    api.get('/reports/missed-calls', { params: { from, to } }).then(r => r.data),
};

export default api;
