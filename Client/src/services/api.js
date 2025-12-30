import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use(
  config => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => Promise.reject(error),
);

export const authAPI = {
  login: async (username, password) => {
    const response = await api.post('/Auth/login', { username, password });
    return response.data;
  },
  register: async (username, email, password) => {
    const response = await api.post('/Auth/register', { username, email, password });
    return response.data;
  },
};

export const cryptoAPI = {
  getPrices: async () => {
    const response = await api.get('/Crypto/prices');
    return response.data;
  },
  getSymbolDetails: async symbol => {
    const response = await api.get(`/Crypto/prices/${symbol}/details`);
    return response.data;
  },
};

export default api;
