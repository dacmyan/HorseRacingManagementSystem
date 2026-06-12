USE master;
GO

-- 1. DROP DATABASE CŨ NẾU ĐÃ TỒN TẠI
IF DB_ID(N'HorseRacingManagementSystem') IS NOT NULL
BEGIN
    ALTER DATABASE [HorseRacingManagementSystem] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [HorseRacingManagementSystem];
END;
GO

-- 2. CREATE DATABASE MỚI
CREATE DATABASE [HorseRacingManagementSystem];
GO

USE [HorseRacingManagementSystem];
GO

-- 3. TẠO BẢNG LỊCH SỬ MIGRATION CỦA EF CORE
CREATE TABLE [__EFMigrationsHistory] (
    [MigrationId] nvarchar(150) NOT NULL,
    [ProductVersion] nvarchar(32) NOT NULL,
    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
);
GO

-- 4. TẠO CÁC BẢNG THEO THỨ TỰ THIẾT KẾ SINGULAR (CHƯA THÊM FK RÀNG BUỘC ĐỂ TRÁNH LỖI PHỤ THUỘC CHÉO)

-- Bảng Role
CREATE TABLE [Role] (
    [RoleId] int NOT NULL IDENTITY(1,1),
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY ([RoleId])
);

-- Bảng AppUser
CREATE TABLE [AppUser] (
    [UserId] int NOT NULL IDENTITY(1,1),
    [Username] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [RoleId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AppUser] PRIMARY KEY ([UserId])
);

-- Bảng JockeyProfile
CREATE TABLE [JockeyProfile] (
    [JockeyId] int NOT NULL IDENTITY(1,1),
    [UserId] int NOT NULL,
    [ExperienceYears] int NOT NULL,
    [RankingPoint] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_JockeyProfile] PRIMARY KEY ([JockeyId])
);

-- Bảng RefereeProfile
CREATE TABLE [RefereeProfile] (
    [RefereeId] int NOT NULL IDENTITY(1,1),
    [UserId] int NOT NULL,
    [LicenseNumber] nvarchar(max) NOT NULL,
    [ExperienceYears] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_RefereeProfile] PRIMARY KEY ([RefereeId])
);

-- Bảng Tournament
CREATE TABLE [Tournament] (
    [TournamentId] bigint NOT NULL IDENTITY(1,1),
    [Name] nvarchar(max) NOT NULL,
    [StartDate] datetime2 NULL,
    [EndDate] datetime2 NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Tournament] PRIMARY KEY ([TournamentId])
);

-- Bảng Round
CREATE TABLE [Round] (
    [RoundId] bigint NOT NULL IDENTITY(1,1),
    [TournamentId] bigint NOT NULL,
    [Name] nvarchar(max) NULL,
    [RoundNumber] int NOT NULL,
    [StartDate] datetime2 NULL,
    [EndDate] datetime2 NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Round] PRIMARY KEY ([RoundId])
);

-- Bảng Registration
CREATE TABLE [Registration] (
    [Id] int NOT NULL IDENTITY(1,1),
    [TournamentId] bigint NOT NULL,
    [HorseId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Registration] PRIMARY KEY ([Id])
);

-- Bảng Horse
CREATE TABLE [Horse] (
    [Id] int NOT NULL IDENTITY(1,1),
    [Name] nvarchar(max) NOT NULL,
    [Age] int NOT NULL,
    [Breed] nvarchar(max) NOT NULL,
    [OwnerId] int NOT NULL,
    [Gender] nvarchar(max) NOT NULL DEFAULT N'',
    [HealthStatus] nvarchar(max) NOT NULL DEFAULT N'',
    [RegistrationId] int NULL,
    CONSTRAINT [PK_Horse] PRIMARY KEY ([Id])
);

-- Bảng HorseDocument
CREATE TABLE [HorseDocument] (
    [Id] int NOT NULL IDENTITY(1,1),
    [HorseId] int NOT NULL,
    [DocumentType] nvarchar(max) NOT NULL,
    [DocumentUrl] nvarchar(max) NOT NULL,
    [UploadedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_HorseDocument] PRIMARY KEY ([Id])
);

-- Bảng HorseStatistic
CREATE TABLE [HorseStatistic] (
    [Id] int NOT NULL IDENTITY(1,1),
    [HorseId] int NOT NULL,
    [TotalRaces] int NOT NULL,
    [TotalWins] int NOT NULL,
    [TotalSecondPlaces] int NOT NULL,
    [TotalThirdPlaces] int NOT NULL,
    [AverageSpeed] decimal(18,2) NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_HorseStatistic] PRIMARY KEY ([Id])
);

-- Bảng JockeyContract
CREATE TABLE [JockeyContract] (
    [Id] int NOT NULL IDENTITY(1,1),
    [HorseId] int NOT NULL,
    [OwnerId] int NOT NULL,
    [JockeyId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_JockeyContract] PRIMARY KEY ([Id])
);

-- Bảng Race
CREATE TABLE [Race] (
    [RaceId] bigint NOT NULL IDENTITY(1,1),
    [MaxLanes] int NOT NULL,
    [Name] nvarchar(max) NULL,
    [RaceDate] datetime2 NOT NULL,
    [DistanceMeter] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [RoundId] bigint NOT NULL,
    CONSTRAINT [PK_Race] PRIMARY KEY ([RaceId])
);

-- Bảng RaceEntry
CREATE TABLE [RaceEntry] (
    [Id] int NOT NULL IDENTITY(1,1),
    [RaceId] bigint NOT NULL,
    [HorseId] int NOT NULL,
    [JockeyId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [HorseId1] int NULL,
    CONSTRAINT [PK_RaceEntry] PRIMARY KEY ([Id])
);

-- Bảng RaceResult
CREATE TABLE [RaceResult] (
    [Id] int NOT NULL IDENTITY(1,1),
    [RaceId] bigint NOT NULL,
    [Winner] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_RaceResult] PRIMARY KEY ([Id])
);

-- Bảng Prize
CREATE TABLE [Prize] (
    [Id] int NOT NULL IDENTITY(1,1),
    [TournamentId] bigint NOT NULL,
    [Rank] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [OwnerPercentage] decimal(18,2) NOT NULL,
    [JockeyPercentage] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Prize] PRIMARY KEY ([Id])
);

-- Bảng Wallet
CREATE TABLE [Wallet] (
    [WalletId] int NOT NULL IDENTITY(1,1),
    [UserId] int NOT NULL,
    [Balance] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Wallet] PRIMARY KEY ([WalletId])
);

-- Bảng TournamentPrizePayout
CREATE TABLE [TournamentPrizePayout] (
    [Id] int NOT NULL IDENTITY(1,1),
    [TournamentId] bigint NOT NULL,
    [UserId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TournamentPrizePayout] PRIMARY KEY ([Id])
);

-- Bảng RaceRefereeAssignment
CREATE TABLE [RaceRefereeAssignment] (
    [AssignmentId] bigint NOT NULL IDENTITY(1,1),
    [RaceId] bigint NOT NULL,
    [RefereeId] int NOT NULL,
    [AssignedAt] datetime2 NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_RaceRefereeAssignment] PRIMARY KEY ([AssignmentId])
);

-- Bảng RefereeReport (Bản mới có bổ sung ReportedUserId và ReportedHorseId)
CREATE TABLE [RefereeReport] (
    [Id] int NOT NULL IDENTITY(1,1),
    [RaceId] bigint NOT NULL,
    [RefereeId] int NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ReportedUserId] int NULL,
    [ReportedHorseId] int NULL,
    CONSTRAINT [PK_RefereeReport] PRIMARY KEY ([Id])
);

-- Bảng RaceViolation
CREATE TABLE [RaceViolation] (
    [Id] int NOT NULL IDENTITY(1,1),
    [RaceId] bigint NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Penalty] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_RaceViolation] PRIMARY KEY ([Id])
);

-- Bảng Bet
CREATE TABLE [Bet] (
    [Id] int NOT NULL IDENTITY(1,1),
    [UserId] int NOT NULL,
    [RaceId] bigint NOT NULL,
    [HorseId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Odds] decimal(18,2) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Bet] PRIMARY KEY ([Id])
);

-- Bảng Payout
CREATE TABLE [Payout] (
    [Id] int NOT NULL IDENTITY(1,1),
    [BetId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Payout] PRIMARY KEY ([Id])
);

-- Bảng WalletTransaction
CREATE TABLE [WalletTransaction] (
    [Id] int NOT NULL IDENTITY(1,1),
    [WalletId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_WalletTransaction] PRIMARY KEY ([Id])
);

-- Bảng Notification
CREATE TABLE [Notification] (
    [Id] int NOT NULL IDENTITY(1,1),
    [UserId] int NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Notification] PRIMARY KEY ([Id])
);

-- Bảng Prediction (Bổ trợ)
CREATE TABLE [Prediction] (
    [Id] int NOT NULL IDENTITY(1,1),
    [RaceId] bigint NOT NULL,
    [UserId] int NOT NULL,
    [PredictedWinner] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Prediction] PRIMARY KEY ([Id])
);
GO

-- 5. THIẾT LẬP RÀNG BUỘC KHÓA NGOẠI (FOREIGN KEYS)

-- Bảng AppUser
ALTER TABLE [AppUser] ADD CONSTRAINT [FK_AppUser_Role_RoleId] 
    FOREIGN KEY ([RoleId]) REFERENCES [Role] ([RoleId]) ON DELETE NO ACTION;

-- Bảng JockeyProfile
ALTER TABLE [JockeyProfile] ADD CONSTRAINT [FK_JockeyProfile_AppUser_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

-- Bảng RefereeProfile
ALTER TABLE [RefereeProfile] ADD CONSTRAINT [FK_RefereeProfile_AppUser_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

-- Bảng Wallet
ALTER TABLE [Wallet] ADD CONSTRAINT [FK_Wallet_AppUser_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

-- Bảng Round
ALTER TABLE [Round] ADD CONSTRAINT [FK_Round_Tournament_TournamentId] 
    FOREIGN KEY ([TournamentId]) REFERENCES [Tournament] ([TournamentId]) ON DELETE CASCADE;

-- Bảng Registration
ALTER TABLE [Registration] ADD CONSTRAINT [FK_Registration_Tournament_TournamentId] 
    FOREIGN KEY ([TournamentId]) REFERENCES [Tournament] ([TournamentId]) ON DELETE NO ACTION;
ALTER TABLE [Registration] ADD CONSTRAINT [FK_Registration_Horse_HorseId] 
    FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE NO ACTION;

-- Bảng Horse
ALTER TABLE [Horse] ADD CONSTRAINT [FK_Horse_AppUser_OwnerId] 
    FOREIGN KEY ([OwnerId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;
ALTER TABLE [Horse] ADD CONSTRAINT [FK_Horse_Registration_RegistrationId] 
    FOREIGN KEY ([RegistrationId]) REFERENCES [Registration] ([Id]);

-- Bảng HorseDocument
ALTER TABLE [HorseDocument] ADD CONSTRAINT [FK_HorseDocument_Horse_HorseId] 
    FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE CASCADE;

-- Bảng HorseStatistic
ALTER TABLE [HorseStatistic] ADD CONSTRAINT [FK_HorseStatistic_Horse_HorseId] 
    FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE CASCADE;

-- Bảng JockeyContract
ALTER TABLE [JockeyContract] ADD CONSTRAINT [FK_JockeyContract_Horse_HorseId] 
    FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [JockeyContract] ADD CONSTRAINT [FK_JockeyContract_AppUser_OwnerId] 
    FOREIGN KEY ([OwnerId]) REFERENCES [AppUser] ([UserId]) ON DELETE NO ACTION;
ALTER TABLE [JockeyContract] ADD CONSTRAINT [FK_JockeyContract_AppUser_JockeyId] 
    FOREIGN KEY ([JockeyId]) REFERENCES [AppUser] ([UserId]) ON DELETE NO ACTION;

-- Bảng Race
ALTER TABLE [Race] ADD CONSTRAINT [FK_Race_Round_RoundId] 
    FOREIGN KEY ([RoundId]) REFERENCES [Round] ([RoundId]) ON DELETE CASCADE;

-- Bảng RaceEntry
ALTER TABLE [RaceEntry] ADD CONSTRAINT [FK_RaceEntry_Race_RaceId] 
    FOREIGN KEY ([RaceId]) REFERENCES [Race] ([RaceId]) ON DELETE CASCADE;
ALTER TABLE [RaceEntry] ADD CONSTRAINT [FK_RaceEntry_Horse_HorseId] 
    FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE NO ACTION;
ALTER TABLE [RaceEntry] ADD CONSTRAINT [FK_RaceEntry_AppUser_JockeyId] 
    FOREIGN KEY ([JockeyId]) REFERENCES [AppUser] ([UserId]) ON DELETE NO ACTION;
ALTER TABLE [RaceEntry] ADD CONSTRAINT [FK_RaceEntry_Horse_HorseId1] 
    FOREIGN KEY ([HorseId1]) REFERENCES [Horse] ([Id]);

-- Bảng RaceResult
ALTER TABLE [RaceResult] ADD CONSTRAINT [FK_RaceResult_Race_RaceId] 
    FOREIGN KEY ([RaceId]) REFERENCES [Race] ([RaceId]) ON DELETE CASCADE;

-- Bảng Prize
ALTER TABLE [Prize] ADD CONSTRAINT [FK_Prize_Tournament_TournamentId] 
    FOREIGN KEY ([TournamentId]) REFERENCES [Tournament] ([TournamentId]) ON DELETE CASCADE;

-- Bảng TournamentPrizePayout
ALTER TABLE [TournamentPrizePayout] ADD CONSTRAINT [FK_TournamentPrizePayout_Tournament_TournamentId] 
    FOREIGN KEY ([TournamentId]) REFERENCES [Tournament] ([TournamentId]) ON DELETE CASCADE;
ALTER TABLE [TournamentPrizePayout] ADD CONSTRAINT [FK_TournamentPrizePayout_AppUser_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

-- Bảng RaceRefereeAssignment
ALTER TABLE [RaceRefereeAssignment] ADD CONSTRAINT [FK_RaceRefereeAssignment_Race_RaceId] 
    FOREIGN KEY ([RaceId]) REFERENCES [Race] ([RaceId]) ON DELETE CASCADE;
ALTER TABLE [RaceRefereeAssignment] ADD CONSTRAINT [FK_RaceRefereeAssignment_RefereeProfile_RefereeId] 
    FOREIGN KEY ([RefereeId]) REFERENCES [RefereeProfile] ([RefereeId]) ON DELETE NO ACTION;

-- Bảng RefereeReport
ALTER TABLE [RefereeReport] ADD CONSTRAINT [FK_RefereeReport_Race_RaceId] 
    FOREIGN KEY ([RaceId]) REFERENCES [Race] ([RaceId]) ON DELETE CASCADE;
ALTER TABLE [RefereeReport] ADD CONSTRAINT [FK_RefereeReport_RefereeProfile_RefereeId] 
    FOREIGN KEY ([RefereeId]) REFERENCES [RefereeProfile] ([RefereeId]) ON DELETE NO ACTION;
ALTER TABLE [RefereeReport] ADD CONSTRAINT [FK_RefereeReport_AppUser] 
    FOREIGN KEY ([ReportedUserId]) REFERENCES [AppUser] ([UserId]) ON DELETE NO ACTION;
ALTER TABLE [RefereeReport] ADD CONSTRAINT [FK_RefereeReport_Horse] 
    FOREIGN KEY ([ReportedHorseId]) REFERENCES [Horse] ([Id]) ON DELETE NO ACTION;

-- Bảng RaceViolation
ALTER TABLE [RaceViolation] ADD CONSTRAINT [FK_RaceViolation_Race_RaceId] 
    FOREIGN KEY ([RaceId]) REFERENCES [Race] ([RaceId]) ON DELETE CASCADE;

-- Bảng Bet
ALTER TABLE [Bet] ADD CONSTRAINT [FK_Bet_AppUser_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;
ALTER TABLE [Bet] ADD CONSTRAINT [FK_Bet_Race_RaceId] 
    FOREIGN KEY ([RaceId]) REFERENCES [Race] ([RaceId]) ON DELETE NO ACTION;
ALTER TABLE [Bet] ADD CONSTRAINT [FK_Bet_Horse_HorseId] 
    FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE NO ACTION;

-- Bảng Payout
ALTER TABLE [Payout] ADD CONSTRAINT [FK_Payout_Bet_BetId] 
    FOREIGN KEY ([BetId]) REFERENCES [Bet] ([Id]) ON DELETE CASCADE;

-- Bảng WalletTransaction
ALTER TABLE [WalletTransaction] ADD CONSTRAINT [FK_WalletTransaction_Wallet_WalletId] 
    FOREIGN KEY ([WalletId]) REFERENCES [Wallet] ([WalletId]) ON DELETE CASCADE;

-- Bảng Notification
ALTER TABLE [Notification] ADD CONSTRAINT [FK_Notification_AppUser_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;
GO

-- 6. TẠO CHỈ MỤC (INDEXES)

CREATE INDEX [IX_Horse_RegistrationId] ON [Horse] ([RegistrationId]);
CREATE INDEX [IX_Horse_OwnerId] ON [Horse] ([OwnerId]);
CREATE UNIQUE INDEX [IX_JockeyProfile_UserId] ON [JockeyProfile] ([UserId]);
CREATE INDEX [IX_RaceEntry_HorseId] ON [RaceEntry] ([HorseId]);
CREATE INDEX [IX_RaceEntry_JockeyId] ON [RaceEntry] ([JockeyId]);
CREATE INDEX [IX_RaceEntry_RaceId] ON [RaceEntry] ([RaceId]);
CREATE INDEX [IX_RaceEntry_HorseId1] ON [RaceEntry] ([HorseId1]);
CREATE INDEX [IX_Race_RoundId] ON [Race] ([RoundId]);
CREATE UNIQUE INDEX [IX_RefereeProfile_UserId] ON [RefereeProfile] ([UserId]);
CREATE INDEX [IX_WalletTransaction_WalletId] ON [WalletTransaction] ([WalletId]);
CREATE INDEX [IX_AppUser_RoleId] ON [AppUser] ([RoleId]);
CREATE INDEX [IX_RaceViolation_RaceId] ON [RaceViolation] ([RaceId]);
CREATE UNIQUE INDEX [IX_Wallet_UserId] ON [Wallet] ([UserId]);
CREATE INDEX [IX_Bet_HorseId] ON [Bet] ([HorseId]);
CREATE INDEX [IX_Bet_RaceId] ON [Bet] ([RaceId]);
CREATE INDEX [IX_Bet_UserId] ON [Bet] ([UserId]);
CREATE INDEX [IX_Notification_UserId] ON [Notification] ([UserId]);
CREATE INDEX [IX_Payout_BetId] ON [Payout] ([BetId]);
CREATE INDEX [IX_Prize_TournamentId] ON [Prize] ([TournamentId]);
CREATE INDEX [IX_TournamentPrizePayout_TournamentId] ON [TournamentPrizePayout] ([TournamentId]);
CREATE INDEX [IX_TournamentPrizePayout_UserId] ON [TournamentPrizePayout] ([UserId]);
CREATE INDEX [IX_RaceRefereeAssignment_RaceId] ON [RaceRefereeAssignment] ([RaceId]);
CREATE INDEX [IX_RaceRefereeAssignment_RefereeId] ON [RaceRefereeAssignment] ([RefereeId]);
CREATE INDEX [IX_Round_TournamentId] ON [Round] ([TournamentId]);
CREATE INDEX [IX_HorseDocument_HorseId] ON [HorseDocument] ([HorseId]);
CREATE UNIQUE INDEX [IX_HorseStatistic_HorseId] ON [HorseStatistic] ([HorseId]);
CREATE INDEX [IX_JockeyContract_HorseId] ON [JockeyContract] ([HorseId]);
CREATE INDEX [IX_JockeyContract_JockeyId] ON [JockeyContract] ([JockeyId]);
CREATE INDEX [IX_JockeyContract_OwnerId] ON [JockeyContract] ([OwnerId]);
CREATE INDEX [IX_Registration_HorseId] ON [Registration] ([HorseId]);
CREATE INDEX [IX_Registration_TournamentId] ON [Registration] ([TournamentId]);
CREATE INDEX [IX_RefereeReport_RaceId] ON [RefereeReport] ([RaceId]);
CREATE INDEX [IX_RefereeReport_RefereeId] ON [RefereeReport] ([RefereeId]);
GO

-- 7. SEED DỮ LIỆU BAN ĐẦU (ROLES, USERS, PROFILES, WALLETS)

-- Seed Roles
SET IDENTITY_INSERT [Role] ON;
INSERT INTO [Role] ([RoleId], [Name])
VALUES 
(1, N'Admin'),
(2, N'HorseOwner'),
(3, N'Jockey'),
(4, N'Referee'),
(5, N'Spectator');
SET IDENTITY_INSERT [Role] OFF;

-- Seed Users (Băm mật khẩu '123456' theo cấu trúc Identity Password Hasher)
SET IDENTITY_INSERT [AppUser] ON;
INSERT INTO [AppUser] ([UserId], [CreatedAt], [Email], [FullName], [PasswordHash], [RoleId], [Status], [Username])
VALUES 
(1, '2026-06-09T00:00:00.0000000Z', N'admin@gmail.com', N'Admin', N'AQAAAAIAAYagAAAAEDQl5GtU6lXMcfTRWSHnVz0AYO/DcesuZdc26L/lMBXERd4y/Ry6KFBnnz3OcVLaiA==', 1, N'Active', N'admin'),
(2, '2026-06-09T00:00:00.0000000Z', N'owner@gmail.com', N'HorseOwner', N'AQAAAAIAAYagAAAAEIU26C1yHTr+reVJwGZJlfXXlzLjVh3ejJo2RFDSgGRU3CQn2cp7nJJIfJ/m6XAm8Q==', 2, N'Active', N'owner'),
(3, '2026-06-09T00:00:00.0000000Z', N'jockey@gmail.com', N'Jockey', N'AQAAAAIAAYagAAAAEETNsom+QeZLwGVXPatXiCXnefira62DVHwdLXgHMsgdrwu7Xy3znwO8r72lMU5Mog==', 3, N'Active', N'jockey'),
(4, '2026-06-09T00:00:00.0000000Z', N'referee@gmail.com', N'Referee', N'AQAAAAIAAYagAAAAEN/0ABNONq8lx6xDeeREIStGpg1ENtlTUY2BLUaNLzyO6hnHvqnoA4Qd+WqRgJadgg==', 4, N'Active', N'referee'),
(5, '2026-06-09T00:00:00.0000000Z', N'spectator@gmail.com', N'Spectator', N'AQAAAAIAAYagAAAAEJKV8UzjVkRV+m8NEvHJi0leRC9j/XUkH19a15TyFSoRWPiDhJfEqjAl28C89Ikcbg==', 5, N'Active', N'spectator');
SET IDENTITY_INSERT [AppUser] OFF;

-- Seed Jockey Profile
SET IDENTITY_INSERT [JockeyProfile] ON;
INSERT INTO [JockeyProfile] ([JockeyId], [ExperienceYears], [RankingPoint], [Status], [UserId])
VALUES (1, 3, 100, N'Active', 3);
SET IDENTITY_INSERT [JockeyProfile] OFF;

-- Seed Referee Profile
SET IDENTITY_INSERT [RefereeProfile] ON;
INSERT INTO [RefereeProfile] ([RefereeId], [ExperienceYears], [LicenseNumber], [Status], [UserId])
VALUES (1, 5, N'LIC-REF-001', N'Active', 4);
SET IDENTITY_INSERT [RefereeProfile] OFF;

-- Seed Wallet cho Spectator
SET IDENTITY_INSERT [Wallet] ON;
INSERT INTO [Wallet] ([WalletId], [Balance], [UserId])
VALUES (1, 0.0, 5);
SET IDENTITY_INSERT [Wallet] OFF;
GO

-- 8. GHI BẢN GHI LỊCH SỬ EF MIGRATIONS (ĐỂ EF CORE KHÔNG CHẠY LẠI CÁC FILE MIGRATION LỊCH SỬ)
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES 
(N'20260609193021_InitialCreate', N'10.0.9'),
(N'20260610083404_AddBetPayoutPrizeNotification', N'10.0.9'),
(N'20260611024654_UpdateTournamentRacingEntities', N'10.0.9'),
(N'20260611035623_AddHorseDocsAndStats', N'10.0.9'),
(N'20260612024026_FixSingularTableMapping', N'10.0.9');
GO

PRINT 'DATABASE HorseRacingManagementSystem HAS BEEN SUCCESSFULLY RECREATED WITH CLEAN SINGULAR SCHEMA!';
