import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';
import { formatIST } from '../utils/dateFormatter';
import Newsletter from '../components/Newsletter';

export default function Profile() {
  const { id } = useParams();
  const { user: currentUser } = useAuth();
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [editingBio, setEditingBio] = useState(false);
  const [bioValue, setBioValue] = useState('');
  const [savingBio, setSavingBio] = useState(false);
  const [bioMsg, setBioMsg] = useState('');
  const [savedPosts, setSavedPosts] = useState([]);
  const [loadingSaved, setLoadingSaved] = useState(false);

  const targetId = id ? parseInt(id) : currentUser?.userId;
  const isOwnProfile = !id || (currentUser && parseInt(id) === currentUser.userId);

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        if (!targetId) { setError('User not found'); setLoading(false); return; }
        const res = await api.get(`/api/auth/profile/${targetId}`);
        setProfile(res.data);
        setBioValue(res.data.bio || '');
      } catch {
        setError('Failed to load profile.');
      } finally {
        setLoading(false);
      }
    };

    const fetchSavedPosts = async () => {
      if (isOwnProfile && targetId) {
        setLoadingSaved(true);
        try {
          const res = await api.get(`/api/posts/saved?userId=${targetId}`);
          setSavedPosts(res.data);
        } catch (err) {
          console.error("Failed to load saved posts", err);
        } finally {
          setLoadingSaved(false);
        }
      }
    };

    fetchProfile();
    fetchSavedPosts();
  }, [targetId, isOwnProfile]);

  const handleSaveBio = async () => {
    setSavingBio(true);
    setBioMsg('');
    try {
      await api.put(`/api/auth/profile/${targetId}/bio`, { bio: bioValue });
      setProfile(prev => ({ ...prev, bio: bioValue }));
      setEditingBio(false);
      setBioMsg('✅ Bio updated!');
      setTimeout(() => setBioMsg(''), 3000);
    } catch {
      setBioMsg('❌ Failed to update bio.');
    } finally {
      setSavingBio(false);
    }
  };

  if (loading) return (
    <div className="page-container" style={{ textAlign: 'center', paddingTop: '100px' }}>
      <div style={{ fontSize: '2rem', marginBottom: '16px' }}>⏳</div>
      <div className="loading-text">Loading profile...</div>
    </div>
  );

  if (error) return (
    <div className="page-container">
      <div className="empty-state">
        <div className="empty-icon">❌</div>
        <h3>Profile Not Found</h3>
        <p>{error}</p>
        <Link to="/" className="btn-primary">← Back to Feed</Link>
      </div>
    </div>
  );

  const initials = (profile.fullName || profile.username || '?').charAt(0).toUpperCase();

  return (
    <div className="page-container">
      <div className="profile-page">

        {/* Profile Header */}
        <div className="profile-header">
          <div className="profile-avatar">{initials}</div>

          <div className="profile-info" style={{ flex: 1 }}>
            <h1 className="profile-name">{profile.fullName || profile.username}</h1>
            {profile.fullName && (
              <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', marginBottom: '10px' }}>
                @{profile.username}
              </p>
            )}

            {/* Role and Verified / Unverified badge */}
            {currentUser && (
              <div style={{ display: 'flex', gap: '8px', marginTop: '10px' }}>
                <div className="profile-role" style={{
                  background: profile.role === 'Admin' ? 'rgba(239,68,68,0.12)' : 'rgba(99,102,241,0.12)',
                  color: profile.role === 'Admin' ? '#ef4444' : '#6366f1'
                }}>
                  {profile.role === 'Admin' ? 'Admin' : 'Author'}
                </div>
                <div className="profile-role" style={{
                  background: profile.isEmailVerified
                    ? 'rgba(34,211,160,0.12)'
                    : 'rgba(245,158,11,0.12)',
                  color: profile.isEmailVerified ? 'var(--success)' : 'var(--warning)'
                }}>
                  {profile.isEmailVerified ? '✅ Verified' : '⚠️ Unverified'}
                </div>
              </div>
            )}
          </div>

          {/* Joined date */}
          <div style={{ textAlign: 'right', flexShrink: 0 }}>
            <div style={{ color: 'var(--text-muted)', fontSize: '0.78rem', marginBottom: '4px' }}>
              Member since
            </div>
            <div style={{ fontWeight: 600, fontSize: '0.88rem' }}>
              {formatIST(profile.createdAt)}
            </div>
            {isOwnProfile && profile.email && (
              <div style={{ color: 'var(--text-muted)', fontSize: '0.8rem', marginTop: '8px' }}>
                📧 {profile.email}
              </div>
            )}
          </div>
        </div>

        {/* About / Bio Section */}
        <div style={{
          background: 'var(--bg-card)', border: '1px solid var(--border)',
          borderRadius: 'var(--radius)', padding: '28px', marginBottom: '28px'
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
            <h3 style={{ fontWeight: 700, fontSize: '1.05rem' }}>📝 About</h3>
            {isOwnProfile && !editingBio && (
              <button
                className="btn-secondary"
                style={{ padding: '6px 14px', fontSize: '0.82rem' }}
                onClick={() => setEditingBio(true)}
              >
                ✏️ {profile.bio ? 'Edit' : 'Add Bio'}
              </button>
            )}
          </div>

          {editingBio ? (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
              <textarea
                value={bioValue}
                onChange={(e) => setBioValue(e.target.value)}
                placeholder="Tell the world about yourself..."
                rows={4}
                style={{
                  background: 'var(--bg-secondary)', border: '1.5px solid var(--border)',
                  borderRadius: 'var(--radius-sm)', padding: '12px 16px',
                  color: 'var(--text-primary)', fontSize: '0.95rem',
                  fontFamily: 'inherit', outline: 'none', width: '100%', resize: 'vertical'
                }}
                onFocus={e => e.target.style.borderColor = 'var(--accent)'}
                onBlur={e => e.target.style.borderColor = 'var(--border)'}
              />
              <div style={{ display: 'flex', gap: '10px', justifyContent: 'flex-end' }}>
                <button className="btn-secondary" style={{ padding: '8px 16px' }}
                  onClick={() => { setEditingBio(false); setBioValue(profile.bio || ''); }}>
                  Cancel
                </button>
                <button className="btn-primary" style={{ padding: '8px 20px' }}
                  onClick={handleSaveBio} disabled={savingBio}>
                  {savingBio ? '⏳ Saving...' : '💾 Save Bio'}
                </button>
              </div>
            </div>
          ) : (
            <p style={{ color: profile.bio ? 'var(--text-secondary)' : 'var(--text-muted)', lineHeight: 1.7, fontSize: '0.95rem', fontStyle: !profile.bio ? 'italic' : 'normal' }}>
              {profile.bio || (isOwnProfile ? 'You haven\'t written a bio yet. Click "Add Bio" to introduce yourself!' : `${profile.fullName || profile.username} hasn't written a bio yet.`)}
            </p>
          )}

          {bioMsg && (
            <p style={{ marginTop: '10px', fontSize: '0.88rem', color: bioMsg.startsWith('✅') ? 'var(--success)' : 'var(--danger)' }}>
              {bioMsg}
            </p>
          )}
        </div>

        {/* Saved Posts Section - own profile only */}
        {isOwnProfile && (
          <div style={{
            background: 'var(--bg-card)', border: '1px solid var(--border)',
            borderRadius: 'var(--radius)', padding: '28px', marginBottom: '28px'
          }}>
            <div style={{ display: 'flex', alignItems: 'center', marginBottom: '16px' }}>
              <h3 style={{ fontWeight: 700, fontSize: '1.05rem' }}>🔖 Saved Posts</h3>
            </div>
            
            {loadingSaved ? (
              <div style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>Loading saved posts...</div>
            ) : savedPosts.length === 0 ? (
              <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>
                You haven't saved any posts yet.
              </p>
            ) : (
              <div style={{ display: 'grid', gridTemplateColumns: '1fr', gap: '16px' }}>
                {savedPosts.map(post => (
                  <Link to={`/post/${post.id}`} key={post.id} style={{
                    display: 'block', padding: '16px', background: 'var(--bg-secondary)',
                    border: '1px solid var(--border)', borderRadius: 'var(--radius-sm)',
                    textDecoration: 'none', color: 'inherit', transition: 'all var(--transition)'
                  }} onMouseEnter={e => e.currentTarget.style.borderColor = 'var(--accent)'} 
                     onMouseLeave={e => e.currentTarget.style.borderColor = 'var(--border)'}>
                    <h4 style={{ margin: '0 0 8px 0', fontSize: '1.05rem', color: 'var(--text-primary)' }}>{post.title}</h4>
                    <p style={{ margin: '0 0 10px 0', fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
                      {post.content.substring(0, 100)}{post.content.length > 100 ? '...' : ''}
                    </p>
                    <div style={{ fontSize: '0.78rem', color: 'var(--text-muted)' }}>
                      By {post.authorName || 'Anonymous'} · {formatIST(post.createdAt)}
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Newsletter - own profile only */}
        {isOwnProfile && <Newsletter />}
      </div>
    </div>
  );
}
