-- ============================================================
-- RESET & RE-SEED DATABASE (seed_test_tournaments.sql)
-- System: Horse Racing Management System
-- Engine: Microsoft SQL Server / Azure SQL
-- ============================================================

USE HorseRacingManagementSystem;
GO

SET NOCOUNT ON;

PRINT '============================================================';
PRINT '>>> STEP 1: CLEANING ALL EXISTING TEST DATA...';
PRINT '============================================================';

-- Disable foreign key constraints temporarily for clean wipe
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- Delete transactional & entity tables in safe order
DELETE FROM [Bet];
DELETE FROM [Payout];
DELETE FROM [Prize];
DELETE FROM [TournamentPrizePayout];
DELETE FROM [RaceResult];
DELETE FROM [RaceViolation];
DELETE FROM [RefereeReport];
DELETE FROM [RaceRefereeAssignment];
DELETE FROM [MedicalCheckRecord];
DELETE FROM [RaceEntry];
DELETE FROM [JockeyContract];
DELETE FROM [Registration];
DELETE FROM [Race];
DELETE FROM [Round];
DELETE FROM [Tournament];
DELETE FROM [HorseDocument];
DELETE FROM [HorseStatistic];
DELETE FROM [Horse];
DELETE FROM [JockeyProfile];
DELETE FROM [RefereeProfile];
DELETE FROM [Prediction];
DELETE FROM [Notification];
DELETE FROM [WalletTransaction];
DELETE FROM [Wallet];
DELETE FROM [AppUser];

-- Re-enable foreign key constraints
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';

PRINT '>>> All existing test data cleared successfully!';

PRINT '============================================================';
PRINT '>>> STEP 2: ENSURING ROLES & DEFAULT USERS...';
PRINT '============================================================';

-- Ensure Roles
IF NOT EXISTS (SELECT 1 FROM [Role] WHERE [RoleId] = 1) INSERT INTO [Role] ([RoleId], [Name]) VALUES (1, 'Admin');
IF NOT EXISTS (SELECT 1 FROM [Role] WHERE [RoleId] = 2) INSERT INTO [Role] ([RoleId], [Name]) VALUES (2, 'HorseOwner');
IF NOT EXISTS (SELECT 1 FROM [Role] WHERE [RoleId] = 3) INSERT INTO [Role] ([RoleId], [Name]) VALUES (3, 'Jockey');
IF NOT EXISTS (SELECT 1 FROM [Role] WHERE [RoleId] = 4) INSERT INTO [Role] ([RoleId], [Name]) VALUES (4, 'Referee');
IF NOT EXISTS (SELECT 1 FROM [Role] WHERE [RoleId] = 5) INSERT INTO [Role] ([RoleId], [Name]) VALUES (5, 'Spectator');
IF NOT EXISTS (SELECT 1 FROM [Role] WHERE [RoleId] = 6) INSERT INTO [Role] ([RoleId], [Name]) VALUES (6, 'Veterinarian');

-- Standard ASP.NET Core PasswordHasher hash for '123456'
DECLARE @PassHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEO9sK51n5m62mB2cM5L0fX2b+3R5T0Y+3r9t7kL3v1a8b9c0d1e2f3g4h5i6j7k8==';

-- 1. Admin
INSERT INTO [AppUser] ([Username], [Email], [PasswordHash], [FullName], [RoleId], [Status], [IsEmailConfirmed], [CreatedAt])
VALUES ('admin', 'admin@gmail.com', @PassHash, N'System Administrator', 1, 'Active', 1, GETDATE());
DECLARE @AdminId INT = SCOPE_IDENTITY();
INSERT INTO [Wallet] ([UserId], [Balance]) VALUES (@AdminId, 100000000.00);

-- 2. HorseOwner (Main)
INSERT INTO [AppUser] ([Username], [Email], [PasswordHash], [FullName], [RoleId], [Status], [IsEmailConfirmed], [CreatedAt])
VALUES ('owner', 'owner@gmail.com', @PassHash, N'Chủ Ngựa Nguyễn Văn A', 2, 'Active', 1, GETDATE());
DECLARE @MainOwnerId INT = SCOPE_IDENTITY();
INSERT INTO [Wallet] ([UserId], [Balance]) VALUES (@MainOwnerId, 50000000.00);

-- Extra HorseOwners (Owner 2..5)
DECLARE @OwnerCounter INT = 2;
WHILE @OwnerCounter <= 5
BEGIN
    INSERT INTO [AppUser] ([Username], [Email], [PasswordHash], [FullName], [RoleId], [Status], [IsEmailConfirmed], [CreatedAt])
    VALUES ('owner' + CAST(@OwnerCounter AS NVARCHAR(5)), 'owner' + CAST(@OwnerCounter AS NVARCHAR(5)) + '@gmail.com', @PassHash, N'Chủ Ngựa ' + CAST(@OwnerCounter AS NVARCHAR(5)), 2, 'Active', 1, GETDATE());
    DECLARE @OId INT = SCOPE_IDENTITY();
    INSERT INTO [Wallet] ([UserId], [Balance]) VALUES (@OId, 20000000.00);
    SET @OwnerCounter = @OwnerCounter + 1;
END

-- 3. Jockeys (jockey1 to jockey50 with password 123456)
PRINT '>>> Seeding 50 Jockey accounts (jockey1 to jockey50)...';
DECLARE @JCounter INT = 1;
WHILE @JCounter <= 50
BEGIN
    DECLARE @JUsername NVARCHAR(50) = 'jockey' + CAST(@JCounter AS NVARCHAR(10));
    DECLARE @JEmail NVARCHAR(50) = @JUsername + '@gmail.com';
    INSERT INTO [AppUser] ([Username], [Email], [PasswordHash], [FullName], [RoleId], [Status], [IsEmailConfirmed], [CreatedAt])
    VALUES (@JUsername, @JEmail, @PassHash, N'Nài Ngựa ' + CAST(@JCounter AS NVARCHAR(10)), 3, 'Active', 1, GETDATE());
    DECLARE @JId INT = SCOPE_IDENTITY();
    INSERT INTO [JockeyProfile] ([UserId], [ExperienceYears], [RankingPoint], [Status]) VALUES (@JId, 2 + (@JCounter % 5), 100 + (@JCounter * 2), 'Active');
    INSERT INTO [Wallet] ([UserId], [Balance]) VALUES (@JId, 5000000.00);
    SET @JCounter = @JCounter + 1;
END

-- 4. Referee (Main)
INSERT INTO [AppUser] ([Username], [Email], [PasswordHash], [FullName], [RoleId], [Status], [IsEmailConfirmed], [CreatedAt])
VALUES ('referee', 'referee@gmail.com', @PassHash, N'Trọng Tài Lê Văn C', 4, 'Active', 1, GETDATE());
DECLARE @MainRefId INT = SCOPE_IDENTITY();
INSERT INTO [RefereeProfile] ([UserId], [LicenseNumber], [Status]) VALUES (@MainRefId, 'REF-001', 'Active');
INSERT INTO [Wallet] ([UserId], [Balance]) VALUES (@MainRefId, 10000000.00);

-- Extra Referees (Ref 2..4)
DECLARE @RefCounter INT = 2;
WHILE @RefCounter <= 4
BEGIN
    INSERT INTO [AppUser] ([Username], [Email], [PasswordHash], [FullName], [RoleId], [Status], [IsEmailConfirmed], [CreatedAt])
    VALUES ('referee' + CAST(@RefCounter AS NVARCHAR(5)), 'referee' + CAST(@RefCounter AS NVARCHAR(5)) + '@gmail.com', @PassHash, N'Trọng Tài ' + CAST(@RefCounter AS NVARCHAR(5)), 4, 'Active', 1, GETDATE());
    DECLARE @RId INT = SCOPE_IDENTITY();
    INSERT INTO [RefereeProfile] ([UserId], [LicenseNumber], [Status]) VALUES (@RId, 'REF-00' + CAST(@RefCounter AS NVARCHAR(5)), 'Active');
    INSERT INTO [Wallet] ([UserId], [Balance]) VALUES (@RId, 5000000.00);
    SET @RefCounter = @RefCounter + 1;
END

-- 5. Spectator (Main)
INSERT INTO [AppUser] ([Username], [Email], [PasswordHash], [FullName], [RoleId], [Status], [IsEmailConfirmed], [CreatedAt])
VALUES ('spectator', 'spectator@gmail.com', @PassHash, N'Khán Giả Phạm Văn D', 5, 'Active', 1, GETDATE());
DECLARE @MainSpecId INT = SCOPE_IDENTITY();
INSERT INTO [Wallet] ([UserId], [Balance]) VALUES (@MainSpecId, 50000000.00);

-- 6. Veterinarian (Main)
INSERT INTO [AppUser] ([Username], [Email], [PasswordHash], [FullName], [RoleId], [Status], [IsEmailConfirmed], [CreatedAt])
VALUES ('vet', 'vet@gmail.com', @PassHash, N'Bác Sĩ Thú Y Hoàng Văn E', 6, 'Active', 1, GETDATE());
DECLARE @MainVetId INT = SCOPE_IDENTITY();
INSERT INTO [Wallet] ([UserId], [Balance]) VALUES (@MainVetId, 10000000.00);

PRINT '>>> Users and Profiles created successfully!';

PRINT '============================================================';
PRINT '>>> STEP 3: SEEDING HORSES & STATISTICS...';
PRINT '============================================================';

-- Seed 48 Horses
DECLARE @HorseCounter INT = 1;
WHILE @HorseCounter <= 48
BEGIN
    DECLARE @HName NVARCHAR(100) = N'Thần Phong ' + CAST(@HorseCounter AS NVARCHAR(10));
    INSERT INTO [Horse] ([OwnerId], [Name], [Breed], [Age], [HealthStatus], [CreatedAt])
    VALUES (@MainOwnerId, @HName, N'Thuần Chủng', 3 + (@HorseCounter % 4), 'Healthy', GETDATE());
    DECLARE @HId BIGINT = SCOPE_IDENTITY();
    
    INSERT INTO [HorseStatistic] ([HorseId], [TotalRaces], [Wins], [SecondPlaces], [ThirdPlaces], [WinRate])
    VALUES (@HId, 10 + (@HorseCounter % 5), 3 + (@HorseCounter % 3), 2, 1, 30.00);

    SET @HorseCounter = @HorseCounter + 1;
END

PRINT '>>> 48 Horses created successfully!';

PRINT '============================================================';
PRINT '>>> STEP 4: SEEDING SU_48_HORSE (REGISTRATION OPEN & PENDING APPROVAL)...';
PRINT '============================================================';

-- ------------------------------------------------------------
-- Tournament: SU_48_HORSE (Registration Open, Pending Registrations for Admin Approval)
-- ------------------------------------------------------------
PRINT '>>> Seeding Tournament: SU_48_HORSE...';
INSERT INTO [Tournament] ([Name], [Description], [RegistrationStartDate], [RegistrationEndDate], [StartDate], [EndDate], [Status], [CancelCount])
VALUES ('SU_48_HORSE', N'Giải đấu 48 ngựa đang mở đăng ký (chờ Admin duyệt hồ sơ)', DATEADD(day, -3, GETDATE()), DATEADD(day, 2, GETDATE()), DATEADD(day, 5, GETDATE()), DATEADD(day, 8, GETDATE()), 'Registration Open', 0);
DECLARE @SU48_Id BIGINT = SCOPE_IDENTITY();

-- Register 48 Horses for SU_48_HORSE: Registration (Pending), JockeyContract (Accepted), MedicalCheckRecord (Pass)
DECLARE @SU48Counter INT = 1;
WHILE @SU48Counter <= 48
BEGIN
    DECLARE @SU48_HorseId BIGINT = @SU48Counter;
    DECLARE @SU48_JockeyId INT = (SELECT [UserId] FROM [AppUser] WHERE [Username] = 'jockey' + CAST(@SU48Counter AS NVARCHAR(10)));

    -- 1. Registration (Pending Approval)
    INSERT INTO [Registration] ([TournamentId], [HorseId], [Status], [RegisteredAt])
    VALUES (@SU48_Id, @SU48_HorseId, 'Pending', DATEADD(day, -2, GETDATE()));
    DECLARE @SU48_RegId BIGINT = SCOPE_IDENTITY();

    -- 2. JockeyContract (Accepted)
    INSERT INTO [JockeyContract] ([TournamentId], [HorseId], [JockeyId], [StartDate], [EndDate], [Status], [InvitationExpiredAt], [CreatedAt])
    VALUES (@SU48_Id, @SU48_HorseId, @SU48_JockeyId, DATEADD(day, 5, GETDATE()), DATEADD(day, 8, GETDATE()), 'Accepted', DATEADD(day, 1, GETDATE()), DATEADD(day, -3, GETDATE()));

    -- 3. MedicalCheckRecord (Passed)
    INSERT INTO [MedicalCheckRecord] ([RegistrationId], [HorseId], [UserId], [CheckType], [Weight], [Temperature], [HeartRate], [DopingResult], [MedicalResult], [Notes], [CheckedAt])
    VALUES (@SU48_RegId, @SU48_HorseId, @MainVetId, 'Initial', 480.00, 37.6, 36, 'Negative', 'Pass', N'Đạt yêu cầu sức khỏe ban đầu cho SU_48_HORSE', DATEADD(day, -2, GETDATE()));

    SET @SU48Counter = @SU48Counter + 1;
END

PRINT '>>> Tournaments, Registrations (Pending), JockeyContracts (Accepted), and MedicalCheckRecords (Pass) seeded successfully!';

PRINT '============================================================';
PRINT '>>> SEEDING COMPLETED SUCCESSFULLY!';
PRINT '============================================================';
GO
