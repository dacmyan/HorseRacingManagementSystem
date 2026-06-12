# Clean Database Recreate Report

## 1. Database bị drop
- **Tên Database:** `HorseRacingManagementSystem`
- **Connection String:** `Server=localhost;Database=HorseRacingManagementSystem;User Id=sa;Password=12345;TrustServerCertificate=True;`
- **Môi trường:** Local / Dev (xác thực chạy trên `localhost`). An toàn tuyệt đối để thực hiện xóa sạch.
- **Cam kết:** Đồng ý chấp nhận mất dữ liệu cũ trên môi trường phát triển local này.

## 2. Lý do recreate database
- Giải quyết triệt để vấn đề xung đột và nhân bản bảng dạng số nhiều (`Users`, `Roles`, `Races`, `RaceEntries`, `RaceResults`, `Wallets`,...) phát sinh do thiếu cấu hình mapping trong EF Core ở các commit trước.
- Xây dựng lại schema chuẩn hóa dạng số ít (singular) từ đầu để đảm bảo tính đồng nhất giữa thiết kế cơ sở dữ liệu, code C# Domain Entities và Fluent API.

## 3. Source schema được dùng
- Sử dụng các câu lệnh tạo bảng, khóa chính, khóa ngoại và chỉ mục nguyên bản từ các Migration lịch sử của dự án.
- Bổ dung cấu trúc hoàn chỉnh cho bảng `RefereeReport` với các cột mới:
  * `ReportedUserId INT NULL` (Tham chiếu tới `AppUser(UserId)`)
  * `ReportedHorseId INT NULL` (Tham chiếu tới `Horse(Id)`)
  * Khóa ngoại `FK_RefereeReport_AppUser` và `FK_RefereeReport_Horse`.

## 4. Danh sách bảng được tạo
Bao gồm 24 bảng thực thể singular chuẩn theo thiết kế DB:
1. `Role`
2. `AppUser`
3. `JockeyProfile`
4. `RefereeProfile`
5. `Horse`
6. `HorseDocument`
7. `HorseStatistic`
8. `Tournament`
9. `Round`
10. `JockeyContract`
11. `Registration`
12. `Race`
13. `RaceEntry`
14. `RaceResult`
15. `Prize`
16. `Wallet`
17. `TournamentPrizePayout`
18. `RaceRefereeAssignment`
19. `RefereeReport`
20. `RaceViolation`
21. `Bet`
22. `Payout`
23. `WalletTransaction`
24. `Notification`

Các bảng bổ trợ:
- `Prediction`
- `__EFMigrationsHistory`

## 5. Seed data đã thêm
- **Bảng `Role`:** Seed 5 vai trò hệ thống (`Admin`, `HorseOwner`, `Jockey`, `Referee`, `Spectator`).
- **Bảng `AppUser`:** Seed 5 tài khoản thử nghiệm tương ứng (`admin`, `owner`, `jockey`, `referee`, `spectator`) với mật khẩu băm bảo mật (mật khẩu gốc `123456`).
- **Dữ liệu bổ trợ:** Seed hồ sơ Jockey Profile (nài ngựa 3 năm kinh nghiệm), Referee Profile (giấy phép `LIC-REF-001`), và ví Spectator với số dư ban đầu bằng `0`.

## 6. Kết quả kiểm tra duplicate table
- **Kết quả query kiểm tra bảng plural:** Đã thực hiện kiểm tra qua `INFORMATION_SCHEMA.TABLES` tìm các bảng số nhiều (`Users`, `Roles`, `Horses`, `Tournaments`, `RaceEntries`, `RaceResults`, `WalletTransactions`, `Notifications`). Kết quả trả về `(0 rows affected)`, xác nhận không còn tồn tại bất kỳ bảng plural cũ nào.
- **Danh sách bảng base table hiện có:** Có tổng cộng 26 bảng (24 bảng singular thực thể chuẩn + `Prediction` + `__EFMigrationsHistory`), toàn bộ đều ở dạng số ít chuẩn xác.

## 7. Kết quả kiểm tra AppDbContext mapping
- **Thực thể mới:** Lớp thực thể [RefereeReport.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Compliance/RefereeReport.cs) đã được tạo mới hoàn chỉnh với các thuộc tính khóa ngoại chuẩn (`ReportedUserId`, `ReportedHorseId` kiểu `int?`).
- **Mapping:** [AppDbContext.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/AppDbContext.cs) đã được cập nhật bổ sung `DbSet<RefereeReport> RefereeReports` và Fluent API ánh xạ `.ToTable("RefereeReport")` với cấu hình khóa ngoại `Restrict` để tránh lỗi cascade loop của SQL Server.

## 8. Kết quả kiểm tra migration cũ
- **Lịch sử migration:** Tất cả 5 file migration lịch sử và 1 file snapshot đã được đồng bộ với DB.
- **Migration bổ sung:** Đã tạo thêm migration `20260612025302_AddRefereeReport` để EF Core snapshot nhận diện cấu trúc thực thể `RefereeReport` mới thêm. Bản ghi migration này đã được cập nhật thủ công vào bảng `__EFMigrationsHistory` để tránh việc EF chạy lại câu lệnh tạo bảng.

## 9. Kết quả build
- **Trạng thái compile:** Build thành công giải pháp `HorseRacing.sln` với kết quả: `0 Error(s)`, `44 Warning(s)` (chỉ là cảnh báo về lỗ hổng bảo mật của các thư viện nuget bên thứ ba).

## 10. Kết quả chạy API/Swagger/Health Check
- **API Health Check kết nối DB:** Phản hồi thành công tại route `http://localhost:5001/api/health/db` với JSON:
  ```json
  {"status":"success","message":"Database connected successfully"}
  ```
- **Swagger UI:** OpenAPI schema json được tải thành công tại `http://localhost:5001/swagger/v1/swagger.json` với đầy đủ định nghĩa các controller.

## 11. Lỗi còn lại nếu có
- **Không có lỗi.** Toàn bộ database local đã sạch bóng các bảng thừa và đồng bộ hoàn hảo với codebase.

