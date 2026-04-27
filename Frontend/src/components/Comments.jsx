import React, { useEffect, useState, useRef } from 'react';
import { Link } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';
import { formatIST } from '../utils/dateFormatter';

export default function Comments({ postId }) {
  const { user } = useAuth();
  const [comments, setComments] = useState([]);
  const [content, setContent] = useState('');
  const [replyTo, setReplyTo] = useState(null);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const textareaRef = useRef(null);

  const fetchComments = () => {
    setLoading(true);
    api.get(`/api/comments/post/${postId}`)
      .then(res => setComments(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchComments(); }, [postId]);

  const handleReply = (comment) => {
    setReplyTo(comment);
    setTimeout(() => textareaRef.current?.focus(), 100);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!content.trim()) return;
    setSubmitting(true);
    try {
      await api.post('/api/comments', {
        postId,
        authorId: user.userId,
        authorName: user.username,
        content,
        parentCommentId: replyTo?.id || null,
      });
      setContent('');
      setReplyTo(null);
      fetchComments();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to post comment.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (commentId) => {
    if (!window.confirm('Are you sure you want to delete this comment?')) return;
    try {
      await api.delete(`/api/comments/${commentId}?authorId=${user.userId}&userRole=${user.role}`);
      fetchComments();
    } catch (err) {
      alert(err.response?.data?.message || 'Failed to delete comment.');
    }
  };

  const topLevel = comments.filter(c => !c.parentCommentId);
  const replies = (parentId) => comments.filter(c => c.parentCommentId === parentId);

  const canManage = (comment) => user && (user.userId === comment.authorId || user.role === 'Admin');

  return (
    <div className="comments-section">
      <h2 className="comments-title">
        💬 Comments
        <span style={{
          background: 'var(--accent-light)', color: 'var(--accent)',
          fontSize: '0.78rem', fontWeight: 600, padding: '3px 10px',
          borderRadius: '20px', marginLeft: '4px'
        }}>
          {comments.length}
        </span>
      </h2>

      {/* Comment Form */}
      {user ? (
        <form onSubmit={handleSubmit} className="comment-form">
          {replyTo && (
            <div className="reply-indicator">
              <span>↩ Replying to <strong>{replyTo.authorName || 'Anonymous'}</strong></span>
              <button type="button" className="cancel-reply" onClick={() => setReplyTo(null)}>
                ✕ Cancel
              </button>
            </div>
          )}
          <textarea
            ref={textareaRef}
            placeholder={replyTo ? `Reply to ${replyTo.authorName}...` : 'Share your thoughts...'}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            rows={3}
            required
          />
          <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
            <button type="submit" className="btn-primary" disabled={submitting || !content.trim()}>
              {submitting ? '⏳ Posting...' : replyTo ? '↩ Post Reply' : '💬 Post Comment'}
            </button>
          </div>
        </form>
      ) : (
        <div className="login-to-comment">
          <Link to="/login">Sign in</Link> to join the conversation and leave a comment.
        </div>
      )}

      {/* Comments List */}
      {loading ? (
        <div className="loading-text">Loading comments...</div>
      ) : topLevel.length === 0 ? (
        <div className="no-comments">
          <div style={{ fontSize: '2rem', marginBottom: '8px' }}>🗨️</div>
          No comments yet — be the first to start the conversation!
        </div>
      ) : (
        <div className="comments-list">
          {topLevel.map(comment => (
            <div key={comment.id} className="comment-thread">
              <div className="comment-card">
                <div className="comment-header">
                  <Link to={`/profile/${comment.authorId}`} className="comment-author">
                    {comment.authorName || 'Anonymous'}
                  </Link>
                  <span className="comment-date">{formatIST(comment.createdAt)}</span>
                </div>
                <p className="comment-content">{comment.content}</p>
                <div className="comment-actions">
                  {user && (
                    <button className="reply-btn" onClick={() => handleReply(comment)}>
                      ↩ Reply
                    </button>
                  )}
                  {canManage(comment) && (
                    <button className="comment-delete-btn" onClick={() => handleDelete(comment.id)}>
                      🗑️
                    </button>
                  )}
                </div>
              </div>

              {replies(comment.id).map(reply => (
                <div key={reply.id} className="reply-card">
                  <div className="comment-header">
                    <Link to={`/profile/${reply.authorId}`} className="comment-author">
                      {reply.authorName || 'Anonymous'}
                    </Link>
                    <span className="comment-date">{formatIST(reply.createdAt)}</span>
                  </div>
                  <p className="comment-content">{reply.content}</p>
                  <div className="comment-actions">
                    {canManage(reply) && (
                      <button className="comment-delete-btn" onClick={() => handleDelete(reply.id)}>
                        🗑️
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
