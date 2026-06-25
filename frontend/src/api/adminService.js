import { api } from '../services/api';

export const getRoles = () => api.get('/admin/roles');

export const createAccount = (data) => api.post('/admin/accounts', data);

export const getAccounts = () => api.get('/admin/accounts');

export const createTournament = (data) => api.post('/admin/tournaments', data);

export const createRace = (data) => api.post('/admin/races', data);

export const createRaceEntry = (raceId, data) => api.post(`/admin/races/${raceId}/entries`, data);

export const assignReferee = (raceId, refereeId) => api.post(`/admin/races/${raceId}/referees`, { refereeId });

export const getRaceReferees = (raceId) => api.get(`/admin/races/${raceId}/referees`);

export const removeReferee = (raceId, refereeId) => api.delete(`/admin/races/${raceId}/referees/${refereeId}`);

export const createPrizes = (data) => api.post('/admin/payouts/prizes', data);

export const triggerPayout = (raceId) => api.post(`/admin/payouts/trigger/${raceId}`);

export const getRegistrations = () => api.get('/admin/registrations');

export const getReferees = () => api.get('/admin/referees');

export const getViolations = () => api.get('/admin/violations');

export const getPredictionStats = () => api.get('/admin/predictions/stats');

export const getPredictions = () => api.get('/admin/predictions');

export const updateUserStatus = (id) => api.put(`/admin/users/${id}/status`);

export const publishRaceResult = (raceId) => api.post(`/admin/races/${raceId}/publish`);

export const getRaceResults = (raceId) => api.get(`/admin/races/${raceId}/results`);

export const getDashboardStats = () => api.get('/admin/dashboard');
