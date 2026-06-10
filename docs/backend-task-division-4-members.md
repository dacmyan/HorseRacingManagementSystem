# Backend Task Division - Horse Racing Management System

## 1. Mục tiêu tài liệu

Tài liệu này được biên soạn để thực hiện việc phân chia nhiệm vụ (task division) phát triển phần Backend cho nhóm gồm **4 thành viên** thuộc dự án **Horse Racing Management System** (môn học SWP391).

Mục tiêu cốt lõi:
*   **Rõ ràng trách nhiệm**: Mỗi thành viên nắm rõ mình phụ trách module nào, làm việc với các thực thể (Entities), bảng cơ sở dữ liệu (Database Tables), dịch vụ (Services) và API Endpoints nào.
*   **Tránh xung đột mã nguồn (Conflict Code)**: Thiết kế ranh giới các module độc lập tối đa để các thành viên có thể làm việc song song, dễ dàng tạo nhánh (Git branch) và Pull Request (PR) mà không đè code lên nhau.
*   **Lộ trình phát triển tối ưu (MVP First)**: Xác định rõ thứ tự thực hiện từ các tính năng nền tảng (Xác thực, Phân quyền) đến các tính năng cốt lõi (Quản lý Ngựa, Giải đấu) và nâng cao (Đặt cược, Thanh toán, Thông báo).
*   **Tính nhất quán trong kiến trúc**: Đảm bảo toàn bộ nhóm tuân thủ cấu trúc **Clean Architecture (Onion Architecture)** đã thiết lập sẵn trong dự án.

---

## 2. Tổng quan module backend

Dựa trên cấu trúc mã nguồn thực tế của dự án (kiến trúc Clean Architecture chia thành 4 lớp: API, Application, Domain, Infrastructure), mã nguồn Backend hiện đã có sẵn khung xương (skeleton) chia theo tính năng (Feature-based). Tuy nhiên, hầu hết các tệp nghiệp vụ và một số thực thể cơ sở dữ liệu hiện tại đang là tệp trống (0 bytes).

Chúng ta phân chia hệ thống thành 4 module backend chính tương ứng với vai trò của 4 thành viên:

### Module 1: Authentication, User & Role Management (Nền tảng hệ thống)
*   **Nghiệp vụ**: Xác thực JWT, đăng nhập, đăng ký, phân quyền dựa trên Role, quản lý tài khoản người dùng, quản lý hồ sơ người dùng (Profiles), thiết lập dữ liệu mẫu (Seed data).
*   **Thực thể Domain**: `Role`, `AppUser`, `JockeyProfile`, `RefereeProfile`, `Wallet` (khởi tạo mặc định cho Spectator).
*   **Folder chính**: [UserManagement](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement).

### Module 2: Horse Owner, Horse Management & Jockey Contract (Quản lý thực thể tham gia)
*   **Nghiệp vụ**: Quản lý hồ sơ ngựa (đăng ký, cập nhật), tài liệu chứng nhận sức khỏe ngựa, thống kê hiệu suất ngựa, quy trình ký kết hợp đồng thuê nài ngựa (Jockey Contract) giữa Chủ ngựa (Horse Owner) và Nài ngựa (Jockey), đăng ký ngựa tham gia giải đấu.
*   **Thực thể Domain**: `Horse`, `HorseDocument`, `HorseStatistic`, `Registration`, `JockeyContract`, `RaceEntry` (tham chiếu), `JockeyProfile` (tham chiếu).
*   **Folder chính**: [HorseManagement](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/HorseManagement) và [ContractAndRegistration](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/ContractAndRegistration).

### Module 3: Tournament, Race Scheduling & Officiating (Quản lý giải đấu & Điều hành trận đua)
*   **Nghiệp vụ**: Quản lý giải đấu (Tournament), vòng đấu (Round), cuộc đua (Race). Xếp lịch thi đấu (gán ngựa và nài vào làn chạy - Lane). Giao việc cho trọng tài (Referee Assignment). Ghi nhận vi phạm cuộc đua (Violation), lập biên bản báo cáo (Referee Report), và ghi nhận kết quả chung cuộc (Race Result).
*   **Thực thể Domain**: `Tournament`, `Round`, `Race`, `RaceEntry`, `RaceResult`, `RaceRefereeAssignment`, `RefereeReport`, `RaceViolation`.
*   **Folder chính**: [TournamentAndRacing](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/TournamentAndRacing) và [OfficiatingAndResults](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/OfficiatingAndResults).

### Module 4: Spectator Betting, Financials & Notifications (Khán giả, Đặt cược & Tài chính)
*   **Nghiệp vụ**: Quản lý ví điện tử của người dùng (Nạp/Rút tiền), đặt cược (Bet) của khán giả, quản lý dự đoán (Prediction) tỷ số không mất tiền, hệ thống tính toán tỷ lệ cược tự động, tự động trả thưởng (Payout) cho khán giả khi có kết quả đua, trả giải thưởng giải đấu (Prize) cho chủ ngựa/nài ngựa đạt giải, hệ thống thông báo thời gian thực.
*   **Thực thể Domain**: `Bet`, `Payout`, `Prediction`, `Wallet`, `WalletTransaction`, `Prize`, `TournamentPrizePayout`, `Notification`.
*   **Folder chính**: [BettingEngine](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine), [FinancialRewards](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards), và [Notifications](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/Notifications).

---

## 3. Phân chia task cho 4 backend members

Bảng dưới đây tóm tắt phân chia trách nhiệm tổng quan của 4 thành viên nhằm đảm bảo tính cân bằng về khối lượng công việc và giảm thiểu tối đa xung đột mã nguồn:

| Member | Module phụ trách | Actor chính | Bảng DB liên quan | Folder/File chính | Mức độ ưu tiên |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Triều** | Auth, User, Role & Profiles | Admin, Public | `Role`<br>`AppUser`<br>`JockeyProfile`<br>`RefereeProfile`<br>`Wallet` | `Application/Features/UserManagement/`<br>`Domain/Entities/Users/`<br>`Infrastructure/Repositories/UserRepository.cs`<br>`API/Controllers/AuthController.cs`<br>`API/Controllers/AdminController.cs`<br>`API/Controllers/AuthTestController.cs` | **Cao nhất** (Phase 1) |
| **Khang** | Horse Owner, Horses & Contracts | HorseOwner, Jockey | `Horse`<br>`HorseDocument`<br>`HorseStatistic`<br>`Registration`<br>`JockeyContract`<br>`RaceEntry` (Xem)<br>`JockeyProfile`<br>`AppUser` | `Application/Features/HorseManagement/`<br>`Application/Features/ContractAndRegistration/`<br>`Domain/Entities/Equines/`<br>`Domain/Entities/Tournaments/Registration.cs`<br>`Domain/Entities/Tournaments/JockeyContract.cs`<br>`API/Controllers/OwnerController.cs`<br>`API/Controllers/JockeyController.cs`<br>`Infrastructure/Repositories/HorseRepository.cs` | **Cao** (Phase 2 & 3) |
| **Hàn** | Tournaments, Races, Referee & Results | Admin, Referee, Public | `Tournament`<br>`Round`<br>`Race`<br>`RaceEntry`<br>`RaceResult`<br>`RaceRefereeAssignment`<br>`RefereeReport`<br>`RaceViolation`<br>`RefereeProfile` | `Application/Features/TournamentAndRacing/`<br>`Application/Features/OfficiatingAndResults/`<br>`Domain/Entities/Tournaments/`<br>`Domain/Entities/Compliance/`<br>`API/Controllers/RefereeController.cs`<br>`API/Controllers/AdminController.cs` (Races)<br>`API/Controllers/PublicController.cs` (Schedules)<br>`Infrastructure/Repositories/TournamentRepository.cs` | **Cao** (Phase 2 & 3) |
| **Đắc** | Spectators, Betting, Wallet & Notification | Spectator, Admin, Public | `Bet`<br>`Payout`<br>`Prediction`<br>`Wallet`<br>`WalletTransaction`<br>`Prize`<br>`TournamentPrizePayout`<br>`Notification` | `Application/Features/BettingEngine/`<br>`Application/Features/FinancialRewards/`<br>`Application/Features/Notifications/`<br>`Domain/Entities/Financials/`<br>`Domain/Entities/Notifications/`<br>`API/Controllers/SpectatorController.cs`<br>`API/Controllers/PublicController.cs` (Rankings)<br>`Infrastructure/Repositories/BetRepository.cs` | **Trung bình** (Phase 4) |

---

## 4. Task chi tiết cho từng member

### Triều: Auth, User, Role & Profiles

| Task ID | Task name | Mô tả | API cần làm | Method | Role được phép gọi | Bảng DB | File/folder cần làm | Dependency | Priority | Acceptance Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **BE-AUTH-01** | Cải tiến Login & Register Validation | Thêm logic kiểm tra tính hợp lệ dữ liệu (FluentValidation) cho đăng nhập và đăng ký Spectator. | `POST /api/auth/login`<br>`POST /api/auth/register` | POST | Public | `AppUser`, `Role`, `Wallet` | [AuthController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AuthController.cs)<br>[AuthService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/Services/AuthService.cs) | Không | Cao | Đăng nhập thành công trả về JWT hợp lệ. Đăng ký Spectator tự động tạo ví rỗng với số dư = 0. Có kiểm tra định dạng email và độ dài mật khẩu (> 6 ký tự). |
| **BE-USER-01** | Admin tạo tài khoản nghiệp vụ | Admin tạo trực tiếp các tài khoản đặc quyền như Referee, Jockey, HorseOwner kèm theo thông tin hồ sơ đi kèm. | `POST /api/admin/accounts` | POST | Admin | `AppUser`, `JockeyProfile`, `RefereeProfile`, `Role` | [AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs)<br>[AdminService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/Services/AdminService.cs) | **BE-AUTH-01** | Cao | Admin tạo thành công tài khoản. Nếu tạo Jockey phải chèn bản ghi tự động vào `JockeyProfile`, tạo Referee chèn vào `RefereeProfile`. Báo lỗi nếu thiếu LicenseNumber đối với Referee. |
| **BE-USER-02** | Xem và cập nhật Profile cá nhân | Lấy thông tin chi tiết tài khoản hiện tại và cập nhật thông tin cá nhân (bao gồm cả cập nhật JockeyProfile/RefereeProfile tương ứng). | `GET /api/users/profile`<br>`PUT /api/users/profile` | GET / PUT | Đã xác thực (All roles) | `AppUser`, `JockeyProfile`, `RefereeProfile` | [ProfileService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/Services/ProfileService.cs)<br>[PublicController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/PublicController.cs) | **BE-USER-01** | Trung bình | Trả về đúng thông tin user dựa trên Token Claims. Cập nhật được FullName, ExperienceYears (đối với Jockey/Referee). Không trả về PasswordHash. |
| **BE-USER-03** | Admin quản lý danh sách Users | Admin xem danh sách toàn bộ người dùng kèm theo bộ lọc theo Role, thay đổi trạng thái hoạt động (Active/Inactive) của tài khoản. | `GET /api/admin/users`<br>`PUT /api/admin/users/{id}/status` | GET / PUT | Admin | `AppUser`, `Role` | [AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs)<br>[AdminService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/Services/AdminService.cs) | **BE-USER-01** | Trung bình | Trả ra danh sách phân trang người dùng. Cập nhật trạng thái người dùng thành công, người dùng bị khóa (Inactive) không thể đăng nhập ở API Login. |

---

### Khang: Horse Owner, Horses & Contracts

| Task ID | Task name | Mô tả | API cần làm | Method | Role được phép gọi | Bảng DB | File/folder cần làm | Dependency | Priority | Acceptance Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **BE-HORSE-01** | Quản lý Ngựa đua (CRUD Horses) | HorseOwner đăng ký mới ngựa đua của mình, cập nhật thông tin ngựa, hoặc xem danh sách ngựa thuộc sở hữu. | `POST /api/horses`<br>`GET /api/horses/my-horses`<br>`PUT /api/horses/{id}`<br>`DELETE /api/horses/{id}` | POST / GET / PUT / DELETE | HorseOwner | `Horse` | [HorseService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/HorseManagement/Services/HorseService.cs)<br>[OwnerController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs)<br>[HorseRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/HorseRepository.cs) | **BE-AUTH-01** | Cao | Thêm mới ngựa thành công lưu đúng `OwnerId` lấy từ Token. Update/Delete chỉ cho phép chính chủ ngựa thực hiện. |
| **BE-HORSE-02** | Quản lý tài liệu và chỉ số ngựa | Quản lý hồ sơ tài liệu chứng nhận sức khỏe ngựa (HorseDocument) và các chỉ số hiệu suất tốc độ (HorseStatistic). | `POST /api/horses/{id}/documents`<br>`GET /api/horses/{id}/stats` | POST / GET | HorseOwner, Referee, Public | `HorseDocument`, `HorseStatistic`, `Horse` | [HorseDocumentService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/HorseManagement/Services/HorseDocumentService.cs)<br>[OwnerController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs) | **BE-HORSE-01** | Thấp | Cần hiện thực hóa thực thể `HorseDocument` và `HorseStatistic` đang trống. Hỗ trợ lưu trữ thông tin/đường dẫn tài liệu sức khỏe ngựa. |
| **BE-CONTRACT-01**| Gửi hợp đồng thuê nài ngựa (Jockey Contract) | HorseOwner tạo hợp đồng nháp và gửi lời mời thuê Jockey hỗ trợ đua ngựa trong một thời kỳ cụ thể. | `POST /api/jockey-contracts` | POST | HorseOwner | `JockeyContract`, `JockeyProfile`, `AppUser` | [JockeyContractService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/ContractAndRegistration/Services/JockeyContractService.cs)<br>[OwnerController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs) | **BE-HORSE-01**, **BE-USER-01** | Trung bình | Hiện thực hóa thực thể trống `JockeyContract.cs`. Trạng thái ban đầu của hợp đồng là `Pending`. Kiểm tra xem Jockey được thuê có đang ở trạng thái `Active` hay không. |
| **BE-CONTRACT-02**| Phản hồi hợp đồng (Jockey Action) | Jockey xem danh sách các lời mời hợp đồng hiện có và phản hồi Đồng ý (Accept) hoặc Từ chối (Reject). | `GET /api/jockeys/contracts`<br>`PUT /api/jockeys/contracts/{id}/respond` | GET / PUT | Jockey | `JockeyContract` | [JockeyContractService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/ContractAndRegistration/Services/JockeyContractService.cs)<br>[JockeyController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/JockeyController.cs) | **BE-CONTRACT-01** | Trung bình | Chỉ Jockey được chỉ định mới được duyệt hợp đồng của mình. Trạng thái hợp đồng chuyển thành `Active` hoặc `Rejected`. |
| **BE-REG-01** | Đăng ký ngựa tham gia Giải đấu | HorseOwner đăng ký ngựa của mình tham gia một giải đấu đang mở đơn đăng ký. | `POST /api/registrations` | POST | HorseOwner | `Registration`, `Tournament`, `Horse` | [RegistrationService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/ContractAndRegistration/Services/RegistrationService.cs)<br>[OwnerController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs) | **BE-HORSE-01**, **BE-RACE-01** | Cao | Hiện thực thực thể `Registration.cs` trống. Người đăng ký phải sở hữu ngựa đó. Giải đấu phải ở trạng thái nhận đơn đăng ký (`AcceptingEntries`). |

---

### Hàn: Tournaments, Races, Referee & Results

| Task ID | Task name | Mô tả | API cần làm | Method | Role được phép gọi | Bảng DB | File/folder cần làm | Dependency | Priority | Acceptance Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **BE-RACE-01** | Quản lý Giải đấu (Tournaments) | Admin tạo mới giải đấu, cập nhật thông tin giải đấu và các vòng đấu (Round). | `POST /api/tournaments`<br>`PUT /api/tournaments/{id}`<br>`POST /api/tournaments/{id}/rounds` | POST / PUT | Admin | `Tournament`, `Round` | [TournamentService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/TournamentAndRacing/Services/TournamentService.cs)<br>[AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs)<br>[TournamentRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/TournamentRepository.cs) | **BE-AUTH-01** | Cao | Hiện thực thực thể `Round.cs` trống. Tạo thành công Tournament và các vòng đua phụ thuộc. Ngày kết thúc giải đấu phải sau ngày bắt đầu. |
| **BE-RACE-02** | Lên lịch thi đấu (Scheduling Races) | Admin tạo các cuộc đua (Races) trong từng vòng đấu của giải đấu và thiết lập thời gian thi đấu, cự ly. | `POST /api/races`<br>`GET /api/races/schedule` | POST / GET | Admin (POST) / Public (GET) | `Race`, `Round`, `Tournament` | [RaceSchedulingService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/TournamentAndRacing/Services/RaceSchedulingService.cs)<br>[AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs)<br>[PublicController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/PublicController.cs) | **BE-RACE-01** | Cao | Tạo thành công trận đua thuộc giải đấu. API Lấy lịch thi đấu trả về đầy đủ các trận đấu sắp diễn ra cho mọi người cùng xem. |
| **BE-RACE-03** | Ghép cặp trận đấu (Lane Assignment) | Hệ thống hoặc Admin chuyển các lượt đăng ký đã được duyệt thành danh sách tham gia cuộc đua (`RaceEntry`), gán Làn chạy (Lane), ngựa và Jockey. | `POST /api/races/{id}/entries` | POST | Admin | `RaceEntry`, `Race`, `Registration`, `JockeyContract` | [RaceSchedulingService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/RaceSchedulingService.cs)<br>[AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs) | **BE-RACE-02**, **BE-REG-01** | Cao | Xếp thành công cặp chạy. Jockey phải có hợp đồng `Active` với Owner của ngựa đó. Một cuộc đua không được chứa trùng Jockey hoặc trùng Ngựa ở các làn chạy khác nhau. |
| **BE-RACE-04** | Gán trọng tài điều hành cuộc đua | Admin gán trọng tài điều hành trận đua (`RaceRefereeAssignment`). | `POST /api/races/{id}/referees` | POST | Admin | `RaceRefereeAssignment`, `Race`, `RefereeProfile` | [RaceSchedulingService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/RaceSchedulingService.cs)<br>[AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs) | **BE-RACE-02**, **BE-USER-01** | Trung bình | Hiện thực thực thể trống `RaceRefereeAssignment.cs`. Lưu vết chính xác Trọng tài nào điều hành cuộc đua nào. |
| **BE-REF-01** | Trọng tài giám sát & vi phạm | Trọng tài kiểm tra thông tin ngựa, ghi nhận các lỗi vi phạm (`RaceViolation`) trong quá trình đua của ngựa/nài. | `POST /api/referee/violations`<br>`GET /api/referee/violations/{raceId}` | POST / GET | Referee | `RaceViolation`, `Race`, `RaceEntry` | [RefereeService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/OfficiatingAndResults/Services/RefereeService.cs)<br>[RefereeController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/RefereeController.cs) | **BE-RACE-04** | Trung bình | Chỉ trọng tài được gán vào cuộc đua này mới được quyền ghi nhận vi phạm. Thông tin vi phạm mô tả rõ lỗi và mức phạt hình thức. |
| **BE-REF-02** | Trọng tài lập biên bản & Báo cáo | Trọng tài viết báo cáo tổng kết diễn biến cuộc đua (`RefereeReport`) sau khi cuộc đua hoàn thành. | `POST /api/referee/reports` | POST | Referee | `RefereeReport`, `Race` | [RefereeService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/OfficiatingAndResults/Services/RefereeService.cs)<br>[RefereeController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/RefereeController.cs) | **BE-REF-01** | Thấp | Hiện thực thực thể trống `RefereeReport.cs`. Chỉ cho viết báo cáo khi trận đấu có trạng thái `Running` hoặc `Finished`. |
| **BE-RESULT-01**| Ghi nhận & Công bố kết quả đua | Trọng tài cập nhật kết quả thứ hạng cuộc đua (`RaceResult`). Admin thực hiện duyệt và công bố (Publish) kết quả để kích hoạt các nghiệp vụ tài chính. | `POST /api/referee/results`<br>`POST /api/admin/races/{id}/publish` | POST | Referee (Result)<br>Admin (Publish) | `RaceResult`, `Race`, `RaceEntry` | [RaceResultService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/OfficiatingAndResults/Services/RaceResultService.cs)<br>[RefereeController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/RefereeController.cs)<br>[AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs) | **BE-REF-01** | Cao | Kết quả cập nhật lưu đúng mã ID cuộc đua, tên người chiến thắng. Khi Admin Publish kết quả, trạng thái cuộc đua chuyển sang `Finished`. |

---

### Đắc: Spectators, Betting, Wallet & Notification

| Task ID | Task name | Mô tả | API cần làm | Method | Role được phép gọi | Bảng DB | File/folder cần làm | Dependency | Priority | Acceptance Criteria |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **BE-WALLET-01** | Giao dịch ví điện tử (Nạp/Rút) | Người dùng Spectator thực hiện nạp tiền (giả lập hoặc qua VNPay/PayOS) và rút tiền. Cập nhật số dư ví và lưu lịch sử giao dịch. | `POST /api/wallet/deposit`<br>`POST /api/wallet/withdraw`<br>`GET /api/wallet/history` | POST / GET | Spectator | `Wallet`, `WalletTransaction` | [WalletService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Services/WalletService.cs)<br>[SpectatorController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs) | **BE-AUTH-01** | Cao | Số dư thay đổi chính xác. Lưu đầy đủ giao dịch `WalletTransaction` với đúng kiểu giao dịch (Deposit/Withdraw) và thời gian thực hiện. Rút tiền không vượt quá số dư hiện có. |
| **BE-BET-01** | Khán giả đặt cược (Place Bets) | Spectator đặt cược một lượng tiền vào một chú ngựa cụ thể trong một cuộc đua sắp diễn ra. | `POST /api/bets`<br>`GET /api/bets/my-bets` | POST / GET | Spectator | `Bet`, `Wallet`, `WalletTransaction`, `Race` | [BettingService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Services/BettingService.cs)<br>[SpectatorController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs)<br>[BetRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/BetRepository.cs) | **BE-WALLET-01**, **BE-RACE-02** | Cao | Hiện thực thực thể `Bet.cs` trống. Chỉ cho phép đặt cược khi trận đua ở trạng thái `Scheduled` (chưa chạy). Số tiền cược được trừ trực tiếp từ ví và tạo giao dịch trừ tiền. |
| **BE-BET-02** | Quản lý dự đoán (Predictions) | Spectator đưa ra dự đoán kết quả không mất phí (Minigame dự đoán của Admin). | `POST /api/predictions`<br>`GET /api/predictions` | POST / GET | Spectator | `Prediction`, `Race` | [PredictionService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Services/PredictionService.cs)<br>[SpectatorController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs) | **BE-RACE-02** | Thấp | Lưu trữ chính xác dự đoán. Hỗ trợ hiển thị tỷ lệ dự đoán từ đám đông cho Admin và khán giả cùng xem. |
| **BE-PAY-01** | Tự động trả thưởng đặt cược | Hệ thống tính toán và tự động trả tiền thắng cược (Payout) vào ví khán giả khi trận đấu chuyển sang trạng thái `Finished`. | Không có API trực tiếp (Chạy ngầm hoặc được trigger khi Publish Result) | Kích hoạt tự động | System / Admin | `Payout`, `Bet`, `RaceResult`, `Wallet`, `WalletTransaction` | [BetPayoutService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Services/BetPayoutService.cs) | **BE-BET-01**, **BE-RESULT-01** | Cao | Hiện thực thực thể `Payout.cs` trống. Thuật toán phân bổ thưởng tính toán đúng tỉ lệ odds cược. Cộng tiền và ghi nhận giao dịch cộng tiền vào ví Spectator thắng cuộc. |
| **BE-PAY-02** | Trả thưởng giải đấu cho HorseOwner/Jockey | Tính toán và phân bổ giải thưởng tiền mặt (Prize) cho Chủ ngựa và Nài ngựa đạt hạng cao trong giải đấu. | `POST /api/admin/payouts/prizes` | POST | Admin | `Prize`, `TournamentPrizePayout`, `Wallet`, `WalletTransaction`, `RaceResult` | [PrizePayoutService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Services/PrizePayoutService.cs)<br>[AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs) | **BE-RESULT-01**, **BE-USER-01** | Trung bình | Hiện thực thực thể `Prize` và `TournamentPrizePayout` trống. Phân bổ đúng tỉ lệ giải thưởng cho Chủ ngựa sở hữu ngựa vô địch và Jockey điều khiển ngựa đó. |
| **BE-NOTI-01** | Tạo và gửi thông báo hệ thống | Tạo các bản ghi thông báo gửi đến người dùng khi có sự kiện đặc biệt (như nạp tiền, lời mời hợp đồng, có lịch đua mới, trả thưởng). | `GET /api/notifications`<br>`PUT /api/notifications/{id}/read` | GET / PUT | Đã xác thực (All roles) | `Notification` | [NotificationService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/Notifications/Services/NotificationService.cs)<br>[PublicController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/PublicController.cs) | **BE-AUTH-01** | Trung bình | Hiện thực thực thể `Notification.cs` trống. Thông báo được đẩy vào DB thành công và trả ra đúng cho người dùng đích qua API. |

---

## 8. Branch strategy cho team

Để quản lý mã nguồn hiệu quả qua Git và tránh xung đột khi merge vào nhánh chính, team backend áp dụng quy trình sau:

### Quy ước đặt tên nhánh (Branching Convention)
Mỗi thành viên làm việc trên các nhánh tính năng (Feature branches) bắt đầu từ nhánh `main` mới nhất:
*   Triều: `feature/auth-user-role`
*   Khang: `feature/horse-registration-jockey`
*   Hàn: `feature/tournament-race-referee`
*   Đắc: `feature/betting-wallet-notification`

Nếu giải quyết một task nhỏ cụ thể, đặt tên theo cấu trúc: `feature/[Tên-Member]/[Mã-Task]` (Ví dụ: `feature/m2/be-horse-01`).

### Quy ước ghi chú Commit (Commit Message Convention)
Sử dụng chuẩn hóa Angular Commit Message để dễ dàng tra cứu lịch sử:
*   `feat: add register horse api` (Thêm tính năng mới)
*   `fix: resolve null pointer exception in auth service` (Sửa lỗi)
*   `refactor: optimize query performance in horse repo` (Tối ưu cấu trúc code)
*   `docs: update API endpoints list in division doc` (Cập nhật tài liệu)

### Quy trình tạo và duyệt Pull Request (PR Workflow)
1.  **Đồng bộ local**: Trước khi viết code mới hoặc trước khi đẩy nhánh lên GitHub, thực hiện kéo code mới nhất từ nhánh chính:
    ```bash
    git checkout main
    git pull origin main
    git checkout feature/[Tên-Nhánh-Của-Bạn]
    git merge main
    ```
2.  **Kiểm tra cú pháp tại Local**: Chạy lệnh `dotnet build` tại thư mục `backend/` đảm bảo dự án không bị lỗi biên dịch.
3.  **Đẩy code**: Thực hiện push nhánh lên GitHub:
    ```bash
    git add .
    git commit -m "feat: [Mô tả ngắn gọn bằng tiếng Anh]"
    git push origin feature/[Tên-Nhánh-Của-Bạn]
    ```
4.  **Tạo Pull Request (PR)**: Tạo PR trên GitHub hướng về nhánh `main`.
5.  **Duyệt Code (Code Review)**: Ít nhất 1 thành viên khác trong nhóm (hoặc Lead Developer) kiểm tra code, đảm bảo không có lỗi logic cơ bản và phê duyệt (Approve) trước khi Merge.

---

## 9. File/folder mà mỗi member nên đụng vào

Bảng chỉ dẫn cụ thể giới hạn không gian làm việc của từng thành viên để giảm thiểu xung đột file tối đa:

| Member | Folder/File chính cần làm việc | Không nên sửa nếu không cần thiết |
| :--- | :--- | :--- |
| **Triều** | *   `src/HorseRacing.Domain/Entities/Users/` (`AppUser.cs`, `Role.cs`, `JockeyProfile.cs`, `RefereeProfile.cs`) <br>*   `src/HorseRacing.Application/Features/UserManagement/`<br>*   `src/HorseRacing.Infrastructure/Repositories/UserRepository.cs`<br>*   `src/HorseRacing.API/Controllers/AuthController.cs`<br>*   `src/HorseRacing.API/Controllers/AdminController.cs` (Phần account) | *   Các thực thể thuộc folder `Equines`, `Compliance`, `Financials`, `Tournaments`. <br>*   Các Feature của thành viên khác.<br>*   `Program.cs` (Báo team trước nếu cần chỉnh sửa cấu hình JWT). |
| **Khang** | *   `src/HorseRacing.Domain/Entities/Equines/` (`Horse.cs`, `HorseDocument.cs`, `HorseStatistic.cs`) <br>*   `src/HorseRacing.Domain/Entities/Tournaments/` (`Registration.cs`, `JockeyContract.cs`) <br>*   `src/HorseRacing.Application/Features/HorseManagement/`<br>*   `src/HorseRacing.Application/Features/ContractAndRegistration/`<br>*   `src/HorseRacing.Infrastructure/Repositories/HorseRepository.cs`<br>*   `src/HorseRacing.API/Controllers/OwnerController.cs`<br>*   `src/HorseRacing.API/Controllers/JockeyController.cs` (Phần contract) | *   `src/HorseRacing.Infrastructure/Persistence/AppDbContext.cs` (Chỉ thêm mapping DbSet của mình). <br>*   `UserRepository.cs` (Chỉ dùng để lấy thông tin Jockey Profile). |
| **Hàn** | *   `src/HorseRacing.Domain/Entities/Tournaments/` (`Tournament.cs`, `Round.cs`, `Race.cs`, `RaceEntry.cs`, `RaceResult.cs`) <br>*   `src/HorseRacing.Domain/Entities/Compliance/` (`RaceRefereeAssignment.cs`, `RefereeReport.cs`, `RaceViolation.cs`) <br>*   `src/HorseRacing.Application/Features/TournamentAndRacing/`<br>*   `src/HorseRacing.Application/Features/OfficiatingAndResults/`<br>*   `src/HorseRacing.Infrastructure/Repositories/TournamentRepository.cs`<br>*   `src/HorseRacing.API/Controllers/RefereeController.cs`<br>*   `src/HorseRacing.API/Controllers/AdminController.cs` (Phần tạo giải/đua/trọng tài) | *   Các feature khác như `BettingEngine` hay `FinancialRewards`. <br>*   Các tệp xác thực tài khoản và ví. |
| **Đắc** | *   `src/HorseRacing.Domain/Entities/Financials/` (`Bet.cs`, `Payout.cs`, `Prize.cs`, `TournamentPrizePayout.cs`, `Wallet.cs`, `WalletTransaction.cs`, `Prediction.cs`) <br>*   `src/HorseRacing.Domain/Entities/Notifications/` (`Notification.cs`) <br>*   `src/HorseRacing.Application/Features/BettingEngine/`<br>*   `src/HorseRacing.Application/Features/FinancialRewards/`<br>*   `src/HorseRacing.Application/Features/Notifications/`<br>*   `src/HorseRacing.Infrastructure/Repositories/BetRepository.cs`<br>*   `src/HorseRacing.API/Controllers/SpectatorController.cs`<br>*   `src/HorseRacing.API/Controllers/PublicController.cs` (Phần ranking) | *   Các thực thể thuộc nhóm `Equines` hay `Tournaments`. <br>*   Mã nguồn của `RefereeController.cs` (Chỉ đọc trạng thái RaceResult để kích hoạt cược). |

---

## 10. Quy tắc tránh conflict code

Khi làm việc nhóm 4 người trong cùng một dự án C#, việc xảy ra xung đột khi gộp mã nguồn (Git Merge Conflict) là rất phổ biến. Hãy tuân thủ checklist quy tắc sau:

*   [ ] **Không sửa chung một Controller lớn**: Mỗi thành viên nên viết API Endpoint của mình vào đúng Controller đã phân chia (Ví dụ: Khang viết ở `OwnerController.cs`, Đắc viết ở `SpectatorController.cs`).
*   [ ] **Phân tách DTO độc lập**: Mỗi API tương ứng nên có một DTO Request và Response riêng nằm trong thư mục `DTOs` của Feature đó. Tuyệt đối không dùng chung một DTO cho nhiều hành động khác nhau của nhiều người.
*   [ ] **Thống nhất cấu trúc cơ sở dữ liệu**:
    *   Không tự ý thay đổi tên bảng hoặc tên thuộc tính (property) của Entity khi chưa họp thống nhất với team.
    *   Khi viết Entity mới, hãy kế thừa quy ước viết hoa chữ cái đầu và đặt kiểu dữ liệu đồng bộ (ví dụ: các thuộc tính khóa chính nên thống nhất là `Id` kiểu `int` hoặc `UserId` kiểu `int`).
*   [ ] **Hạn chế sửa Program.cs**: Tệp `Program.cs` là điểm khởi chạy hệ thống, sửa nhiều người cùng lúc chắc chắn gây conflict. Nếu cần đăng ký Dependency Injection, hãy thực hiện trong file [ServiceExtensions.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Extensions/ServiceExtensions.cs) (đăng ký Service) hoặc [DependencyInjection.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/DependencyInjection.cs) (đăng ký Repository/DbContext).
*   [ ] **Quy trình Migration an toàn**:
    *   Trước khi chạy lệnh tạo Migration mới (`dotnet ef migrations add`), bắt buộc phải tải code mới nhất từ nhánh `main` về local để kiểm tra xem có ai vừa đẩy Migration mới lên không.
    *   If có Migration mới của người khác, phải chạy lệnh `dotnet ef database update` trước rồi mới tạo Migration của mình.
*   [ ] **Không hard-code tên vai trò (Roles)**:
    *   Sử dụng thống nhất chính xác 5 tên vai trò: `Admin`, `HorseOwner`, `Jockey`, `Referee`, `Spectator` (phân biệt hoa thường đúng như đã cấu hình trong `DataSeeder`).
*   [ ] **Chạy Build Local trước khi Push**: Luôn chạy lệnh `dotnet build` ở local trước khi commit. Code bị lỗi biên dịch tuyệt đối không được đẩy lên GitHub.

---

## 11. Definition of Done cho backend task

Một Task Backend chỉ được coi là hoàn thành (Done) khi đáp ứng đủ các tiêu chí trong checklist sau:

*   [ ] **Biên dịch thành công**: Dự án build không có bất kỳ cảnh báo đỏ (Errors) nào từ Compiler (`dotnet build` trả về thành công).
*   [ ] **Swagger hiển thị đúng**: Endpoint API xuất hiện trên giao diện Swagger UI với đầy đủ các tham số đầu vào và kiểu dữ liệu mong đợi.
*   [ ] **Xác thực hoạt động chính xác**:
    *   Tag `[Authorize(Roles = "...")]` được gắn đúng quyền truy cập cho API.
    *   Gọi thử API không truyền Token trả về `401 Unauthorized`.
    *   Truyền Token sai Role trả về `403 Forbidden`.
*   [ ] **Kiểm tra dữ liệu đầu vào (Input Validation)**: Đầu vào API được kiểm tra đầy đủ (ví dụ: không để trống thông tin bắt buộc, số tiền cược phải lớn hơn 0). Trả về đúng mã lỗi `400 BadRequest` kèm thông điệp rõ ràng khi dữ liệu sai định dạng.
*   [ ] **Bảo mật dữ liệu nhạy cảm**: Không bao giờ trả về PasswordHash hoặc thông tin thẻ/ví nhạy cảm trong Response DTO gửi ra Frontend.
*   [ ] **Mã kết quả phản hồi chuẩn RESTful**: Trả ra đúng mã trạng thái HTTP:
    *   `200 OK` cho hành động lấy dữ liệu hoặc cập nhật thành công.
    *   `201 Created` khi tạo mới bản ghi thành công.
    *   `400 Bad Request` khi dữ liệu đầu vào không hợp lệ.
    *   `401 Unauthorized`/`403 Forbidden` liên quan đến bảo mật.
    *   `404 Not Found` khi không tìm thấy bản ghi.
    *   `500 Internal Server Error` khi có lỗi không mong muốn ở hệ thống.
*   [ ] **Kiểm thử thành công**: Đã chạy thử nghiệm API trực tiếp trên Swagger hoặc qua Postman và cho ra kết quả đúng với nghiệp vụ yêu cầu.
*   [ ] **Commit code sạch sẽ**: Mã nguồn được đẩy lên nhánh Git đúng quy ước đặt tên và có ghi chú commit rõ ràng.

---

## 12. Rủi ro và điểm cần thống nhất trước khi code

Để quá trình làm việc của 4 thành viên diễn ra suôn sẻ, nhóm cần họp bàn và thống nhất trước một số điểm mấu chốt sau:

### 12.1. Quy trình đăng ký tài khoản (Register Flow)
*   **Vấn đề**: Hiện tại API `POST /api/auth/register` chỉ cho phép người dùng tự do đăng ký với vai trò `Spectator`. Vậy làm sao để có tài khoản `HorseOwner`, `Jockey`, `Referee` trên thực tế?
*   **Phương án thống nhất**:
    *   *Phương án 1 (Áp dụng cho SWP391)*: Khách hàng đăng ký tài khoản Spectator trước, sau đó trong trang cá nhân có tính năng "Đăng ký làm Chủ ngựa/Nài ngựa/Trọng tài" -> Gửi đơn đăng ký -> Admin phê duyệt ở API duyệt đơn -> Hệ thống tự động cập nhật RoleID mới và tạo hồ sơ đi kèm.
    *   *Phương án 2*: Admin tạo thủ công tất cả các tài khoản đặc quyền này qua API `POST /api/admin/accounts` từ đầu.
    *   *Khuyến nghị*: Áp dụng **Phương án 1** để tạo trải nghiệm người dùng thực tế. Thành viên 1 và 2 cần phối hợp phần này.

### 12.2. Quy trình liên kết Jockey - Ngựa (Jockey Contract)
*   **Vấn đề**: Một chú ngựa khi tham gia đua bắt buộc phải có Nài ngựa (Jockey) điều khiển. Làm sao để thiết lập quan hệ này?
*   **Phương án thống nhất**:
    *   Chủ ngựa (HorseOwner) phải tạo lời mời hợp đồng (`JockeyContract`) gửi cho Jockey.
    *   Hợp đồng có thời gian hiệu lực (`StartDate` đến `EndDate`).
    *   Khi Jockey đồng ý (`Active`), Chủ ngựa mới được phép gán Jockey đó vào ngựa của mình khi đăng ký đua (`RaceEntry`).
    *   Hợp đồng hết hạn thì mối quan hệ thuê kết thúc.

### 12.3. Quy trình Đăng ký giải đấu và Phân làn chạy (Registration & RaceEntry)
*   **Vấn đề**: Bản ghi `Registration` lưu gì và bản ghi `RaceEntry` lưu gì?
*   **Phương án thống nhất**:
    *   `Registration` đại diện cho việc **Chủ ngựa đăng ký cho Ngựa** tham gia vào một Giải đấu (`Tournament`). Lúc này chưa biết ngựa sẽ đua trận nào trong giải.
    *   Sau khi đóng cổng đăng ký giải đấu, Admin sẽ thực hiện chia bảng đấu, tạo các trận đua (`Race`) thuộc giải.
    *   Từ danh sách ngựa đã đăng ký giải đấu, Admin thực hiện phân bổ ngựa và Jockey tương ứng vào các làn chạy cụ thể trong trận đua, tạo nên các bản ghi `RaceEntry` (làn chạy 1, làn chạy 2...).

### 12.4. Đặt cược và khóa cổng cược (Betting & Lock Gates)
*   **Vấn đề**: Khán giả được đặt cược khi nào và khi nào cổng đặt cược đóng lại để tránh gian lận?
*   **Phương án thống nhất**:
    *   Khán giả chỉ được đặt cược khi cuộc đua ở trạng thái `Scheduled`.
    *   Ngay khi cuộc đua bắt đầu chuyển sang trạng thái `Running` (hoặc trước giờ đua 5 phút), hệ thống phải từ chối tất cả các yêu cầu đặt cược mới của API `POST /api/bets`.

### 12.5. Phân biệt thưởng giải đấu và thưởng đặt cược (Prize vs Payout)
*   **Vấn đề**: Cần phân biệt rõ hai dòng tiền này để tránh nhầm lẫn trong lập trình tài chính:
    *   `Prize` (Tiền thưởng giải): Được trích từ ngân sách giải đấu của ban tổ chức. Chỉ phát cho Chủ ngựa (Owner) và Nài ngựa (Jockey) đạt giải 1, 2, 3 dựa trên kết quả trận đua (`RaceResult`).
    *   `Payout` (Trả thưởng cược): Được tính toán dựa trên tổng tiền cược của khán giả (Pool) và tỷ lệ đặt cược (Odds). Tiền này chỉ chuyển vào ví (`Wallet`) của khán giả (Spectator) đã đặt cược chính xác vào chú ngựa chiến thắng.

---

## 13. Kết luận và kế hoạch làm việc đề xuất

### 13.1. Phân bổ công việc tuần đầu tiên (Tuần khởi động)
*   **Mục tiêu**: Cả team cài đặt môi trường thành công, chạy được dự án local và kết nối được Database SQL Server cá nhân.
*   **Nhiệm vụ cụ thể**:
    *   **Cả team**: Clone dự án backend, chạy `dotnet restore`, tạo database local và chạy lệnh `dotnet ef database update` để đồng bộ database mẫu ban đầu.
    *   **Triều**: Kiểm tra kỹ luồng Auth, chạy thử Swagger để test đăng nhập 5 tài khoản mẫu, đảm bảo phân quyền Role hoạt động đúng.
    *   **Khang**: Hiện thực hóa thực thể `Horse.cs` trên DbContext, viết CRUD cơ bản cho ngựa.
    *   **Hàn**: Hiện thực hóa thực thể `Tournament.cs` trên DbContext, viết CRUD giải đấu.
    *   **Đắc**: Xem xét thực thể `Wallet.cs`, viết API truy vấn số dư ví của Spectator.

### 13.2. Khởi tạo Git ban đầu
*   Team nên bắt đầu bằng cách checkout ra các nhánh tính năng của riêng mình dựa trên nhánh chính `main` hiện có:
    *   Triều checkout nhánh `feature/auth-user-role`
    *   Khang checkout nhánh `feature/horse-registration-jockey`
    *   Hàn checkout nhánh `feature/tournament-race-referee`
    *   Đắc checkout nhánh `feature/betting-wallet-notification`

Mọi thắc mắc trong quá trình code cần được trao đổi trực tiếp trên kênh chat chung của team để Senior Lead/Project Manager kịp thời giải đáp và điều chỉnh kế hoạch!
