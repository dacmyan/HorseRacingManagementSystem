/*
    Delete old tournaments created with the wrong round structure.

    This script only deletes invalid tournaments that have no business data
    attached outside their generated rounds/races. Tournaments with
    registrations, contracts, prizes, prize payouts, bets, or predictions are
    reported as skipped so real user data is not removed accidentally.
*/

BEGIN TRANSACTION;

DECLARE @InvalidTournamentIds TABLE (TournamentId BIGINT PRIMARY KEY);
DECLARE @DeletableTournamentIds TABLE (TournamentId BIGINT PRIMARY KEY);

INSERT INTO @InvalidTournamentIds (TournamentId)
SELECT t.TournamentId
FROM Tournament t
LEFT JOIN Round r ON r.TournamentId = t.TournamentId
GROUP BY t.TournamentId
HAVING COUNT(r.RoundId) <> 2
   OR SUM(CASE WHEN r.RoundNumber = 1 AND r.Name = 'Pre' THEN 1 ELSE 0 END) <> 1
   OR SUM(CASE WHEN r.RoundNumber = 2 AND r.Name = 'Final' THEN 1 ELSE 0 END) <> 1;

INSERT INTO @DeletableTournamentIds (TournamentId)
SELECT i.TournamentId
FROM @InvalidTournamentIds i
WHERE NOT EXISTS (SELECT 1 FROM Registration r WHERE r.TournamentId = i.TournamentId)
  AND NOT EXISTS (SELECT 1 FROM JockeyContract jc WHERE jc.TournamentId = i.TournamentId)
  AND NOT EXISTS (SELECT 1 FROM Prize p WHERE p.TournamentId = i.TournamentId)
  AND NOT EXISTS (SELECT 1 FROM TournamentPrizePayout tpp WHERE tpp.TournamentId = i.TournamentId)
  AND NOT EXISTS (
      SELECT 1
      FROM Bet b
      JOIN Race raceData ON raceData.RaceId = b.RaceId
      JOIN Round roundData ON roundData.RoundId = raceData.RoundId
      WHERE roundData.TournamentId = i.TournamentId
  )
  AND NOT EXISTS (
      SELECT 1
      FROM Prediction p
      JOIN Race raceData ON raceData.RaceId = p.RaceId
      JOIN Round roundData ON roundData.RoundId = raceData.RoundId
      WHERE roundData.TournamentId = i.TournamentId
  );

DECLARE @RaceIds TABLE (RaceId BIGINT PRIMARY KEY);

INSERT INTO @RaceIds (RaceId)
SELECT race.RaceId
FROM Race race
JOIN Round roundData ON roundData.RoundId = race.RoundId
JOIN @DeletableTournamentIds bad ON bad.TournamentId = roundData.TournamentId;

DELETE FROM RaceViolation WHERE RaceId IN (SELECT RaceId FROM @RaceIds);
DELETE FROM RaceRefereeAssignment WHERE RaceId IN (SELECT RaceId FROM @RaceIds);
DELETE FROM RaceResult WHERE RaceId IN (SELECT RaceId FROM @RaceIds);
DELETE FROM RaceEntry WHERE RaceId IN (SELECT RaceId FROM @RaceIds);
DELETE FROM Prediction WHERE RaceId IN (SELECT RaceId FROM @RaceIds);
DELETE FROM Bet WHERE RaceId IN (SELECT RaceId FROM @RaceIds);
DELETE FROM Race WHERE RaceId IN (SELECT RaceId FROM @RaceIds);
DELETE FROM Round WHERE TournamentId IN (SELECT TournamentId FROM @DeletableTournamentIds);
DELETE FROM Tournament WHERE TournamentId IN (SELECT TournamentId FROM @DeletableTournamentIds);

SELECT 'Deleted invalid tournament' AS Action, TournamentId
FROM @DeletableTournamentIds;

SELECT 'Skipped invalid tournament with business data' AS Action, TournamentId
FROM @InvalidTournamentIds
WHERE TournamentId NOT IN (SELECT TournamentId FROM @DeletableTournamentIds);

COMMIT TRANSACTION;
