DECLARE @TournamentId INT;
DECLARE @RaceId INT;
DECLARE @OwnerId INT;
DECLARE @JockeyId1 INT;
DECLARE @JockeyId2 INT;
DECLARE @JockeyId3 INT;
DECLARE @JockeyId4 INT;
DECLARE @RegistrationId1 INT;
DECLARE @RegistrationId2 INT;
DECLARE @RegistrationId3 INT;
DECLARE @RegistrationId4 INT;
DECLARE @HorseId1 INT;
DECLARE @HorseId2 INT;
DECLARE @HorseId3 INT;
DECLARE @HorseId4 INT;

-- Find an active tournament
SELECT TOP 1 @TournamentId = TournamentId
FROM Tournament
WHERE Status IN ('AcceptingEntries', 'Scheduled', 'Open', 'Active')
ORDER BY TournamentId DESC;

IF @TournamentId IS NULL
BEGIN
    PRINT 'No suitable tournament found. Creating one...';
    INSERT INTO Tournament (Name, Status, RegistrationFee, PrizePool, StartDate, EndDate) 
    VALUES (N'Giải Đua Mùa Xuân ' + CAST(YEAR(GETDATE()) AS NVARCHAR), 'Scheduled', 500000, 100000000, GETDATE(), DATEADD(day, 30, GETDATE()));
    SET @TournamentId = SCOPE_IDENTITY();
END

-- Ensure a Round exists
DECLARE @RoundId INT;
SELECT TOP 1 @RoundId = RoundId FROM Round WHERE TournamentId = @TournamentId;
IF @RoundId IS NULL
BEGIN
    INSERT INTO Round (TournamentId, RoundNumber, Name, Status) 
    VALUES (@TournamentId, 1, N'Vòng Sơ Loại', 'Scheduled');
    SET @RoundId = SCOPE_IDENTITY();
END

-- Find or create a Scheduled Race
SELECT TOP 1 @RaceId = RaceId
FROM Race
WHERE RoundId = @RoundId AND Status IN ('Scheduled', 'Live')
ORDER BY RaceId DESC;

IF @RaceId IS NULL
BEGIN
    INSERT INTO Race (RoundId, Name, DistanceMeter, MaxLanes, Status, RaceDate)
    VALUES (@RoundId, N'Đua Bán Kết Vòng Loại 1', 1200, 4, 'Scheduled', DATEADD(day, 1, GETDATE()));
    SET @RaceId = SCOPE_IDENTITY();
END

-- Find a HorseOwner
SELECT TOP 1 @OwnerId = UserId FROM [User] WHERE Role = 'HorseOwner' AND Status = 'Active';

-- Create Horses
IF NOT EXISTS (SELECT 1 FROM Horse WHERE Name = 'Red Lightning')
BEGIN
    INSERT INTO Horse (OwnerId, Name, Age, Status) VALUES (@OwnerId, 'Red Lightning', 4, 'Active');
    SET @HorseId1 = SCOPE_IDENTITY();
END
ELSE SELECT TOP 1 @HorseId1 = HorseId FROM Horse WHERE Name = 'Red Lightning';

IF NOT EXISTS (SELECT 1 FROM Horse WHERE Name = 'Blue Thunder')
BEGIN
    INSERT INTO Horse (OwnerId, Name, Age, Status) VALUES (@OwnerId, 'Blue Thunder', 5, 'Active');
    SET @HorseId2 = SCOPE_IDENTITY();
END
ELSE SELECT TOP 1 @HorseId2 = HorseId FROM Horse WHERE Name = 'Blue Thunder';

IF NOT EXISTS (SELECT 1 FROM Horse WHERE Name = 'Golden Storm')
BEGIN
    INSERT INTO Horse (OwnerId, Name, Age, Status) VALUES (@OwnerId, 'Golden Storm', 3, 'Active');
    SET @HorseId3 = SCOPE_IDENTITY();
END
ELSE SELECT TOP 1 @HorseId3 = HorseId FROM Horse WHERE Name = 'Golden Storm';

IF NOT EXISTS (SELECT 1 FROM Horse WHERE Name = 'Black Wind')
BEGIN
    INSERT INTO Horse (OwnerId, Name, Age, Status) VALUES (@OwnerId, 'Black Wind', 6, 'Active');
    SET @HorseId4 = SCOPE_IDENTITY();
END
ELSE SELECT TOP 1 @HorseId4 = HorseId FROM Horse WHERE Name = 'Black Wind';

-- Find Jockeys
SELECT TOP 1 @JockeyId1 = UserId FROM [User] WHERE Role = 'Jockey' AND Status = 'Active' ORDER BY UserId DESC;
SELECT TOP 1 @JockeyId2 = UserId FROM [User] WHERE Role = 'Jockey' AND Status = 'Active' AND UserId <> ISNULL(@JockeyId1, 0) ORDER BY UserId DESC;
SELECT TOP 1 @JockeyId3 = UserId FROM [User] WHERE Role = 'Jockey' AND Status = 'Active' AND UserId NOT IN (ISNULL(@JockeyId1, 0), ISNULL(@JockeyId2, 0)) ORDER BY UserId DESC;
SELECT TOP 1 @JockeyId4 = UserId FROM [User] WHERE Role = 'Jockey' AND Status = 'Active' AND UserId NOT IN (ISNULL(@JockeyId1, 0), ISNULL(@JockeyId2, 0), ISNULL(@JockeyId3, 0)) ORDER BY UserId DESC;

-- Fallbacks if not enough jockeys
IF @JockeyId1 IS NULL SET @JockeyId1 = @OwnerId; -- just for testing
IF @JockeyId2 IS NULL SET @JockeyId2 = @JockeyId1;
IF @JockeyId3 IS NULL SET @JockeyId3 = @JockeyId1;
IF @JockeyId4 IS NULL SET @JockeyId4 = @JockeyId1;

-- Add Registrations
IF NOT EXISTS (SELECT 1 FROM Registration WHERE HorseId = @HorseId1 AND TournamentId = @TournamentId)
BEGIN
    INSERT INTO Registration (TournamentId, HorseId, Status) VALUES (@TournamentId, @HorseId1, 'Approved');
    SET @RegistrationId1 = SCOPE_IDENTITY();
END
ELSE SELECT TOP 1 @RegistrationId1 = RegistrationId FROM Registration WHERE HorseId = @HorseId1 AND TournamentId = @TournamentId;

IF NOT EXISTS (SELECT 1 FROM Registration WHERE HorseId = @HorseId2 AND TournamentId = @TournamentId)
BEGIN
    INSERT INTO Registration (TournamentId, HorseId, Status) VALUES (@TournamentId, @HorseId2, 'Approved');
    SET @RegistrationId2 = SCOPE_IDENTITY();
END
ELSE SELECT TOP 1 @RegistrationId2 = RegistrationId FROM Registration WHERE HorseId = @HorseId2 AND TournamentId = @TournamentId;

IF NOT EXISTS (SELECT 1 FROM Registration WHERE HorseId = @HorseId3 AND TournamentId = @TournamentId)
BEGIN
    INSERT INTO Registration (TournamentId, HorseId, Status) VALUES (@TournamentId, @HorseId3, 'Approved');
    SET @RegistrationId3 = SCOPE_IDENTITY();
END
ELSE SELECT TOP 1 @RegistrationId3 = RegistrationId FROM Registration WHERE HorseId = @HorseId3 AND TournamentId = @TournamentId;

IF NOT EXISTS (SELECT 1 FROM Registration WHERE HorseId = @HorseId4 AND TournamentId = @TournamentId)
BEGIN
    INSERT INTO Registration (TournamentId, HorseId, Status) VALUES (@TournamentId, @HorseId4, 'Approved');
    SET @RegistrationId4 = SCOPE_IDENTITY();
END
ELSE SELECT TOP 1 @RegistrationId4 = RegistrationId FROM Registration WHERE HorseId = @HorseId4 AND TournamentId = @TournamentId;

-- Add Race Entries with Odds
IF NOT EXISTS (SELECT 1 FROM RaceEntry WHERE RaceId = @RaceId AND LaneNo = 1)
BEGIN
    INSERT INTO RaceEntry (RaceId, RegistrationId, JockeyId, LaneNo, WinningProbability, CurrentOdds, Status)
    VALUES (@RaceId, @RegistrationId1, @JockeyId1, 1, 0.4, 2.5, 'Assigned');
END

IF NOT EXISTS (SELECT 1 FROM RaceEntry WHERE RaceId = @RaceId AND LaneNo = 2)
BEGIN
    INSERT INTO RaceEntry (RaceId, RegistrationId, JockeyId, LaneNo, WinningProbability, CurrentOdds, Status)
    VALUES (@RaceId, @RegistrationId2, @JockeyId2, 2, 0.3, 3.2, 'Assigned');
END

IF NOT EXISTS (SELECT 1 FROM RaceEntry WHERE RaceId = @RaceId AND LaneNo = 3)
BEGIN
    INSERT INTO RaceEntry (RaceId, RegistrationId, JockeyId, LaneNo, WinningProbability, CurrentOdds, Status)
    VALUES (@RaceId, @RegistrationId3, @JockeyId3, 3, 0.2, 4.0, 'Assigned');
END

IF NOT EXISTS (SELECT 1 FROM RaceEntry WHERE RaceId = @RaceId AND LaneNo = 4)
BEGIN
    INSERT INTO RaceEntry (RaceId, RegistrationId, JockeyId, LaneNo, WinningProbability, CurrentOdds, Status)
    VALUES (@RaceId, @RegistrationId4, @JockeyId4, 4, 0.1, 6.5, 'Assigned');
END

PRINT 'Seed completed successfully!';
