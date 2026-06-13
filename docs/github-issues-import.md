# GitHub Issues Import Templates for Backend Roadmap

Tài liệu này chứa danh sách toàn bộ 23 task backend được thiết kế sẵn thành các tiêu đề, nhãn (labels), người phụ trách (assignee), cột trạng thái (Project Status), và nội dung chi tiết (Issue Body) để bạn có thể dễ dàng sao chép thủ công lên GitHub Issues hoặc dùng làm dữ liệu đầu vào cho công cụ import.

---

## 1. Master Issue List & Metadata Mapping

| Task ID | Issue Title | Labels | Assignee (Role Owner) | Project Status | Milestone / Sprint |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **BE-AUTH-01** | `[BE-AUTH-01] Cải tiến Login & Register Validation` | `backend`, `phase-1`, `priority-p0`, `auth` | Triều | **Todo** | Sprint 1 - Foundation |
| **BE-USER-01** | `[BE-USER-01] Admin tạo tài khoản nghiệp vụ` | `backend`, `phase-1`, `priority-p0`, `user`, `blocked` | Triều | **Todo** | Sprint 1 - Foundation |
| **BE-WALLET-01**| `[BE-WALLET-01] Giao dịch ví điện tử (Nạp/Rút)` | `backend`, `phase-1`, `priority-p0`, `wallet`, `blocked` | Đắc | **Todo** | Sprint 1 - Foundation |
| **BE-NOTI-01** | `[BE-NOTI-01] Tạo và gửi thông báo hệ thống` | `backend`, `phase-1`, `priority-p1`, `notification`, `blocked` | Đắc | **Todo** | Sprint 1 - Foundation |
| **BE-USER-02** | `[BE-USER-02] Xem và cập nhật Profile cá nhân` | `backend`, `phase-1`, `priority-p2`, `user`, `blocked` | Triều | **Backlog** | Sprint 2 - Core Master Data |
| **BE-USER-03** | `[BE-USER-03] Admin quản lý danh sách Users` | `backend`, `phase-1`, `priority-p2`, `user`, `blocked` | Triều | **Backlog** | Sprint 2 - Core Master Data |
| **BE-HORSE-01**| `[BE-HORSE-01] Quản lý Ngựa đua (CRUD Horses)` | `backend`, `phase-2`, `priority-p0`, `horse`, `blocked` | Khang | **Todo** | Sprint 2 - Core Master Data |
| **BE-RACE-01** | `[BE-RACE-01] Quản lý Giải đấu (Tournaments)` | `backend`, `phase-2`, `priority-p0`, `race`, `blocked` | Hàn | **Todo** | Sprint 2 - Core Master Data |
| **BE-RACE-02** | `[BE-RACE-02] Lên lịch thi đấu (Scheduling Races)` | `backend`, `phase-2`, `priority-p1`, `race`, `blocked` | Hàn | **Backlog** | Sprint 2 - Core Master Data |
| **BE-HORSE-02**| `[BE-HORSE-02] Quản lý tài liệu và chỉ số ngựa` | `backend`, `phase-2`, `priority-p3`, `horse`, `blocked` | Khang | **Backlog** | Sprint 2 - Core Master Data |
| **BE-CONTRACT-01**| `[BE-CONTRACT-01] Gửi hợp đồng thuê nài ngựa` | `backend`, `phase-3`, `priority-p1`, `horse`, `blocked` | Khang | **Backlog** | Sprint 3 - Registration & Assignment |
| **BE-CONTRACT-02**| `[BE-CONTRACT-02] Phản hồi hợp đồng (Jockey Action)` | `backend`, `phase-3`, `priority-p1`, `horse`, `blocked` | Khang | **Backlog** | Sprint 3 - Registration & Assignment |
| **BE-REG-01**  | `[BE-REG-01] Đăng ký ngựa tham gia Giải đấu` | `backend`, `phase-3`, `priority-p1`, `horse`, `blocked` | Khang | **Backlog** | Sprint 3 - Registration & Assignment |
| **BE-RACE-04** | `[BE-RACE-04] Gán trọng tài điều hành cuộc đua` | `backend`, `phase-3`, `priority-p2`, `race`, `blocked` | Hàn | **Backlog** | Sprint 3 - Registration & Assignment |
| **BE-RACE-03** | `[BE-RACE-03] Ghép cặp trận đấu (Lane Assignment)` | `backend`, `phase-3`, `priority-p0`, `race`, `blocked` | Hàn | **Backlog** | Sprint 3 - Registration & Assignment |
| **BE-REF-01**  | `[BE-REF-01] Trọng tài giám sát & vi phạm` | `backend`, `phase-4`, `priority-p1`, `race`, `blocked` | Hàn | **Backlog** | Sprint 4 - Result & Financial Flow |
| **BE-RESULT-01**| `[BE-RESULT-01] Ghi nhận & Công bố kết quả đua` | `backend`, `phase-4`, `priority-p0`, `race`, `blocked` | Hàn | **Backlog** | Sprint 4 - Result & Financial Flow |
| **BE-REF-02**  | `[BE-REF-02] Trọng tài lập biên bản & Báo cáo` | `backend`, `phase-4`, `priority-p3`, `race`, `blocked` | Hàn | **Backlog** | Sprint 4 - Result & Financial Flow |
| **BE-BET-01**  | `[BE-BET-01] Khán giả đặt cược (Place Bets)` | `backend`, `phase-5`, `priority-p0`, `betting`, `blocked` | Đắc | **Backlog** | Sprint 4 - Result & Financial Flow |
| **BE-BET-02**  | `[BE-BET-02] Quản lý dự đoán (Predictions)` | `backend`, `phase-5`, `priority-p2`, `betting`, `blocked` | Đắc | **Backlog** | Sprint 4 - Result & Financial Flow |
| **BE-PAY-01**  | `[BE-PAY-01] Tự động trả thưởng đặt cược` | `backend`, `phase-5`, `priority-p0`, `wallet`, `blocked` | Đắc | **Backlog** | Sprint 4 - Result & Financial Flow |
| **BE-PAY-02**  | `[BE-PAY-02] Trả thưởng giải đấu cho HorseOwner/Jockey` | `backend`, `phase-5`, `priority-p1`, `wallet`, `blocked` | Đắc | **Backlog** | Sprint 4 - Result & Financial Flow |
| **Integration**| `[Integration] Tích hợp thông báo toàn hệ thống` | `backend`, `phase-5`, `priority-p1`, `notification`, `blocked` | Đắc | **Backlog** | Sprint 4 - Result & Financial Flow |

*Ghi chú:* 
- Vui lòng cấu hình các Milestone/Sprint và Labels trên Repo trước khi tạo issue.
- Nhãn `blocked` thể hiện task đó đang phải chờ một hoặc nhiều task khác hoàn thành trước khi được phép chuyển sang cột **Doing**.

---

## 2. Chi tiết 23 Issue Body Templates

### [BE-AUTH-01] Cải tiến Login & Register Validation
- **Labels:** `backend`, `phase-1`, `priority-p0`, `auth`
- **Milestone:** `Sprint 1 - Foundation`
- **Project Status:** `Todo`
- **Assignee:** Triều

```markdown
## Task ID
BE-AUTH-01

## Module
Authentication, User & Role Management

## Owner
Triều

## Phase
Phase 1

## Priority
P0

## Description
Thêm logic kiểm tra tính hợp lệ dữ liệu (FluentValidation) cho đăng nhập và đăng ký Spectator.

## APIs
- POST /api/auth/login
- POST /api/auth/register

## Roles
- Public
- Spectator (khi đăng ký)

## Database Tables
- AppUser
- Role
- Wallet (Khởi tạo tự động)

## Dependencies
- Không

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Đăng nhập thành công trả về JWT hợp lệ chứa các Claims cần thiết.
- [ ] Đăng ký Spectator tự động tạo ví rỗng trong DB với số dư = 0.
- [ ] Có validation kiểm tra định dạng email và độ dài mật khẩu (> 6 ký tự).

## Test Notes
Đăng nhập 5 tài khoản mẫu hoặc đăng ký mới tài khoản Spectator trên Swagger và kiểm tra token trả về.

## Related Files / Folders
- backend/src/HorseRacing.API/Controllers/AuthController.cs
- backend/src/HorseRacing.Application/Features/UserManagement/Services/AuthService.cs
```

---

### [BE-USER-01] Admin tạo tài khoản nghiệp vụ
- **Labels:** `backend`, `phase-1`, `priority-p0`, `user`, `blocked`
- **Milestone:** `Sprint 1 - Foundation`
- **Project Status:** `Todo`
- **Assignee:** Triều

```markdown
## Task ID
BE-USER-01

## Module
Authentication, User & Role Management

## Owner
Triều

## Phase
Phase 1

## Priority
P0

## Description
Admin tạo trực tiếp các tài khoản đặc quyền như Referee, Jockey, HorseOwner kèm theo thông tin hồ sơ đi kèm.

## APIs
- POST /api/admin/accounts

## Roles
- Admin

## Database Tables
- AppUser
- JockeyProfile
- RefereeProfile
- Role

## Dependencies
- BE-AUTH-01 (Cần làm trước)

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Admin tạo thành công tài khoản.
- [ ] Nếu tạo Jockey phải tự động tạo bản ghi trống trong `JockeyProfile`.
- [ ] Nếu tạo Referee phải tự động tạo bản ghi trống trong `RefereeProfile`.
- [ ] Yêu cầu điền đầy đủ và báo lỗi nếu thiếu `LicenseNumber` đối với Referee.

## Test Notes
Gọi API bằng Token Admin để tạo tài khoản, sau đó kiểm tra DB xem Profile tương ứng đã được chèn chưa.

## Related Files / Folders
- backend/src/HorseRacing.API/Controllers/AdminController.cs
- backend/src/HorseRacing.Application/Features/UserManagement/Services/AdminService.cs
```

---

### [BE-WALLET-01] Giao dịch ví điện tử (Nạp/Rút)
- **Labels:** `backend`, `phase-1`, `priority-p0`, `wallet`, `blocked`
- **Milestone:** `Sprint 1 - Foundation`
- **Project Status:** `Todo`
- **Assignee:** Đắc

```markdown
## Task ID
BE-WALLET-01

## Module
Spectator Betting, Financials & Notifications

## Owner
Đắc

## Phase
Phase 1

## Priority
P0

## Description
Người dùng Spectator thực hiện nạp tiền (giả lập hoặc qua VNPay/PayOS) và rút tiền. Cập nhật số dư ví và lưu lịch sử giao dịch.

## APIs
- POST /api/wallet/deposit
- POST /api/wallet/withdraw
- GET /api/wallet/history

## Roles
- Spectator

## Database Tables
- Wallet
- WalletTransaction

## Dependencies
- BE-AUTH-01 (Để xác thực vai trò Spectator)

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Số dư thay đổi chính xác sau khi nạp hoặc rút.
- [ ] Lưu đầy đủ giao dịch `WalletTransaction` với đúng kiểu giao dịch (Deposit/Withdraw) và thời gian thực hiện.
- [ ] Rút tiền không được vượt quá số dư ví hiện có và số tiền giao dịch phải > 0.

## Test Notes
Ví đã có sẵn khung xương trong DB. Cần hoàn thiện API nạp/rút để spectator có số dư thử nghiệm đặt cược.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/FinancialRewards/Services/WalletService.cs
- backend/src/HorseRacing.API/Controllers/SpectatorController.cs
```

---

### [BE-NOTI-01] Tạo và gửi thông báo hệ thống
- **Labels:** `backend`, `phase-1`, `priority-p1`, `notification`, `blocked`
- **Milestone:** `Sprint 1 - Foundation`
- **Project Status:** `Todo`
- **Assignee:** Đắc

```markdown
## Task ID
BE-NOTI-01

## Module
Spectator Betting, Financials & Notifications

## Owner
Đắc

## Phase
Phase 1

## Priority
P1

## Description
Tạo các bản ghi thông báo gửi đến người dùng khi có sự kiện đặc biệt (như nạp tiền, lời mời hợp đồng, có lịch đua mới, trả thưởng).

## APIs
- GET /api/notifications
- PUT /api/notifications/{id}/read

## Roles
- Đã xác thực (All roles)

## Database Tables
- Notification

## Dependencies
- BE-AUTH-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Hiện thực thực thể `Notification.cs` trống.
- [ ] Thông báo được đẩy vào DB thành công và trả ra đúng cho người dùng đích qua API.
- [ ] Viết thành công hàm helper/service gửi thông báo chung để sẵn sàng gọi từ các service nghiệp vụ khác.

## Test Notes
API lấy thông báo và đánh dấu đã đọc.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/Notifications/Services/NotificationService.cs
- backend/src/HorseRacing.API/Controllers/PublicController.cs
```

---

### [BE-USER-02] Xem và cập nhật Profile cá nhân
- **Labels:** `backend`, `phase-1`, `priority-p2`, `user`, `blocked`
- **Milestone:** `Sprint 2 - Core Master Data`
- **Project Status:** `Backlog`
- **Assignee:** Triều

```markdown
## Task ID
BE-USER-02

## Module
Authentication, User & Role Management

## Owner
Triều

## Phase
Phase 1

## Priority
P2

## Description
Lấy thông tin chi tiết tài khoản hiện tại và cập nhật thông tin cá nhân (bao gồm cả cập nhật JockeyProfile/RefereeProfile tương ứng).

## APIs
- GET /api/users/profile
- PUT /api/users/profile

## Roles
- Đã xác thực (All roles)

## Database Tables
- AppUser
- JockeyProfile
- RefereeProfile

## Dependencies
- BE-USER-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Trả về đúng thông tin user dựa trên Token Claims.
- [ ] Cập nhật được FullName, ExperienceYears (đối với Jockey/Referee).
- [ ] Không trả về PasswordHash.

## Test Notes
Cần có tài khoản Jockey/Referee được tạo kèm hồ sơ trống từ `BE-USER-01` thì mới có data để GET/PUT profile.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/UserManagement/Services/ProfileService.cs
- backend/src/HorseRacing.API/Controllers/PublicController.cs
```

---

### [BE-USER-03] Admin quản lý danh sách Users
- **Labels:** `backend`, `phase-1`, `priority-p2`, `user`, `blocked`
- **Milestone:** `Sprint 2 - Core Master Data`
- **Project Status:** `Backlog`
- **Assignee:** Triều

```markdown
## Task ID
BE-USER-03

## Module
Authentication, User & Role Management

## Owner
Triều

## Phase
Phase 1

## Priority
P2

## Description
Admin xem danh sách toàn bộ người dùng kèm theo bộ lọc theo Role, thay đổi trạng thái hoạt động (Active/Inactive) của tài khoản.

## APIs
- GET /api/admin/users
- PUT /api/admin/users/{id}/status

## Roles
- Admin

## Database Tables
- AppUser
- Role

## Dependencies
- BE-USER-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Trả ra danh sách phân trang người dùng.
- [ ] Cập nhật trạng thái người dùng thành công, người dùng bị khóa (Inactive) không thể đăng nhập ở API Login.

## Test Notes
Chỉ làm sau khi hệ thống đã có nhiều user được tạo để kiểm thử tính năng phân trang và khóa tài khoản.

## Related Files / Folders
- backend/src/HorseRacing.API/Controllers/AdminController.cs
- backend/src/HorseRacing.Application/Features/UserManagement/Services/AdminService.cs
```

---

### [BE-HORSE-01] Quản lý Ngựa đua (CRUD Horses)
- **Labels:** `backend`, `phase-2`, `priority-p0`, `horse`, `blocked`
- **Milestone:** `Sprint 2 - Core Master Data`
- **Project Status:** `Todo`
- **Assignee:** Khang

```markdown
## Task ID
BE-HORSE-01

## Module
Horse Owner, Horse Management & Jockey Contract

## Owner
Khang

## Phase
Phase 2

## Priority
P0

## Description
HorseOwner đăng ký mới ngựa đua của mình, cập nhật thông tin ngựa, hoặc xem danh sách ngựa thuộc sở hữu.

## APIs
- POST /api/horses
- GET /api/horses/my-horses
- PUT /api/horses/{id}
- DELETE /api/horses/{id}

## Roles
- HorseOwner

## Database Tables
- Horse

## Dependencies
- BE-AUTH-01
- BE-USER-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Thêm mới ngựa thành công lưu đúng `OwnerId` lấy từ Token.
- [ ] Update/Delete chỉ cho phép chính chủ ngựa thực hiện.
- [ ] Tệp `Horse.cs` được map đầy đủ thuộc tính trong DbContext.

## Test Notes
Ngựa là thực thể chính. Cần làm ngay sau khi Triều cấp tài khoản HorseOwner để lưu thông tin ngựa vào DB.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/HorseManagement/Services/HorseService.cs
- backend/src/HorseRacing.API/Controllers/OwnerController.cs
- backend/src/HorseRacing.Infrastructure/Repositories/HorseRepository.cs
```

---

### [BE-RACE-01] Quản lý Giải đấu (Tournaments)
- **Labels:** `backend`, `phase-2`, `priority-p0`, `race`, `blocked`
- **Milestone:** `Sprint 2 - Core Master Data`
- **Project Status:** `Todo`
- **Assignee:** Hàn

```markdown
## Task ID
BE-RACE-01

## Module
Tournament, Race Scheduling & Officiating

## Owner
Hàn

## Phase
Phase 2

## Priority
P0

## Description
Admin tạo mới giải đấu, cập nhật thông tin giải đấu và các vòng đấu (Round).

## APIs
- POST /api/tournaments
- PUT /api/tournaments/{id}
- POST /api/tournaments/{id}/rounds

## Roles
- Admin

## Database Tables
- Tournament
- Round

## Dependencies
- BE-AUTH-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Hiện thực thực thể `Round.cs` trống.
- [ ] Tạo thành công Tournament và các vòng đua phụ thuộc.
- [ ] Ngày kết thúc giải đấu phải sau ngày bắt đầu.

## Test Notes
Giải đấu là thực thể chính quản lý trận đấu. Cần làm sớm để Khang lấy Tournament ID đăng ký ngựa (`BE-REG-01`).

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/TournamentAndRacing/Services/TournamentService.cs
- backend/src/HorseRacing.API/Controllers/AdminController.cs
- backend/src/HorseRacing.Infrastructure/Repositories/TournamentRepository.cs
```

---

### [BE-RACE-02] Lên lịch thi đấu (Scheduling Races)
- **Labels:** `backend`, `phase-2`, `priority-p1`, `race`, `blocked`
- **Milestone:** `Sprint 2 - Core Master Data`
- **Project Status:** `Backlog`
- **Assignee:** Hàn

```markdown
## Task ID
BE-RACE-02

## Module
Tournament, Race Scheduling & Officiating

## Owner
Hàn

## Phase
Phase 2

## Priority
P1

## Description
Admin tạo các cuộc đua (Races) trong từng vòng đấu của giải đấu và thiết lập thời gian thi đấu, cự ly.

## APIs
- POST /api/races
- GET /api/races/schedule

## Roles
- Admin (POST)
- Public (GET)

## Database Tables
- Race
- Round
- Tournament

## Dependencies
- BE-RACE-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Tạo thành công trận đua thuộc giải đấu.
- [ ] API Lấy lịch thi đấu trả về đầy đủ các trận đấu sắp diễn ra cho mọi người cùng xem công khai.

## Test Notes
Tạo cuộc đua và xem lịch thi đấu công khai.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/TournamentAndRacing/Services/RaceSchedulingService.cs
- backend/src/HorseRacing.API/Controllers/AdminController.cs
- backend/src/HorseRacing.API/Controllers/PublicController.cs
```

---

### [BE-HORSE-02] Quản lý tài liệu và chỉ số ngựa
- **Labels:** `backend`, `phase-2`, `priority-p3`, `horse`, `blocked`
- **Milestone:** `Sprint 2 - Core Master Data`
- **Project Status:** `Backlog`
- **Assignee:** Khang

```markdown
## Task ID
BE-HORSE-02

## Module
Horse Owner, Horse Management & Jockey Contract

## Owner
Khang

## Phase
Phase 2

## Priority
P3

## Description
Quản lý hồ sơ tài liệu chứng nhận sức khỏe ngựa (HorseDocument) và các chỉ số hiệu suất tốc độ (HorseStatistic).

## APIs
- POST /api/horses/{id}/documents
- GET /api/horses/{id}/stats

## Roles
- HorseOwner
- Referee
- Public

## Database Tables
- HorseDocument
- HorseStatistic
- Horse

## Dependencies
- BE-HORSE-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Hiện thực hóa `HorseDocument` và `HorseStatistic` đang trống để lưu hồ sơ ngựa.
- [ ] Hỗ trợ lưu trữ thông tin/đường dẫn tài liệu sức khỏe ngựa.

## Test Notes
Task này không block ai nên có thể làm sau cùng ở Phase 2.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/HorseManagement/Services/HorseDocumentService.cs
- backend/src/HorseRacing.API/Controllers/OwnerController.cs
```

---

### [BE-CONTRACT-01] Gửi hợp đồng thuê nài ngựa
- **Labels:** `backend`, `phase-3`, `priority-p1`, `horse`, `blocked`
- **Milestone:** `Sprint 3 - Registration & Assignment`
- **Project Status:** `Backlog`
- **Assignee:** Khang

```markdown
## Task ID
BE-CONTRACT-01

## Module
Horse Owner, Horse Management & Jockey Contract

## Owner
Khang

## Phase
Phase 3

## Priority
P1

## Description
HorseOwner tạo hợp đồng nháp và gửi lời mời thuê Jockey hỗ trợ đua ngựa trong một thời kỳ cụ thể.

## APIs
- POST /api/jockey-contracts

## Roles
- HorseOwner

## Database Tables
- JockeyContract
- JockeyProfile
- AppUser

## Dependencies
- BE-HORSE-01
- BE-USER-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Hiện thực hóa thực thể `JockeyContract.cs`.
- [ ] API gửi hợp đồng từ Owner tới Jockey ở trạng thái ban đầu là `Pending`.
- [ ] Kiểm tra xem Jockey được thuê có đang ở trạng thái `Active` hay không.

## Test Notes
Yêu cầu hệ thống phải có ngựa và tài khoản Jockey đang active.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/ContractAndRegistration/Services/JockeyContractService.cs
- backend/src/HorseRacing.API/Controllers/OwnerController.cs
```

---

### [BE-CONTRACT-02] Phản hồi hợp đồng (Jockey Action)
- **Labels:** `backend`, `phase-3`, `priority-p1`, `horse`, `blocked`
- **Milestone:** `Sprint 3 - Registration & Assignment`
- **Project Status:** `Backlog`
- **Assignee:** Khang

```markdown
## Task ID
BE-CONTRACT-02

## Module
Horse Owner, Horse Management & Jockey Contract

## Owner
Khang

## Phase
Phase 3

## Priority
P1

## Description
Jockey xem danh sách các lời mời hợp đồng hiện có và phản hồi Đồng ý (Accept) hoặc Từ chối (Reject).

## APIs
- GET /api/jockeys/contracts
- PUT /api/jockeys/contracts/{id}/respond

## Roles
- Jockey

## Database Tables
- JockeyContract

## Dependencies
- BE-CONTRACT-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Chỉ Jockey được chỉ định mới được duyệt/từ chối hợp đồng của mình.
- [ ] Hợp đồng duyệt thành công chuyển trạng thái thành `Active` hoặc `Rejected`.

## Test Notes
API duyệt/từ chối hợp đồng. Hợp đồng chuyển sang `Active`.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/ContractAndRegistration/Services/JockeyContractService.cs
- backend/src/HorseRacing.API/Controllers/JockeyController.cs
```

---

### [BE-REG-01] Đăng ký ngựa tham gia Giải đấu
- **Labels:** `backend`, `phase-3`, `priority-p1`, `horse`, `blocked`
- **Milestone:** `Sprint 3 - Registration & Assignment`
- **Project Status:** `Backlog`
- **Assignee:** Khang

```markdown
## Task ID
BE-REG-01

## Module
Horse Owner, Horse Management & Jockey Contract

## Owner
Khang

## Phase
Phase 3

## Priority
P1

## Description
HorseOwner đăng ký ngựa của mình tham gia một giải đấu đang mở đơn đăng ký.

## APIs
- POST /api/registrations

## Roles
- HorseOwner

## Database Tables
- Registration
- Tournament
- Horse

## Dependencies
- BE-HORSE-01
- BE-RACE-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Hiện thực thực thể `Registration.cs` trống.
- [ ] Người đăng ký phải sở hữu ngựa đó.
- [ ] Giải đấu phải ở trạng thái nhận đơn đăng ký (`AcceptingEntries`).

## Test Notes
API Owner đăng ký ngựa vào giải đấu đang mở.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/ContractAndRegistration/Services/RegistrationService.cs
- backend/src/HorseRacing.API/Controllers/OwnerController.cs
```

---

### [BE-RACE-04] Gán trọng tài điều hành cuộc đua
- **Labels:** `backend`, `phase-3`, `priority-p2`, `race`, `blocked`
- **Milestone:** `Sprint 3 - Registration & Assignment`
- **Project Status:** `Backlog`
- **Assignee:** Hàn

```markdown
## Task ID
BE-RACE-04

## Module
Tournament, Race Scheduling & Officiating

## Owner
Hàn

## Phase
Phase 3

## Priority
P2

## Description
Admin gán trọng tài điều hành trận đua (`RaceRefereeAssignment`).

## APIs
- POST /api/races/{id}/referees

## Roles
- Admin

## Database Tables
- RaceRefereeAssignment
- Race
- RefereeProfile

## Dependencies
- BE-RACE-02
- BE-USER-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Hiện thực thực thể trống `RaceRefereeAssignment.cs`.
- [ ] Lưu vết chính xác Trọng tài nào điều hành cuộc đua nào.

## Test Notes
Cần có trận đấu (`Race`) và tài khoản trọng tài để gán việc.

## Related Files / Folders
- backend/src/RaceSchedulingService.cs (cần được chuyển/hiện thực đúng module)
- backend/src/AdminController.cs
```

---

### [BE-RACE-03] Ghép cặp trận đấu (Lane Assignment)
- **Labels:** `backend`, `phase-3`, `priority-p0`, `race`, `blocked`
- **Milestone:** `Sprint 3 - Registration & Assignment`
- **Project Status:** `Backlog`
- **Assignee:** Hàn

```markdown
## Task ID
BE-RACE-03

## Module
Tournament, Race Scheduling & Officiating

## Owner
Hàn

## Phase
Phase 3

## Priority
P0

## Description
Hệ thống hoặc Admin chuyển các lượt đăng ký đã được duyệt thành danh sách tham gia cuộc đua (`RaceEntry`), gán Làn chạy (Lane), ngựa và Jockey.

## APIs
- POST /api/races/{id}/entries

## Roles
- Admin

## Database Tables
- RaceEntry
- Race
- Registration
- JockeyContract

## Dependencies
- BE-RACE-02
- BE-REG-01
- BE-CONTRACT-02

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Xếp ngựa và Jockey vào làn chạy (`RaceEntry`).
- [ ] Jockey phải có hợp đồng `Active` với Owner của ngựa đó.
- [ ] Một cuộc đua không được chứa trùng Jockey hoặc trùng Ngựa ở các làn chạy khác nhau.

## Test Notes
**Task cực kỳ quan trọng**. Kết nối Ngựa, Jockey (hợp đồng Active), Giải đấu và Trận đấu vào làn chạy (`RaceEntry`). Quyết định đầu vào cho việc đặt cược và kết quả.

## Related Files / Folders
- backend/src/RaceSchedulingService.cs
- backend/src/AdminController.cs
```

---

### [BE-REF-01] Trọng tài giám sát & vi phạm
- **Labels:** `backend`, `phase-4`, `priority-p1`, `race`, `blocked`
- **Milestone:** `Sprint 4 - Result & Financial Flow`
- **Project Status:** `Backlog`
- **Assignee:** Hàn

```markdown
## Task ID
BE-REF-01

## Module
Tournament, Race Scheduling & Officiating

## Owner
Hàn

## Phase
Phase 4

## Priority
P1

## Description
Trọng tài kiểm tra thông tin ngựa, ghi nhận các lỗi vi phạm (`RaceViolation`) trong quá trình đua của ngựa/nài.

## APIs
- POST /api/referee/violations
- GET /api/referee/violations/{raceId}

## Roles
- Referee

## Database Tables
- RaceViolation
- Race
- RaceEntry

## Dependencies
- BE-RACE-04
- BE-RACE-03

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Chỉ trọng tài được gán vào cuộc đua này mới được quyền ghi nhận vi phạm.
- [ ] Thông tin vi phạm mô tả rõ lỗi và mức phạt hình thức.

## Test Notes
API ghi nhận lỗi vi phạm của ngựa/Jockey trong làn chạy.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/OfficiatingAndResults/Services/RefereeService.cs
- backend/src/HorseRacing.API/Controllers/RefereeController.cs
```

---

### [BE-RESULT-01] Ghi nhận & Công bố kết quả đua
- **Labels:** `backend`, `phase-4`, `priority-p0`, `race`, `blocked`
- **Milestone:** `Sprint 4 - Result & Financial Flow`
- **Project Status:** `Backlog`
- **Assignee:** Hàn

```markdown
## Task ID
BE-RESULT-01

## Module
Tournament, Race Scheduling & Officiating

## Owner
Hàn

## Phase
Phase 4

## Priority
P0

## Description
Trọng tài cập nhật kết quả thứ hạng cuộc đua (`RaceResult`). Admin thực hiện duyệt và công bố (Publish) kết quả để kích hoạt các nghiệp vụ tài chính.

## APIs
- POST /api/referee/results
- POST /api/admin/races/{id}/publish

## Roles
- Referee (Result)
- Admin (Publish)

## Database Tables
- RaceResult
- Race
- RaceEntry

## Dependencies
- BE-REF-01
- BE-RACE-03

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Kết quả cập nhật lưu đúng mã ID cuộc đua, tên người chiến thắng.
- [ ] Khi Admin Publish kết quả, trạng thái cuộc đua chuyển sang `Finished`.

## Test Notes
Kết quả xếp hạng dựa trên danh sách làn chạy. Cần công bố kết quả để kích hoạt tính năng trả thưởng của Đắc.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/OfficiatingAndResults/Services/RaceResultService.cs
- backend/src/HorseRacing.API/Controllers/RefereeController.cs
- backend/src/HorseRacing.API/Controllers/AdminController.cs
```

---

### [BE-REF-02] Trọng tài lập biên bản & Báo cáo
- **Labels:** `backend`, `phase-4`, `priority-p3`, `race`, `blocked`
- **Milestone:** `Sprint 4 - Result & Financial Flow`
- **Project Status:** `Backlog`
- **Assignee:** Hàn

```markdown
## Task ID
BE-REF-02

## Module
Tournament, Race Scheduling & Officiating

## Owner
Hàn

## Phase
Phase 4

## Priority
P3

## Description
Trọng tài viết báo cáo tổng kết diễn biến cuộc đua (`RefereeReport`) sau khi cuộc đua hoàn thành.

## APIs
- POST /api/referee/reports

## Roles
- Referee

## Database Tables
- RefereeReport
- Race

## Dependencies
- BE-REF-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Hiện thực `RefereeReport.cs` trống.
- [ ] API viết báo cáo trận đấu. Chỉ cho viết báo cáo khi trận đấu có trạng thái `Running` hoặc `Finished`.

## Test Notes
Biên bản báo cáo không trực tiếp ảnh hưởng đến luồng trả thưởng nên có thể hoàn thiện sau cùng.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/OfficiatingAndResults/Services/RefereeService.cs
- backend/src/HorseRacing.API/Controllers/RefereeController.cs
```

---

### [BE-BET-01] Khán giả đặt cược (Place Bets)
- **Labels:** `backend`, `phase-5`, `priority-p0`, `betting`, `blocked`
- **Milestone:** `Sprint 4 - Result & Financial Flow`
- **Project Status:** `Backlog`
- **Assignee:** Đắc

```markdown
## Task ID
BE-BET-01

## Module
Spectator Betting, Financials & Notifications

## Owner
Đắc

## Phase
Phase 5

## Priority
P0

## Description
Spectator đặt cược một lượng tiền vào một chú ngựa cụ thể trong một cuộc đua sắp diễn ra.

## APIs
- POST /api/bets
- GET /api/bets/my-bets

## Roles
- Spectator

## Database Tables
- Bet
- Wallet
- WalletTransaction
- Race

## Dependencies
- BE-WALLET-01
- BE-RACE-03

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Hiện thực `Bet.cs` trống.
- [ ] Chỉ cho phép đặt cược khi trận đua ở trạng thái `Scheduled` (chưa chạy).
- [ ] Số tiền cược được trừ trực tiếp từ ví và tạo giao dịch trừ tiền (`WalletTransaction`).

## Test Notes
Cần Hàn hoàn thành xếp làn chạy (`BE-RACE-03`) để Spectator cược vào ngựa chạy ở làn cụ thể. Đắc cần bỏ mock dữ liệu làn chạy.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/BettingEngine/Services/BettingService.cs
- backend/src/HorseRacing.API/Controllers/SpectatorController.cs
- backend/src/HorseRacing.Infrastructure/Repositories/BetRepository.cs
```

---

### [BE-BET-02] Quản lý dự đoán (Predictions)
- **Labels:** `backend`, `phase-5`, `priority-p2`, `betting`, `blocked`
- **Milestone:** `Sprint 4 - Result & Financial Flow`
- **Project Status:** `Backlog`
- **Assignee:** Đắc

```markdown
## Task ID
BE-BET-02

## Module
Spectator Betting, Financials & Notifications

## Owner
Đắc

## Phase
Phase 5

## Priority
P2

## Description
Spectator đưa ra dự đoán kết quả không mất phí (Minigame dự đoán của Admin).

## APIs
- POST /api/predictions
- GET /api/predictions

## Roles
- Spectator

## Database Tables
- Prediction
- Race

## Dependencies
- BE-RACE-02

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Lưu trữ chính xác dự đoán.
- [ ] Hỗ trợ hiển thị tỷ lệ dự đoán từ đám đông cho Admin và khán giả cùng xem.

## Test Notes
Dự đoán miễn phí chỉ cần có lịch thi đấu (`Race`) của Hàn là có thể chạy thử nghiệm được ngay.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/BettingEngine/Services/PredictionService.cs
- backend/src/HorseRacing.API/Controllers/SpectatorController.cs
```

---

### [BE-PAY-01] Tự động trả thưởng đặt cược
- **Labels:** `backend`, `phase-5`, `priority-p0`, `wallet`, `blocked`
- **Milestone:** `Sprint 4 - Result & Financial Flow`
- **Project Status:** `Backlog`
- **Assignee:** Đắc

```markdown
## Task ID
BE-PAY-01

## Module
Spectator Betting, Financials & Notifications

## Owner
Đắc

## Phase
Phase 5

## Priority
P0

## Description
Hệ thống tính toán và tự động trả tiền thắng cược (Payout) vào ví khán giả khi trận đấu chuyển sang trạng thái `Finished`.

## APIs
Không có API trực tiếp (được trigger từ API Publish của Admin khi công bố kết quả trận đấu).

## Roles
- System
- Admin (Duyệt kết quả)

## Database Tables
- Payout
- Bet
- RaceResult
- Wallet
- WalletTransaction

## Dependencies
- BE-BET-01
- BE-RESULT-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Hiện thực thực thể `Payout.cs` trống.
- [ ] Tự động cộng tiền ví khán giả thắng cược khi Admin Publish kết quả.
- [ ] Thuật toán phân bổ thưởng tính toán đúng tỉ lệ odds cược.
- [ ] Cộng tiền và ghi nhận giao dịch cộng tiền vào ví Spectator thắng cuộc.

## Test Notes
Đắc cần kết nối service trả thưởng vào API Publish của Hàn.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/FinancialRewards/Services/BetPayoutService.cs
```

---

### [BE-PAY-02] Trả thưởng giải đấu cho HorseOwner/Jockey
- **Labels:** `backend`, `phase-5`, `priority-p1`, `wallet`, `blocked`
- **Milestone:** `Sprint 4 - Result & Financial Flow`
- **Project Status:** `Backlog`
- **Assignee:** Đắc

```markdown
## Task ID
BE-PAY-02

## Module
Spectator Betting, Financials & Notifications

## Owner
Đắc

## Phase
Phase 5

## Priority
P1

## Description
Tính toán và phân bổ giải thưởng tiền mặt (Prize) cho Chủ ngựa và Nài ngựa đạt hạng cao trong giải đấu.

## APIs
- POST /api/admin/payouts/prizes

## Roles
- Admin

## Database Tables
- Prize
- TournamentPrizePayout
- Wallet
- WalletTransaction
- RaceResult

## Dependencies
- BE-RESULT-01
- BE-USER-01
- BE-WALLET-01

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Hiện thực `Prize` và `TournamentPrizePayout` trống.
- [ ] API Admin duyệt chi thưởng 70/30.
- [ ] Phân bổ đúng tỉ lệ giải thưởng cho Chủ ngựa sở hữu ngựa vô địch và Jockey điều khiển ngựa đó.

## Test Notes
API này chỉ chạy được khi đã xác định được người thắng cuộc giải đấu và có ví của Jockey/Owner.

## Related Files / Folders
- backend/src/HorseRacing.Application/Features/FinancialRewards/Services/PrizePayoutService.cs
- backend/src/HorseRacing.API/Controllers/AdminController.cs
```

---

### [Integration] Tích hợp thông báo toàn hệ thống
- **Labels:** `backend`, `phase-5`, `priority-p1`, `notification`, `blocked`
- **Milestone:** `Sprint 4 - Result & Financial Flow`
- **Project Status:** `Backlog`
- **Assignee:** Đắc

```markdown
## Task ID
Integration

## Module
Spectator Betting, Financials & Notifications

## Owner
Đắc

## Phase
Phase 5

## Priority
P1

## Description
Toàn bộ các hành động nạp/rút, cược, trúng thưởng, có lời mời hợp đồng đều tự động gửi thông báo.

## APIs
Không có API trực tiếp (được gọi từ các service nghiệp vụ khác)

## Roles
- Đã xác thực (tất cả các vai trò liên quan nhận thông báo)

## Database Tables
- Notification

## Dependencies
- BE-NOTI-01 (Helper Service thông báo)
- Các sự kiện Phase 3, 4, 5

## Acceptance Criteria
- [ ] API build không lỗi
- [ ] API hiện trong Swagger
- [ ] Authorization đúng
- [ ] Validate input cơ bản
- [ ] Test được bằng Swagger/Postman
- [ ] Không trả dữ liệu nhạy cảm
- [ ] Đúng status code 200/201/400/401/403/404
- [ ] Khi nạp/rút thành công, hệ thống tự động bắn thông báo cho Spectator.
- [ ] Khi Owner gửi JockeyContract, gửi thông báo cho Jockey.
- [ ] Khi Jockey phản hồi hợp đồng, gửi thông báo cho Owner.
- [ ] Khi có kết quả cuộc đua và trả thưởng, gửi thông báo chúc mừng trúng cược cho Spectator và kết quả giải đấu cho Owner/Jockey.

## Test Notes
Toàn bộ các hành động nạp/rút, cược, trúng thưởng, có lời mời hợp đồng đều tự động gửi thông báo.

## Related Files / Folders
- Các Service nghiệp vụ của Phase 3, 4, 5
- backend/src/HorseRacing.Application/Features/Notifications/Services/NotificationService.cs
```
