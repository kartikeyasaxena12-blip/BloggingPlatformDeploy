import React from 'react';
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
