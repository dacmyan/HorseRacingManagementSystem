import { api } from './api';

export async function login(email, password) {
  const data = await api.post('/auth/login', { email, password });
  if (data.token) localStorage.setItem('token', data.token);
  return data;
}

export async function register(name, email, password, role) {
  return api.post('/auth/register', { name, email, password, role });
}

export function logout() {
  localStorage.removeItem('token');
}

export function getToken() {
  return localStorage.getItem('token');
}

export function isAuthenticated() {
  return !!getToken();
}
