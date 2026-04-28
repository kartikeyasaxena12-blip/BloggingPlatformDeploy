import React, { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import Navbar from './components/Navbar';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import PostDetails from './pages/PostDetails';
import CreatePost from './pages/CreatePost';
import EditPost from './pages/EditPost';
import Profile from './pages/Profile';
import Unsubscribe from './pages/Unsubscribe';
import NewsletterAdmin from './pages/NewsletterAdmin';

function ProtectedRoute({ children }) {
  const { user } = useAuth();
  return user ? children : <Navigate to="/login" />;
}

function AdminRoute({ children }) {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" />;
  if (user.role !== 'Admin') return <Navigate to="/" />;
  return children;
}

function AppRoutes() {
  const [isWakingUp, setIsWakingUp] = useState(true);
  const [wakeStatus, setWakeStatus] = useState('Initializing services...');

  useEffect(() => {
    const services = [
      'https://inkwell-api-n55y.onrender.com/health',
      'https://inkwell-auth.onrender.com/health',
      'https://inkwell-post.onrender.com/health',
      'https://inkwell-comment.onrender.com/health',
      'https://inkwell-category.onrender.com/health',
      'https://inkwell-newsletter.onrender.com/health',
      'https://inkwell-notification.onrender.com/health'
    ];

    const wakeAll = async () => {
      try {
        // We use Promise.allSettled to continue even if one fails (health check might be flaky)
        // But we want to wait for them to at least try to wake up
        await Promise.allSettled(services.map(url => 
          fetch(url, { mode: 'no-cors' }).catch(e => console.log("Waking...", url))
        ));
        
        // Give it a small extra buffer for the DBs to connect
        setWakeStatus('Finalizing connection...');
        setTimeout(() => setIsWakingUp(false), 1500);
      } catch (err) {
        console.error("Wake up sequence error", err);
        setIsWakingUp(false);
      }
    };

    wakeAll();
  }, []);

  if (isWakingUp) {
    return (
      <div className="waking-up-screen">
        <div className="waking-up-content">
          <div className="waking-up-spinner"></div>
          <h1>InkWell</h1>
          <p>{wakeStatus}</p>
          <span className="waking-up-note">
            Render free tier services are spinning up. This may take a moment.
          </span>
        </div>
      </div>
    );
  }

  return (
    <>
      <Navbar />
      <main className="main-content">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/post/:id" element={<PostDetails />} />
          <Route path="/profile" element={
            <ProtectedRoute><Profile /></ProtectedRoute>
          } />
          <Route path="/profile/:id" element={<Profile />} />
          <Route path="/create-post" element={
            <ProtectedRoute><CreatePost /></ProtectedRoute>
          } />
          <Route path="/edit-post/:id" element={
            <ProtectedRoute><EditPost /></ProtectedRoute>
          } />
          <Route path="/unsubscribe" element={<Unsubscribe />} />
          <Route path="/newsletter-admin" element={
            <AdminRoute><NewsletterAdmin /></AdminRoute>
          } />
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </main>
    </>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </BrowserRouter>
  );
}
