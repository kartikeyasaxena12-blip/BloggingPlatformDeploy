import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';

export default function CreatePost() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ title: '', content: '', categoryId: '' });
  const [categories, setCategories] = useState([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const wordCount = form.content.trim() ? form.content.trim().split(/\s+/).length : 0;
  const charCount = form.content.length;

  useEffect(() => {
    api.get('/api/categories')
      .then(res => setCategories(res.data))
      .catch(err => console.error('Failed to fetch categories:', err));
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.title.trim()) { setError('Please add a title.'); return; }
    if (!form.content.trim()) { setError('Please add some content.'); return; }
    setLoading(true);
    setError('');
    try {
      const postData = {
        title: form.title,
        content: form.content,
        authorId: user.userId,
        authorName: user.username,
        categoryId: form.categoryId ? parseInt(form.categoryId) : null
      };
      const res = await api.post('/api/posts', postData);
      navigate(`/post/${res.data.id}`);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to publish post. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container">
      <button onClick={() => navigate('/')} className="back-btn">
        ← Back to Feed
      </button>

      <div className="form-card">
        <div className="form-card-header">
          <h1>✏️ Write a New Story</h1>
          <p>Share your thoughts, ideas, and stories with the InkWell community</p>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Title</label>
            <input
              type="text"
              placeholder="Give your story a captivating title..."
              value={form.title}
              onChange={(e) => setForm({ ...form, title: e.target.value })}
              required
            />
          </div>

          <div className="form-group">
            <label>
              Category <span className="optional">(optional)</span>
            </label>
            <div style={{ position: 'relative' }}>
              <select
                value={form.categoryId}
                onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
                style={{ paddingLeft: '16px' }}
              >
                <option value="">Select a category...</option>
                {categories.map(cat => (
                  <option key={cat.id} value={cat.id}>{cat.name}</option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-group">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <label>Content</label>
              <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)' }}>
                {wordCount} words · {charCount} chars
              </span>
            </div>
            <textarea
              placeholder="Write your story here... Let your imagination flow."
              value={form.content}
              onChange={(e) => setForm({ ...form, content: e.target.value })}
              rows={14}
              required
              style={{ minHeight: '280px' }}
            />
          </div>

          {error && <div className="error-msg">⚠️ {error}</div>}

          <div className="form-actions">
            <button type="button" className="btn-secondary" onClick={() => navigate('/')}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={loading}>
              {loading ? '⏳ Publishing...' : '🚀 Publish Story'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
