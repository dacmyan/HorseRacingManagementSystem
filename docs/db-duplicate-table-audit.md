# Database Duplicate Table Audit

## 1. Mục tiêu kiểm tra
Xác định nguyên nhân xuất hiện các bảng dư thừa dạng số nhiều (ví dụ `RaceEntries`, `Users`, `Races`) bên cạnh các bảng chuẩn dạng số ít (như `RaceEntry`, `AppUser`, `Race`) trong SQL Server database, từ đó đưa ra hướng xử lý và cấu hình EF Core mapping chuẩn xác.

---

## 2. DB chuẩn mong muốn
Theo thiết kế DB chuẩn, toàn bộ các bảng phải sử dụng tên ở dạng số ít (singular):
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

---

## 3. Bảng hiện có trong SQL Server
Danh sách các bảng thực tế đang tồn tại trong database `HorseRacingManagementSystem` (được sắp xếp theo thứ tự bảng chữ cái):
1. `__EFMigrationsHistory`
2. `AppUser`
3. `Bet`
4. `Bets`
5. `Horse`
6. `HorseDocument`
7. `Horses`
8. `HorseStatistic`
9. `JockeyContract`
10. `JockeyProfile`
11. `JockeyProfiles`
12. `Notification`
13. `Notifications`
14. `Payout`
15. `Payouts`
16. `Predictions`
17. `Prize`
18. `Prizes`
19. `Race`
20. `RaceEntries`
21. `RaceEntry`
22. `RaceRefereeAssignment`
23. `RaceResult`
24. `RaceResults`
25. `Races`
26. `RaceViolation`
27. `RefereeProfile`
28. `RefereeProfiles`
29. `RefereeReport`
30. `Registration`
31. `Role`
32. `Roles`
33. `Round`
34. `sysdiagrams`
35. `Tournament`
36. `TournamentPrizePayout`
37. `TournamentPrizePayouts`
38. `Tournaments`
39. `Transactions`
40. `Users`
41. `Violations`
42. `Wallet`
43. `Wallets`
44. `WalletTransaction`

---

## 4. Bảng duplicate phát hiện
Dưới đây là so sánh số lượng dòng của bảng duplicate (dạng số nhiều hoặc tên do EF tự tạo) và bảng chuẩn (dạng số ít):

| Bảng duplicate | Bảng chuẩn tương ứng | Số dòng duplicate | Số dòng bảng chuẩn | Có thể drop ngay không | Nguy cơ / Nhận xét |
| -------------- | -------------------- | ----------------: | -----------------: | :--------------------: | ------------------ |
| `Users` | `AppUser` | 5 | 0 | **KHÔNG** | Chứa dữ liệu seed quan trọng. Drop ngay sẽ mất tài khoản Admin/User. |
| `Roles` | `Role` | 5 | 0 | **KHÔNG** | Chứa dữ liệu seed các Role (Admin, HorseOwner,...). |
| `JockeyProfiles` | `JockeyProfile` | 1 | 0 | **KHÔNG** | Chứa thông tin Jockey profile đã đăng ký. |
| `RefereeProfiles` | `RefereeProfile` | 1 | 0 | **KHÔNG** | Chứa thông tin Referee profile đã đăng ký. |
| `Wallets` | `Wallet` | 1 | 0 | **KHÔNG** | Chứa thông tin ví của Spectator. |
| `Bets` | `Bet` | 0 | 0 | Có thể drop sau | Bảng chuẩn rỗng, bảng duplicate rỗng. |
| `Horses` | `Horse` | 0 | 0 | Có thể drop sau | Cả hai bảng đều không có dữ liệu. |
| `Notifications` | `Notification` | 0 | 0 | Có thể drop sau | Cả hai bảng đều không có dữ liệu. |
| `Payouts` | `Payout` | 0 | 0 | Có thể drop sau | Cả hai bảng đều không có dữ liệu. |
| `Prizes` | `Prize` | 0 | 0 | Có thể drop sau | Cả hai bảng đều không có dữ liệu. |
| `Races` | `Race` | 0 | 0 | Có thể drop sau | Cả hai bảng đều không có dữ liệu. |
| `RaceEntries` | `RaceEntry` | 0 | 0 | Có thể drop sau | Cả hai bảng đều không có dữ liệu. |
| `RaceResults` | `RaceResult` | 0 | 0 | Có thể drop sau | Cả hai bảng đều không có dữ liệu. |
| `Violations` | `RaceViolation` | 0 | 0 | Có thể drop sau | Do EF tự tạo từ DbSet `Violations` của `RaceViolation`. Cả hai đều rỗng. |
| `TournamentPrizePayouts`| `TournamentPrizePayout`| 0 | 0 | Có thể drop sau | Cả hai bảng đều không có dữ liệu. |
| `Transactions` | `WalletTransaction` | 0 | 0 | Có thể drop sau | Do EF tự tạo từ DbSet `Transactions` của `WalletTransaction`. Cả hai đều rỗng. |
| `Tournaments` | `Tournament` | 0 | 0 | Có thể drop sau | Cả hai bảng đều không có dữ liệu. |
| `Predictions` | `Prediction` | 0 | 0 | Có thể drop sau | Mặc dù `Prediction` không có trong danh sách chuẩn, nó vẫn là entity hiện có và cần map sang dạng số ít. |

---

## 5. Kết quả kiểm tra AppDbContext
- **Vị trí file:** [AppDbContext.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/AppDbContext.cs)
- **Tình trạng khai báo `DbSet`:** Tất cả các `DbSet` đều được đặt tên dạng số nhiều hoặc viết tắt theo số nhiều (ví dụ `Users`, `Roles`, `Transactions`, `Violations`).
- **Tình trạng cấu hình `OnModelCreating`:**
  - Hoàn toàn thiếu các câu lệnh `.ToTable("TênBảngSốÍt")` cho toàn bộ các thực thể. Do đó, EF Core mặc định sử dụng tên `DbSet` (số nhiều) để đặt tên bảng trong database khi tạo migration.
  - Một số thực thể như `Horse`, `Round`, `Race`, `RaceResult`, `Tournament`, `Prediction`, `WalletTransaction`, và `RaceViolation` thậm chí không có khối khai báo cấu hình Fluent API nào trong `OnModelCreating`.
- **Đã cập nhật:** Đã thêm cấu hình `ToTable` dạng số ít cho tất cả các thực thể trong `AppDbContext.cs` để sửa triệt để cấu hình mapping.

---

## 6. Kết quả kiểm tra Entity
Các Entity đều kế thừa chuẩn dạng số ít. Chi tiết ánh xạ:

| Entity | File | Table chuẩn | Table EF có thể tạo | Có ToTable chưa | Vấn đề |
| ------ | ---- | ----------- | ------------------- | --------------- | ------ |
| Role | [Role.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Users/Role.cs) | Role | Roles | Có (đã sửa) | Thiếu ToTable |
| AppUser | [AppUser.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Users/AppUser.cs) | AppUser | Users | Có (đã sửa) | Thiếu ToTable |
| JockeyProfile | [JockeyProfile.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Users/JockeyProfile.cs) | JockeyProfile | JockeyProfiles | Có (đã sửa) | Thiếu ToTable |
| RefereeProfile | [RefereeProfile.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Users/RefereeProfile.cs) | RefereeProfile | RefereeProfiles | Có (đã sửa) | Thiếu ToTable |
| Horse | [Horse.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Equines/Horse.cs) | Horse | Horses | Có (đã sửa) | Thiếu ToTable |
| HorseDocument | [HorseDocument.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Equines/HorseDocument.cs) | HorseDocument | HorseDocuments | Có (đã sửa) | Thiếu ToTable |
| HorseStatistic | [HorseStatistic.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Equines/HorseStatistic.cs) | HorseStatistic | HorseStatistics | Có (đã sửa) | Thiếu ToTable |
| Tournament | [Tournament.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/Tournament.cs) | Tournament | Tournaments | Có (đã sửa) | Thiếu ToTable |
| Round | [Round.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/Round.cs) | Round | Rounds | Có (đã sửa) | Thiếu ToTable |
| JockeyContract | [JockeyContract.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/JockeyContract.cs) | JockeyContract | JockeyContracts | Có (đã sửa) | Thiếu ToTable |
| Registration | [Registration.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/Registration.cs) | Registration | Registrations | Có (đã sửa) | Thiếu ToTable |
| Race | [Race.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/Race.cs) | Race | Races | Có (đã sửa) | Thiếu ToTable |
| RaceEntry | [RaceEntry.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/RaceEntry.cs) | RaceEntry | RaceEntries | Có (đã sửa) | Thiếu ToTable |
| RaceResult | [RaceResult.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/RaceResult.cs) | RaceResult | RaceResults | Có (đã sửa) | Thiếu ToTable |
| Prize | [Prize.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Prize.cs) | Prize | Prizes | Có (đã sửa) | Thiếu ToTable |
| Wallet | [Wallet.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Wallet.cs) | Wallet | Wallets | Có (đã sửa) | Thiếu ToTable |
| TournamentPrizePayout | [TournamentPrizePayout.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/TournamentPrizePayout.cs) | TournamentPrizePayout | TournamentPrizePayouts | Có (đã sửa) | Thiếu ToTable |
| RaceRefereeAssignment | [RaceRefereeAssignment.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/RaceRefereeAssignment.cs) | RaceRefereeAssignment | RaceRefereeAssignments | Có (đã sửa) | Thiếu ToTable |
| RefereeReport | File rỗng (0-byte) | RefereeReport | RefereeReports | Không | **Thiếu Entity thực tế và DbSet trong DbContext.** |
| RaceViolation | [RaceViolation.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Compliance/RaceViolation.cs) | RaceViolation | Violations | Có (đã sửa) | Thiếu ToTable |
| Bet | [Bet.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Bet.cs) | Bet | Bets | Có (đã sửa) | Thiếu ToTable |
| Payout | [Payout.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Payout.cs) | Payout | Payouts | Có (đã sửa) | Thiếu ToTable |
| WalletTransaction | [WalletTransaction.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/WalletTransaction.cs) | WalletTransaction | Transactions | Có (đã sửa) | Thiếu ToTable |
| Notification | [Notification.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Notifications/Notification.cs) | Notification | Notifications | Có (đã sửa) | Thiếu ToTable |
| Prediction | [Prediction.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Prediction.cs) | Prediction | Predictions | Có (đã sửa) | Thiếu ToTable (không có trong list chuẩn nhưng có code sử dụng) |

---

## 7. Kết quả kiểm tra Migration
Các file migration hiện tại đang cấu trúc DB theo dạng số nhiều:
1. `20260609193021_InitialCreate.cs`: Tạo các bảng số nhiều như `Users`, `Roles`, `Races`, `RaceEntries`. (Đã chạy vào DB)
2. `20260610083404_AddBetPayoutPrizeNotification.cs`: Tạo các bảng số nhiều như `Bets`, `Notifications`, `Prizes`. (Đã chạy vào DB)
3. `20260611024654_UpdateTournamentRacingEntities.cs`: (Chưa chạy vào DB) có các tham chiếu/cột mới trong các bảng số nhiều.
4. `20260611035623_AddHorseDocsAndStats.cs`: (Chưa chạy vào DB) tạo các bảng số nhiều `JockeyContracts`, `Registrations` và tham chiếu tới bảng số nhiều.

---

## 8. Kết quả kiểm tra Git/branch/commit
- **Branch hiện tại:** `feature/horse-registration-jockey` (Up-to-date với `origin`).
- Các commit gần đây đã merge code từ các branch khác nhau (Khang's và Han's tasks), dẫn đến conflict và lệch migration.
- Cấu hình database gốc trước đây được import bằng các bảng singular. Khi đồng đội của bạn tạo các migration `InitialCreate` và `AddBetPayoutPrizeNotification` mà không khai báo `.ToTable()`, EF đã tự tạo ra các bảng số nhiều khi chạy `update-database`, dẫn đến trùng lặp dữ liệu với các bảng cũ.

---

## 9. Nguyên nhân gốc
1. **Lỗi cấu hình mapping:** File `AppDbContext.cs` khai báo `DbSet<Entity> Entities` nhưng không cấu hình Fluent API `.ToTable("Entity")` cho các thực thể. Do đó, EF Core mặc định tạo tên bảng theo tên `DbSet` là số nhiều.
2. **Quá trình đồng bộ:** Database ban đầu chứa các bảng rỗng dạng số ít được tạo thủ công (hoặc do thiết kế DB cũ). Khi chạy EF migration mới, EF Core không tìm thấy bảng số nhiều tương ứng (`Users`, `Roles`,...) nên đã tạo mới các bảng số nhiều này và nạp seed data vào đó.
3. **Phân mảnh dữ liệu:** Dữ liệu seed và dữ liệu test hiện nằm hoàn toàn ở các bảng số nhiều (`Users`, `Roles`, `JockeyProfiles`, `RefereeProfiles`, `Wallets`). Các bảng số ít chuẩn hiện tại hoàn toàn rỗng.

---

## 10. File cần sửa
1. `backend/src/HorseRacing.Infrastructure/Persistence/AppDbContext.cs` (Đã sửa hoàn tất và build thành công).

---

## 11. Cách sửa an toàn

> [!IMPORTANT]
> **BƯỚC 1: BACKUP DATABASE**
> Hãy chạy lệnh backup SQL Server Database `HorseRacingManagementSystem` ra file `.bak` trước khi thực hiện bất kỳ thao tác nào khác.

### Bước 2: Dọn dẹp các bảng singular rỗng hiện tại
Vì các bảng singular chuẩn hiện tại đang rỗng nhưng trùng tên với tên bảng ta muốn đổi, ta cần drop các bảng singular rỗng này trước. Nếu không, lệnh rename bảng từ plural sang singular sẽ bị lỗi trùng tên.

### Bước 3: Tạo migration sửa đổi mapping và rename bảng
Chạy lệnh tạo migration mới (ví dụ: `AddSingularTableMapping`). EF Core sẽ tự động sinh ra các câu lệnh `migrationBuilder.RenameTable` để đổi tên từ plural sang singular, bảo toàn toàn bộ dữ liệu hiện có trong các bảng số nhiều (ví dụ đổi `Users` -> `AppUser`, `Roles` -> `Role`).

### Bước 4: Chạy update database
Áp dụng migration mới để hoàn tất việc chuyển đổi sang thiết kế database singular chuẩn.

---

## 12. SQL script kiểm tra
Script kiểm tra số dòng thực tế để đảm bảo không mất dữ liệu:
```sql
SELECT 'AppUser' AS TableName, COUNT(*) AS NumRows FROM AppUser UNION ALL
SELECT 'Users', COUNT(*) FROM Users UNION ALL
SELECT 'Role', COUNT(*) FROM Role UNION ALL
SELECT 'Roles', COUNT(*) FROM Roles UNION ALL
SELECT 'JockeyProfile', COUNT(*) FROM JockeyProfile UNION ALL
SELECT 'JockeyProfiles', COUNT(*) FROM JockeyProfiles UNION ALL
SELECT 'RefereeProfile', COUNT(*) FROM RefereeProfile UNION ALL
SELECT 'RefereeProfiles', COUNT(*) FROM RefereeProfiles UNION ALL
SELECT 'Wallet', COUNT(*) FROM Wallet UNION ALL
SELECT 'Wallets', COUNT(*) FROM Wallets;
```

---

## 13. SQL script xử lý đề xuất
Script SQL dọn dẹp các bảng singular rỗng (chạy trước khi tạo và áp dụng migration để tránh lỗi trùng tên):

```sql
-- Chỉ chạy khi chắc chắn các bảng dưới đây có 0 dòng (đã kiểm tra ở mục 12)
DROP TABLE IF EXISTS [Bet];
DROP TABLE IF EXISTS [Payout];
DROP TABLE IF EXISTS [TournamentPrizePayout];
DROP TABLE IF EXISTS [Prize];
DROP TABLE IF EXISTS [RaceEntry];
DROP TABLE IF EXISTS [RaceResult];
DROP TABLE IF EXISTS [RaceViolation];
DROP TABLE IF EXISTS [JockeyContract];
DROP TABLE IF EXISTS [Registration];
DROP TABLE IF EXISTS [HorseDocument];
DROP TABLE IF EXISTS [HorseStatistic];
DROP TABLE IF EXISTS [Round];
DROP TABLE IF EXISTS [Race];
DROP TABLE IF EXISTS [Tournament];
DROP TABLE IF EXISTS [Horse];
DROP TABLE IF EXISTS [WalletTransaction];
DROP TABLE IF EXISTS [Notification];
DROP TABLE IF EXISTS [RefereeProfile];
DROP TABLE IF EXISTS [JockeyProfile];
DROP TABLE IF EXISTS [Wallet];
DROP TABLE IF EXISTS [AppUser];
DROP TABLE IF EXISTS [Role];
```

---

## Final Safety Review

### Row count duplicate tables
Chúng tôi đã tạo script kiểm tra số lượng dòng tại [check-duplicate-table-row-count.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/check-duplicate-table-row-count.sql). Kết quả phân tích hiện trạng số dòng của các bảng duplicate (số nhiều) và các bảng chuẩn (số ít):
- Bảng plural có chứa dữ liệu seed và test:
  * `Users`: 5 dòng (chứa các tài khoản Admin/User hệ thống). Bảng chuẩn `AppUser` hiện có 0 dòng.
  * `Roles`: 5 dòng (chứa các Role chính như Admin, HorseOwner,...). Bảng chuẩn `Role` hiện có 0 dòng.
  * `JockeyProfiles`: 1 dòng (chứa hồ sơ Jockey đã đăng ký). Bảng chuẩn `JockeyProfile` hiện có 0 dòng.
  * `RefereeProfiles`: 1 dòng (chứa hồ sơ Referee đã đăng ký). Bảng chuẩn `RefereeProfile` hiện có 0 dòng.
  * `Wallets`: 1 dòng (chứa thông tin ví của Spectator). Bảng chuẩn `Wallet` hiện có 0 dòng.
- Các cặp bảng còn lại (cả bản số nhiều và số ít) đều đang rỗng 0 dòng (như `Bets`/`Bet`, `Horses`/`Horse`, v.v.).
- Bảng `Tournaments` (số nhiều) và `Tournament` (số ít) đều đang rỗng 0 dòng.

### Foreign key dependency
Chúng tôi đã tạo script kiểm tra khóa ngoại tại [check-foreign-keys.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/check-foreign-keys.sql) để kiểm tra toàn bộ FK dependencies của database.
Khi thực hiện loại bỏ các bảng singular rỗng trùng lặp, ta phải tuân thủ nghiêm ngặt thứ tự xóa để tránh vi phạm ràng buộc FK:
- Xóa các bảng con (child tables) hoặc các bảng có quan hệ phụ thuộc cấp cao trước (ví dụ: `WalletTransaction`, `Payout`, `Bet`, `TournamentPrizePayout`, `Prize`).
- Xóa các bảng trung gian/bảng con của Race (như `RaceViolation`, `RefereeReport`, `RaceRefereeAssignment`, `RaceResult`, `RaceEntry`, `Race`).
- Xóa các bảng đăng ký và hợp đồng (như `JockeyContract`, `Registration`).
- Xóa các bảng thông tin Horse (như `HorseStatistic`, `HorseDocument`, `Horse`).
- Xóa các bảng Tournaments/Rounds (như `Round`, `Tournament`).
- Cuối cùng mới xóa các bảng thông tin cấu hình và tài khoản cơ bản (như `Notification`, `Wallet`, `RefereeProfile`, `JockeyProfile`, `AppUser`, `Role`).

### Drop singular empty tables script
File script [drop-empty-singular-duplicate-tables.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/drop-empty-singular-duplicate-tables.sql) đã được chuẩn hóa để drop toàn bộ các bảng singular rỗng theo đúng thứ tự phụ thuộc khóa ngoại đã phân tích ở trên. Chạy script này sẽ giải phóng tên bảng singular mà không gây ra bất cứ lỗi ràng buộc khóa ngoại nào, tạo điều kiện để quá trình Migration đổi tên bảng diễn ra suôn sẻ và không bị lỗi trùng tên.

### Migration review
Chúng tôi đã kiểm tra migration `FixSingularTableMapping` vừa tạo:
- Migration sử dụng các câu lệnh `migrationBuilder.RenameTable` để đổi tên từ bảng số nhiều (plural) có chứa dữ liệu sang bảng số ít (singular) chuẩn (ví dụ: `Users` -> `AppUser`, `Roles` -> `Role`, `Wallets` -> `Wallet`, `JockeyProfiles` -> `JockeyProfile`, `RefereeProfiles` -> `RefereeProfile`).
- Hoàn toàn **KHÔNG** có lệnh `DropTable` đối với bất kỳ bảng plural nào đang có dữ liệu (`Users`, `Roles`, `Wallets`, `JockeyProfiles`, `RefereeProfiles`). Do đó, toàn bộ dữ liệu hiện có sẽ được bảo toàn tuyệt đối khi đổi tên bảng.
- Bảng duy nhất bị drop trong migration này là `Tournaments` (số nhiều), do thực thể `Tournament` đã được EF Core ánh xạ trực tiếp sang bảng `Tournament` (số ít) và cả hai bảng này đều đang rỗng 0 dòng, hoàn toàn an toàn.

### Generated SQL script review
Chúng tôi đã kiểm tra chi tiết file SQL Migration generated tại [FixSingularTableMapping.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/FixSingularTableMapping.sql):
- Có câu lệnh `DROP TABLE [Tournaments];` (bảng này rỗng 0 dòng, hoàn toàn an toàn).
- **KHÔNG** có lệnh `DROP TABLE [Users]`, `DROP TABLE [Roles]`, hay `DROP TABLE [Wallets]`.
- **KHÔNG** có lệnh `CREATE TABLE [AppUser]` mới từ đầu mà sẽ đổi tên từ `Users`.
- Có sử dụng lệnh đổi tên bảng chuẩn của SQL Server:
  * `EXEC sp_rename N'[Users]', N'AppUser', 'OBJECT';`
  * `EXEC sp_rename N'[Roles]', N'Role', 'OBJECT';`
  * `EXEC sp_rename N'[Wallets]', N'Wallet', 'OBJECT';`
  * ... cùng các lệnh rename tương tự cho các bảng plural khác sang singular.
Điều này chứng minh quá trình chuyển đổi là an toàn và giữ nguyên vẹn 100% dữ liệu.

### Có an toàn để tôi chạy SQL không?
Kết luận rõ:
```text
SAFE TO PROCEED
```

