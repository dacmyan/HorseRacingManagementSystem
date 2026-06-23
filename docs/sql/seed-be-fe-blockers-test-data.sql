-- ============================================================================
-- Seed Test Data for Backend FE Blockers
-- Safe to run multiple times (uses IF NOT EXISTS checks)
-- Does NOT drop tables or delete existing data
-- Uses int IDs, references existing users
-- ============================================================================

-- ============================================================================
-- 1. Sample Registration (Pending) - for testing admin approve/reject
-- ============================================================================
-- Check if we have at least one tournament and one horse to create a registration
IF EXISTS (SELECT 1 FROM Tournament) AND EXISTS (SELECT 1 FROM Horse)
BEGIN
    DECLARE @SeedTournamentId BIGINT = (SELECT TOP 1 TournamentId FROM Tournament ORDER BY TournamentId);
    DECLARE @SeedHorseId INT = (SELECT TOP 1 Id FROM Horse ORDER BY Id);
    
    IF NOT EXISTS (
        SELECT 1 FROM Registration 
        WHERE TournamentId = @SeedTournamentId AND HorseId = @SeedHorseId
    )
    BEGIN
        INSERT INTO Registration (TournamentId, HorseId, Status, CreatedAt)
        VALUES (@SeedTournamentId, @SeedHorseId, 'Pending', GETUTCDATE());
        PRINT 'Inserted sample Pending registration';
    END
    ELSE
    BEGIN
        PRINT 'Sample registration already exists';
    END
END
ELSE
BEGIN
    PRINT 'Skipping Registration seed - no Tournament or Horse found';
END
GO

-- ============================================================================
-- 2. Sample RaceEntry with LaneNo - for testing lane assignment
-- ============================================================================
-- RaceEntry requires Race + Registration + optionally JockeyProfile
-- Only seed if we have the prerequisites
IF EXISTS (SELECT 1 FROM Race) AND EXISTS (SELECT 1 FROM Registration WHERE Status = 'Approved')
BEGIN
    DECLARE @SeedRaceId BIGINT = (SELECT TOP 1 RaceId FROM Race ORDER BY RaceId);
    DECLARE @SeedRegId INT = (SELECT TOP 1 Id FROM Registration WHERE Status = 'Approved' ORDER BY Id);
    
    IF NOT EXISTS (
        SELECT 1 FROM RaceEntry 
        WHERE RaceId = @SeedRaceId AND RegistrationId = @SeedRegId
    )
    BEGIN
        INSERT INTO RaceEntry (RaceId, RegistrationId, LaneNo, Status)
        VALUES (@SeedRaceId, @SeedRegId, 1, 'Ready');
        PRINT 'Inserted sample RaceEntry with LaneNo=1';
    END
    ELSE
    BEGIN
        PRINT 'Sample RaceEntry already exists';
    END
END
ELSE
BEGIN
    PRINT 'Skipping RaceEntry seed - no Race or Approved Registration found';
END
GO

-- ============================================================================
-- 3. Sample RaceRefereeAssignment - for testing referee dashboard
-- ============================================================================
IF EXISTS (SELECT 1 FROM Race) AND EXISTS (SELECT 1 FROM RefereeProfile)
BEGIN
    DECLARE @SeedRaceIdRef BIGINT = (SELECT TOP 1 RaceId FROM Race ORDER BY RaceId);
    DECLARE @SeedRefereeId INT = (SELECT TOP 1 RefereeId FROM RefereeProfile ORDER BY RefereeId);
    
    IF NOT EXISTS (
        SELECT 1 FROM RaceRefereeAssignment 
        WHERE RaceId = @SeedRaceIdRef AND RefereeId = @SeedRefereeId
    )
    BEGIN
        INSERT INTO RaceRefereeAssignment (RaceId, RefereeId, AssignedAt, Status)
        VALUES (@SeedRaceIdRef, @SeedRefereeId, GETUTCDATE(), 'Active');
        PRINT 'Inserted sample RaceRefereeAssignment';
    END
    ELSE
    BEGIN
        PRINT 'Sample RaceRefereeAssignment already exists';
    END
END
ELSE
BEGIN
    PRINT 'Skipping RaceRefereeAssignment seed - no Race or RefereeProfile found';
END
GO

-- ============================================================================
-- 4. Sample RaceViolation - for testing violation views
-- ============================================================================
IF EXISTS (SELECT 1 FROM Race)
BEGIN
    DECLARE @SeedRaceIdViol BIGINT = (SELECT TOP 1 RaceId FROM Race ORDER BY RaceId);
    
    IF NOT EXISTS (
        SELECT 1 FROM RaceViolation WHERE RaceId = @SeedRaceIdViol
    )
    BEGIN
        INSERT INTO RaceViolation (RaceId, [Description], Penalty)
        VALUES (@SeedRaceIdViol, N'Xuất phát sớm: Ngựa xuất phát trước tín hiệu', N'Cảnh cáo');
        PRINT 'Inserted sample RaceViolation';
    END
    ELSE
    BEGIN
        PRINT 'Sample RaceViolation already exists';
    END
END
GO

-- ============================================================================
-- 5. Sample RefereeReport - for testing referee reports
-- ============================================================================
IF EXISTS (SELECT 1 FROM RaceRefereeAssignment)
BEGIN
    DECLARE @SeedAssignmentId BIGINT = (SELECT TOP 1 AssignmentId FROM RaceRefereeAssignment ORDER BY AssignmentId);
    
    IF NOT EXISTS (
        SELECT 1 FROM RefereeReport WHERE AssignmentId = @SeedAssignmentId
    )
    BEGIN
        INSERT INTO RefereeReport (AssignmentId, Content, ViolationNote, CreatedAt)
        VALUES (
            @SeedAssignmentId, 
            N'Trận đấu diễn ra suôn sẻ. Tất cả ngựa đều trong tình trạng tốt.', 
            N'Không có vi phạm nghiêm trọng',
            GETUTCDATE()
        );
        PRINT 'Inserted sample RefereeReport';
    END
    ELSE
    BEGIN
        PRINT 'Sample RefereeReport already exists';
    END
END
GO

-- ============================================================================
-- 6. Sample Prediction - for testing prediction feature
-- ============================================================================
IF EXISTS (SELECT 1 FROM Race) AND EXISTS (SELECT 1 FROM RaceEntry) AND EXISTS (SELECT 1 FROM AppUser WHERE RoleId = (SELECT RoleId FROM Role WHERE Name = 'Spectator'))
BEGIN
    DECLARE @SeedRaceIdPred BIGINT = (SELECT TOP 1 RaceId FROM Race ORDER BY RaceId);
    DECLARE @SeedEntryId BIGINT = (SELECT TOP 1 RaceEntryId FROM RaceEntry WHERE RaceId = @SeedRaceIdPred ORDER BY RaceEntryId);
    DECLARE @SeedSpectatorId INT = (SELECT TOP 1 UserId FROM AppUser WHERE RoleId = (SELECT RoleId FROM Role WHERE Name = 'Spectator') ORDER BY UserId);
    
    IF @SeedEntryId IS NOT NULL AND @SeedSpectatorId IS NOT NULL
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM Prediction WHERE UserId = @SeedSpectatorId AND RaceId = @SeedRaceIdPred
        )
        BEGIN
            INSERT INTO Prediction (UserId, RaceId, RaceEntryId, PredictedAt, Status, IsCorrect, Point)
            VALUES (@SeedSpectatorId, @SeedRaceIdPred, @SeedEntryId, GETUTCDATE(), 'Pending', NULL, 0);
            PRINT 'Inserted sample Prediction';
        END
        ELSE
        BEGIN
            PRINT 'Sample Prediction already exists';
        END
    END
    ELSE
    BEGIN
        PRINT 'Skipping Prediction seed - missing RaceEntry or Spectator';
    END
END
ELSE
BEGIN
    PRINT 'Skipping Prediction seed - prerequisites not met';
END
GO

PRINT '=== Seed script completed ===';
GO
