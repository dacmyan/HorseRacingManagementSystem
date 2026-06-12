IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Predictions] (
    [Id] int NOT NULL IDENTITY,
    [RaceId] int NOT NULL,
    [UserId] int NOT NULL,
    [PredictedWinner] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Predictions] PRIMARY KEY ([Id])
);

CREATE TABLE [RaceResults] (
    [Id] int NOT NULL IDENTITY,
    [RaceId] int NOT NULL,
    [Winner] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_RaceResults] PRIMARY KEY ([Id])
);

CREATE TABLE [Roles] (
    [RoleId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleId])
);

CREATE TABLE [Tournaments] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Location] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Tournaments] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [RoleId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId]) ON DELETE NO ACTION
);

CREATE TABLE [Races] (
    [Id] int NOT NULL IDENTITY,
    [TournamentId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [ScheduledTime] datetime2 NOT NULL,
    [Distance] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Races] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Races_Tournaments_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Horses] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Age] int NOT NULL,
    [Breed] nvarchar(max) NOT NULL,
    [OwnerId] int NOT NULL,
    CONSTRAINT [PK_Horses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Horses_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [JockeyProfiles] (
    [JockeyId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ExperienceYears] int NOT NULL,
    [RankingPoint] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_JockeyProfiles] PRIMARY KEY ([JockeyId]),
    CONSTRAINT [FK_JockeyProfiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [RefereeProfiles] (
    [RefereeId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [LicenseNumber] nvarchar(max) NOT NULL,
    [ExperienceYears] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_RefereeProfiles] PRIMARY KEY ([RefereeId]),
    CONSTRAINT [FK_RefereeProfiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Wallets] (
    [WalletId] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Balance] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Wallets] PRIMARY KEY ([WalletId]),
    CONSTRAINT [FK_Wallets_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Violations] (
    [Id] int NOT NULL IDENTITY,
    [RaceId] int NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Penalty] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Violations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Violations_Races_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Races] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RaceEntries] (
    [Id] int NOT NULL IDENTITY,
    [RaceId] int NOT NULL,
    [HorseId] int NOT NULL,
    [JockeyId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_RaceEntries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RaceEntries_Horses_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_RaceEntries_Races_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Races] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RaceEntries_Users_JockeyId] FOREIGN KEY ([JockeyId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [Transactions] (
    [Id] int NOT NULL IDENTITY,
    [WalletId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transactions_Wallets_WalletId] FOREIGN KEY ([WalletId]) REFERENCES [Wallets] ([WalletId]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([RoleId], [Name])
VALUES (1, N'Admin'),
(2, N'HorseOwner'),
(3, N'Jockey'),
(4, N'Referee'),
(5, N'Spectator');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'Name') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'CreatedAt', N'Email', N'FullName', N'PasswordHash', N'RoleId', N'Status', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [CreatedAt], [Email], [FullName], [PasswordHash], [RoleId], [Status], [Username])
VALUES (1, '2026-06-09T00:00:00.0000000Z', N'admin@gmail.com', N'Admin', N'AQAAAAIAAYagAAAAEBHZnY+70v3ZN7pUL3rtSMWpbtyCbJEIdtaqX7t/btFc6c5ZWnNWY2TpZIPobFtCjg==', 1, N'Active', N'admin'),
(2, '2026-06-09T00:00:00.0000000Z', N'owner@gmail.com', N'HorseOwner', N'AQAAAAIAAYagAAAAECSVuDp0YQF9fcZ+QUrYzV2eEH+If+4JlZTibfpdFZpymN2n4nZk6mu4wxjLdrWShA==', 2, N'Active', N'owner'),
(3, '2026-06-09T00:00:00.0000000Z', N'jockey@gmail.com', N'Jockey', N'AQAAAAIAAYagAAAAELr6IVoiJvMKODJp6FQklyjvCjfGMFKeLmLMPpd9NbSUNBZttgbx3qxGH0A9jPa87A==', 3, N'Active', N'jockey'),
(4, '2026-06-09T00:00:00.0000000Z', N'referee@gmail.com', N'Referee', N'AQAAAAIAAYagAAAAEN4xH2tzD2r22z1nWMBfuUtr8fnHL+T9xpyRC+Olerw9nVEHE4CEHWAaIgvLYaxo8Q==', 4, N'Active', N'referee'),
(5, '2026-06-09T00:00:00.0000000Z', N'spectator@gmail.com', N'Spectator', N'AQAAAAIAAYagAAAAEJlcwSBzeiueDuCzXyYKX0O5EmlJ1gwYhh7ApgzautxHMfpNfMST9jX6Q8KOJbzoKg==', 5, N'Active', N'spectator');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'CreatedAt', N'Email', N'FullName', N'PasswordHash', N'RoleId', N'Status', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'JockeyId', N'ExperienceYears', N'RankingPoint', N'Status', N'UserId') AND [object_id] = OBJECT_ID(N'[JockeyProfiles]'))
    SET IDENTITY_INSERT [JockeyProfiles] ON;
INSERT INTO [JockeyProfiles] ([JockeyId], [ExperienceYears], [RankingPoint], [Status], [UserId])
VALUES (1, 3, 100, N'Active', 3);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'JockeyId', N'ExperienceYears', N'RankingPoint', N'Status', N'UserId') AND [object_id] = OBJECT_ID(N'[JockeyProfiles]'))
    SET IDENTITY_INSERT [JockeyProfiles] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RefereeId', N'ExperienceYears', N'LicenseNumber', N'Status', N'UserId') AND [object_id] = OBJECT_ID(N'[RefereeProfiles]'))
    SET IDENTITY_INSERT [RefereeProfiles] ON;
INSERT INTO [RefereeProfiles] ([RefereeId], [ExperienceYears], [LicenseNumber], [Status], [UserId])
VALUES (1, 5, N'LIC-REF-001', N'Active', 4);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RefereeId', N'ExperienceYears', N'LicenseNumber', N'Status', N'UserId') AND [object_id] = OBJECT_ID(N'[RefereeProfiles]'))
    SET IDENTITY_INSERT [RefereeProfiles] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'WalletId', N'Balance', N'UserId') AND [object_id] = OBJECT_ID(N'[Wallets]'))
    SET IDENTITY_INSERT [Wallets] ON;
INSERT INTO [Wallets] ([WalletId], [Balance], [UserId])
VALUES (1, 0.0, 5);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'WalletId', N'Balance', N'UserId') AND [object_id] = OBJECT_ID(N'[Wallets]'))
    SET IDENTITY_INSERT [Wallets] OFF;

CREATE INDEX [IX_Horses_OwnerId] ON [Horses] ([OwnerId]);

CREATE UNIQUE INDEX [IX_JockeyProfiles_UserId] ON [JockeyProfiles] ([UserId]);

CREATE INDEX [IX_RaceEntries_HorseId] ON [RaceEntries] ([HorseId]);

CREATE INDEX [IX_RaceEntries_JockeyId] ON [RaceEntries] ([JockeyId]);

CREATE INDEX [IX_RaceEntries_RaceId] ON [RaceEntries] ([RaceId]);

CREATE INDEX [IX_Races_TournamentId] ON [Races] ([TournamentId]);

CREATE UNIQUE INDEX [IX_RefereeProfiles_UserId] ON [RefereeProfiles] ([UserId]);

CREATE INDEX [IX_Transactions_WalletId] ON [Transactions] ([WalletId]);

CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);

CREATE INDEX [IX_Violations_RaceId] ON [Violations] ([RaceId]);

CREATE UNIQUE INDEX [IX_Wallets_UserId] ON [Wallets] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260609193021_InitialCreate', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [Bets] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [RaceId] int NOT NULL,
    [HorseId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Odds] decimal(18,2) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Bets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Bets_Horses_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Bets_Races_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Races] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Bets_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Notifications] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [IsRead] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Prizes] (
    [Id] int NOT NULL IDENTITY,
    [TournamentId] int NOT NULL,
    [Rank] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [OwnerPercentage] decimal(18,2) NOT NULL,
    [JockeyPercentage] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Prizes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Prizes_Tournaments_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [TournamentPrizePayouts] (
    [Id] int NOT NULL IDENTITY,
    [TournamentId] int NOT NULL,
    [UserId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TournamentPrizePayouts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TournamentPrizePayouts_Tournaments_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TournamentPrizePayouts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE TABLE [Payouts] (
    [Id] int NOT NULL IDENTITY,
    [BetId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Payouts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Payouts_Bets_BetId] FOREIGN KEY ([BetId]) REFERENCES [Bets] ([Id]) ON DELETE CASCADE
);

UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEATglvte26OBE9xd/qWKA6OIR0/FpdZGiWJpiSyoEimQFkrHsR1R2M9MuSTW0Z5S3w=='
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEHQ3c4q/N3Z1wWBLk2KhVZK6kobOEpaXNqph7uZ5sXsnwVsVzu1GY/w4SyG4TgJQUw=='
WHERE [UserId] = 2;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEAvcQRwm2FTrZSZ6PXIYZx8yRipbwVPIk/6NN0CxB76y3JKs+BbWlbYzphyKRlhOMg=='
WHERE [UserId] = 3;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEOESdzbETNWYcgmcIuib6/nTOBVlZPEkPqRyxj4hn0J29FPBj9nQVNRUpS4oF5x/vg=='
WHERE [UserId] = 4;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEG1NuO/n0jgVTwQoetXiTo1eLV9QRv1venSg8T730Bd9BJsA728X77MZDlX7+Uq9pQ=='
WHERE [UserId] = 5;
SELECT @@ROWCOUNT;


CREATE INDEX [IX_Bets_HorseId] ON [Bets] ([HorseId]);

CREATE INDEX [IX_Bets_RaceId] ON [Bets] ([RaceId]);

CREATE INDEX [IX_Bets_UserId] ON [Bets] ([UserId]);

CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);

CREATE INDEX [IX_Payouts_BetId] ON [Payouts] ([BetId]);

CREATE INDEX [IX_Prizes_TournamentId] ON [Prizes] ([TournamentId]);

CREATE INDEX [IX_TournamentPrizePayouts_TournamentId] ON [TournamentPrizePayouts] ([TournamentId]);

CREATE INDEX [IX_TournamentPrizePayouts_UserId] ON [TournamentPrizePayouts] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260610083404_AddBetPayoutPrizeNotification', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Bets] DROP CONSTRAINT [FK_Bets_Races_RaceId];

ALTER TABLE [Prizes] DROP CONSTRAINT [FK_Prizes_Tournaments_TournamentId];

ALTER TABLE [RaceEntries] DROP CONSTRAINT [FK_RaceEntries_Races_RaceId];

ALTER TABLE [Races] DROP CONSTRAINT [FK_Races_Tournaments_TournamentId];

ALTER TABLE [TournamentPrizePayouts] DROP CONSTRAINT [FK_TournamentPrizePayouts_Tournaments_TournamentId];

ALTER TABLE [Violations] DROP CONSTRAINT [FK_Violations_Races_RaceId];

ALTER TABLE [Tournaments] DROP CONSTRAINT [PK_Tournaments];

ALTER TABLE [Races] DROP CONSTRAINT [PK_Races];

DROP INDEX [IX_Races_TournamentId] ON [Races];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tournaments]') AND [c].[name] = N'Id');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Tournaments] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Tournaments] DROP COLUMN [Id];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tournaments]') AND [c].[name] = N'Location');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Tournaments] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Tournaments] DROP COLUMN [Location];

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Races]') AND [c].[name] = N'Id');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Races] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Races] DROP COLUMN [Id];

EXEC sp_rename N'[Races].[TournamentId]', N'MaxLanes', 'COLUMN';

EXEC sp_rename N'[Races].[ScheduledTime]', N'RaceDate', 'COLUMN';

EXEC sp_rename N'[Races].[Distance]', N'DistanceMeter', 'COLUMN';

DROP INDEX [IX_Violations_RaceId] ON [Violations];
DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Violations]') AND [c].[name] = N'RaceId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Violations] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [Violations] ALTER COLUMN [RaceId] bigint NOT NULL;
CREATE INDEX [IX_Violations_RaceId] ON [Violations] ([RaceId]);

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tournaments]') AND [c].[name] = N'StartDate');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Tournaments] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [Tournaments] ALTER COLUMN [StartDate] datetime2 NULL;

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tournaments]') AND [c].[name] = N'EndDate');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Tournaments] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [Tournaments] ALTER COLUMN [EndDate] datetime2 NULL;

ALTER TABLE [Tournaments] ADD [TournamentId] bigint NOT NULL IDENTITY;

DROP INDEX [IX_TournamentPrizePayouts_TournamentId] ON [TournamentPrizePayouts];
DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TournamentPrizePayouts]') AND [c].[name] = N'TournamentId');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [TournamentPrizePayouts] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [TournamentPrizePayouts] ALTER COLUMN [TournamentId] bigint NOT NULL;
CREATE INDEX [IX_TournamentPrizePayouts_TournamentId] ON [TournamentPrizePayouts] ([TournamentId]);

DECLARE @var7 nvarchar(max);
SELECT @var7 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Races]') AND [c].[name] = N'Name');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Races] DROP CONSTRAINT ' + @var7 + ';');
ALTER TABLE [Races] ALTER COLUMN [Name] nvarchar(max) NULL;

ALTER TABLE [Races] ADD [RaceId] bigint NOT NULL IDENTITY;

ALTER TABLE [Races] ADD [RoundId] bigint NOT NULL DEFAULT CAST(0 AS bigint);

DECLARE @var8 nvarchar(max);
SELECT @var8 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RaceResults]') AND [c].[name] = N'RaceId');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [RaceResults] DROP CONSTRAINT ' + @var8 + ';');
ALTER TABLE [RaceResults] ALTER COLUMN [RaceId] bigint NOT NULL;

DROP INDEX [IX_RaceEntries_RaceId] ON [RaceEntries];
DECLARE @var9 nvarchar(max);
SELECT @var9 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RaceEntries]') AND [c].[name] = N'RaceId');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [RaceEntries] DROP CONSTRAINT ' + @var9 + ';');
ALTER TABLE [RaceEntries] ALTER COLUMN [RaceId] bigint NOT NULL;
CREATE INDEX [IX_RaceEntries_RaceId] ON [RaceEntries] ([RaceId]);

DROP INDEX [IX_Prizes_TournamentId] ON [Prizes];
DECLARE @var10 nvarchar(max);
SELECT @var10 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Prizes]') AND [c].[name] = N'TournamentId');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Prizes] DROP CONSTRAINT ' + @var10 + ';');
ALTER TABLE [Prizes] ALTER COLUMN [TournamentId] bigint NOT NULL;
CREATE INDEX [IX_Prizes_TournamentId] ON [Prizes] ([TournamentId]);

DECLARE @var11 nvarchar(max);
SELECT @var11 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Predictions]') AND [c].[name] = N'RaceId');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Predictions] DROP CONSTRAINT ' + @var11 + ';');
ALTER TABLE [Predictions] ALTER COLUMN [RaceId] bigint NOT NULL;

DROP INDEX [IX_Bets_RaceId] ON [Bets];
DECLARE @var12 nvarchar(max);
SELECT @var12 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bets]') AND [c].[name] = N'RaceId');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Bets] DROP CONSTRAINT ' + @var12 + ';');
ALTER TABLE [Bets] ALTER COLUMN [RaceId] bigint NOT NULL;
CREATE INDEX [IX_Bets_RaceId] ON [Bets] ([RaceId]);

ALTER TABLE [Tournaments] ADD CONSTRAINT [PK_Tournaments] PRIMARY KEY ([TournamentId]);

ALTER TABLE [Races] ADD CONSTRAINT [PK_Races] PRIMARY KEY ([RaceId]);

CREATE TABLE [RaceRefereeAssignments] (
    [AssignmentId] bigint NOT NULL IDENTITY,
    [RaceId] bigint NOT NULL,
    [RefereeId] int NOT NULL,
    [AssignedAt] datetime2 NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_RaceRefereeAssignments] PRIMARY KEY ([AssignmentId]),
    CONSTRAINT [FK_RaceRefereeAssignments_Races_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Races] ([RaceId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RaceRefereeAssignments_RefereeProfiles_RefereeId] FOREIGN KEY ([RefereeId]) REFERENCES [RefereeProfiles] ([RefereeId]) ON DELETE NO ACTION
);

CREATE TABLE [Rounds] (
    [RoundId] bigint NOT NULL IDENTITY,
    [TournamentId] bigint NOT NULL,
    [Name] nvarchar(max) NULL,
    [RoundNumber] int NOT NULL,
    [StartDate] datetime2 NULL,
    [EndDate] datetime2 NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Rounds] PRIMARY KEY ([RoundId]),
    CONSTRAINT [FK_Rounds_Tournaments_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments] ([TournamentId]) ON DELETE CASCADE
);

UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEOCrttfaCVXXKv/Wy+gaYr7tVyJOwyV+HSs32fMKAssoSs0sVJHPz/k+M+vWi3bKQQ=='
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEHwZ6VVoS037mIguxw+jqz7ak90xeEVNXLgyh4UPKuJyhPLZptFsDBXqipJGT6gLOw=='
WHERE [UserId] = 2;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEGghxh7vAiSQ5BaGlvd975fc4KUzEYzwSkskU45lcryGMmmSQ+tiPQMMJ/lvm769yw=='
WHERE [UserId] = 3;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEKpXVcz+pbPeqnCDD6zooCAAG41O/p0QbcisCuTVKBY3NuuKEbxrFad4Wf+2tURsMw=='
WHERE [UserId] = 4;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEI7KFJe27AeVB7P+vqMsLmvFp3PBKSTno9RVSx+LT3nytSCHRFwDx8fvBP06Rx6liA=='
WHERE [UserId] = 5;
SELECT @@ROWCOUNT;


CREATE INDEX [IX_Races_RoundId] ON [Races] ([RoundId]);

CREATE INDEX [IX_RaceRefereeAssignments_RaceId] ON [RaceRefereeAssignments] ([RaceId]);

CREATE INDEX [IX_RaceRefereeAssignments_RefereeId] ON [RaceRefereeAssignments] ([RefereeId]);

CREATE INDEX [IX_Rounds_TournamentId] ON [Rounds] ([TournamentId]);

ALTER TABLE [Bets] ADD CONSTRAINT [FK_Bets_Races_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Races] ([RaceId]) ON DELETE NO ACTION;

ALTER TABLE [Prizes] ADD CONSTRAINT [FK_Prizes_Tournaments_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments] ([TournamentId]) ON DELETE CASCADE;

ALTER TABLE [RaceEntries] ADD CONSTRAINT [FK_RaceEntries_Races_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Races] ([RaceId]) ON DELETE CASCADE;

ALTER TABLE [Races] ADD CONSTRAINT [FK_Races_Rounds_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Rounds] ([RoundId]) ON DELETE CASCADE;

ALTER TABLE [TournamentPrizePayouts] ADD CONSTRAINT [FK_TournamentPrizePayouts_Tournaments_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments] ([TournamentId]) ON DELETE CASCADE;

ALTER TABLE [Violations] ADD CONSTRAINT [FK_Violations_Races_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Races] ([RaceId]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260611024654_UpdateTournamentRacingEntities', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [RaceEntries] ADD [HorseId1] int NULL;

ALTER TABLE [Horses] ADD [Gender] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Horses] ADD [HealthStatus] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Horses] ADD [RegistrationId] int NULL;

CREATE TABLE [HorseDocuments] (
    [Id] int NOT NULL IDENTITY,
    [HorseId] int NOT NULL,
    [DocumentType] nvarchar(max) NOT NULL,
    [DocumentUrl] nvarchar(max) NOT NULL,
    [UploadedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_HorseDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HorseDocuments_Horses_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horses] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [HorseStatistics] (
    [Id] int NOT NULL IDENTITY,
    [HorseId] int NOT NULL,
    [TotalRaces] int NOT NULL,
    [TotalWins] int NOT NULL,
    [TotalSecondPlaces] int NOT NULL,
    [TotalThirdPlaces] int NOT NULL,
    [AverageSpeed] decimal(18,2) NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_HorseStatistics] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HorseStatistics_Horses_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horses] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [JockeyContracts] (
    [Id] int NOT NULL IDENTITY,
    [HorseId] int NOT NULL,
    [OwnerId] int NOT NULL,
    [JockeyId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_JockeyContracts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JockeyContracts_Horses_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_JockeyContracts_Users_JockeyId] FOREIGN KEY ([JockeyId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_JockeyContracts_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
);

CREATE TABLE [Registrations] (
    [Id] int NOT NULL IDENTITY,
    [TournamentId] int NOT NULL,
    [HorseId] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Registrations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Registrations_Horses_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Registrations_Tournaments_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournaments] ([Id]) ON DELETE NO ACTION
);

UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEBJni0PXeYtMwPxuCquVlJp/vfZ3T5NnXbL4ITA+wICO09HwDpr8wKiZe/SHw7zHkg=='
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEI/YtzgdjI6CI7pmJwDWxYUsgvNE0VR1INfQBA/HJrBpt4H/1jRb9TsnuI3ay69gdQ=='
WHERE [UserId] = 2;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEFzjyabBrnWWObzTRPf2QN1AOsPQA+iBv+MMBSsnI/l9WZm49fn2/+X0qHIelygN0w=='
WHERE [UserId] = 3;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEB8SFpN5wOe/nlVsq/Su4b3pvp8aPd1qHOdXKQWPj+98Ca9A9W3ieFp9C05tYbk3Gg=='
WHERE [UserId] = 4;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEBhri9xP2iah7DDqBdF6EqjQJdezvKu5jRXQ5fSaSCvMZxQ2E4ErUcb1W8vQRsSTJQ=='
WHERE [UserId] = 5;
SELECT @@ROWCOUNT;


CREATE INDEX [IX_RaceEntries_HorseId1] ON [RaceEntries] ([HorseId1]);

CREATE INDEX [IX_Horses_RegistrationId] ON [Horses] ([RegistrationId]);

CREATE INDEX [IX_HorseDocuments_HorseId] ON [HorseDocuments] ([HorseId]);

CREATE UNIQUE INDEX [IX_HorseStatistics_HorseId] ON [HorseStatistics] ([HorseId]);

CREATE INDEX [IX_JockeyContracts_HorseId] ON [JockeyContracts] ([HorseId]);

CREATE INDEX [IX_JockeyContracts_JockeyId] ON [JockeyContracts] ([JockeyId]);

CREATE INDEX [IX_JockeyContracts_OwnerId] ON [JockeyContracts] ([OwnerId]);

CREATE INDEX [IX_Registrations_HorseId] ON [Registrations] ([HorseId]);

CREATE INDEX [IX_Registrations_TournamentId] ON [Registrations] ([TournamentId]);

ALTER TABLE [Horses] ADD CONSTRAINT [FK_Horses_Registrations_RegistrationId] FOREIGN KEY ([RegistrationId]) REFERENCES [Registrations] ([Id]);

ALTER TABLE [RaceEntries] ADD CONSTRAINT [FK_RaceEntries_Horses_HorseId1] FOREIGN KEY ([HorseId1]) REFERENCES [Horses] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260611035623_AddHorseDocsAndStats', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Bets] DROP CONSTRAINT [FK_Bets_Horses_HorseId];

ALTER TABLE [Bets] DROP CONSTRAINT [FK_Bets_Races_RaceId];

ALTER TABLE [Bets] DROP CONSTRAINT [FK_Bets_Users_UserId];

ALTER TABLE [HorseDocuments] DROP CONSTRAINT [FK_HorseDocuments_Horses_HorseId];

ALTER TABLE [Horses] DROP CONSTRAINT [FK_Horses_Registrations_RegistrationId];

ALTER TABLE [Horses] DROP CONSTRAINT [FK_Horses_Users_OwnerId];

ALTER TABLE [HorseStatistics] DROP CONSTRAINT [FK_HorseStatistics_Horses_HorseId];

ALTER TABLE [JockeyContracts] DROP CONSTRAINT [FK_JockeyContracts_Horses_HorseId];

ALTER TABLE [JockeyContracts] DROP CONSTRAINT [FK_JockeyContracts_Users_JockeyId];

ALTER TABLE [JockeyContracts] DROP CONSTRAINT [FK_JockeyContracts_Users_OwnerId];

ALTER TABLE [JockeyProfiles] DROP CONSTRAINT [FK_JockeyProfiles_Users_UserId];

ALTER TABLE [Notifications] DROP CONSTRAINT [FK_Notifications_Users_UserId];

ALTER TABLE [Payouts] DROP CONSTRAINT [FK_Payouts_Bets_BetId];

ALTER TABLE [Prizes] DROP CONSTRAINT [FK_Prizes_Tournaments_TournamentId];

ALTER TABLE [RaceEntries] DROP CONSTRAINT [FK_RaceEntries_Horses_HorseId];

ALTER TABLE [RaceEntries] DROP CONSTRAINT [FK_RaceEntries_Horses_HorseId1];

ALTER TABLE [RaceEntries] DROP CONSTRAINT [FK_RaceEntries_Races_RaceId];

ALTER TABLE [RaceEntries] DROP CONSTRAINT [FK_RaceEntries_Users_JockeyId];

ALTER TABLE [RaceRefereeAssignments] DROP CONSTRAINT [FK_RaceRefereeAssignments_Races_RaceId];

ALTER TABLE [RaceRefereeAssignments] DROP CONSTRAINT [FK_RaceRefereeAssignments_RefereeProfiles_RefereeId];

ALTER TABLE [Races] DROP CONSTRAINT [FK_Races_Rounds_RoundId];

ALTER TABLE [RefereeProfiles] DROP CONSTRAINT [FK_RefereeProfiles_Users_UserId];

ALTER TABLE [Registrations] DROP CONSTRAINT [FK_Registrations_Horses_HorseId];

ALTER TABLE [Registrations] DROP CONSTRAINT [FK_Registrations_Tournament_TournamentId];

ALTER TABLE [Rounds] DROP CONSTRAINT [FK_Rounds_Tournaments_TournamentId];

ALTER TABLE [TournamentPrizePayouts] DROP CONSTRAINT [FK_TournamentPrizePayouts_Tournaments_TournamentId];

ALTER TABLE [TournamentPrizePayouts] DROP CONSTRAINT [FK_TournamentPrizePayouts_Users_UserId];

ALTER TABLE [Transactions] DROP CONSTRAINT [FK_Transactions_Wallets_WalletId];

ALTER TABLE [Users] DROP CONSTRAINT [FK_Users_Roles_RoleId];

ALTER TABLE [Violations] DROP CONSTRAINT [FK_Violations_Races_RaceId];

ALTER TABLE [Wallets] DROP CONSTRAINT [FK_Wallets_Users_UserId];

DROP TABLE [Tournaments];

ALTER TABLE [Tournament] DROP CONSTRAINT [AK_Tournament_TempId];

ALTER TABLE [Wallets] DROP CONSTRAINT [PK_Wallets];

ALTER TABLE [Violations] DROP CONSTRAINT [PK_Violations];

ALTER TABLE [Users] DROP CONSTRAINT [PK_Users];

ALTER TABLE [Transactions] DROP CONSTRAINT [PK_Transactions];

ALTER TABLE [TournamentPrizePayouts] DROP CONSTRAINT [PK_TournamentPrizePayouts];

ALTER TABLE [Rounds] DROP CONSTRAINT [PK_Rounds];

ALTER TABLE [Roles] DROP CONSTRAINT [PK_Roles];

ALTER TABLE [Registrations] DROP CONSTRAINT [PK_Registrations];

ALTER TABLE [RefereeProfiles] DROP CONSTRAINT [PK_RefereeProfiles];

ALTER TABLE [Races] DROP CONSTRAINT [PK_Races];

ALTER TABLE [RaceResults] DROP CONSTRAINT [PK_RaceResults];

ALTER TABLE [RaceRefereeAssignments] DROP CONSTRAINT [PK_RaceRefereeAssignments];

ALTER TABLE [RaceEntries] DROP CONSTRAINT [PK_RaceEntries];

ALTER TABLE [Prizes] DROP CONSTRAINT [PK_Prizes];

ALTER TABLE [Predictions] DROP CONSTRAINT [PK_Predictions];

ALTER TABLE [Payouts] DROP CONSTRAINT [PK_Payouts];

ALTER TABLE [Notifications] DROP CONSTRAINT [PK_Notifications];

ALTER TABLE [JockeyProfiles] DROP CONSTRAINT [PK_JockeyProfiles];

ALTER TABLE [JockeyContracts] DROP CONSTRAINT [PK_JockeyContracts];

ALTER TABLE [HorseStatistics] DROP CONSTRAINT [PK_HorseStatistics];

ALTER TABLE [Horses] DROP CONSTRAINT [PK_Horses];

ALTER TABLE [HorseDocuments] DROP CONSTRAINT [PK_HorseDocuments];

ALTER TABLE [Bets] DROP CONSTRAINT [PK_Bets];

DECLARE @var13 nvarchar(max);
SELECT @var13 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tournament]') AND [c].[name] = N'TempId');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Tournament] DROP CONSTRAINT ' + @var13 + ';');
ALTER TABLE [Tournament] DROP COLUMN [TempId];

EXEC sp_rename N'[Wallets]', N'Wallet', 'OBJECT';

EXEC sp_rename N'[Violations]', N'RaceViolation', 'OBJECT';

EXEC sp_rename N'[Users]', N'AppUser', 'OBJECT';

EXEC sp_rename N'[Transactions]', N'WalletTransaction', 'OBJECT';

EXEC sp_rename N'[TournamentPrizePayouts]', N'TournamentPrizePayout', 'OBJECT';

EXEC sp_rename N'[Rounds]', N'Round', 'OBJECT';

EXEC sp_rename N'[Roles]', N'Role', 'OBJECT';

EXEC sp_rename N'[Registrations]', N'Registration', 'OBJECT';

EXEC sp_rename N'[RefereeProfiles]', N'RefereeProfile', 'OBJECT';

EXEC sp_rename N'[Races]', N'Race', 'OBJECT';

EXEC sp_rename N'[RaceResults]', N'RaceResult', 'OBJECT';

EXEC sp_rename N'[RaceRefereeAssignments]', N'RaceRefereeAssignment', 'OBJECT';

EXEC sp_rename N'[RaceEntries]', N'RaceEntry', 'OBJECT';

EXEC sp_rename N'[Prizes]', N'Prize', 'OBJECT';

EXEC sp_rename N'[Predictions]', N'Prediction', 'OBJECT';

EXEC sp_rename N'[Payouts]', N'Payout', 'OBJECT';

EXEC sp_rename N'[Notifications]', N'Notification', 'OBJECT';

EXEC sp_rename N'[JockeyProfiles]', N'JockeyProfile', 'OBJECT';

EXEC sp_rename N'[JockeyContracts]', N'JockeyContract', 'OBJECT';

EXEC sp_rename N'[HorseStatistics]', N'HorseStatistic', 'OBJECT';

EXEC sp_rename N'[Horses]', N'Horse', 'OBJECT';

EXEC sp_rename N'[HorseDocuments]', N'HorseDocument', 'OBJECT';

EXEC sp_rename N'[Bets]', N'Bet', 'OBJECT';

EXEC sp_rename N'[Wallet].[IX_Wallets_UserId]', N'IX_Wallet_UserId', 'INDEX';

EXEC sp_rename N'[RaceViolation].[IX_Violations_RaceId]', N'IX_RaceViolation_RaceId', 'INDEX';

EXEC sp_rename N'[AppUser].[IX_Users_RoleId]', N'IX_AppUser_RoleId', 'INDEX';

EXEC sp_rename N'[WalletTransaction].[IX_Transactions_WalletId]', N'IX_WalletTransaction_WalletId', 'INDEX';

EXEC sp_rename N'[TournamentPrizePayout].[IX_TournamentPrizePayouts_UserId]', N'IX_TournamentPrizePayout_UserId', 'INDEX';

EXEC sp_rename N'[TournamentPrizePayout].[IX_TournamentPrizePayouts_TournamentId]', N'IX_TournamentPrizePayout_TournamentId', 'INDEX';

EXEC sp_rename N'[Round].[IX_Rounds_TournamentId]', N'IX_Round_TournamentId', 'INDEX';

EXEC sp_rename N'[Registration].[IX_Registrations_TournamentId]', N'IX_Registration_TournamentId', 'INDEX';

EXEC sp_rename N'[Registration].[IX_Registrations_HorseId]', N'IX_Registration_HorseId', 'INDEX';

EXEC sp_rename N'[RefereeProfile].[IX_RefereeProfiles_UserId]', N'IX_RefereeProfile_UserId', 'INDEX';

EXEC sp_rename N'[Race].[IX_Races_RoundId]', N'IX_Race_RoundId', 'INDEX';

EXEC sp_rename N'[RaceRefereeAssignment].[IX_RaceRefereeAssignments_RefereeId]', N'IX_RaceRefereeAssignment_RefereeId', 'INDEX';

EXEC sp_rename N'[RaceRefereeAssignment].[IX_RaceRefereeAssignments_RaceId]', N'IX_RaceRefereeAssignment_RaceId', 'INDEX';

EXEC sp_rename N'[RaceEntry].[IX_RaceEntries_RaceId]', N'IX_RaceEntry_RaceId', 'INDEX';

EXEC sp_rename N'[RaceEntry].[IX_RaceEntries_JockeyId]', N'IX_RaceEntry_JockeyId', 'INDEX';

EXEC sp_rename N'[RaceEntry].[IX_RaceEntries_HorseId1]', N'IX_RaceEntry_HorseId1', 'INDEX';

EXEC sp_rename N'[RaceEntry].[IX_RaceEntries_HorseId]', N'IX_RaceEntry_HorseId', 'INDEX';

EXEC sp_rename N'[Prize].[IX_Prizes_TournamentId]', N'IX_Prize_TournamentId', 'INDEX';

EXEC sp_rename N'[Payout].[IX_Payouts_BetId]', N'IX_Payout_BetId', 'INDEX';

EXEC sp_rename N'[Notification].[IX_Notifications_UserId]', N'IX_Notification_UserId', 'INDEX';

EXEC sp_rename N'[JockeyProfile].[IX_JockeyProfiles_UserId]', N'IX_JockeyProfile_UserId', 'INDEX';

EXEC sp_rename N'[JockeyContract].[IX_JockeyContracts_OwnerId]', N'IX_JockeyContract_OwnerId', 'INDEX';

EXEC sp_rename N'[JockeyContract].[IX_JockeyContracts_JockeyId]', N'IX_JockeyContract_JockeyId', 'INDEX';

EXEC sp_rename N'[JockeyContract].[IX_JockeyContracts_HorseId]', N'IX_JockeyContract_HorseId', 'INDEX';

EXEC sp_rename N'[HorseStatistic].[IX_HorseStatistics_HorseId]', N'IX_HorseStatistic_HorseId', 'INDEX';

EXEC sp_rename N'[Horse].[IX_Horses_RegistrationId]', N'IX_Horse_RegistrationId', 'INDEX';

EXEC sp_rename N'[Horse].[IX_Horses_OwnerId]', N'IX_Horse_OwnerId', 'INDEX';

EXEC sp_rename N'[HorseDocument].[IX_HorseDocuments_HorseId]', N'IX_HorseDocument_HorseId', 'INDEX';

EXEC sp_rename N'[Bet].[IX_Bets_UserId]', N'IX_Bet_UserId', 'INDEX';

EXEC sp_rename N'[Bet].[IX_Bets_RaceId]', N'IX_Bet_RaceId', 'INDEX';

EXEC sp_rename N'[Bet].[IX_Bets_HorseId]', N'IX_Bet_HorseId', 'INDEX';

ALTER TABLE [Tournament] ADD [TournamentId] bigint NOT NULL IDENTITY;

ALTER TABLE [Tournament] ADD [EndDate] datetime2 NULL;

ALTER TABLE [Tournament] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Tournament] ADD [StartDate] datetime2 NULL;

ALTER TABLE [Tournament] ADD [Status] nvarchar(max) NOT NULL DEFAULT N'';

DROP INDEX [IX_Registration_TournamentId] ON [Registration];
DECLARE @var14 nvarchar(max);
SELECT @var14 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Registration]') AND [c].[name] = N'TournamentId');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Registration] DROP CONSTRAINT ' + @var14 + ';');
ALTER TABLE [Registration] ALTER COLUMN [TournamentId] bigint NOT NULL;
CREATE INDEX [IX_Registration_TournamentId] ON [Registration] ([TournamentId]);

ALTER TABLE [Tournament] ADD CONSTRAINT [PK_Tournament] PRIMARY KEY ([TournamentId]);

ALTER TABLE [Wallet] ADD CONSTRAINT [PK_Wallet] PRIMARY KEY ([WalletId]);

ALTER TABLE [RaceViolation] ADD CONSTRAINT [PK_RaceViolation] PRIMARY KEY ([Id]);

ALTER TABLE [AppUser] ADD CONSTRAINT [PK_AppUser] PRIMARY KEY ([UserId]);

ALTER TABLE [WalletTransaction] ADD CONSTRAINT [PK_WalletTransaction] PRIMARY KEY ([Id]);

ALTER TABLE [TournamentPrizePayout] ADD CONSTRAINT [PK_TournamentPrizePayout] PRIMARY KEY ([Id]);

ALTER TABLE [Round] ADD CONSTRAINT [PK_Round] PRIMARY KEY ([RoundId]);

ALTER TABLE [Role] ADD CONSTRAINT [PK_Role] PRIMARY KEY ([RoleId]);

ALTER TABLE [Registration] ADD CONSTRAINT [PK_Registration] PRIMARY KEY ([Id]);

ALTER TABLE [RefereeProfile] ADD CONSTRAINT [PK_RefereeProfile] PRIMARY KEY ([RefereeId]);

ALTER TABLE [Race] ADD CONSTRAINT [PK_Race] PRIMARY KEY ([RaceId]);

ALTER TABLE [RaceResult] ADD CONSTRAINT [PK_RaceResult] PRIMARY KEY ([Id]);

ALTER TABLE [RaceRefereeAssignment] ADD CONSTRAINT [PK_RaceRefereeAssignment] PRIMARY KEY ([AssignmentId]);

ALTER TABLE [RaceEntry] ADD CONSTRAINT [PK_RaceEntry] PRIMARY KEY ([Id]);

ALTER TABLE [Prize] ADD CONSTRAINT [PK_Prize] PRIMARY KEY ([Id]);

ALTER TABLE [Prediction] ADD CONSTRAINT [PK_Prediction] PRIMARY KEY ([Id]);

ALTER TABLE [Payout] ADD CONSTRAINT [PK_Payout] PRIMARY KEY ([Id]);

ALTER TABLE [Notification] ADD CONSTRAINT [PK_Notification] PRIMARY KEY ([Id]);

ALTER TABLE [JockeyProfile] ADD CONSTRAINT [PK_JockeyProfile] PRIMARY KEY ([JockeyId]);

ALTER TABLE [JockeyContract] ADD CONSTRAINT [PK_JockeyContract] PRIMARY KEY ([Id]);

ALTER TABLE [HorseStatistic] ADD CONSTRAINT [PK_HorseStatistic] PRIMARY KEY ([Id]);

ALTER TABLE [Horse] ADD CONSTRAINT [PK_Horse] PRIMARY KEY ([Id]);

ALTER TABLE [HorseDocument] ADD CONSTRAINT [PK_HorseDocument] PRIMARY KEY ([Id]);

ALTER TABLE [Bet] ADD CONSTRAINT [PK_Bet] PRIMARY KEY ([Id]);

UPDATE [AppUser] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEDQl5GtU6lXMcfTRWSHnVz0AYO/DcesuZdc26L/lMBXERd4y/Ry6KFBnnz3OcVLaiA=='
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;


UPDATE [AppUser] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEIU26C1yHTr+reVJwGZJlfXXlzLjVh3ejJo2RFDSgGRU3CQn2cp7nJJIfJ/m6XAm8Q=='
WHERE [UserId] = 2;
SELECT @@ROWCOUNT;


UPDATE [AppUser] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEETNsom+QeZLwGVXPatXiCXnefira62DVHwdLXgHMsgdrwu7Xy3znwO8r72lMU5Mog=='
WHERE [UserId] = 3;
SELECT @@ROWCOUNT;


UPDATE [AppUser] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEN/0ABNONq8lx6xDeeREIStGpg1ENtlTUY2BLUaNLzyO6hnHvqnoA4Qd+WqRgJadgg=='
WHERE [UserId] = 4;
SELECT @@ROWCOUNT;


UPDATE [AppUser] SET [PasswordHash] = N'AQAAAAIAAYagAAAAEJKV8UzjVkRV+m8NEvHJi0leRC9j/XUkH19a15TyFSoRWPiDhJfEqjAl28C89Ikcbg=='
WHERE [UserId] = 5;
SELECT @@ROWCOUNT;


ALTER TABLE [AppUser] ADD CONSTRAINT [FK_AppUser_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([RoleId]) ON DELETE NO ACTION;

ALTER TABLE [Bet] ADD CONSTRAINT [FK_Bet_AppUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

ALTER TABLE [Bet] ADD CONSTRAINT [FK_Bet_Horse_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Bet] ADD CONSTRAINT [FK_Bet_Race_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Race] ([RaceId]) ON DELETE NO ACTION;

ALTER TABLE [Horse] ADD CONSTRAINT [FK_Horse_AppUser_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

ALTER TABLE [Horse] ADD CONSTRAINT [FK_Horse_Registration_RegistrationId] FOREIGN KEY ([RegistrationId]) REFERENCES [Registration] ([Id]);

ALTER TABLE [HorseDocument] ADD CONSTRAINT [FK_HorseDocument_Horse_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE CASCADE;

ALTER TABLE [HorseStatistic] ADD CONSTRAINT [FK_HorseStatistic_Horse_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE CASCADE;

ALTER TABLE [JockeyContract] ADD CONSTRAINT [FK_JockeyContract_AppUser_JockeyId] FOREIGN KEY ([JockeyId]) REFERENCES [AppUser] ([UserId]) ON DELETE NO ACTION;

ALTER TABLE [JockeyContract] ADD CONSTRAINT [FK_JockeyContract_AppUser_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [AppUser] ([UserId]) ON DELETE NO ACTION;

ALTER TABLE [JockeyContract] ADD CONSTRAINT [FK_JockeyContract_Horse_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [JockeyProfile] ADD CONSTRAINT [FK_JockeyProfile_AppUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

ALTER TABLE [Notification] ADD CONSTRAINT [FK_Notification_AppUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

ALTER TABLE [Payout] ADD CONSTRAINT [FK_Payout_Bet_BetId] FOREIGN KEY ([BetId]) REFERENCES [Bet] ([Id]) ON DELETE CASCADE;

ALTER TABLE [Prize] ADD CONSTRAINT [FK_Prize_Tournament_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournament] ([TournamentId]) ON DELETE CASCADE;

ALTER TABLE [Race] ADD CONSTRAINT [FK_Race_Round_RoundId] FOREIGN KEY ([RoundId]) REFERENCES [Round] ([RoundId]) ON DELETE CASCADE;

ALTER TABLE [RaceEntry] ADD CONSTRAINT [FK_RaceEntry_AppUser_JockeyId] FOREIGN KEY ([JockeyId]) REFERENCES [AppUser] ([UserId]) ON DELETE NO ACTION;

ALTER TABLE [RaceEntry] ADD CONSTRAINT [FK_RaceEntry_Horse_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [RaceEntry] ADD CONSTRAINT [FK_RaceEntry_Horse_HorseId1] FOREIGN KEY ([HorseId1]) REFERENCES [Horse] ([Id]);

ALTER TABLE [RaceEntry] ADD CONSTRAINT [FK_RaceEntry_Race_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Race] ([RaceId]) ON DELETE CASCADE;

ALTER TABLE [RaceRefereeAssignment] ADD CONSTRAINT [FK_RaceRefereeAssignment_Race_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Race] ([RaceId]) ON DELETE CASCADE;

ALTER TABLE [RaceRefereeAssignment] ADD CONSTRAINT [FK_RaceRefereeAssignment_RefereeProfile_RefereeId] FOREIGN KEY ([RefereeId]) REFERENCES [RefereeProfile] ([RefereeId]) ON DELETE NO ACTION;

ALTER TABLE [RaceViolation] ADD CONSTRAINT [FK_RaceViolation_Race_RaceId] FOREIGN KEY ([RaceId]) REFERENCES [Race] ([RaceId]) ON DELETE CASCADE;

ALTER TABLE [RefereeProfile] ADD CONSTRAINT [FK_RefereeProfile_AppUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

ALTER TABLE [Registration] ADD CONSTRAINT [FK_Registration_Horse_HorseId] FOREIGN KEY ([HorseId]) REFERENCES [Horse] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Registration] ADD CONSTRAINT [FK_Registration_Tournament_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournament] ([TournamentId]) ON DELETE NO ACTION;

ALTER TABLE [Round] ADD CONSTRAINT [FK_Round_Tournament_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournament] ([TournamentId]) ON DELETE CASCADE;

ALTER TABLE [TournamentPrizePayout] ADD CONSTRAINT [FK_TournamentPrizePayout_AppUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

ALTER TABLE [TournamentPrizePayout] ADD CONSTRAINT [FK_TournamentPrizePayout_Tournament_TournamentId] FOREIGN KEY ([TournamentId]) REFERENCES [Tournament] ([TournamentId]) ON DELETE CASCADE;

ALTER TABLE [Wallet] ADD CONSTRAINT [FK_Wallet_AppUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [AppUser] ([UserId]) ON DELETE CASCADE;

ALTER TABLE [WalletTransaction] ADD CONSTRAINT [FK_WalletTransaction_Wallet_WalletId] FOREIGN KEY ([WalletId]) REFERENCES [Wallet] ([WalletId]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260612024026_FixSingularTableMapping', N'10.0.9');

COMMIT;
GO

