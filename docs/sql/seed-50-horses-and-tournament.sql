DECLARE @TournamentId BIGINT;
DECLARE @OwnerId INT;

-- Find a HorseOwner
SELECT TOP 1 @OwnerId = UserId FROM [AppUser] WHERE RoleId = (SELECT TOP 1 RoleId FROM [Role] WHERE Name = 'HorseOwner') AND Status = 'Active';
IF @OwnerId IS NULL
BEGIN
    INSERT INTO [AppUser] (Username, Email, PasswordHash, FullName, RoleId, Status, CreatedAt)
    VALUES ('owner50', 'owner50@example.com', 'hash', N'Chủ Ngựa Test 50', (SELECT TOP 1 RoleId FROM [Role] WHERE Name = 'HorseOwner'), 'Active', GETDATE());
    SET @OwnerId = SCOPE_IDENTITY();
END

-- Check if our 50-horse tournament exists, else create it
SELECT TOP 1 @TournamentId = TournamentId FROM Tournament WHERE Name = N'Giải Đua Siêu Cấp 50 Ngựa 2026';
IF @TournamentId IS NULL
BEGIN
    INSERT INTO Tournament (Name, Status, StartDate, EndDate) 
    VALUES (N'Giải Đua Siêu Cấp 50 Ngựa 2026', 'Scheduled', GETDATE(), DATEADD(day, 60, GETDATE()));
    SET @TournamentId = SCOPE_IDENTITY();
    
    INSERT INTO Round (TournamentId, RoundNumber, Name, Status, StartDate, EndDate) 
    VALUES (@TournamentId, 1, N'Vòng Phân Hạng Tập Trung', 'Scheduled', GETDATE(), DATEADD(day, 30, GETDATE()));
END

DECLARE @Counter INT = 1;
DECLARE @HorseName NVARCHAR(100);
DECLARE @HorseId INT;
DECLARE @CurrentYear INT = YEAR(GETDATE());

WHILE @Counter <= 50
BEGIN
    SET @HorseName = 'Siêu Mã ' + CAST(@Counter AS NVARCHAR(10));
    
    -- Insert Horse
    IF NOT EXISTS (SELECT 1 FROM Horse WHERE Name = @HorseName)
    BEGIN
        INSERT INTO Horse (OwnerId, Name, Age, Breed, Gender, HealthStatus) 
        VALUES (@OwnerId, @HorseName, 3 + (@Counter % 5), N'Ả Rập', CASE WHEN @Counter % 2 = 0 THEN 'Male' ELSE 'Female' END, 'Healthy');
        SET @HorseId = SCOPE_IDENTITY();
    END
    ELSE 
    BEGIN
        SELECT TOP 1 @HorseId = Id FROM Horse WHERE Name = @HorseName;
    END

    -- Register Horse to Tournament
    IF NOT EXISTS (SELECT 1 FROM Registration WHERE HorseId = @HorseId AND TournamentId = @TournamentId)
    BEGIN
        INSERT INTO Registration (TournamentId, HorseId, Status, CreatedAt) VALUES (@TournamentId, @HorseId, 'Approved', GETDATE());
    END
    
    SET @Counter = @Counter + 1;
END

PRINT 'Seed completed: 50 horses added and registered to Tournament ID ' + CAST(@TournamentId AS NVARCHAR(10));
