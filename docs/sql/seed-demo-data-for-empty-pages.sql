-- Safe, non-destructive SQL Seed script for empty/static pages
-- Saved at docs/sql/seed-demo-data-for-empty-pages.sql

USE [HorseRacingManagementSystem];
GO

-- 1. Ensure Tournament exists
IF NOT EXISTS (SELECT 1 FROM [Tournament] WHERE [Name] = N'Giải Đua Vô Địch Quốc Gia 2026')
BEGIN
    INSERT INTO [Tournament] ([Name], [StartDate], [EndDate], [Status])
    VALUES (N'Giải Đua Vô Địch Quốc Gia 2026', '2026-06-01', '2026-06-30', N'Active');
END;

DECLARE @TournamentId INT;
SELECT @TournamentId = CAST([TournamentId] AS INT) FROM [Tournament] WHERE [Name] = N'Giải Đua Vô Địch Quốc Gia 2026';

-- 2. Ensure Round exists
IF NOT EXISTS (SELECT 1 FROM [Round] WHERE [TournamentId] = @TournamentId AND [RoundNumber] = 1)
BEGIN
    INSERT INTO [Round] ([TournamentId], [Name], [RoundNumber], [StartDate], [EndDate], [Status])
    VALUES (@TournamentId, N'Vòng Loại 1', 1, '2026-06-01', '2026-06-15', N'Active');
END;

DECLARE @RoundId INT;
SELECT @RoundId = CAST([RoundId] AS INT) FROM [Round] WHERE [TournamentId] = @TournamentId AND [RoundNumber] = 1;

-- 3. Ensure Horses exist for Owner (UserId = 2)
IF NOT EXISTS (SELECT 1 FROM [Horse] WHERE [Name] = N'Xích Thố')
BEGIN
    INSERT INTO [Horse] ([Name], [Age], [Breed], [OwnerId], [Gender], [HealthStatus])
    VALUES (N'Xích Thố', 5, N'Thoroughbred', 2, N'Stallion', N'Good');
END;
IF NOT EXISTS (SELECT 1 FROM [Horse] WHERE [Name] = N'Bạch Long Mã')
BEGIN
    INSERT INTO [Horse] ([Name], [Age], [Breed], [OwnerId], [Gender], [HealthStatus])
    VALUES (N'Bạch Long Mã', 4, N'Arabian', 2, N'Gelding', N'Good');
END;
IF NOT EXISTS (SELECT 1 FROM [Horse] WHERE [Name] = N'Hắc Phong')
BEGIN
    INSERT INTO [Horse] ([Name], [Age], [Breed], [OwnerId], [Gender], [HealthStatus])
    VALUES (N'Hắc Phong', 6, N'Quarter Horse', 2, N'Stallion', N'Good');
END;

DECLARE @Horse1Id INT, @Horse2Id INT, @Horse3Id INT;
SELECT TOP 1 @Horse1Id = [Id] FROM [Horse] WHERE [Name] = N'Xích Thố';
SELECT TOP 1 @Horse2Id = [Id] FROM [Horse] WHERE [Name] = N'Bạch Long Mã';
SELECT TOP 1 @Horse3Id = [Id] FROM [Horse] WHERE [Name] = N'Hắc Phong';

-- 4. Ensure Registrations exist
IF NOT EXISTS (SELECT 1 FROM [Registration] WHERE [TournamentId] = @TournamentId AND [HorseId] = @Horse1Id)
BEGIN
    INSERT INTO [Registration] ([TournamentId], [HorseId], [Status], [CreatedAt])
    VALUES (@TournamentId, @Horse1Id, N'Approved', '2026-06-01T10:00:00');
END;
IF NOT EXISTS (SELECT 1 FROM [Registration] WHERE [TournamentId] = @TournamentId AND [HorseId] = @Horse2Id)
BEGIN
    INSERT INTO [Registration] ([TournamentId], [HorseId], [Status], [CreatedAt])
    VALUES (@TournamentId, @Horse2Id, N'Pending', '2026-06-02T11:00:00');
END;
IF NOT EXISTS (SELECT 1 FROM [Registration] WHERE [TournamentId] = @TournamentId AND [HorseId] = @Horse3Id)
BEGIN
    INSERT INTO [Registration] ([TournamentId], [HorseId], [Status], [CreatedAt])
    VALUES (@TournamentId, @Horse3Id, N'Rejected', '2026-06-03T12:00:00');
END;

DECLARE @Reg1Id INT, @Reg2Id INT, @Reg3Id INT;
SELECT TOP 1 @Reg1Id = [Id] FROM [Registration] WHERE [HorseId] = @Horse1Id AND [TournamentId] = @TournamentId;
SELECT TOP 1 @Reg2Id = [Id] FROM [Registration] WHERE [HorseId] = @Horse2Id AND [TournamentId] = @TournamentId;
SELECT TOP 1 @Reg3Id = [Id] FROM [Registration] WHERE [HorseId] = @Horse3Id AND [TournamentId] = @TournamentId;

-- 5. Ensure Races exist
IF NOT EXISTS (SELECT 1 FROM [Race] WHERE [RoundId] = @RoundId AND [Name] = N'Đua Chung Kết Vòng Loại 1')
BEGIN
    INSERT INTO [Race] ([MaxLanes], [Name], [RaceDate], [DistanceMeter], [Status], [RoundId])
    VALUES (8, N'Đua Chung Kết Vòng Loại 1', '2026-06-16T15:00:00', 1200, N'Finished', @RoundId);
END;
IF NOT EXISTS (SELECT 1 FROM [Race] WHERE [RoundId] = @RoundId AND [Name] = N'Đua Bán Kết Vòng Loại 1')
BEGIN
    INSERT INTO [Race] ([MaxLanes], [Name], [RaceDate], [DistanceMeter], [Status], [RoundId])
    VALUES (8, N'Đua Bán Kết Vòng Loại 1', '2026-06-17T16:00:00', 1000, N'Scheduled', @RoundId);
END;

DECLARE @Race1Id INT, @Race2Id INT;
SELECT TOP 1 @Race1Id = CAST([RaceId] AS INT) FROM [Race] WHERE [Name] = N'Đua Chung Kết Vòng Loại 1';
SELECT TOP 1 @Race2Id = CAST([RaceId] AS INT) FROM [Race] WHERE [Name] = N'Đua Bán Kết Vòng Loại 1';

-- 6. Ensure Race Entries exist
IF NOT EXISTS (SELECT 1 FROM [RaceEntry] WHERE [RaceId] = @Race1Id AND [RegistrationId] = @Reg1Id)
BEGIN
    INSERT INTO [RaceEntry] ([RaceId], [RegistrationId], [JockeyId], [WinningProbability], [CurrentOdds], [LaneNo], [Status])
    VALUES (@Race1Id, @Reg1Id, 1, 35.5, 2.5, 1, N'Confirmed');
END;

IF NOT EXISTS (SELECT 1 FROM [RaceEntry] WHERE [RaceId] = @Race2Id AND [RegistrationId] = @Reg1Id)
BEGIN
    INSERT INTO [RaceEntry] ([RaceId], [RegistrationId], [JockeyId], [WinningProbability], [CurrentOdds], [LaneNo], [Status])
    VALUES (@Race2Id, @Reg1Id, 1, 40.0, 1.8, 1, N'Pending');
END;

DECLARE @RaceEntry1Id INT;
SELECT TOP 1 @RaceEntry1Id = CAST([RaceEntryId] AS INT) FROM [RaceEntry] WHERE [RaceId] = @Race1Id AND [RegistrationId] = @Reg1Id;

-- 7. Ensure Race Result exists for finished race
IF NOT EXISTS (SELECT 1 FROM [RaceResult] WHERE [RaceId] = @Race1Id)
BEGIN
    INSERT INTO [RaceResult] ([RaceId], [Winner])
    VALUES (@Race1Id, N'Xích Thố');
END;

-- 8. Ensure Referee Assignments exist
IF NOT EXISTS (SELECT 1 FROM [RaceRefereeAssignment] WHERE [RefereeId] = 1 AND [RaceId] = @Race1Id)
BEGIN
    INSERT INTO [RaceRefereeAssignment] ([RaceId], [RefereeId], [AssignedAt], [Status])
    VALUES (@Race1Id, 1, '2026-06-15T09:00:00', N'Assigned');
END;

IF NOT EXISTS (SELECT 1 FROM [RaceRefereeAssignment] WHERE [RefereeId] = 1 AND [RaceId] = @Race2Id)
BEGIN
    INSERT INTO [RaceRefereeAssignment] ([RaceId], [RefereeId], [AssignedAt], [Status])
    VALUES (@Race2Id, 1, '2026-06-15T09:00:00', N'Assigned');
END;

DECLARE @AssignmentId INT;
SELECT TOP 1 @AssignmentId = CAST([AssignmentId] AS INT) FROM [RaceRefereeAssignment] WHERE [RefereeId] = 1 AND [RaceId] = @Race1Id;

-- 9. Ensure Referee Report exists
IF NOT EXISTS (SELECT 1 FROM [RefereeReport] WHERE [AssignmentId] = @AssignmentId)
BEGIN
    INSERT INTO [RefereeReport] ([AssignmentId], [Content], [ViolationNote], [CreatedAt], [ReportedUserId], [ReportedHorseId])
    VALUES (@AssignmentId, N'Cuộc đua diễn ra tốt đẹp, không có sự cố nghiêm trọng ngoại trừ lỗi cản trở ở làn số 1.', N'Cản trở làn chạy ở góc cua thứ 2', '2026-06-16T15:30:00', 3, @Horse1Id);
END;

-- 10. Ensure Race Violation exists
IF NOT EXISTS (SELECT 1 FROM [RaceViolation] WHERE [RaceId] = @Race1Id AND [Description] LIKE N'Cản trở%')
BEGIN
    INSERT INTO [RaceViolation] ([RaceId], [Description], [Penalty])
    VALUES 
    (@Race1Id, N'Cản trở: Nài ngựa jockey chèn ép làn chạy của đối thủ tại khúc cua số 2', N'Cảnh cáo');
END;

IF NOT EXISTS (SELECT 1 FROM [RaceViolation] WHERE [RaceId] = @Race1Id AND [Description] LIKE N'Kháng cáo%')
BEGIN
    INSERT INTO [RaceViolation] ([RaceId], [Description], [Penalty])
    VALUES 
    (@Race1Id, N'Kháng cáo: Jockey khiếu nại quyết định phạt cảnh cáo vì cho rằng bị mất thăng bằng khách quan', N'Đang xem xét');
END;

-- 11. Ensure Predictions exist (for Spectator UserId = 5)
IF @RaceEntry1Id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Prediction] WHERE [UserId] = 5 AND [RaceId] = @Race1Id)
BEGIN
    INSERT INTO [Prediction] ([UserId], [RaceId], [RaceEntryId], [Point], [IsCorrect], [Status], [PredictedAt])
    VALUES (5, @Race1Id, @RaceEntry1Id, 100, 1, N'Settled', '2026-06-16T14:30:00');
END;

-- 12. Ensure Wallet exists and contains some transactions
DECLARE @WalletId INT;
SELECT @WalletId = [WalletId] FROM [Wallet] WHERE [UserId] = 5;

IF @WalletId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [WalletTransaction] WHERE [WalletId] = @WalletId)
BEGIN
    INSERT INTO [WalletTransaction] ([WalletId], [BetId], [PayoutId], [PrizePayoutId], [Amount], [Type], [Status], [PaymentMethod], [GatewayTransactionId], [Description], [CreatedAt])
    VALUES 
    (@WalletId, NULL, NULL, NULL, 500000.0, N'Deposit', N'Success', N'Bank Transfer', N'TXN001', N'Nạp tiền vào tài khoản', '2026-06-16T10:00:00'),
    (@WalletId, NULL, NULL, NULL, -200000.0, N'Withdraw', N'Success', N'Bank Transfer', N'TXN002', N'Rút tiền về tài khoản ngân hàng', '2026-06-16T12:00:00'),
    (@WalletId, NULL, NULL, NULL, 150000.0, N'Refund', N'Success', N'System', N'TXN003', N'Hoàn trả điểm dự đoán đúng', '2026-06-16T16:00:00');
    
    UPDATE [Wallet] SET [Balance] = [Balance] + 450000.0 WHERE [WalletId] = @WalletId;
END;

PRINT 'Safe demo data successfully seeded!';
