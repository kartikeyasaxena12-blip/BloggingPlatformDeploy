import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import NotificationBell from './NotificationBell';


export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [search, setSearch] = useState('');

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const handleSearch = (e) => {
    e.preventDefault();
    navigate(`/?search=${encodeURIComponent(search)}`);
  };

  const getRoleLabel = () => {
    if (!user) return 'Reader';
    if (user.role === 'Admin') return 'Admin';
    return 'Author'; // Logged in regular user is 'Author'
  };

  const roleLabel = getRoleLabel();

  return (
    <nav className="navbar">
      <div className="nav-inner">
        <Link to="/" className="nav-brand">
          <span className="brand-icon">✍️</span>
          <span className="brand-name">InkWell</span>
        </Link>

        <div className="nav-links">
          <Link to="/" className="nav-link">Home</Link>
          {user?.role === 'Admin' && (
            <Link to="/newsletter-admin" className="nav-link nav-link-admin">
              📬 Newsletter
            </Link>
          )}
          {user ? (
            <>
              <NotificationBell />
              <div className="nav-user">
                <Link to="/profile" className="nav-username" style={{ cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '8px' }}>
                  {user.avatarUrl ? (
                    <img
                      src={user.avatarUrl}
                      alt={user.username}
                      style={{
                        width: '30px',
                        height: '30px',
                        borderRadius: '50%',
                        objectFit: 'cover',
                        border: '2px solid var(--accent)',
                      }}
                      onError={(e) => { e.target.style.display = 'none'; }}
                    />
                  ) : (
                    <span>👤</span>
                  )}
                  <span style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-start' }}>
                    <span style={{ fontSize: '0.9rem', fontWeight: 600 }}>{user.fullName || user.username}</span>
                    <span style={{ 
                      fontSize: '0.65rem', 
                      background: user.role === 'Admin' ? 'rgba(239,68,68,0.1)' : 'rgba(99,102,241,0.1)',
                      color: user.role === 'Admin' ? '#ef4444' : '#6366f1',
                      padding: '1px 6px',
                      borderRadius: '4px',
                      textTransform: 'uppercase',
                      letterSpacing: '0.5px'
                    }}>
                      {roleLabel}
                    </span>
                  </span>
                </Link>
                <button onClick={handleLogout} className="btn-logout">Sign Out</button>
              </div>
            </>
          ) : (
            <>
              <span style={{ 
                fontSize: '0.7rem', 
                background: 'rgba(107,114,128,0.1)', 
                color: '#6b7280',
                padding: '2px 8px',
                borderRadius: '12px',
                marginRight: '8px',
                textTransform: 'uppercase',
                fontWeight: 600
              }}>
                {roleLabel}
              </span>
              <Link to="/login" className="nav-link">Sign In</Link>
              <Link to="/register" className="btn-primary nav-register">Get Started</Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
}
