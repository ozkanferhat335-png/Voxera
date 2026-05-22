import React, { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './store/authStore';
import { connectCallHub, connectDashboardHub, disconnectHubs } from './services/signalr';

// Layouts
import MainLayout from './components/layout/MainLayout';
import AuthLayout from './components/layout/AuthLayout';

// Pages
import LoginPage from './pages/auth/LoginPage';
import RegisterPage from './pages/auth/RegisterPage';
import DashboardPage from './pages/dashboard/DashboardPage';
import CallsPage from './pages/calls/CallsPage';
import ExtensionsPage from './pages/extensions/ExtensionsPage';
import SipAccountsPage from './pages/sip/SipAccountsPage';
import IvrPage from './pages/ivr/IvrPage';
import QueuesPage from './pages/queues/QueuesPage';
import ReportsPage from './pages/reports/ReportsPage';
import SettingsPage from './pages/settings/SettingsPage';
import ApiKeysPage from './pages/settings/ApiKeysPage';

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
};

const App: React.FC = () => {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

  useEffect(() => {
    if (isAuthenticated) {
      connectCallHub();
      connectDashboardHub();
    }
    return () => { disconnectHubs(); };
  }, [isAuthenticated]);

  return (
    <BrowserRouter>
      <Routes>
        {/* Auth routes */}
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Route>

        {/* Protected routes */}
        <Route element={<ProtectedRoute><MainLayout /></ProtectedRoute>}>
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/calls" element={<CallsPage />} />
          <Route path="/extensions" element={<ExtensionsPage />} />
          <Route path="/sip-accounts" element={<SipAccountsPage />} />
          <Route path="/ivr" element={<IvrPage />} />
          <Route path="/queues" element={<QueuesPage />} />
          <Route path="/reports" element={<ReportsPage />} />
          <Route path="/settings" element={<SettingsPage />} />
          <Route path="/settings/api-keys" element={<ApiKeysPage />} />
        </Route>

        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
};

export default App;
