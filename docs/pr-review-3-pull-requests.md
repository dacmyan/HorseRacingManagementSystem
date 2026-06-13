# Review 3 Pull Requests

Tài liệu này đánh giá chi tiết 3 Pull Requests (PR) hiện tại của dự án:
- `feature/han-race-violation` (PR #50)
- `fix/align-domain-with-standard-db` (PR #49)
- `feature/han-race-entry` (PR #44)

Mục tiêu là kiểm tra xung đột (conflict), lỗi build, lỗi database/migration, và đưa ra đề xuất thứ tự merge an toàn.

---

## 1. Danh sách PR kiểm tra

| PR | Branch | Author | Target | URL |
| :--- | :--- | :--- | :--- | :--- |
| **#50** | `feature/han-race-violation` | XuanHan105205 (Lê Xuân Hàn) | `main` | https://github.com/dacmyan/HorseRacingManagementSystem/pull/50 |
| **#49** | `fix/align-domain-with-standard-db` | XuanHan105205 (Lê Xuân Hàn) | `main` | https://github.com/dacmyan/HorseRacingManagementSystem/pull/49 |
| **#44** | `feature/han-race-entry` | XuanHan105205 (Lê Xuân Hàn) | `main` | https://github.com/dacmyan/HorseRacingManagementSystem/pull/44 |

---

## 2. Kết quả kiểm tra từng PR

| PR | Conflict với main | Build | Migration/DB | Risk | Kết luận |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **#50** | Không | Pass | **Lỗi nghiêm trọng** (Kế thừa lỗi PR #49, lệch cột bảng `RaceViolation`) | **Cao** | **Need fixes before merge** |
| **#49** | Không | Pass | **Lỗi nghiêm trọng** (Sửa migration cũ, lỗi Alter cột Identity) | **Cao** | **Need fixes before merge** |
| **#44** | **Có** (ở `PublicController.cs`) | Pass | Không ảnh hưởng trực tiếp | **Trung bình** (Conflict nặng với PR #49/50 về kiểu dữ liệu) | **Need fixes before merge** |

---

## 3. File thay đổi của từng PR

### PR #50 (`feature/han-race-violation`)
- **File thêm mới (A):**
  - `backend/src/HorseRacing.API/Controllers/RefereeController.cs`
  - `backend/src/HorseRacing.Application/Features/OfficiatingAndResults/DTOs/CreateRaceViolationRequest.cs`
  - `backend/src/HorseRacing.Application/Features/OfficiatingAndResults/DTOs/RaceViolationResponse.cs`
  - `backend/src/HorseRacing.Application/Features/OfficiatingAndResults/Interfaces/IRaceViolationRepository.cs`
  - `backend/src/HorseRacing.Application/Features/OfficiatingAndResults/Interfaces/IRaceViolationService.cs`
  - `backend/src/HorseRacing.Application/Features/OfficiatingAndResults/Services/RaceViolationService.cs`
  - `backend/src/HorseRacing.Infrastructure/Persistence/Migrations/20260613032733_AddRaceViolationFields.Designer.cs`
  - `backend/src/HorseRacing.Infrastructure/Persistence/Migrations/20260613032733_AddRaceViolationFields.cs`
  - `backend/src/HorseRacing.Infrastructure/Repositories/RaceViolationRepository.cs`
  - *Và tất cả các file thêm mới của PR #49 do PR #50 được stack lên PR #49.*
- **File chỉnh sửa (M):**
  - `RaceViolation.cs` (Entity), `AppDbContext.cs`, `AppDbContextModelSnapshot.cs`
  - Đăng ký dịch vụ: `ServiceExtensions.cs`, `DependencyInjection.cs`
  - *Và tất cả các file chỉnh sửa của PR #49.*
- **Có sửa AppDbContext.cs / Entity / Migration:** **Có**.

### PR #49 (`fix/align-domain-with-standard-db`)
- **File thêm mới (A):**
  - `RaceRefereeResponse.cs`, `IRefereeAssignmentService.cs`, `RefereeAssignmentService.cs`, `CreateRaceEntryRequest.cs`, `RaceEntryResponse.cs`, `RefereeAssignmentRepository.cs`, và migration `20260613024315_AddRefereeAssignmentFeature.cs`.
- **File chỉnh sửa (M):**
  - Các Entity (`JockeyProfile`, `RefereeProfile`, `Registration`, `Horse`,...), `AppDbContext.cs`, và sửa trực tiếp migration cũ `20260612024026_FixSingularTableMapping.cs`.
- **Có sửa AppDbContext.cs / Entity / Migration:** **Có**.

### PR #44 (`feature/han-race-entry`)
- **File thêm mới (A):**
  - `RaceEntryResponse.cs` (Trùng đường dẫn với PR #49), `IRaceEntryRepository.cs`, `IRaceEntryService.cs`, `RaceEntryService.cs`, `RaceEntryRepository.cs`.
- **File chỉnh sửa (M):**
  - `PublicController.cs`, `ServiceExtensions.cs`, `DependencyInjection.cs`.
- **Có sửa AppDbContext.cs / Entity / Migration:** **Không**.

---

## 4. File overlap giữa 3 PR

| File | PR #50 sửa? | PR #49 sửa? | PR #44 sửa? | Nguy cơ conflict |
| :--- | :---: | :---: | :---: | :--- |
| `PublicController.cs` | Có | Có | Có | **Rất cao** (Constructor injection và các endpoint public đọc dữ liệu) |
| `ServiceExtensions.cs` | Có | Có | Có | **Thấp** (Chỉ là các dòng đăng ký Dependency Injection cho Service) |
| `DependencyInjection.cs` | Có | Có | Có | **Thấp** (Chỉ là các dòng đăng ký Dependency Injection cho Repository) |
| `RaceEntryResponse.cs` | **Tạo mới** | **Tạo mới** | **Tạo mới** | **Rất cao** (Conflict add/add, trùng tên file và đường dẫn nhưng cấu trúc khác nhau) |

---

## 5. Conflict giữa các PR

1. **Conflict của PR #44 với `main`:**
   - Nhánh `main` đã merge PR #43, dẫn đến việc `PublicController.cs` bị thay đổi. PR #44 sửa đổi cùng phân vùng code này nên bị **Conflict** trực tiếp với `main`.
2. **Conflict add/add của `RaceEntryResponse.cs`:**
   - PR #49/50 và PR #44 tự định nghĩa file này ở cùng một đường dẫn nhưng với cấu trúc properties khác biệt (`long` vs `int`, tên thuộc tính `RaceEntryId` vs `EntryId`).
3. **Constructor Injection Conflict ở `PublicController.cs`:**
   - Cả 3 PR đăng ký các service khác nhau gây xung đột khi tự động merge đồng thời.

---

## 6. Lỗi build nếu có

- Cả 3 PR build độc lập đều **Pass**.
- Tuy nhiên, nếu merge PR #49/50 trước thì PR #44 sẽ bị gãy build do mismatch kiểu dữ liệu (`long` của các cột ID ở PR #49/50 so với `int` ở PR #44).

---

## 7. Lỗi migration/database nếu có

### Vấn đề tại PR #49 & PR #50:
1. **Sửa đổi Migration lịch sử:**
   - PR #49 và PR #50 sửa đổi trực tiếp logic trong file migration cũ đã apply (`20260612024026_FixSingularTableMapping.cs`). Đây là hành vi cấm kỵ vì gây lệch DB schema giữa các máy dev đã chạy migration đó trước đó.
2. **Lỗi chạy SQL Alter Identity Column trên SQL Server:**
   - Migration `20260613024315_AddRefereeAssignmentFeature.cs` cố gắng thay đổi cột `JockeyId` (bảng `JockeyProfile`) và `RefereeId` (bảng `RefereeProfile`) từ `int` sang `bigint`. Do đây là hai cột `IDENTITY`, SQL Server sẽ báo lỗi và không cho phép thực thi trực tiếp qua `ALTER COLUMN`.
3. **Sai lệch Schema bảng `RaceViolation` ở PR #50:**
   - PR #50 thêm cột `RaceEntryId`, `RefereeId`, và `CreatedAt` vào thực thể `RaceViolation`. Nhưng trong file SQL chuẩn `recreate-clean-database.sql` không hề có các cột này. Điều này làm lệch cấu trúc DB so với chuẩn của dự án.

---

## 8. Lỗi phân quyền/API nếu có

- **`RefereeController` (PR #50):** Sử dụng `[Authorize(Roles = "Referee")]` ở cấp độ Class -> **Hợp lệ, an toàn**.
- **`AdminController` (PR #49):** Thêm các API phân quyền Admin (`[Authorize(Roles = "Admin")]`) -> **Hợp lệ, an toàn**.
- **`PublicController` (PR #44):** Thêm API public đọc entries (`[AllowAnonymous]`) -> **Hợp lệ**.

---

## 9. Thứ tự merge đề xuất

| Thứ tự đề xuất | PR | Lý do | Điều kiện trước khi merge |
| :---: | :--- | :--- | :--- |
| **1** | **PR #49** (`fix/align-domain-with-standard-db`) | Là nền tảng thay đổi kiểu dữ liệu ID của Entity/DbContext. | **Phải sửa lỗi DB/migration:**<br>1. Khôi phục migration cũ `20260612024026_FixSingularTableMapping.cs` về nguyên bản.<br>2. Sửa lỗi `AlterColumn` trên identity column (giữ JockeyId/RefereeId là `int` trong DB hoặc viết migration tạo lại bảng). |
| **2** | **PR #50** (`feature/han-race-violation`) | Stack trên PR #49, bổ sung tính năng quản lý vi phạm của trọng tài. | 1. PR #49 đã được merge vào main.<br>2. Cập nhật file SQL chuẩn `recreate-clean-database.sql` để bổ sung thêm các cột mới của `RaceViolation` nhằm đồng bộ schema. |
| **3** | **PR #44** (`feature/han-race-entry`) | Phụ thuộc cấu trúc dữ liệu mới từ PR #49. | 1. PR #49 và #50 đã được merge lên main.<br>2. Rebase/Merge main mới vào PR #44, đổi các ID sang `long` và sửa các lỗi compile.<br>3. Giải quyết conflict add/add của file `RaceEntryResponse.cs`. |

---

## 10. Checklist trước khi merge

- [ ] Các PR không còn xung đột (conflict) với nhánh `main`.
- [ ] Tất cả các PR đều build pass sau khi giải quyết conflict.
- [ ] Không có việc chỉnh sửa trực tiếp các file migration cũ đã được commit/apply.
- [ ] Không có các câu lệnh migration đổi kiểu dữ liệu cột `IDENTITY` trực tiếp.
- [ ] Dữ liệu Seed sử dụng PasswordHasher động tại runtime thay vì lưu chuỗi hash cố định.
- [ ] API mới được phân quyền chính xác (`[Authorize]` cho Admin/Referee và `[AllowAnonymous]` cho API Public).
- [ ] Giải quyết triệt để xung đột trùng file `RaceEntryResponse.cs`.
- [ ] Cập nhật file SQL chuẩn `recreate-clean-database.sql` tương thích với các cột mới của `RaceViolation`.

---

## 11. Kết luận cuối cùng

1. **PR #49 (`fix/align-domain-with-standard-db`):** **Need fixes before merge** (Cần khôi phục file migration cũ, sửa cách đổi kiểu dữ liệu cột Identity để không gây lỗi SQL Server).
2. **PR #50 (`feature/han-race-violation`):** **Need fixes before merge** (Kế thừa lỗi của PR #49, cần bổ sung cột mới của `RaceViolation` vào file SQL chuẩn).
3. **PR #44 (`feature/han-race-entry`):** **Need fixes before merge** (Bị conflict với main do PR #43 đã merge, cần cập nhật các ID sang `long` và sửa conflict file trùng).
