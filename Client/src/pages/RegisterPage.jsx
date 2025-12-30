import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authAPI } from '../services/api';

const RegisterPage = () => {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async e => {
    e.preventDefault();
    setError('');

    if (password.length < 6) {
      setError('Password must be at least 6 characters');
      return;
    }

    setLoading(true);

    try {
      const response = await authAPI.register(username, email, password);
      login(response);
      navigate('/');
    } catch (err) {
      setError(err.response?.data?.message || 'Registration failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        padding: '2rem',
      }}
    >
      <div
        style={{
          width: '100%',
          maxWidth: '400px',
          backgroundColor: '#1a1f3a',
          padding: '2rem',
          borderRadius: '12px',
          boxShadow: '0 4px 6px rgba(0, 0, 0, 0.3)',
        }}
      >
        <h1 style={{ marginBottom: '2rem', textAlign: 'center' }}>Register</h1>

        {error && (
          <div
            style={{
              padding: '0.75rem',
              marginBottom: '1rem',
              backgroundColor: '#7f1d1d',
              color: '#fca5a5',
              borderRadius: '6px',
            }}
          >
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: '1.5rem' }}>
            <label
              htmlFor="username"
              style={{ display: 'block', marginBottom: '0.5rem', color: '#d1d5db' }}
            >
              Username
            </label>
            <input
              id="username"
              type="text"
              value={username}
              onChange={e => setUsername(e.target.value)}
              required
              style={{
                width: '100%',
                padding: '0.75rem',
                backgroundColor: '#0f172a',
                border: '1px solid #374151',
                borderRadius: '6px',
                color: '#ffffff',
                fontSize: '1rem',
              }}
            />
          </div>

          <div style={{ marginBottom: '1.5rem' }}>
            <label
              htmlFor="email"
              style={{ display: 'block', marginBottom: '0.5rem', color: '#d1d5db' }}
            >
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              required
              style={{
                width: '100%',
                padding: '0.75rem',
                backgroundColor: '#0f172a',
                border: '1px solid #374151',
                borderRadius: '6px',
                color: '#ffffff',
                fontSize: '1rem',
              }}
            />
          </div>

          <div style={{ marginBottom: '1.5rem' }}>
            <label
              htmlFor="password"
              style={{ display: 'block', marginBottom: '0.5rem', color: '#d1d5db' }}
            >
              Password
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
              minLength={6}
              style={{
                width: '100%',
                padding: '0.75rem',
                backgroundColor: '#0f172a',
                border: '1px solid #374151',
                borderRadius: '6px',
                color: '#ffffff',
                fontSize: '1rem',
              }}
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            style={{
              width: '100%',
              padding: '0.75rem',
              backgroundColor: '#3b82f6',
              color: '#ffffff',
              border: 'none',
              borderRadius: '6px',
              fontSize: '1rem',
              fontWeight: '600',
              cursor: loading ? 'not-allowed' : 'pointer',
              opacity: loading ? 0.6 : 1,
              marginBottom: '1rem',
            }}
          >
            {loading ? 'Registering...' : 'Register'}
          </button>
        </form>

        <div style={{ textAlign: 'center', color: '#9ca3af' }}>
          Already have an account?
          <Link to="/login" style={{ paddingLeft: '0.5rem', color: '#3b82f6', textDecoration: 'none' }}>
            Login
          </Link>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
