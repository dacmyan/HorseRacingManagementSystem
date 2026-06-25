-- ============================================================
-- SEED FUNCTIONAL REQUIREMENTS TEST DATA
-- Horse Racing Management System
-- ============================================================
-- Safety rules:
--   - No DROP TABLE
--   - No DELETE
--   - No TRUNCATE
--   - Uses IF NOT EXISTS
--   - Uses int for IDs
--   - Uses N'...' for Vietnamese
--   - No raw password inserts
-- ============================================================

USE HorseRacingManagementSystem;
GO

PRINT '>>> Checking existing data counts...';
SELECT 'Tournament' AS [Table], COUNT(*) AS [Count] FROM Tournament
UNION ALL SELECT 'Round', COUNT(*) FROM Round
UNION ALL SELECT 'Race', COUNT(*) FROM Race
UNION ALL SELECT 'Horse', COUNT(*) FROM Horse
UNION ALL SELECT 'RaceEntry', COUNT(*) FROM RaceEntry
UNION ALL SELECT 'RaceRefereeAssignment', COUNT(*) FROM RaceRefereeAssignment
UNION ALL SELECT 'RaceViolation', COUNT(*) FROM RaceViolation
UNION ALL SELECT 'RefereeReport', COUNT(*) FROM RefereeReport
UNION ALL SELECT 'RaceResult', COUNT(*) FROM RaceResult
UNION ALL SELECT 'Bet', COUNT(*) FROM Bet
UNION ALL SELECT 'Payout', COUNT(*) FROM Payout
UNION ALL SELECT 'Prediction', COUNT(*) FROM Prediction
UNION ALL SELECT 'Notification', COUNT(*) FROM Notification
UNION ALL SELECT 'Wallet', COUNT(*) FROM Wallet
UNION ALL SELECT 'Registration', COUNT(*) FROM Registration
UNION ALL SELECT 'JockeyContract', COUNT(*) FROM JockeyContract;
GO

-- ============================================================
-- VERIFY EXISTING TEST USERS
-- Expected: UserId 1=Admin, 2=HorseOwner, 3=Jockey, 4=Referee, 5=Spectator
-- ============================================================
PRINT '>>> Verifying test users...';
SELECT u.UserId, u.FullName, u.Email, r.Name AS RoleName, u.Status
FROM AppUser u
JOIN Role r ON u.RoleId = r.RoleId
WHERE u.Email IN (
    'admin@gmail.com',
    'owner@gmail.com',
    'jockey@gmail.com', 
    'referee@gmail.com',
    'spectator@gmail.com'
);
GO

-- ============================================================
-- ENSURE WALLETS for ALL ROLES
-- ============================================================
PRINT '>>> Ensuring wallets for test users...';

-- HorseOwner (UserId=2)
IF NOT EXISTS (SELECT 1 FROM Wallet WHERE UserId = 2)
BEGIN
    INSERT INTO Wallet (UserId, Balance) VALUES (2, 5000000.00);
    PRINT '  Created wallet for HorseOwner (UserId=2)';
END
ELSE PRINT '  Wallet for HorseOwner already exists';

-- Jockey (UserId=3)  
IF NOT EXISTS (SELECT 1 FROM Wallet WHERE UserId = 3)
BEGIN
    INSERT INTO Wallet (UserId, Balance) VALUES (3, 1500000.00);
    PRINT '  Created wallet for Jockey (UserId=3)';
END
ELSE PRINT '  Wallet for Jockey already exists';

-- Referee (UserId=4)
IF NOT EXISTS (SELECT 1 FROM Wallet WHERE UserId = 4)
BEGIN
    INSERT INTO Wallet (UserId, Balance) VALUES (4, 2000000.00);
    PRINT '  Created wallet for Referee (UserId=4)';
END
ELSE PRINT '  Wallet for Referee already exists';

-- Spectator (UserId=5)
IF NOT EXISTS (SELECT 1 FROM Wallet WHERE UserId = 5)
BEGIN
    INSERT INTO Wallet (UserId, Balance) VALUES (5, 1000000.00);
    PRINT '  Created wallet for Spectator (UserId=5)';
END
ELSE PRINT '  Wallet for Spectator already exists';
GO

-- ============================================================
-- ENSURE JOCKEY PROFILE
-- ============================================================
PRINT '>>> Ensuring JockeyProfile for Jockey (UserId=3)...';
IF NOT EXISTS (SELECT 1 FROM JockeyProfile WHERE UserId = 3)
BEGIN
    INSERT INTO JockeyProfile (UserId, ExperienceYears, RankingPoint, Status)
    VALUES (3, 3, 100, 'Active');
    PRINT '  Created JockeyProfile for UserId=3';
END
ELSE PRINT '  JockeyProfile for UserId=3 already exists';
GO

-- ============================================================
-- ENSURE REFEREE PROFILE
-- ============================================================
PRINT '>>> Ensuring RefereeProfile for Referee (UserId=4)...';
IF NOT EXISTS (SELECT 1 FROM RefereeProfile WHERE UserId = 4)
BEGIN
    INSERT INTO RefereeProfile (UserId, LicenseNumber, ExperienceYears, Status)
    VALUES (4, 'LIC-REF-001', 5, 'Active');
    PRINT '  Created RefereeProfile for UserId=4';
END
ELSE PRINT '  RefereeProfile for UserId=4 already exists';
GO

-- ============================================================
-- ENSURE TOURNAMENT DATA
-- ============================================================
PRINT '>>> Ensuring Tournament data...';

-- Tournament that is Active (for registrations)
IF NOT EXISTS (SELECT 1 FROM Tournament WHERE Name = N'Giải Đua Vô Địch Quốc Gia 2026')
BEGIN
    INSERT INTO Tournament (Name, StartDate, EndDate, Status)
    VALUES (N'Giải Đua Vô Địch Quốc Gia 2026', '2026-06-01', '2026-06-30', 'Active');
    PRINT '  Created Active Tournament';
END
ELSE PRINT '  Active Tournament already exists';

-- Tournament that is Completed (for results viewing)
IF NOT EXISTS (SELECT 1 FROM Tournament WHERE Name = N'Hanoi Championship 2026')
BEGIN
    INSERT INTO Tournament (Name, StartDate, EndDate, Status)
    VALUES (N'Hanoi Championship 2026', '2026-06-20', '2026-07-20', 'Completed');
    PRINT '  Created Completed Tournament';
END
ELSE PRINT '  Completed Tournament already exists';
GO

-- ============================================================
-- ENSURE ROUND DATA (for the Active Tournament TournamentId=4)
-- ============================================================
PRINT '>>> Ensuring Round data...';
DECLARE @activeTournamentId INT;
SELECT @activeTournamentId = TournamentId FROM Tournament WHERE Name = N'Giải Đua Vô Địch Quốc Gia 2026';

IF @activeTournamentId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Round WHERE TournamentId = @activeTournamentId AND RoundNumber = 1)
BEGIN
    INSERT INTO Round (TournamentId, RoundNumber, Name, StartDate, EndDate, Status)
    VALUES (@activeTournamentId, 1, N'Vòng Loại 1', '2026-06-01', '2026-06-15', 'Completed');
    PRINT '  Created Round 1 for Active Tournament';
END
GO

-- ============================================================
-- ENSURE RACE DATA (covering multiple statuses)
-- ============================================================
PRINT '>>> Verifying Race statuses...';
SELECT RaceId, Name, Status FROM Race ORDER BY RaceId;

-- Ensure at least one Scheduled race
IF NOT EXISTS (SELECT 1 FROM Race WHERE Status = 'Scheduled')
BEGIN
    DECLARE @roundId INT;
    SELECT TOP 1 @roundId = RoundId FROM Round ORDER BY RoundId;
    IF @roundId IS NOT NULL
    BEGIN
        INSERT INTO Race (Name, RaceDate, DistanceMeter, MaxLanes, Status, RoundId)
        VALUES (N'Đua Vòng Loại Ngày 2', DATEADD(DAY, 5, GETDATE()), 1500, 8, 'Scheduled', @roundId);
        PRINT '  Created Scheduled race';
    END
END

-- Ensure at least one Live race
IF NOT EXISTS (SELECT 1 FROM Race WHERE Status = 'Live')
BEGIN
    DECLARE @roundId2 INT;
    SELECT TOP 1 @roundId2 = RoundId FROM Round ORDER BY RoundId;
    UPDATE TOP(1) Race SET Status = 'Live' WHERE Status = 'Scheduled';
    PRINT '  Updated one race to Live status';
END

-- Ensure at least one Finished/Completed race
IF NOT EXISTS (SELECT 1 FROM Race WHERE Status IN ('Finished', 'Completed'))
BEGIN
    DECLARE @roundId3 INT;
    SELECT TOP 1 @roundId3 = RoundId FROM Round ORDER BY RoundId;
    IF @roundId3 IS NOT NULL
    BEGIN
        INSERT INTO Race (Name, RaceDate, DistanceMeter, MaxLanes, Status, RoundId)
        VALUES (N'Đua Chung Kết Vòng 1 - Demo', DATEADD(DAY, -2, GETDATE()), 2000, 8, 'Finished', @roundId3);
        PRINT '  Created Finished race for result testing';
    END
END
GO

-- ============================================================
-- ENSURE HORSE DATA
-- Horse table uses column 'Id' (not 'HorseId')
-- ============================================================
PRINT '>>> Ensuring Horse data for HorseOwner (UserId=2, maps to OwnerId=2 in appuser)...';

-- Check actual OwnerId mapping (OwnerId in Horse table = UserId)
IF NOT EXISTS (SELECT 1 FROM Horse WHERE OwnerId = 2 AND Name = N'Thần Phong')
BEGIN
    INSERT INTO Horse (Name, Age, Breed, OwnerId, Gender, HealthStatus)
    VALUES (N'Thần Phong', 5, 'Arabian', 2, 'Male', 'Healthy');
    PRINT '  Created Horse Thần Phong for Owner UserId=2';
END

IF NOT EXISTS (SELECT 1 FROM Horse WHERE OwnerId = 2 AND Name = N'Bạch Mã')
BEGIN
    INSERT INTO Horse (Name, Age, Breed, OwnerId, Gender, HealthStatus)
    VALUES (N'Bạch Mã', 4, 'Thoroughbred', 2, 'Female', 'Healthy');
    PRINT '  Created Horse Bạch Mã for Owner UserId=2';
END
GO

-- ============================================================
-- ENSURE REGISTRATION DATA  
-- Registration: TournamentId, HorseId (using Id from Horse), Status
-- ============================================================
PRINT '>>> Ensuring Registration data...';
DECLARE @horseId INT, @tournamentId INT;
SELECT TOP 1 @horseId = Id FROM Horse WHERE OwnerId = 2 ORDER BY Id;
SELECT TOP 1 @tournamentId = TournamentId FROM Tournament WHERE Status = 'Active' ORDER BY TournamentId;

IF @horseId IS NOT NULL AND @tournamentId IS NOT NULL
BEGIN
    -- Pending registration
    IF NOT EXISTS (SELECT 1 FROM Registration WHERE HorseId = @horseId AND TournamentId = @tournamentId AND Status = 'Pending')
    BEGIN
        INSERT INTO Registration (TournamentId, HorseId, Status, CreatedAt)
        VALUES (@tournamentId, @horseId, 'Pending', GETDATE());
        PRINT '  Created Pending Registration';
    END

    -- Approved registration (use another horse)
    DECLARE @horseId2 INT;
    SELECT TOP 1 @horseId2 = Id FROM Horse WHERE OwnerId = 2 AND Id != @horseId ORDER BY Id;
    IF @horseId2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Registration WHERE HorseId = @horseId2 AND TournamentId = @tournamentId AND Status = 'Approved')
    BEGIN
        INSERT INTO Registration (TournamentId, HorseId, Status, CreatedAt)
        VALUES (@tournamentId, @horseId2, 'Approved', DATEADD(DAY, -1, GETDATE()));
        PRINT '  Created Approved Registration';
    END
END
GO

-- ============================================================
-- ENSURE JOCKEY CONTRACT DATA
-- ============================================================
PRINT '>>> Ensuring JockeyContract data...';
DECLARE @horseId INT, @jockeyId INT, @tournamentId INT;
SELECT TOP 1 @horseId = Id FROM Horse WHERE OwnerId = 2 ORDER BY Id;
SELECT TOP 1 @jockeyId = JockeyId FROM JockeyProfile WHERE UserId = 3;
SELECT TOP 1 @tournamentId = TournamentId FROM Tournament WHERE Status = 'Active';

IF @horseId IS NOT NULL AND @jockeyId IS NOT NULL AND @tournamentId IS NOT NULL
BEGIN
    -- Pending contract
    IF NOT EXISTS (SELECT 1 FROM JockeyContract WHERE HorseId = @horseId AND JockeyId = @jockeyId AND Status = 'Pending')
    BEGIN
        INSERT INTO JockeyContract (TournamentId, HorseId, JockeyId, StartDate, EndDate, Status, CreatedAt)
        VALUES (@tournamentId, @horseId, @jockeyId, GETDATE(), DATEADD(MONTH, 1, GETDATE()), 'Pending', GETDATE());
        PRINT '  Created Pending JockeyContract';
    END
END
GO

-- ============================================================
-- ENSURE RACE ENTRY DATA (Verify RaceEntry columns)
-- RaceEntry: RaceEntryId, RaceId, RegistrationId, JockeyId, LaneNo, Status
-- ============================================================
PRINT '>>> Ensuring RaceEntry data...';
DECLARE @raceId INT, @regId INT, @jockeyId INT;
SELECT TOP 1 @raceId = RaceId FROM Race WHERE Status = 'Live' ORDER BY RaceId;
SELECT TOP 1 @regId = Id FROM Registration WHERE Status = 'Approved' ORDER BY Id;
SELECT TOP 1 @jockeyId = JockeyId FROM JockeyProfile WHERE UserId = 3;

IF @raceId IS NOT NULL AND @regId IS NOT NULL AND @jockeyId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RaceEntry WHERE RaceId = @raceId AND RegistrationId = @regId)
    BEGIN
        INSERT INTO RaceEntry (RaceId, RegistrationId, JockeyId, LaneNo, Status)
        VALUES (@raceId, @regId, @jockeyId, 2, 'Ready');
        PRINT '  Created RaceEntry for Live race';
    END
END
GO

-- ============================================================
-- ENSURE REFEREE ASSIGNMENT (for Referee tests)
-- ============================================================
PRINT '>>> Ensuring RaceRefereeAssignment data...';
DECLARE @raceId INT, @refereeId INT;
SELECT TOP 1 @raceId = RaceId FROM Race WHERE Status IN ('Live', 'Scheduled') ORDER BY RaceId;
SELECT TOP 1 @refereeId = RefereeId FROM RefereeProfile WHERE UserId = 4;

IF @raceId IS NOT NULL AND @refereeId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RaceRefereeAssignment WHERE RaceId = @raceId AND RefereeId = @refereeId)
    BEGIN
        INSERT INTO RaceRefereeAssignment (RaceId, RefereeId, AssignedAt, Status)
        VALUES (@raceId, @refereeId, GETDATE(), 'Active');
        PRINT '  Created RaceRefereeAssignment for Referee';
    END
END
GO

-- ============================================================
-- ENSURE RACE VIOLATIONS (for Referee/Jockey view)
-- RaceViolation: Id, RaceId, Description, Penalty
-- ============================================================
PRINT '>>> Ensuring RaceViolation data...';
DECLARE @raceId INT;
SELECT TOP 1 @raceId = RaceId FROM Race WHERE Status IN ('Live', 'Finished', 'Completed') ORDER BY RaceId;

IF @raceId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RaceViolation WHERE RaceId = @raceId AND Description = N'Lấn làn - Ngựa số 2 lấn sang làn số 3')
    BEGIN
        INSERT INTO RaceViolation (RaceId, Description, Penalty)
        VALUES (@raceId, N'Lấn làn - Ngựa số 2 lấn sang làn số 3', 'Warning');
        PRINT '  Created RaceViolation sample (Lấn làn)';
    END

    IF NOT EXISTS (SELECT 1 FROM RaceViolation WHERE RaceId = @raceId AND Description = N'Cản trở - Giảm tốc đột ngột gây cản trở đội phía sau')
    BEGIN
        INSERT INTO RaceViolation (RaceId, Description, Penalty)
        VALUES (@raceId, N'Cản trở - Giảm tốc đột ngột gây cản trở đội phía sau', 'Time Penalty');
        PRINT '  Created RaceViolation sample (Cản trở)';
    END
END
GO

-- ============================================================
-- ENSURE REFEREE REPORT
-- RefereeReport: ReportId, AssignmentId, Content, ViolationNote, CreatedAt
-- ============================================================
PRINT '>>> Ensuring RefereeReport data...';
DECLARE @assignId INT;
SELECT TOP 1 @assignId = AssignmentId FROM RaceRefereeAssignment ORDER BY AssignmentId;

IF @assignId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RefereeReport WHERE AssignmentId = @assignId AND Content LIKE N'%Báo cáo demo%')
    BEGIN
        INSERT INTO RefereeReport (AssignmentId, Content, ViolationNote, CreatedAt, ReportedUserId, ReportedHorseId)
        VALUES (
            @assignId, 
            N'Báo cáo demo - Cuộc đua diễn ra an toàn. Có 1 vi phạm lấn làn nhỏ đã được ghi nhận.',
            N'Lấn làn nhẹ ở làn 2',
            GETDATE(),
            3,  -- ReportedUserId (Jockey)
            1   -- ReportedHorseId
        );
        PRINT '  Created demo RefereeReport';
    END
END
GO

-- ============================================================
-- ENSURE RACE RESULT
-- RaceResult: Id, RaceId, Winner
-- ============================================================
PRINT '>>> Ensuring RaceResult data...';
DECLARE @finishedRaceId INT;
SELECT TOP 1 @finishedRaceId = RaceId FROM Race WHERE Status IN ('Finished', 'Completed', 'Published') ORDER BY RaceId;

IF @finishedRaceId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RaceResult WHERE RaceId = @finishedRaceId)
    BEGIN
        -- Get horse name for winner
        DECLARE @winnerName NVARCHAR(256);
        SELECT TOP 1 @winnerName = h.Name 
        FROM RaceEntry re 
        JOIN Registration reg ON re.RegistrationId = reg.Id
        JOIN Horse h ON reg.HorseId = h.Id
        WHERE re.RaceId = @finishedRaceId;

        IF @winnerName IS NOT NULL
        BEGIN
            INSERT INTO RaceResult (RaceId, Winner) VALUES (@finishedRaceId, @winnerName);
            PRINT '  Created RaceResult with winner: ' + @winnerName;
        END
        ELSE
        BEGIN
            INSERT INTO RaceResult (RaceId, Winner) VALUES (@finishedRaceId, N'Thần Phong');
            PRINT '  Created RaceResult with default winner';
        END
    END
END
GO

-- ============================================================
-- ENSURE BET DATA (Spectator bets for testing)
-- Bet: Id, UserId, RaceId, HorseId, Amount, Odds, Status, CreatedAt
-- ============================================================
PRINT '>>> Ensuring Bet data for Spectator (UserId=5)...';
DECLARE @raceId INT, @horseId INT;
SELECT TOP 1 @raceId = RaceId FROM Race WHERE Status IN ('Scheduled', 'Live') ORDER BY RaceId;
SELECT TOP 1 @horseId = Id FROM Horse ORDER BY Id;

IF @raceId IS NOT NULL AND @horseId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Bet WHERE UserId = 5 AND RaceId = @raceId AND Status = 'Pending')
    BEGIN
        INSERT INTO Bet (UserId, RaceId, HorseId, Amount, Odds, Status, CreatedAt)
        VALUES (5, @raceId, @horseId, 150000.00, 2.50, 'Pending', GETDATE());
        PRINT '  Created Pending Bet for Spectator';
    END
END

-- Won bet
DECLARE @finishedRaceId INT;
SELECT TOP 1 @finishedRaceId = RaceId FROM Race WHERE Status IN ('Finished', 'Completed', 'Published') ORDER BY RaceId;
SELECT TOP 1 @horseId = Id FROM Horse ORDER BY Id;

IF @finishedRaceId IS NOT NULL AND @horseId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Bet WHERE UserId = 5 AND RaceId = @finishedRaceId AND Status = 'Won')
    BEGIN
        INSERT INTO Bet (UserId, RaceId, HorseId, Amount, Odds, Status, CreatedAt)
        VALUES (5, @finishedRaceId, @horseId, 200000.00, 3.00, 'Won', DATEADD(DAY, -1, GETDATE()));
        PRINT '  Created Won Bet for Spectator';
    END

    IF NOT EXISTS (SELECT 1 FROM Bet WHERE UserId = 5 AND RaceId = @finishedRaceId AND Status = 'Lost')
    BEGIN
        INSERT INTO Bet (UserId, RaceId, HorseId, Amount, Odds, Status, CreatedAt)
        VALUES (5, @finishedRaceId, @horseId, 50000.00, 2.00, 'Lost', DATEADD(DAY, -1, GETDATE()));
        PRINT '  Created Lost Bet for Spectator';
    END
END
GO

-- ============================================================
-- ENSURE PAYOUT DATA (for won bets)
-- Payout: Id, BetId, Amount, CreatedAt
-- ============================================================
PRINT '>>> Ensuring Payout data for Won bets...';
DECLARE @betId INT;
SELECT TOP 1 @betId = Id FROM Bet WHERE Status = 'Won' AND UserId = 5 ORDER BY Id;

IF @betId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Payout WHERE BetId = @betId)
    BEGIN
        DECLARE @betAmount DECIMAL(18,2), @betOdds DECIMAL(18,2);
        SELECT @betAmount = Amount, @betOdds = Odds FROM Bet WHERE Id = @betId;
        INSERT INTO Payout (BetId, Amount, CreatedAt)
        VALUES (@betId, @betAmount * @betOdds, GETDATE());
        PRINT '  Created Payout for Won bet';
    END
END
GO

-- ============================================================
-- ENSURE PREDICTION DATA
-- Prediction: PredictionId, UserId, RaceId, RaceEntryId, PredictedAt, Status, IsCorrect, Point
-- ============================================================
PRINT '>>> Ensuring Prediction data for Spectator (UserId=5)...';
DECLARE @raceId INT, @entryId BIGINT;
SELECT TOP 1 @raceId = re.RaceId, @entryId = re.RaceEntryId
FROM RaceEntry re 
JOIN Race r ON re.RaceId = r.RaceId
WHERE r.Status IN ('Scheduled', 'Live')
ORDER BY re.RaceEntryId;

IF @raceId IS NOT NULL AND @entryId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Prediction WHERE UserId = 5 AND RaceId = @raceId AND RaceEntryId = @entryId)
    BEGIN
        INSERT INTO Prediction (UserId, RaceId, RaceEntryId, PredictedAt, Status, IsCorrect, Point)
        VALUES (5, @raceId, @entryId, GETDATE(), 'Pending', 0, 0);
        PRINT '  Created Pending Prediction for Spectator';
    END
END

-- Settled prediction (correct)
DECLARE @finishedRaceId INT, @finishedEntryId BIGINT;
SELECT TOP 1 @finishedRaceId = re.RaceId, @finishedEntryId = re.RaceEntryId
FROM RaceEntry re
JOIN Race r ON re.RaceId = r.RaceId
WHERE r.Status IN ('Finished', 'Completed', 'Published')
ORDER BY re.RaceEntryId;

IF @finishedRaceId IS NOT NULL AND @finishedEntryId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Prediction WHERE UserId = 5 AND RaceId = @finishedRaceId AND Status = 'Settled' AND IsCorrect = 1)
    BEGIN
        INSERT INTO Prediction (UserId, RaceId, RaceEntryId, PredictedAt, Status, IsCorrect, Point)
        VALUES (5, @finishedRaceId, @finishedEntryId, DATEADD(DAY, -1, GETDATE()), 'Settled', 1, 100);
        PRINT '  Created Settled (correct) Prediction';
    END
END
GO

-- ============================================================
-- ENSURE NOTIFICATIONS for all roles
-- ============================================================
PRINT '>>> Ensuring sample Notifications...';

-- Admin notification
IF NOT EXISTS (SELECT 1 FROM Notification WHERE UserId = 1 AND Message LIKE N'%đăng ký mới%')
BEGIN
    INSERT INTO Notification (UserId, Message, IsRead, CreatedAt)
    VALUES (1, N'Có 1 đăng ký mới cần phê duyệt cho giải đấu Vô Địch Quốc Gia 2026', 0, GETDATE());
    PRINT '  Created notification for Admin';
END

-- HorseOwner notification
IF NOT EXISTS (SELECT 1 FROM Notification WHERE UserId = 2 AND Message LIKE N'%đăng ký%phê duyệt%')
BEGIN
    INSERT INTO Notification (UserId, Message, IsRead, CreatedAt)
    VALUES (2, N'Đăng ký của Bạch Mã đã được phê duyệt. Chúc mừng!', 0, GETDATE());
    PRINT '  Created notification for HorseOwner';
END

-- Jockey notification
IF NOT EXISTS (SELECT 1 FROM Notification WHERE UserId = 3 AND Message LIKE N'%hợp đồng%')
BEGIN
    INSERT INTO Notification (UserId, Message, IsRead, CreatedAt)
    VALUES (3, N'Bạn có lời mời hợp đồng mới từ chủ ngựa. Vui lòng xem xét và phản hồi.', 0, GETDATE());
    PRINT '  Created notification for Jockey';
END

-- Referee notification  
IF NOT EXISTS (SELECT 1 FROM Notification WHERE UserId = 4 AND Message LIKE N'%được phân công%')
BEGIN
    INSERT INTO Notification (UserId, Message, IsRead, CreatedAt)
    VALUES (4, N'Bạn đã được phân công làm trọng tài cho trận Race 1 - Qualifying.', 0, GETDATE());
    PRINT '  Created notification for Referee';
END

-- Spectator notification
IF NOT EXISTS (SELECT 1 FROM Notification WHERE UserId = 5 AND Message LIKE N'%cược%thắng%')
BEGIN
    INSERT INTO Notification (UserId, Message, IsRead, CreatedAt)
    VALUES (5, N'Cược của bạn đã thắng! Số tiền 600,000 VNĐ đã được cộng vào ví.', 0, GETDATE());
    PRINT '  Created notification for Spectator';
END
GO

-- ============================================================
-- FINAL DATA SUMMARY
-- ============================================================
PRINT '>>> Final data summary after seeding:';
SELECT 'AppUser' AS [Table], COUNT(*) AS [Count] FROM AppUser
UNION ALL SELECT 'Tournament', COUNT(*) FROM Tournament
UNION ALL SELECT 'Round', COUNT(*) FROM Round
UNION ALL SELECT 'Race', COUNT(*) FROM Race
UNION ALL SELECT 'Horse', COUNT(*) FROM Horse
UNION ALL SELECT 'Registration', COUNT(*) FROM Registration
UNION ALL SELECT 'JockeyContract', COUNT(*) FROM JockeyContract
UNION ALL SELECT 'RaceEntry', COUNT(*) FROM RaceEntry
UNION ALL SELECT 'RaceRefereeAssignment', COUNT(*) FROM RaceRefereeAssignment
UNION ALL SELECT 'RaceViolation', COUNT(*) FROM RaceViolation
UNION ALL SELECT 'RefereeReport', COUNT(*) FROM RefereeReport
UNION ALL SELECT 'RaceResult', COUNT(*) FROM RaceResult
UNION ALL SELECT 'Bet', COUNT(*) FROM Bet
UNION ALL SELECT 'Payout', COUNT(*) FROM Payout
UNION ALL SELECT 'Prediction', COUNT(*) FROM Prediction
UNION ALL SELECT 'Notification', COUNT(*) FROM Notification
UNION ALL SELECT 'Wallet', COUNT(*) FROM Wallet;
GO

PRINT '>>> Seed script completed successfully!';
GO
