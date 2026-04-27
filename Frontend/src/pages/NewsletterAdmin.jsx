import React, { useState, useEffect, useCallback } from 'react';
import { Navigate } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';

export default function NewsletterAdmin() {
  const { user } = useAuth();
  const [postTitle, setPostTitle]     = useState('');
  const [authorName, setAuthorName]   = useState('');
  const [postUrl, setPostUrl]         = useState('http://localhost:3000');
  const [sending, setSending]         = useState(false);
  const [result, setResult]           = useState(null);
  const [stats, setStats]             = useState(null);
  const [subscribers, setSubscribers] = useState([]);
  const [loadingSubs, setLoadingSubs] = useState(true);
  const [confirmingId, setConfirmingId] = useState(null);

  // Guard: Admin only
  if (!user || user.role !== 'Admin') return <Navigate to="/" />;

  // Load stats & subscriber list
  const loadData = useCallback(() => {
    setLoadingSubs(true);
    api.get('/api/newsletter/stats')
      .then(res => setStats(res.data))
      .catch(console.error);

    api.get('/api/newsletter')
      .then(res => setSubscribers(res.data))
      .catch(console.error)
      .finally(() => setLoadingSubs(false));
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // ── Broadcast notify ───────────────────────────────────────────────────
  const handleNotify = async (e) => {
    e.preventDefault();
    setSending(true);
    setResult(null);
    try {
      const res = await api.post('/api/newsletter/notify', {
        postTitle,
        authorName,
        postUrl,
      });
      // ASP.NET Core returns camelCase JSON: message, sentCount
      const msg   = res.data.message || res.data.Message || 'Done.';
      const count = res.data.sentCount ?? res.data.SentCount ?? 0;
      setResult({ type: 'success', message: msg, count });
      setPostTitle('');
      setAuthorName('');
      loadData(); // refresh subscriber list
    } catch (err) {
      const msg = err.response?.data?.message
               || err.response?.data?.Message
               || 'Failed to send notifications.';
      setResult({ type: 'error', message: msg });
    } finally {
      setSending(false);
    }
  };

  // ── Admin force-confirm subscriber ─────────────────────────────────────
  const handleForceConfirm = async (id, email) => {
    setConfirmingId(id);
    try {
      await api.post(`/api/newsletter/admin-confirm/${id}`);
      loadData(); // refresh list & stats
    } catch (err) {
      alert(`Could not confirm ${email}. ` + (err.response?.data?.message || ''));
    } finally {
      setConfirmingId(null);
    }
  };

  return (
    <div className="na-page">
      {/* Page header */}
      <div className="page-header">
        <div>
          <h1>📬 Newsletter Admin</h1>
          <p>Manage subscribers and broadcast new-post notifications</p>
        </div>
      </div>

      {/* Stats row */}
      {stats && (
        <div className="na-stats">
          <div className="na-stat-card">
            <div className="na-stat-val">{stats.total}</div>
            <div className="na-stat-lbl">Total Subscribers</div>
          </div>
          <div className="na-stat-card na-stat-accent">
            <div className="na-stat-val">{stats.confirmed}</div>
            <div className="na-stat-lbl">Confirmed ✅</div>
          </div>
          <div className="na-stat-card">
            <div className="na-stat-val">{stats.pending}</div>
            <div className="na-stat-lbl">Pending ⏳</div>
          </div>
        </div>
      )}

      {/* Pending-confirm warning banner */}
      {stats && stats.confirmed === 0 && stats.total > 0 && (
        <div className="na-warn-banner">
          ⚠️ You have <strong>{stats.total}</strong> subscriber(s) but none are confirmed yet.
          Click <strong>"Confirm"</strong> next to each subscriber below, then try sending again.
        </div>
      )}

      <div className="na-grid">
        {/* ── Broadcast Form ─────────────────────────────────── */}
        <div className="na-broadcast">
          <div className="na-section-header">
            <h2>📣 Broadcast Notification</h2>
            <p>Sends to all <strong>confirmed</strong> subscribers</p>
          </div>

          <form onSubmit={handleNotify} className="na-form">
            <div className="form-group">
              <label>Post Title *</label>
              <input
                type="text"
                placeholder="e.g. Getting Started with React 19"
                value={postTitle}
                onChange={e => setPostTitle(e.target.value)}
                required
              />
            </div>

            <div className="form-group">
              <label>Author Name *</label>
              <input
                type="text"
                placeholder="e.g. Jane Doe"
                value={authorName}
                onChange={e => setAuthorName(e.target.value)}
                required
              />
            </div>

            <div className="form-group">
              <label>Post URL <span className="optional">(optional)</span></label>
              <input
                type="url"
                placeholder="http://localhost:3000/post/42"
                value={postUrl}
                onChange={e => setPostUrl(e.target.value)}
              />
            </div>

            {result && (
              <div className={result.type === 'success' ? 'na-result-success' : 'na-result-error'}>
                {result.type === 'success'
                  ? `✅ ${result.message} — ${result.count} recipient${result.count !== 1 ? 's' : ''} notified.`
                  : `❌ ${result.message}`}
              </div>
            )}

            <button
              type="submit"
              className="btn-primary na-send-btn"
              disabled={sending}
            >
              {sending ? '⏳ Sending…' : '📨 Send Notification'}
            </button>
          </form>
        </div>

        {/* ── Subscriber List ────────────────────────────────── */}
        <div className="na-subscribers">
          <div className="na-section-header">
            <h2>👥 Subscribers</h2>
            <p>
              {loadingSubs ? 'Loading…' : `${subscribers.length} total`}
              {!loadingSubs && stats?.pending > 0 && (
                <span style={{ color: 'var(--warning)', marginLeft: 8 }}>
                  — {stats.pending} need confirmation
                </span>
              )}
            </p>
          </div>

          <div className="na-sub-list">
            {loadingSubs ? (
              <div className="na-loading">Loading subscribers…</div>
            ) : subscribers.length === 0 ? (
              <div className="na-empty">
                No subscribers yet. Go to the home page and subscribe with an email.
              </div>
            ) : (
              subscribers.map(sub => (
                <div key={sub.id} className={`na-sub-row ${sub.isConfirmed ? '' : 'na-sub-pending'}`}>
                  <span className="na-sub-email">{sub.email}</span>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexShrink: 0 }}>
                    <span className={`na-sub-badge ${sub.isConfirmed ? 'na-badge-confirmed' : 'na-badge-pending'}`}>
                      {sub.isConfirmed ? '✅ Confirmed' : '⏳ Pending'}
                    </span>
                    {!sub.isConfirmed && (
                      <button
                        className="na-confirm-btn"
                        onClick={() => handleForceConfirm(sub.id, sub.email)}
                        disabled={confirmingId === sub.id}
                        title="Manually confirm this subscriber (admin override)"
                      >
                        {confirmingId === sub.id ? '…' : 'Confirm'}
                      </button>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
