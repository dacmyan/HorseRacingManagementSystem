# Database Schema Fix Report

## 1. Lỗi schema ban đầu
- Database xuất hiện các bảng trùng lặp ở dạng số nhiều (plural).
- Bảng `RaceEntry` sai tên khóa chính (`Id`), sai kiểu (`int`), thiếu các cột `RegistrationId`, `WinningProbability`, `CurrentOdds`, `LaneNo` và trỏ sai FK `JockeyId` về `AppUser`.
- Bảng `RefereeReport` sai tên khóa chính (`Id`), sai kiểu (`int`), thiếu `ViolationNote`, và trỏ trực tiếp đến `RaceId` / `RefereeId` thay vì qua bảng trung gian `RaceRefereeAssignment`.
- Bảng `JockeyContract` thiếu `TournamentId`, dư `OwnerId` (do Owner sở hữu Horse và Horse liên kết JockeyContract).
- Bảng `WalletTransaction` thiếu các trường liên kết nullable `BetId`, `PayoutId`, `PrizePayoutId`.
- Bảng `Horse` thừa cột `RegistrationId`.
- Bảng `Prize` dùng cột `Rank` thay vì `RankPosition`.
- Bảng `Prediction` dư thừa so với 24 bảng trong DB schema chuẩn.

## 2. Các entity đã sửa
Đã chỉnh sửa cấu trúc các lớp thực thể trong project `backend/src/HorseRacing.Domain/Entities/` bao gồm:
- **`RaceEntry.cs`**: Đổi PK sang `long RaceEntryId`, thay `HorseId` bằng `int RegistrationId`, đặt `int? JockeyId` trỏ tới `JockeyProfile`, thêm `WinningProbability`, `CurrentOdds` và `LaneNo`.
- **`RefereeReport.cs`**: Đổi PK sang `long ReportId`, thay thế `RaceId`/`RefereeId` bằng `long AssignmentId` trỏ tới `RaceRefereeAssignment`, thêm `ViolationNote`.
- **`JockeyContract.cs`**: Sửa PK thành `ContractId`, thêm `long TournamentId`, loại bỏ `OwnerId`.
- **`WalletTransaction.cs`**: Sửa PK thành `TransactionId`, thêm nullable FK `BetId`, `PayoutId`, `PrizePayoutId`.
- **`Horse.cs`**: Loại bỏ `RegistrationId` và quan hệ circular.
- **`Prize.cs`**: Sửa `Rank` thành `RankPosition`.
- **`Prediction.cs`**: Gỡ bỏ hoàn toàn thực thể cùng các DTO và Logic nghiệp vụ liên quan để khớp 100% với 24 bảng của Schema chuẩn.

## 3. AppDbContext mapping đã sửa
- Cấu hình lại các mapping trong `AppDbContext.cs` bằng Fluent API `.ToTable("TênBảngSốÍt")`.
- Cấu hình khóa chính cho `RaceEntryId`, `ReportId`, `ContractId`, `TransactionId`.
- Cài đặt đầy đủ các Unique Index bắt buộc:
  - `Round`: `(TournamentId, RoundNumber)`
  - `JockeyContract`: `(TournamentId, HorseId, JockeyId)`
  - `Registration`: `(TournamentId, HorseId)`
  - `RaceEntry`: `(RaceId, LaneNo)` và `(RaceId, RegistrationId)`
  - `Prize`: `(TournamentId, RankPosition)`
  - `RaceRefereeAssignment`: `(RaceId, RefereeId)`
- Gỡ bỏ `Prediction` DbSet và mapping khỏi `AppDbContext.cs`.

## 4. SQL recreate script đã sửa
Tệp tin [recreate-clean-database.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/recreate-clean-database.sql) đã được sửa đổi toàn bộ:
- Cập nhật đúng cấu trúc cột và khóa chính/phụ của 24 bảng.
- Không chứa bất kỳ bảng số nhiều (plural) nào.
- Gỡ bỏ hoàn toàn bảng `Prediction` và các tham chiếu liên quan.
- Thêm các lệnh tạo Unique Index/Constraint.
- Cập nhật lịch sử EF migrations seeded tại bảng `__EFMigrationsHistory` với migration mới nhất `20260612035556_AlignDatabaseWithStandardSchema`.

## 5. Migration mới
Tạo thành công migration EF Core:
- **Tên:** `20260612035556_AlignDatabaseWithStandardSchema`
- Cấu trúc file di trú tự động xác định việc loại bỏ/đổi tên các cột và khóa chính/phụ của các bảng, gỡ bỏ bảng `Prediction`, đồng thời đồng bộ hóa hoàn toàn model snapshot với code thực tế.

## 6. Database local đã recreate
Đã thực thi script tái tạo database local thông qua `sqlcmd`:
- Lệnh chạy: `sqlcmd -S localhost -U sa -P 12345 -i docs/sql/recreate-clean-database.sql`
- Kết quả: Drop database cũ và tạo mới thành công, seed đầy đủ dữ liệu tĩnh Roles, Profiles, Accounts mẫu cùng lịch sử di trú.

## 7. Kết quả verify SQL Server
Chạy truy vấn lấy danh sách bảng và cột thực tế trong SQL Server:
- Bảng thực tế: Đúng 24 bảng singular chuẩn + bảng `__EFMigrationsHistory`. Không có bất kỳ bảng số nhiều hay bảng `Prediction` dư thừa nào.
- Bảng `RaceEntry`: Đầy đủ `RaceEntryId (bigint)`, `RaceId (bigint)`, `RegistrationId (int)`, `JockeyId (int, null)`, `WinningProbability (decimal)`, `CurrentOdds (decimal)`, `LaneNo (int)`, `Status (nvarchar)`.
- Bảng `RefereeReport`: Đầy đủ `ReportId (bigint)`, `AssignmentId (bigint)`, `Content`, `ViolationNote`, `CreatedAt`, `ReportedUserId`, `ReportedHorseId`.
- Bảng `WalletTransaction`: Đầy đủ `BetId`, `PayoutId`, `PrizePayoutId`.
- Bảng `JockeyContract`: Đầy đủ `TournamentId`, không còn `OwnerId`.
- Bảng `Horse`: Đã gỡ `RegistrationId`.
- Bảng `Prize`: Dùng `RankPosition`.

## 8. Kết quả build
- Lệnh chạy: `dotnet build` tại thư mục backend.
- Kết quả: **Build Succeeded** - 0 Error(s), 44 Warning(s).

## 9. Kết quả Swagger/Health Check
- API host: `http://localhost:5001`
- Swagger UI hoạt động bình thường tại `http://localhost:5001/swagger`.
- Database Health Check endpoint `http://localhost:5001/api/health/db` trả về phản hồi:
  ```json
  {"status":"success","message":"Database connected successfully"}
  ```

## 10. Lỗi còn lại nếu có
- **Không có**. Hệ thống đồng bộ 100% từ Database thật, Migration, Mappings Fluent API, C# Entities cho đến API Endpoint/Services.
