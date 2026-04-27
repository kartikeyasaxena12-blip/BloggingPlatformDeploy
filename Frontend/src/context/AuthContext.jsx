import React, { createContext, useContext, useState } from 'react';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const token = localStorage.getItem('token');
    if (!token) return null;
    return {
      token,
      userId:    parseInt(localStorage.getItem('userId') || '0'),
      username:  localStorage.getItem('username')  || '',
      email:     localStorage.getItem('email')     || '',
      fullName:  localStorage.getItem('fullName')  || '',
      avatarUrl: localStorage.getItem('avatarUrl') || '',
      role:      localStorage.getItem('role')      || 'Reader',
    };
  });

  const login = (data) => {
    localStorage.setItem('token',     data.token);
    localStorage.setItem('userId',    data.userId);
    localStorage.setItem('username',  data.username);
    localStorage.setItem('email',     data.email     || '');
    localStorage.setItem('fullName',  data.fullName  || '');
    localStorage.setItem('avatarUrl', data.avatarUrl || '');
    localStorage.setItem('role',      data.role      || 'Reader');
    setUser({
      token:     data.token,
      userId:    data.userId,
      username:  data.username,
      email:     data.email     || '',
      fullName:  data.fullName  || '',
      avatarUrl: data.avatarUrl || '',
      role:      data.role      || 'Reader',
    });
  };

  const logout = () => {
    localStorage.clear();
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
