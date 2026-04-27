import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';

export default function EditPost() {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ title: '', content: '', categoryId: '', authorId: null });
  const [categories, setCategories] = useState([]);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState(false);

  useEffect(() => {
    Promise.all([
      api.get(`/api/posts/${id}`),
      api.get('/api/categories')
    ])
    .then(([postRes, catRes]) => {
      const post = postRes.data;
      setForm({
        title: post.title,
        content: post.content,
        categoryId: post.categoryId || '',
        authorId: post.authorId
      });
      setCategories(catRes.data);
      
      // Security check: Only author or Admin can edit
      if (user && user.userId !== post.authorId && user.role !== 'Admin') {
        navigate('/');
      }
    })
    .catch(err => {
      console.error('Failed to fetch post details:', err);
      navigate('/');
    })
    .finally(() => setLoading(false));
  }, [id, user, navigate]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.title.trim()) { setError('Please add a title.'); return; }
    if (!form.content.trim()) { setError('Please add some content.'); return; }
    setUpdating(true);
    setError('');
    try {
      const postData = {
        title: form.title,
        content: form.content,
        authorId: user.userId, // Send CURRENT user's ID
        userRole: user.role,    // Send CURRENT user's role
        categoryId: form.categoryId ? parseInt(form.categoryId) : null
      };
      await api.put(`/api/posts/${id}`, postData);
      navigate(`/post/${id}`);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update post. Please try again.');
    } finally {
      setUpdating(false);
    }
  };

  if (loading) return (
    <div className="page-container" style={{ textAlign: 'center', paddingTop: '100px' }}>
      <div className="loading-text">Loading post details...</div>
    </div>
  );

  return (
    <div className="page-container">
      <button onClick={() => navigate(`/post/${id}`)} className="back-btn">
        ← Back to Post
      </button>

      <div className="form-card">
        <div className="form-card-header">
          <h1>✏️ Edit Your Story</h1>
          <p>Refine your thoughts and update your story for the community</p>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Title</label>
            <input
              type="text"
              placeholder="Post title..."
              value={form.title}
              onChange={(e) => setForm({ ...form, title: e.target.value })}
              required
            />
          </div>

          <div className="form-group">
            <label>Category</label>
            <select
              value={form.categoryId}
              onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
            >
              <option value="">Select a category...</option>
              {categories.map(cat => (
                <option key={cat.id} value={cat.id}>{cat.name}</option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label>Content</label>
            <textarea
              placeholder="Write your story here..."
              value={form.content}
              onChange={(e) => setForm({ ...form, content: e.target.value })}
              rows={14}
              required
              style={{ minHeight: '280px' }}
            />
          </div>

          {error && <div className="error-msg">⚠️ {error}</div>}

          <div className="form-actions">
            <button type="button" className="btn-secondary" onClick={() => navigate(`/post/${id}`)}>
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={updating}>
              {updating ? '⏳ Updating...' : '💾 Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
