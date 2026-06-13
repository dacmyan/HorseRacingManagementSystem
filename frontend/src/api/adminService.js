import { api } from '../services/api';

export const getRoles = () => api.get('/admin/roles');

export const getAccounts = () => api.get('/admin/accounts');

export const createAccount = (data) => api.post('/admin/accounts', data);

export const createTournament = (data) => api.post('/admin/tournaments', data);

export const createRace = (data) => api.post('/admin/races', data);

export const createPrizes = (data) => api.post('/admin/payouts/prizes', data);

export const triggerPayout = (raceId) => api.post(`/admin/payouts/trigger/${raceId}`);
