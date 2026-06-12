# Full Database Schema Audit

Tài liệu này báo cáo chi tiết kết quả đối chiếu 100% cấu trúc của SQL Server database thực tế, mã nguồn Entity C#, cấu hình mapping AppDbContext, các tệp Migration, SQL recreate script, và các lớp DTO/API liên quan với **Thiết kế DB Schema chuẩn**.

---

## 1. Database đang kiểm tra
* **Kết nối cấu hình:**
  * **Server:** `localhost` (SQL Server)
  * **Database Name:** `HorseRacingManagementSystem`
  * **Authentication:** SQL Server Authentication (`sa` user)
  * **Tệp cấu hình:** [appsettings.json](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/appsettings.json) và [appsettings.Development.json](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/appsettings.Development.json)
* **Xác nhận kết nối thực tế:**
  * Đã chạy lệnh `SELECT DB_NAME();` thành công trên instance SQL Server local. Cơ sở dữ liệu đang kiểm tra chính xác là **`HorseRacingManagementSystem`**.

---

## 2. DB schema chuẩn được dùng để đối chiếu
Gồm **24 bảng singular** và các cấu hình index/ràng buộc bắt buộc như mô tả trong yêu cầu.

---

## 3. Kết quả kiểm tra danh sách bảng
Chạy truy vấn lấy danh sách bảng thực tế đang có trong SQL Server:
```sql
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;
```

### Bảng đối chiếu thực tế:

| Table trong SQL Server | Có trong schema chuẩn không | Ghi chú |
| :--- | :---: | :--- |
| `__EFMigrationsHistory` | **Không** | Bảng metadata của EF Core để theo dõi migration. |
| `AppUser` | **Có** | Khớp 100%. |
| `Bet` | **Có** | Khớp 100%. |
| `Horse` | **Có** | Khớp 100%. |
| `HorseDocument` | **Có** | Khớp 100%. |
| `HorseStatistic` | **Có** | Khớp 100%. |
| `JockeyContract` | **Có** | Khớp 100%. |
| `JockeyProfile` | **Có** | Khớp 100%. |
| `Notification` | **Có** | Khớp 100%. |
| `Payout` | **Có** | Khớp 100%. |
| `Prediction` | **Không** | Bảng dư thừa trong database (Dùng bổ trợ dự đoán kết quả). |
| `Prize` | **Có** | Khớp 100%. |
| `Race` | **Có** | Khớp 100%. |
| `RaceEntry` | **Có** | Khớp 100%. |
| `RaceRefereeAssignment` | **Có** | Khớp 100%. |
| `RaceResult` | **Có** | Khớp 100%. |
| `RaceViolation` | **Có** | Khớp 100%. |
| `RefereeProfile` | **Có** | Khớp 100%. |
| `RefereeReport` | **Có** | Khớp 100%. |
| `Registration` | **Có** | Khớp 100%. |
| `Role` | **Có** | Khớp 100%. |
| `Round` | **Có** | Khớp 100%. |
| `Tournament` | **Có** | Khớp 100%. |
| `TournamentPrizePayout` | **Có** | Khớp 100%. |
| `Wallet` | **Có** | Khớp 100%. |
| `WalletTransaction` | **Có** | Khớp 100%. |

### Kết luận danh sách bảng:
* **Thiếu bảng:** Không thiếu bảng nào trong số 24 bảng chuẩn.
* **Dư bảng:** Dư 1 bảng là `Prediction` (đang có dữ liệu code hỗ trợ nên tạm thời giữ lại).
* **Bảng plural/duplicate:** **Không có**. Toàn bộ các bảng số nhiều (plural) trùng lặp cũ đã được dọn sạch hoàn toàn, chỉ giữ lại các bảng singular chuẩn.

---

## 4. Kết quả kiểm tra từng cột
Đối chiếu chi tiết giữa schema thực tế (trong file dump CSV) với schema thiết kế chuẩn phát hiện các sai lệch sau:

1. **Bảng `Horse`:**
   * Thực tế có thêm cột `RegistrationId (int, null)` làm FK trỏ sang `Registration`.
   * *Sai lệch:* Đây là quan hệ 1-N (một Horse đăng ký nhiều giải đấu thông qua nhiều bản ghi Registration). Việc đặt `RegistrationId` trực tiếp ở bảng `Horse` là sai thiết kế quan hệ và bị dư thừa, vì bảng `Registration` đã có cột `HorseId` trỏ ngược lại.
2. **Bảng `JockeyContract`:**
   * Thực tế thiếu cột `TournamentId`.
3. **Bảng `Prize`:**
   * Thực tế dùng cột tên `Rank (int)` thay vì `RankPosition` như kỳ vọng thiết kế.
4. **Bảng `WalletTransaction`:**
   * Thực tế thiếu 3 cột liên kết nghiệp vụ: `BetId`, `PayoutId`, `PrizePayoutId`.

---

## 5. Kiểm tra đặc biệt bảng RaceEntry
Đối chiếu chi tiết bảng `RaceEntry` giữa database thực tế (hoặc tệp recreate SQL) và schema chuẩn bắt buộc:

* **LaneNo có tồn tại không:** Chưa có trong database thực tế (do chưa chạy lệnh `database update`), nhưng **đã có** trong code và file migration `AddLaneNoToRaceEntry`.
* **LaneNo có phải `INT NOT NULL` không:** Cột thiết kế trong migration là `int` với thuộc tính `nullable: false`.
* **Có unique constraint `(RaceId, LaneNo)` không:** Có unique index trong cấu hình mapping và tệp migration mới, nhưng chưa được cập nhật vào database thật.
* **Có unique constraint `(RaceId, RegistrationId)` không:** **Không**.
* **Có FK tới `Race`, `Registration`, `JockeyProfile` không:**
  * FK tới `Race` (`RaceId`): Có.
  * FK tới `Registration` (`RegistrationId`): **Chưa có**. Bảng hiện tại đang liên kết trực tiếp tới `HorseId` thay vì `RegistrationId`.
  * FK tới `JockeyProfile` (`JockeyId`): Sai lệch liên kết. Khóa ngoại hiện tại trong DB là `FK_RaceEntry_AppUser_JockeyId` trỏ trực tiếp tới bảng `AppUser(UserId)` thay vì `JockeyProfile(JockeyId)`.

### So sánh cột RaceEntry thực tế và chuẩn:
| Tên cột thực tế | Tên cột chuẩn kỳ vọng | Kiểu dữ liệu thực tế | Kiểu dữ liệu chuẩn | Mức độ lệch |
| :--- | :--- | :--- | :--- | :--- |
| `Id` | `RaceEntryId` | `int` (PK) | `bigint` (PK) | High (Sai kiểu & tên PK) |
| `RaceId` | `RaceId` | `bigint` | `bigint` | Khớp |
| `HorseId` | `RegistrationId` | `int` | `bigint` | High (Lệch thiết kế quan hệ) |
| `JockeyId` | `JockeyId` | `int` (FK to AppUser) | `bigint` (FK to JockeyProfile) | High (Sai đích trỏ FK) |
| `Status` | `Status` | `nvarchar(max)` | `varchar(50) DEFAULT 'Ready'` | Medium (Sai loại string) |
| N/A | `WinningProbability` | N/A | `decimal(5,2)` | High (Thiếu cột) |
| N/A | `CurrentOdds` | N/A | `decimal(10,2)` | High (Thiếu cột) |
| `LaneNo` | `LaneNo` | N/A (Đang ở migration) | `int` | High (Thiếu trong DB thật) |

---

## 6. Kiểm tra đặc biệt RefereeReport
Đối chiếu chi tiết bảng `RefereeReport` thực tế với schema chuẩn bắt buộc:

* **Có cột ReportedUserId và ReportedHorseId không:** Có.
* **Có FK tương ứng về AppUser(UserId) và Horse(Id) không:** Có.
* **Sai lệch phát hiện:**
  * **Primary Key:** Thực tế là cột `Id (int)` trong khi chuẩn yêu cầu `ReportId (bigint)`.
  * **Liên kết giải đấu:** Thực tế sử dụng `RaceId (bigint)` và `RefereeId (int)` trực tiếp trên bảng, trong khi chuẩn yêu cầu liên kết thông qua `AssignmentId (bigint)` trỏ tới bảng trung gian `RaceRefereeAssignment(AssignmentId)`.
  * **Thiếu cột:** Thiếu cột `ViolationNote (text)`.
  * **Kiểu dữ liệu:** Cột `Content` thực tế là `nvarchar(max)` thay vì `text`.

---

## 7. Kết quả kiểm tra Entity C#
Scan toàn bộ thư mục `src/HorseRacing.Domain/Entities/` và đối chiếu thuộc tính:

| Entity | File | Table chuẩn | Property thiếu | Property dư | Sai kiểu dữ liệu | Ghi chú |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `RaceEntry` | [RaceEntry.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/RaceEntry.cs) | `RaceEntry` | `RegistrationId`, `WinningProbability`, `CurrentOdds` | `HorseId` | `Id` (nên là `RaceEntryId` kiểu `long`), `JockeyId` (nên là `long?`) | Cần tái cấu trúc toàn diện theo schema chuẩn. |
| `RefereeReport` | [RefereeReport.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Compliance/RefereeReport.cs) | `RefereeReport` | `AssignmentId`, `ViolationNote` | `RaceId`, `RefereeId` | `Id` (nên là `ReportId` kiểu `long`) | Cần cấu hình lại để liên kết qua `RaceRefereeAssignment`. |
| `WalletTransaction` | [WalletTransaction.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/WalletTransaction.cs) | `WalletTransaction` | `BetId`, `PayoutId`, `PrizePayoutId` | Không | Không | Thiếu các cột liên kết giao dịch tài chính. |
| `JockeyContract` | [JockeyContract.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/JockeyContract.cs) | `JockeyContract` | `TournamentId` | `OwnerId` | Không | Thiếu cột liên kết Tournament. |
| `Registration` | [Registration.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/Registration.cs) | `Registration` | Không | `Horses` (ICollection) | Không | Dư thừa navigation property `Horses` ở quan hệ 1-N. |

---

## 8. Kết quả kiểm tra AppDbContext mapping
* **Tệp cấu hình:** [AppDbContext.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/AppDbContext.cs)
* **Đánh giá mapping bảng singular:** Đạt yêu cầu. Toàn bộ 24 thực thể đều đã được cấu hình mapping bằng Fluent API `.ToTable("TênBảngSốÍt")` hoặc ánh xạ mặc định số ít.
* **Đối chiếu các Unique Index bắt buộc:**
  1. `Round` (`TournamentId`, `RoundNumber`): **Thiếu** unique index mapping.
  2. `JockeyContract` (`TournamentId`, `HorseId`, `JockeyId`): **Thiếu** unique index mapping (và thiếu thuộc tính `TournamentId`).
  3. `Registration` (`TournamentId`, `HorseId`): **Thiếu** unique index mapping.
  4. `RaceEntry` (`RaceId`, `LaneNo`): **Có** unique index mapping.
  5. `RaceEntry` (`RaceId`, `RegistrationId`): **Thiếu** unique index mapping.
  6. `Prize` (`TournamentId`, `RankPosition`): **Thiếu** unique index mapping.
  7. `RaceRefereeAssignment` (`RaceId`, `RefereeId`): **Thiếu** unique index mapping.

---

## 9. Kết quả kiểm tra DTO/API
* **Tạo RaceEntry:** Chưa có DTO và API xử lý tạo hay cập nhật `RaceEntry`. `LaneNo` chưa được validate hay kiểm tra trùng lặp ở tầng API/Service do chưa có code logic nghiệp vụ này.
* **RefereeReport:** Chưa có API/Controller hay DTO xử lý cho `RefereeReport`.
* **Betting:** 
  * API đặt cược `PlaceBetRequest` hiện tại nhận `RaceId` và `HorseId` trực tiếp thay vì nhận `RaceEntryId`.
  * Điều này khớp với thiết kế thực thể `Bet.cs` hiện tại, nhưng bị lệch nếu nghiệp vụ yêu cầu Bet liên kết thông qua `RaceEntryId`.

---

## 10. Kết quả kiểm tra Migration
* **Bảng plural:** Trong lịch sử migration, các lệnh ban đầu (`20260609193021_InitialCreate` và `20260610083404_AddBetPayoutPrizeNotification`) đã tạo ra các bảng dạng số nhiều (`Users`, `Roles`, `RaceEntries`,...). Tuy nhiên, các migration sau đó đã đổi tên chúng thành dạng số ít và dọn dẹp sạch sẽ.
* **Tình trạng hiện tại:** Các migration sau cùng (`FixSingularTableMapping` và `FixRuntimeUserSeeding`) đã đưa model snapshot về dạng số ít chuẩn. Migration untracked `AddLaneNoToRaceEntry` vừa tạo cũng chuẩn bị đầy đủ việc thêm cột `LaneNo` và sửa lỗi shadow column.

---

## 11. Kết quả kiểm tra SQL recreate script
* **Tệp tin:** [recreate-clean-database.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/recreate-clean-database.sql)
* **Đánh giá đối chiếu:**
  * Script tạo đủ 24 bảng chuẩn số ít, cộng thêm bảng bổ trợ `Prediction` và hệ thống `__EFMigrationsHistory`.
  * Script **không** tạo bảng plural nào.
  * **Lệch cấu trúc:** Cấu trúc bảng `RaceEntry` và `RefereeReport` trong file script SQL này vẫn đang ở định dạng cũ (giống database local thực tế của bạn), chưa khớp với các yêu cầu của schema chuẩn bắt buộc (như thiếu `RegistrationId`, thiếu `WinningProbability`, `CurrentOdds` trong `RaceEntry`; thiếu liên kết `AssignmentId` trong `RefereeReport`).
  * **Seeding:** Các câu lệnh seeding dữ liệu tĩnh `Role` và tài khoản test diễn ra an toàn, đúng loại và băm mật khẩu chuẩn.

---

## 12. Kết quả build dự án
* Chạy thử lệnh khôi phục gói nuget và biên dịch:
  * `dotnet restore HorseRacing.sln`
  * `dotnet build HorseRacing.sln`
* **Kết quả:** **0 Error(s)**, 46 Warning(s). Solution biên dịch thành công hoàn toàn.

---

## 13. Danh sách lỗi phát hiện

| Mức độ | Vị trí | Lỗi | So với schema chuẩn | Cách sửa đề xuất |
| :--- | :--- | :--- | :--- | :--- |
| **High** | `RaceEntry` (Entity & DB) | Thiếu các cột `RegistrationId`, `WinningProbability`, `CurrentOdds`, `LaneNo` (trong DB thật), và sai tên/kiểu cột khóa chính `Id` (nên là `RaceEntryId` kiểu `bigint`). | Schema chuẩn yêu cầu đầy đủ các cột trên với khóa ngoại và kiểu bigint. | Tái cấu trúc lại thực thể `RaceEntry.cs`, đổi tên PK, thêm thuộc tính, cập nhật AppDbContext mapping và tạo migration cập nhật DB. |
| **High** | `RefereeReport` (Entity & DB) | Sai khóa chính `Id` (nên là `ReportId` kiểu `bigint`), thiếu `ViolationNote`, thiết lập sai liên kết thông qua `RaceId`/`RefereeId` trực tiếp. | Schema chuẩn yêu cầu PK `ReportId`, có cột `ViolationNote`, và bắt buộc liên kết qua `AssignmentId` của bảng `RaceRefereeAssignment`. | Tái cấu trúc thực thể `RefereeReport.cs`, sửa lại quan hệ khóa ngoại và cập nhật cấu hình mapping. |
| **High** | `JockeyContract` (Entity & DB) | Thiếu cột `TournamentId`. | Schema chuẩn yêu cầu liên kết với Tournament để cấu hình unique index. | Bổ sung `TournamentId` và navigation property vào entity và mapping. |
| **High** | `WalletTransaction` (Entity & DB) | Thiếu 3 cột FK liên kết giao dịch: `BetId`, `PayoutId`, `PrizePayoutId`. | Schema chuẩn yêu cầu liên kết ví giao dịch cụ thể với Bet/Payout. | Thêm các thuộc tính này (nullable) vào entity `WalletTransaction.cs` và cấu hình FK. |
| **High** | `AppDbContext.cs` | Thiếu cấu hình Unique Index cho nhiều cặp bảng quan trọng (`Round`, `JockeyContract`, `Registration`, `Prize`, `RaceRefereeAssignment`, `RaceEntry` với `RegistrationId`). | Schema chuẩn yêu cầu cài đặt unique index Fluent API tương ứng. | Cấu hình thêm các index `.HasIndex(...).IsUnique()` trong `OnModelCreating`. |
| **Medium** | `Horse` (Entity & DB) | Dư thừa cột `RegistrationId` trên bảng `Horse`. | Schema chuẩn chỉ thiết lập quan hệ 1-N từ Tournament/Horse sang Registration (Registration giữ `HorseId`). | Loại bỏ thuộc tính `RegistrationId` khỏi entity `Horse.cs` và cập nhật database. |
| **Medium** | DTO/API | Các API/DTO và Service chưa được cài đặt để hỗ trợ validate/nhận `LaneNo` và các trường mới của `RaceEntry` hay `RefereeReport`. | Chưa exposing trường dữ liệu ra API. | Cần tạo mới các DTO và controller tương ứng khi phát triển module này. |

---

## 14. Kết luận
```text
NOT MATCH - FIX REQUIRED
```

**Lý do:** Mặc dù hệ thống đã xử lý triệt để các bảng số nhiều (plural) trùng lặp, cấu trúc thực tế của một số bảng quan trọng (đặc biệt là `RaceEntry`, `RefereeReport`, `JockeyContract`, `WalletTransaction`) vẫn đang ở phiên bản cũ và thiếu nhiều cột, ràng buộc khóa ngoại, và index duy nhất so với **Database Schema chuẩn** bắt buộc.
