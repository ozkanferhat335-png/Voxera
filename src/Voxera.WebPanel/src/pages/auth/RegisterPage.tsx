import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { authApi } from '../../services/api';
import toast from 'react-hot-toast';

const RegisterPage: React.FC = () => {
  const [form, setForm] = useState({ companyName: '', firstName: '', lastName: '', email: '', password: '' });
  const [loading, setLoading] = useState(false);
  const { setAuth } = useAuthStore();
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm(f => ({ ...f, [e.target.name]: e.target.value }));

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const data = await authApi.register(form);
      setAuth(
        { userId: data.userId, fullName: data.fullName, email: data.email, role: data.role, companyId: data.companyId, companyName: data.companyName },
        data.accessToken,
        data.refreshToken
      );
      navigate('/dashboard');
      toast.success('Account created! Welcome to Voxera.');
    } catch (err: any) {
      toast.error(err.response?.data?.detail ?? 'Registration failed.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2 className="text-xl font-bold text-white mb-1">Create your account</h2>
      <p className="text-slate-400 text-sm mb-6">Start your 14-day free trial</p>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-slate-300 mb-1.5">Company Name</label>
          <input name="companyName" value={form.companyName} onChange={handleChange} className="input" placeholder="Acme Corp" required />
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-1.5">First Name</label>
            <input name="firstName" value={form.firstName} onChange={handleChange} className="input" placeholder="John" required />
          </div>
          <div>
            <label className="block text-sm font-medium text-slate-300 mb-1.5">Last Name</label>
            <input name="lastName" value={form.lastName} onChange={handleChange} className="input" placeholder="Doe" required />
          </div>
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-300 mb-1.5">Email</label>
          <input type="email" name="email" value={form.email} onChange={handleChange} className="input" placeholder="john@company.com" required />
        </div>
        <div>
          <label className="block text-sm font-medium text-slate-300 mb-1.5">Password</label>
          <input type="password" name="password" value={form.password} onChange={handleChange} className="input" placeholder="Min. 8 characters" required minLength={8} />
        </div>

        <button type="submit" disabled={loading} className="btn-primary w-full py-2.5 mt-2">
          {loading ? 'Creating account...' : 'Create account'}
        </button>
      </form>

      <p className="text-center text-slate-500 text-sm mt-6">
        Already have an account?{' '}
        <Link to="/login" className="text-blue-400 hover:text-blue-300 font-medium">Sign in</Link>
      </p>
    </div>
  );
};

export default RegisterPage;
