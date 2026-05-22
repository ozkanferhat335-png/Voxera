import React, { useState } from 'react';
import { Outlet, NavLink, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { useCallStore } from '../../store/callStore';
import IncomingCallPopup from '../calls/IncomingCallPopup';
import AgentStatusSelector from '../calls/AgentStatusSelector';

const navItems = [
  { path: '/dashboard', label: 'Dashboard', icon: '⊞' },
  { path: '/calls', label: 'Call Logs', icon: '📞' },
  { path: '/extensions', label: 'Extensions', icon: '☎' },
  { path: '/sip-accounts', label: 'SIP Accounts', icon: '🔗' },
  { path: '/ivr', label: 'IVR Menus', icon: '🎛' },
  { path: '/queues', label: 'Call Queues', icon: '📋' },
  { path: '/reports', label: 'Reports', icon: '📊' },
  { path: '/settings', label: 'Settings', icon: '⚙' },
];

const MainLayout: React.FC = () => {
  const { user, logout } = useAuthStore();
  const { incomingCall, agentStatus } = useCallStore();
  const navigate = useNavigate();
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const statusColors: Record<string, string> = {
    Available: 'bg-green-500',
    Busy: 'bg-red-500',
    Away: 'bg-yellow-500',
    DoNotDisturb: 'bg-red-700',
    WrapUp: 'bg-orange-500',
    Offline: 'bg-slate-500',
  };

  return (
    <div className="flex h-screen bg-slate-950 overflow-hidden">
      {/* Sidebar */}
      <aside className={`${sidebarOpen ? 'w-64' : 'w-16'} bg-slate-900 border-r border-slate-800 flex flex-col transition-all duration-300 flex-shrink-0`}>
        {/* Logo */}
        <div className="flex items-center gap-3 px-4 py-5 border-b border-slate-800">
          <div className="w-8 h-8 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-lg flex items-center justify-center text-white font-bold text-sm flex-shrink-0">V</div>
          {sidebarOpen && <span className="text-white font-bold text-lg tracking-tight">Voxera</span>}
        </div>

        {/* Navigation */}
        <nav className="flex-1 px-2 py-4 space-y-1 overflow-y-auto">
          {navItems.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors duration-150 ${
                  isActive
                    ? 'bg-blue-600/20 text-blue-400 border border-blue-600/30'
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800'
                }`
              }
            >
              <span className="text-lg flex-shrink-0">{item.icon}</span>
              {sidebarOpen && <span>{item.label}</span>}
            </NavLink>
          ))}
        </nav>

        {/* User info */}
        <div className="border-t border-slate-800 p-3">
          {sidebarOpen ? (
            <div className="flex items-center gap-3">
              <div className="relative">
                <div className="w-8 h-8 bg-gradient-to-br from-purple-500 to-pink-500 rounded-full flex items-center justify-center text-white text-xs font-bold">
                  {user?.fullName?.charAt(0) ?? 'U'}
                </div>
                <div className={`absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full border-2 border-slate-900 ${statusColors[agentStatus] ?? 'bg-slate-500'}`} />
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-slate-200 truncate">{user?.fullName}</p>
                <p className="text-xs text-slate-500 truncate">{user?.role}</p>
              </div>
              <button onClick={handleLogout} className="text-slate-500 hover:text-slate-300 text-xs" title="Logout">⏻</button>
            </div>
          ) : (
            <div className="flex justify-center">
              <div className="w-8 h-8 bg-gradient-to-br from-purple-500 to-pink-500 rounded-full flex items-center justify-center text-white text-xs font-bold">
                {user?.fullName?.charAt(0) ?? 'U'}
              </div>
            </div>
          )}
        </div>
      </aside>

      {/* Main content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Top bar */}
        <header className="bg-slate-900 border-b border-slate-800 px-6 py-3 flex items-center justify-between flex-shrink-0">
          <div className="flex items-center gap-4">
            <button
              onClick={() => setSidebarOpen(!sidebarOpen)}
              className="text-slate-400 hover:text-slate-200 transition-colors"
            >
              ☰
            </button>
            <div className="text-slate-400 text-sm">{user?.companyName}</div>
          </div>
          <div className="flex items-center gap-4">
            <AgentStatusSelector />
            <div className="text-slate-500 text-xs">{new Date().toLocaleDateString('tr-TR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}</div>
          </div>
        </header>

        {/* Page content */}
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>

      {/* Incoming call popup */}
      {incomingCall && <IncomingCallPopup call={incomingCall} />}
    </div>
  );
};

export default MainLayout;
