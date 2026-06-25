USE HorseRacingManagementSystem;
GO

-- Make some races live
UPDATE Race SET Status = 'Live' WHERE RaceId = 1;

-- Make some races completed (waiting for referee or admin to publish)
UPDATE Race SET Status = 'Completed' WHERE RaceId = 2;

-- Make some races published
UPDATE Race SET Status = 'Published' WHERE RaceId = 3;

-- Give the referee (UserId = 4, typically RefereeId = 1) some assignments
IF NOT EXISTS (SELECT 1 FROM RaceRefereeAssignment WHERE RaceId = 1 AND RefereeId = 1)
BEGIN
    INSERT INTO RaceRefereeAssignment (RaceId, RefereeId) VALUES (1, 1);
END

IF NOT EXISTS (SELECT 1 FROM RaceRefereeAssignment WHERE RaceId = 2 AND RefereeId = 1)
BEGIN
    INSERT INTO RaceRefereeAssignment (RaceId, RefereeId) VALUES (2, 1);
END
