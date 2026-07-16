-- ================================================================
-- HOTFIX: Fix IsEmailConfirmed for all existing users
-- Run này trực tiếp trên Azure SQL nếu cần fix ngay mà không chờ deploy
--
-- Vấn đề: Migration AddEmailVerification thêm field IsEmailConfirmed
-- với defaultValue=0 (false) → tất cả user cũ bị chặn đăng nhập
-- với lỗi 401 "Tài khoản chưa được kích hoạt"
--
-- Fix: Set IsEmailConfirmed=1 cho các user:
--   - Không có VerificationToken (= đã verify hoặc không cần verify)
--   - Có PasswordHash rỗng (= Google login account)
-- ================================================================

-- Xem trước: số user bị ảnh hưởng
SELECT COUNT(*) AS AffectedUsers
FROM [AppUser]
WHERE [IsEmailConfirmed] = 0
  AND (
        [VerificationToken] IS NULL
        OR [PasswordHash] = ''
      );

-- Xem chi tiết user bị ảnh hưởng
SELECT [UserId], [Username], [Email], [FullName], [IsEmailConfirmed], 
       CASE WHEN [VerificationToken] IS NULL THEN 'No token' ELSE 'Has token' END AS TokenStatus,
       CASE WHEN [PasswordHash] = '' THEN 'Google Login' ELSE 'Normal' END AS LoginType
FROM [AppUser]
WHERE [IsEmailConfirmed] = 0
  AND (
        [VerificationToken] IS NULL
        OR [PasswordHash] = ''
      );

-- CHẠY FIX:
UPDATE [AppUser]
SET [IsEmailConfirmed] = 1
WHERE [IsEmailConfirmed] = 0
  AND (
        [VerificationToken] IS NULL
        OR [PasswordHash] = ''
      );

-- Xác nhận kết quả
SELECT COUNT(*) AS UsersStillBlocked
FROM [AppUser]
WHERE [IsEmailConfirmed] = 0
  AND (
        [VerificationToken] IS NULL
        OR [PasswordHash] = ''
      );
