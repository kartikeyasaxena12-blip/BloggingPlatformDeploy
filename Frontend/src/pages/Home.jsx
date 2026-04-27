import React, { useEffect, useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import api from '../api/axios';
import { useAuth } from '../context/AuthContext';
import { formatIST } from '../utils/dateFormatter';
import Newsletter from '../components/Newsletter';

export default function Home() {
  const [posts, setPosts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [loading, setLoading] = useState(true);
  const [internalSearch, setInternalSearch] = useState('');
  const [sortBy, setSortBy] = useState('latest');
  const [showSortMenu, setShowSortMenu] = useState(false);
  const [savedPostIds, setSavedPostIds] = useState(new Set());
  const { user } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    const searchParams = new URLSearchParams(location.search);
    const search = searchParams.get('search');
    if (search) setInternalSearch(search);
    else setInternalSearch('');
  }, [location.search]);

  useEffect(() => {
    api.get('/api/categories').then(res => setCategories(res.data)).catch(console.error);
    
    if (user) {
      // Fetch user's saved posts to toggle the bookmark icon correctly
      api.get(`/api/posts/saved?userId=${user.id}`)
        .then(res => {
          const ids = new Set(res.data.map(p => p.id));
          setSavedPostIds(ids);
        })
        .catch(console.error);
    }
  }, [user]);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    const params = new URLSearchParams(location.search);
    if (internalSearch) params.set('search', internalSearch);
    else params.delete('search');
    navigate(`/?${params.toString()}`);
  };

  useEffect(() => {
    setLoading(true);
    const searchParams = new URLSearchParams(location.search);
    const search = searchParams.get('search');
    let url = '/api/posts';
    const params = [];
    if (selectedCategory) params.push(`categoryId=${selectedCategory}`);
    if (search) params.push(`search=${encodeURIComponent(search)}`);
    if (sortBy && sortBy !== 'latest') params.push(`sortBy=${sortBy}`);
    
    if (params.length > 0) url += `?${params.join('&')}`;
    api.get(url)
      .then(res => setPosts(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [selectedCategory, location.search, sortBy]);

  const handleSavePost = async (postId, e) => {
    e.preventDefault();
    e.stopPropagation();
    if (!user) {
      navigate('/login');
      return;
    }

    const isSaved = savedPostIds.has(postId);
    try {
      if (isSaved) {
        await api.delete(`/api/posts/${postId}/save?userId=${user.id}`);
        setSavedPostIds(prev => {
          const newSet = new Set(prev);
          newSet.delete(postId);
          return newSet;
        });
      } else {
        await api.post(`/api/posts/${postId}/save?userId=${user.id}`);
        setSavedPostIds(prev => {
          const newSet = new Set(prev);
          newSet.add(postId);
          return newSet;
        });
      }
    } catch (err) {
      console.error('Failed to toggle save post', err);
    }
  };

  const searchQuery = new URLSearchParams(location.search).get('search');

  return (
    <div className="page-container">
      {/* Header */}
      <div className="page-header">
        <div>
          <h1>✨ Latest Stories</h1>
          <p>
            {searchQuery
              ? `Showing results for "${searchQuery}"`
              : 'Discover stories from the InkWell community'}
          </p>
        </div>

        <div className="header-actions">
          <form className="home-header-search" onSubmit={handleSearchSubmit}>
            <input
              type="text"
              placeholder="Search posts..."
              value={internalSearch}
              onChange={(e) => setInternalSearch(e.target.value)}
            />
            <button type="submit" className="search-btn-icon">🔍</button>
          </form>

          <div className="custom-sort-container" onMouseLeave={() => setShowSortMenu(false)}>
            <button 
              className="sort-btn-icon" 
              onClick={() => setShowSortMenu(!showSortMenu)}
              title="Sort posts"
            >
              <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <line x1="21" y1="10" x2="7" y2="10"></line>
                <line x1="21" y1="6" x2="3" y2="6"></line>
                <line x1="21" y1="14" x2="11" y2="14"></line>
                <line x1="21" y1="18" x2="15" y2="18"></line>
              </svg>
            </button>

            {showSortMenu && (
              <div className="sort-menu-dropdown">
                <div className={`sort-menu-item ${sortBy === 'latest' ? 'active' : ''}`} onClick={() => { setSortBy('latest'); setShowSortMenu(false); }}>
                  ⏱️ Latest
                </div>
                <div className={`sort-menu-item ${sortBy === 'oldest' ? 'active' : ''}`} onClick={() => { setSortBy('oldest'); setShowSortMenu(false); }}>
                  🕰️ Oldest
                </div>
                <div className={`sort-menu-item ${sortBy === 'popular' ? 'active' : ''}`} onClick={() => { setSortBy('popular'); setShowSortMenu(false); }}>
                  🔥 Popular
                </div>
                <div className={`sort-menu-item ${sortBy === 'most_liked' ? 'active' : ''}`} onClick={() => { setSortBy('most_liked'); setShowSortMenu(false); }}>
                  ❤️ Most Liked
                </div>
              </div>
            )}
          </div>

          {user && (
            <Link to="/create-post" className="btn-primary write-btn-header">
              ✏️ Write Post
            </Link>
          )}
        </div>
      </div>

      {/* Category Filter */}
      <div className="category-filter">
        <button
          className={`filter-chip ${selectedCategory === null ? 'active' : ''}`}
          onClick={() => setSelectedCategory(null)}
        >
          🌐 All
        </button>
        {categories.map(cat => (
          <button
            key={cat.id}
            className={`filter-chip ${selectedCategory === cat.id ? 'active' : ''}`}
            onClick={() => setSelectedCategory(cat.id)}
          >
            {cat.name}
          </button>
        ))}
      </div>

      {/* Posts */}
      {loading ? (
        <div className="loading-grid">
          {[1, 2, 3, 4, 5, 6].map(i => <div key={i} className="skeleton-card" />)}
        </div>
      ) : posts.length === 0 ? (
        <div className="empty-state">
          <div className="empty-icon">{searchQuery ? '🔍' : '📝'}</div>
          <h3>{searchQuery ? 'No results found' : 'No stories yet'}</h3>
          <p>
            {searchQuery
              ? `We couldn't find any posts matching "${searchQuery}". Try a different search.`
              : 'Be the first to share something with the community!'}
          </p>
          {searchQuery && (
            <button className="btn-secondary" onClick={() => navigate('/')}>
              ← Clear Search
            </button>
          )}
          {user && !searchQuery && (
            <Link to="/create-post" className="btn-primary">✏️ Write the first post</Link>
          )}
        </div>
      ) : (
        <div className="posts-grid">
          {posts.map(post => {
            const category = categories.find(c => c.id === post.categoryId);
            return (
              <Link to={`/post/${post.id}`} key={post.id} className="post-card">
                <div className="post-card-header">
                  {category && (
                    <div className="post-card-category">{category.name}</div>
                  )}
                  <button 
                    className={`bookmark-btn ${savedPostIds.has(post.id) ? 'saved' : ''}`}
                    onClick={(e) => handleSavePost(post.id, e)}
                    title={savedPostIds.has(post.id) ? "Unsave Post" : "Save Post"}
                  >
                    {savedPostIds.has(post.id) ? '🔖' : '📑'}
                  </button>
                </div>
                <h2 className="post-card-title">{post.title}</h2>
                <p className="post-card-excerpt">
                  {post.content.substring(0, 140)}{post.content.length > 140 ? '...' : ''}
                </p>

                <div className="post-engagement-meta">
                  <span className="engagement-item">👁️ {post.viewCount || 0} views</span>
                  <span className="engagement-item">❤️ {post.likeCount || 0} likes</span>
                </div>

                <div className="post-meta">
                  <Link
                    to={`/profile/${post.authorId}`}
                    className="post-author hover-link"
                    onClick={e => e.stopPropagation()}
                  >
                    👤 {post.authorName || 'Anonymous'}
                  </Link>
                  <span className="post-date">{formatIST(post.createdAt)}</span>
                </div>
              </Link>
            );
          })}
        </div>
      )}

      {/* ── Newsletter Subscription ────────────────────────────── */}
      <Newsletter />
    </div>
  );
}

