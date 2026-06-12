-- Script kiểm tra số lượng dòng của các bảng duplicate (số nhiều) và bảng chuẩn (số ít)
-- Giúp xác nhận bảng nào chứa dữ liệu, bảng nào đang rỗng để xử lý an toàn.

SELECT 'Users' AS TableName, COUNT(*) AS RowCount FROM [Users] UNION ALL
SELECT 'AppUser', COUNT(*) FROM [AppUser] UNION ALL

SELECT 'Roles', COUNT(*) FROM [Roles] UNION ALL
SELECT 'Role', COUNT(*) FROM [Role] UNION ALL

SELECT 'JockeyProfiles', COUNT(*) FROM [JockeyProfiles] UNION ALL
SELECT 'JockeyProfile', COUNT(*) FROM [JockeyProfile] UNION ALL

SELECT 'RefereeProfiles', COUNT(*) FROM [RefereeProfiles] UNION ALL
SELECT 'RefereeProfile', COUNT(*) FROM [RefereeProfile] UNION ALL

SELECT 'Wallets', COUNT(*) FROM [Wallets] UNION ALL
SELECT 'Wallet', COUNT(*) FROM [Wallet] UNION ALL

SELECT 'Bets', COUNT(*) FROM [Bets] UNION ALL
SELECT 'Bet', COUNT(*) FROM [Bet] UNION ALL

SELECT 'Horses', COUNT(*) FROM [Horses] UNION ALL
SELECT 'Horse', COUNT(*) FROM [Horse] UNION ALL

SELECT 'Notifications', COUNT(*) FROM [Notifications] UNION ALL
SELECT 'Notification', COUNT(*) FROM [Notification] UNION ALL

SELECT 'Payouts', COUNT(*) FROM [Payouts] UNION ALL
SELECT 'Payout', COUNT(*) FROM [Payout] UNION ALL

SELECT 'Prizes', COUNT(*) FROM [Prizes] UNION ALL
SELECT 'Prize', COUNT(*) FROM [Prize] UNION ALL

SELECT 'Races', COUNT(*) FROM [Races] UNION ALL
SELECT 'Race', COUNT(*) FROM [Race] UNION ALL

SELECT 'RaceEntries', COUNT(*) FROM [RaceEntries] UNION ALL
SELECT 'RaceEntry', COUNT(*) FROM [RaceEntry] UNION ALL

SELECT 'RaceResults', COUNT(*) FROM [RaceResults] UNION ALL
SELECT 'RaceResult', COUNT(*) FROM [RaceResult] UNION ALL

SELECT 'Violations', COUNT(*) FROM [Violations] UNION ALL
SELECT 'RaceViolation', COUNT(*) FROM [RaceViolation] UNION ALL

SELECT 'TournamentPrizePayouts', COUNT(*) FROM [TournamentPrizePayouts] UNION ALL
SELECT 'TournamentPrizePayout', COUNT(*) FROM [TournamentPrizePayout] UNION ALL

SELECT 'Transactions', COUNT(*) FROM [Transactions] UNION ALL
SELECT 'WalletTransaction', COUNT(*) FROM [WalletTransaction] UNION ALL

SELECT 'Tournaments', COUNT(*) FROM [Tournaments] UNION ALL
SELECT 'Tournament', COUNT(*) FROM [Tournament] UNION ALL

SELECT 'Predictions', COUNT(*) FROM [Predictions] UNION ALL
SELECT 'Prediction', CASE WHEN OBJECT_ID('Prediction') IS NOT NULL THEN (SELECT COUNT(*) FROM [Prediction]) ELSE 0 END;
