import React, { useState, useEffect, useRef } from 'react';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';
import { formatIST } from '../utils/dateFormatter';

export default function NotificationBell() {
  const { user } = useAuth();
  const [notifications, setNotifications] = useState([]);
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef(null);

  const fetchNotifications = async () => {
    try {
      const res = await api.get(`/api/notifications/user/${user.userId}`);
      setNotifications(res.data);
    } catch (err) {
      console.error('Failed to fetch notifications:', err);
    }
  };

  useEffect(() => {
    if (user) {
      fetchNotifications();
      const interval = setInterval(fetchNotifications, 30000);
      return () => clearInterval(interval);
    }
  }, [user]);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const markAsRead = async (id) => {
    try {
      await api.put(`/api/notifications/${id}/read`);
      setNotifications(prev => prev.map(n => n.id === id ? { ...n, isRead: true } : n));
    } catch (err) {
      console.error('Failed to mark notification as read:', err);
    }
  };

  const markAllAsRead = async () => {
    const unread = notifications.filter(n => !n.isRead);
    await Promise.all(unread.map(n => markAsRead(n.id)));
  };

  const unreadCount = notifications.filter(n => !n.isRead).length;

  return (
    <div className="notification-bell-container" ref={dropdownRef}>
      <button
        className="notification-bell"
        onClick={() => setIsOpen(!isOpen)}
        title="Notifications"
        style={{ position: 'relative' }}
      >
        🔔
        {unreadCount > 0 && (
          <span
            className="notification-badge"
            style={{
              position: 'absolute', top: '2px', right: '2px',
              background: 'var(--danger)', color: '#fff',
              fontSize: '0.65rem', fontWeight: 700, lineHeight: 1,
              minWidth: '17px', height: '17px', borderRadius: '10px',
              display: 'flex', alignItems: 'center', justifyContent: 'center',
              padding: '0 4px', border: '2px solid var(--bg-primary)'
            }}
          >
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="notification-dropdown">
          <div className="notification-header">
            <span>🔔 Notifications</span>
            {unreadCount > 0 && (
              <button
                onClick={markAllAsRead}
                style={{
                  background: 'none', border: 'none', color: 'var(--accent)',
                  cursor: 'pointer', fontSize: '0.78rem', fontWeight: 600
                }}
              >
                Mark all read
              </button>
            )}
          </div>

          <div style={{ maxHeight: '340px', overflowY: 'auto' }}>
            {notifications.length === 0 ? (
              <div className="notification-empty">
                <div style={{ fontSize: '1.8rem', marginBottom: '8px' }}>🔕</div>
                You're all caught up!
              </div>
            ) : (
              notifications.map(n => (
                <div key={n.id} className={`notification-item ${!n.isRead ? 'unread' : ''}`}>
                  <p style={{ marginBottom: '6px', lineHeight: 1.5 }}>{n.message}</p>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <span style={{ fontSize: '0.73rem', color: 'var(--text-muted)' }}>
                      {formatIST(n.createdAt)}
                    </span>
                    {!n.isRead && (
                      <button
                        onClick={() => markAsRead(n.id)}
                        style={{
                          background: 'var(--accent-light)', border: 'none',
                          color: 'var(--accent)', cursor: 'pointer',
                          fontSize: '0.73rem', fontWeight: 600,
                          padding: '3px 10px', borderRadius: '10px'
                        }}
                      >
                        ✓ Read
                      </button>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
