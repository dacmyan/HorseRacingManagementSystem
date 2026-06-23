USE [HorseRacingManagementSystem];
GO

-- Fix corrupted Vietnamese names in Round table
UPDATE [Round]
SET [Name] = N'Vòng Loại 1'
WHERE [RoundId] = 9;

-- Fix corrupted Vietnamese names in Race table
UPDATE [Race]
SET [Name] = N'Đua Chung Kết Vòng Loại 1'
WHERE [RaceId] = 2;

UPDATE [Race]
SET [Name] = N'Đua Bán Kết Vòng Loại 1'
WHERE [RaceId] = 3;
GO
