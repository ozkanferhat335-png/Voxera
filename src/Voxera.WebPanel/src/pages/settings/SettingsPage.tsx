import React from 'react';
import { Link } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';

const settingsSections = [
  { title: 'API Keys', description: 'Manage API keys for third-party integrations', icon: '🔑', href: '/settings/api-keys' },
  { title: 'Webhooks', description: 'Configure webhook endpoints for call events', icon: '🔗', href: '/settings/webhooks' },
  { title: 'Company Profile', description: 'Update company information and branding', icon: '🏢', href: '/settings/company' },
  { title: 'SIP Trunk', description: 'Configure SIP trunk for PSTN connectivity', icon: '📡', href: '/settings/trunk' },
  { title: 'Billing & Plan', description: 'Manage subscription and invoices', icon: '💳', href: '/settings/billing' },
  { title: 'Security', description: 'IP whitelist, 2FA, and audit logs', icon: '🔒', href: '/settings/security' },
];

const SettingsPage: React.FC = () => {
  const { user } = useAuthStore();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-white">Settings</h1>
        <p className="text-slate-400 text-sm mt-1">Manage your account and platform configuration</p>
      </div>

      {/* Account info */}
      <div className="card">
        <h2 className="text-white font-semibold mb-4">Account Information</h2>
        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <p className="text-slate-500">Full Name</p>
            <p className="text-slate-200 font-medium mt-0.5">{user?.fullName}</p>
          </div>
          <div>
            <p className="text-slate-500">Email</p>
            <p className="text-slate-200 font-medium mt-0.5">{user?.email}</p>
          </div>
          <div>
            <p className="text-slate-500">Role</p>
            <p className="text-slate-200 font-medium mt-0.5">{user?.role}</p>
          </div>
          <div>
            <p className="text-slate-500">Company</p>
            <p className="text-slate-200 font-medium mt-0.5">{user?.companyName}</p>
          </div>
        </div>
      </div>

      {/* Settings sections */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {settingsSections.map(section => (
          <Link
            key={section.title}
            to={section.href}
            className="card hover:border-slate-700 hover:bg-slate-800/50 transition-all cursor-pointer group"
          >
            <div className="flex items-start gap-3">
              <span className="text-2xl">{section.icon}</span>
              <div>
                <h3 className="text-white font-semibold group-hover:text-blue-400 transition-colors">{section.title}</h3>
                <p className="text-slate-400 text-sm mt-1">{section.description}</p>
              </div>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
};

export default SettingsPage;
