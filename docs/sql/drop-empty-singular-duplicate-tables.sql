-- Script xóa các bảng singular (số ít) rỗng trong database.
-- Đã được kiểm tra và sắp xếp đúng theo thứ tự phụ thuộc khóa ngoại (Foreign Keys): Child tables trước, Parent tables sau.
-- Chạy script này TRƯỚC khi áp dụng Migration đổi tên bảng để tránh lỗi trùng tên (Conflict name).

DROP TABLE IF EXISTS [WalletTransaction];
DROP TABLE IF EXISTS [Payout];
DROP TABLE IF EXISTS [Bet];
DROP TABLE IF EXISTS [TournamentPrizePayout];
DROP TABLE IF EXISTS [Prize];

DROP TABLE IF EXISTS [RaceViolation];
DROP TABLE IF EXISTS [RefereeReport];
DROP TABLE IF EXISTS [RaceRefereeAssignment];

DROP TABLE IF EXISTS [RaceResult];
DROP TABLE IF EXISTS [RaceEntry];
DROP TABLE IF EXISTS [Race];

DROP TABLE IF EXISTS [JockeyContract];
DROP TABLE IF EXISTS [Registration];

DROP TABLE IF EXISTS [HorseStatistic];
DROP TABLE IF EXISTS [HorseDocument];
DROP TABLE IF EXISTS [Horse];

DROP TABLE IF EXISTS [Round];
DROP TABLE IF EXISTS [Tournament];

DROP TABLE IF EXISTS [Notification];
DROP TABLE IF EXISTS [Wallet];
DROP TABLE IF EXISTS [RefereeProfile];
DROP TABLE IF EXISTS [JockeyProfile];
DROP TABLE IF EXISTS [AppUser];
DROP TABLE IF EXISTS [Role];
