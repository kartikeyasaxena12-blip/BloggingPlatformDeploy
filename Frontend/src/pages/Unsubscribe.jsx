import React, { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import api from '../api/axios';

export default function Unsubscribe() {
  const [searchParams] = useSearchParams();
  const [status, setStatus] = useState('loading'); // 'loading' | 'success' | 'error'
  const [message, setMessage] = useState('');

  useEffect(() => {
    const token = searchParams.get('token');

    if (!token) {
      setStatus('error');
      setMessage('No unsubscribe token found. Please use the link from your email.');
      return;
    }

    // Token-based unsubscribe via GET (link clicked from email)
    api.get(`/api/newsletter/unsubscribe?token=${token}`)
      .then(() => {
        setStatus('success');
        setMessage("You've been successfully unsubscribed from InkWell. We're sorry to see you go!");
      })
      .catch((err) => {
        const msg = err.response?.data?.Message || err.response?.data || 'Something went wrong.';
        setStatus('error');
        setMessage(typeof msg === 'string' ? msg : 'This unsubscribe link is invalid or has already been used.');
      });
  }, [searchParams]);

  return (
    <div className="unsub-container">
      <div className="unsub-card">
        {status === 'loading' && (
          <>
            <div className="unsub-icon unsub-spin">⏳</div>
            <h1>Processing…</h1>
            <p>Please wait while we process your unsubscribe request.</p>
          </>
        )}

        {status === 'success' && (
          <>
            <div className="unsub-icon">👋</div>
            <div className="unsub-badge unsub-success">Unsubscribed</div>
            <h1>You've been removed</h1>
            <p>{message}</p>
            <a href="/" className="btn-primary unsub-btn">← Back to InkWell</a>
          </>
        )}

        {status === 'error' && (
          <>
            <div className="unsub-icon">❌</div>
            <div className="unsub-badge unsub-error">Error</div>
            <h1>Oops!</h1>
            <p>{message}</p>
            <a href="/" className="btn-primary unsub-btn">← Back to InkWell</a>
          </>
        )}
      </div>
    </div>
  );
}
