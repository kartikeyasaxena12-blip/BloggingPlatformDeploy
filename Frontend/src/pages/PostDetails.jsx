import React, { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';
import Comments from '../components/Comments';
import { formatIST } from '../utils/dateFormatter';

export default function PostDetails() {
  const { id } = useParams();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [post, setPost] = useState(null);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [deleting, setDeleting] = useState(false);
  const [likeCount, setLikeCount] = useState(0);
  const [isLiking, setIsLiking] = useState(false);
  const [likeSuccess, setLikeSuccess] = useState(false);
  const [isSaved, setIsSaved] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    Promise.all([
      api.get(`/api/posts/${id}`),
      api.get('/api/categories'),
      user ? api.get(`/api/posts/saved?userId=${user.id || user.userId}`) : Promise.resolve({ data: [] })
    ])
    .then(([postRes, catRes, savedRes]) => {
      setPost(postRes.data);
      setCategories(catRes.data);
      setLikeCount(postRes.data.likeCount || 0);
      if (user) {
        setIsSaved(savedRes.data.some(p => p.id === postRes.data.id));
      }
    })
    .catch(() => navigate('/'))
    .finally(() => setLoading(false));
  }, [id, user]);

  const handleLike = async () => {
    if (!user) {
      alert('Please sign in to like posts.');
      return;
    }
    setIsLiking(true);
    try {
      const res = await api.post(`/api/posts/${id}/like?userId=${user.userId}`);
      setLikeCount(res.data.likeCount);
      setLikeSuccess(true);
      setTimeout(() => setLikeSuccess(false), 2000);
    } catch (err) {
      console.error('Failed to like post', err);
    } finally {
      setIsLiking(false);
    }
  };

  const handleSaveToggle = async () => {
    if (!user) {
      navigate('/login');
      return;
    }
    setIsSaving(true);
    try {
      if (isSaved) {
        await api.delete(`/api/posts/${id}/save?userId=${user.id || user.userId}`);
        setIsSaved(false);
      } else {
        await api.post(`/api/posts/${id}/save?userId=${user.id || user.userId}`);
        setIsSaved(true);
      }
    } catch (err) {
      console.error('Failed to toggle save post', err);
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this post? This cannot be undone.')) return;
    setDeleting(true);
    try {
      await api.delete(`/api/posts/${id}?authorId=${user.userId}&userRole=${user.role}`);
      navigate('/');
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to delete post.');
      setDeleting(false);
    }
  };

  if (loading) return (
    <div className="page-container" style={{ textAlign: 'center', paddingTop: '100px' }}>
      <div style={{ fontSize: '2rem', marginBottom: '16px' }}>⏳</div>
      <div className="loading-text">Loading post...</div>
    </div>
  );
  if (!post) return null;

  const isAuthor = user && user.userId === post.authorId;
  const isAdmin = user && user.role === 'Admin';
  const category = categories.find(c => c.id === post.categoryId);

  return (
    <div className="page-container">
      <button onClick={() => navigate('/')} className="back-btn">
        ← Back to Feed
      </button>

      <article className="post-article">
        <header className="post-header">
          {category && (
            <div className="post-card-category" style={{ marginBottom: '14px' }}>
              {category.name}
            </div>
          )}
          <h1>{post.title}</h1>
          <div className="post-meta" style={{ marginTop: '14px' }}>
            <Link to={`/profile/${post.authorId}`} className="post-author hover-link">
              👤 {post.authorName || 'Anonymous'}
            </Link>
            <span className="post-date">{formatIST(post.createdAt)}</span>
            <span className="post-stats-divider">·</span>
            <span className="post-stat">👁️ {post.viewCount || 0} views</span>
            <span className="post-stat">❤️ {likeCount} likes</span>
          </div>
        </header>

        <div className="post-content">{post.content}</div>

        <div className="post-engagement-bar">
          <button
            className={`like-btn ${isLiking ? 'loading' : ''} ${likeSuccess ? 'liked' : ''}`}
            onClick={handleLike}
            disabled={isLiking}
          >
            {likeSuccess ? '❤️ Liked!' : isLiking ? '⏳ Liking...' : '❤️ Like this Post'}
          </button>
          
          <button
            className={`save-btn ${isSaved ? 'saved' : ''}`}
            onClick={handleSaveToggle}
            disabled={isSaving}
          >
            {isSaving ? '⏳...' : isSaved ? '🔖 Saved' : '📑 Save Post'}
          </button>

          {(isAuthor || isAdmin) && (
            <>
              <button className="btn-secondary" onClick={() => navigate(`/edit-post/${id}`)}>
                ✏️ Edit Story
              </button>
              <button className="btn-danger" onClick={handleDelete} disabled={deleting}>
                {deleting ? 'Deleting...' : '🗑️ Delete Post'}
              </button>
            </>
          )}
        </div>
      </article>

      <Comments postId={parseInt(id)} />
    </div>
  );
}
