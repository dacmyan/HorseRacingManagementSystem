USE [HorseRacingManagementSystem];
GO

-- Safe update for corrupted Vietnamese tournament name using TournamentId
UPDATE [Tournament]
SET [Name] = N'Giải Đua Vô Địch Quốc Gia 2026'
WHERE [TournamentId] = 4;
GO
