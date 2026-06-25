import { api } from '../services/api';

export const getJockeyRankings = () => api.get('/public/rankings/jockeys');
export const getHorseRankings = () => api.get('/public/rankings/horses');
export const getRaceSchedule = () => api.get('/public/races/schedule');
export const getNotifications = () => api.get('/public/notifications');
export const markNotificationRead = (id) => api.put(`/public/notifications/${id}/read`);

export const getTournaments = () => api.get('/public/tournaments');
export const getTournamentDetail = (id) => api.get(`/public/tournaments/${id}`);

export const getLiveRaces = () => api.get('/public/races/live');

export const getRaceDetail = (id) => api.get(`/public/races/${id}`);
export const getRaceEntries = (raceId) => api.get(`/public/races/${raceId}/entries`);
