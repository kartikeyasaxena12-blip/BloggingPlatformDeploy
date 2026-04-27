import React, { useState, useEffect } from 'react';
import api from '../api/axios';

export default function Newsletter() {
  const [email, setEmail]         = useState('');
  const [status, setStatus]       = useState({ type: '', message: '' });
  const [loading, setLoading]     = useState(false);
  const [confirmedCount, setConfirmedCount] = useState(null);

  // Fetch subscriber count for social proof
  useEffect(() => {
    api.get('/api/newsletter/stats')
      .then(res => setConfirmedCount(res.data.confirmed))
      .catch(() => {}); // Silently fail — count is optional UI detail
  }, []);

  const handleSubscribe = async (e) => {
    e.preventDefault();
    setLoading(true);
    setStatus({ type: '', message: '' });

    try {
      const res = await api.post('/api/newsletter/subscribe', { email });
      // ASP.NET Core returns camelCase JSON
      const msg = res.data?.message || res.data?.Message || '🎉 Check your inbox to confirm your subscription!';
      setStatus({ type: 'success', message: msg });
      setEmail('');
    } catch (err) {
      const msg = err.response?.data?.message
               || err.response?.data?.Message
               || 'Something went wrong. Please try again.';
      setStatus({ type: 'error', message: msg });
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="newsletter-section">
      <div className="newsletter-card">
        <div className="newsletter-content">
          <div className="newsletter-text">
            <h2>📬 Join the InkWell Newsletter</h2>
            <p>
              Get the latest stories, tutorials, and insights delivered straight to your inbox every week.
              {confirmedCount !== null && confirmedCount > 0 && (
                <span className="newsletter-count"> Join {confirmedCount.toLocaleString()} readers already subscribed.</span>
              )}
            </p>
          </div>

          <form onSubmit={handleSubscribe} className="newsletter-form">
            <div className="newsletter-input-group">
              <input
                id="newsletter-email"
                type="email"
                placeholder="Enter your email address..."
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="newsletter-input"
                disabled={loading}
              />
              <button
                id="newsletter-subscribe-btn"
                type="submit"
                className="btn-primary"
                disabled={loading}
              >
                {loading ? '⏳ Sending…' : '✉️ Subscribe'}
              </button>
            </div>
          </form>

          {status.message && (
            <div className={`newsletter-status ${status.type}`}>
              {status.message}
            </div>
          )}

          <p className="newsletter-privacy">
            🔒 No spam, ever. Unsubscribe with one click anytime.
          </p>
        </div>
      </div>
    </section>
  );
}
