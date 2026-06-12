# GitHub Project Issue Plan

Tài liệu này tổng hợp kế hoạch quản lý, phân bổ và theo dõi trạng thái các task Backend của dự án **Horse Racing Management System** trên GitHub Project. 

Tất cả các issue và trạng thái của chúng đã được tạo và đồng bộ hóa tự động vào GitHub Project 10 bằng GitHub CLI (`gh`). Bạn có thể theo dõi trực tiếp trên bảng Kanban của Project.

---

## 1. Project Information
- **Project URL:** [https://github.com/users/dacmyan/projects/10](https://github.com/users/dacmyan/projects/10)
- **Repository URL:** [https://github.com/dacmyan/HorseRacingManagementSystem.git](https://github.com/dacmyan/HorseRacingManagementSystem.git)

---

## 2. Master Table: Backend Issues & Status

| Task ID | Issue Title | Proposed Issue URL | Owner | Phase | Priority | Initial Status |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: |
| **BE-AUTH-01** | `[BE-AUTH-01] Cải tiến Login & Register Validation` | [Issue #14](https://github.com/dacmyan/HorseRacingManagementSystem/issues/14) | Triều | Phase 1 | P0 | **Todo** |
| **BE-USER-01** | `[BE-USER-01] Admin tạo tài khoản nghiệp vụ` | [Issue #18](https://github.com/dacmyan/HorseRacingManagementSystem/issues/18) | Triều | Phase 1 | P0 | **Todo** |
| **BE-WALLET-01**| `[BE-WALLET-01] Giao dịch ví điện tử (Nạp/Rút)` | [Issue #19](https://github.com/dacmyan/HorseRacingManagementSystem/issues/19) | Đắc | Phase 1 | P0 | **Todo** |
| **BE-NOTI-01** | `[BE-NOTI-01] Tạo và gửi thông báo hệ thống` | [Issue #20](https://github.com/dacmyan/HorseRacingManagementSystem/issues/20) | Đắc | Phase 1 | P1 | **Todo** |
| **BE-USER-02** | `[BE-USER-02] Xem và cập nhật Profile cá nhân` | [Issue #21](https://github.com/dacmyan/HorseRacingManagementSystem/issues/21) | Triều | Phase 1 | P2 | **Backlog** |
| **BE-USER-03** | `[BE-USER-03] Admin quản lý danh sách Users` | [Issue #22](https://github.com/dacmyan/HorseRacingManagementSystem/issues/22) | Triều | Phase 1 | P2 | **Backlog** |
| **BE-HORSE-01**| `[BE-HORSE-01] Quản lý Ngựa đua (CRUD Horses)` | [Issue #23](https://github.com/dacmyan/HorseRacingManagementSystem/issues/23) | Khang | Phase 2 | P0 | **Todo** |
| **BE-RACE-01** | `[BE-RACE-01] Quản lý Giải đấu (Tournaments)` | [Issue #24](https://github.com/dacmyan/HorseRacingManagementSystem/issues/24) | Hàn | Phase 2 | P0 | **Todo** |
| **BE-RACE-02** | `[BE-RACE-02] Lên lịch thi đấu (Scheduling Races)` | [Issue #25](https://github.com/dacmyan/HorseRacingManagementSystem/issues/25) | Hàn | Phase 2 | P1 | **Backlog** |
| **BE-HORSE-02**| `[BE-HORSE-02] Quản lý tài liệu và chỉ số ngựa` | [Issue #26](https://github.com/dacmyan/HorseRacingManagementSystem/issues/26) | Khang | Phase 2 | P3 | **Backlog** |
| **BE-CONTRACT-01**| `[BE-CONTRACT-01] Gửi hợp đồng thuê nài ngựa` | [Issue #27](https://github.com/dacmyan/HorseRacingManagementSystem/issues/27) | Khang | Phase 3 | P1 | **Backlog** |
| **BE-CONTRACT-02**| `[BE-CONTRACT-02] Phản hồi hợp đồng (Jockey Action)` | [Issue #28](https://github.com/dacmyan/HorseRacingManagementSystem/issues/28) | Khang | Phase 3 | P1 | **Backlog** |
| **BE-REG-01**  | `[BE-REG-01] Đăng ký ngựa tham gia Giải đấu` | [Issue #29](https://github.com/dacmyan/HorseRacingManagementSystem/issues/29) | Khang | Phase 3 | P1 | **Backlog** |
| **BE-RACE-04** | `[BE-RACE-04] Gán trọng tài điều hành cuộc đua` | [Issue #30](https://github.com/dacmyan/HorseRacingManagementSystem/issues/30) | Hàn | Phase 3 | P2 | **Backlog** |
| **BE-RACE-03** | `[BE-RACE-03] Ghép cặp trận đấu (Lane Assignment)` | [Issue #31](https://github.com/dacmyan/HorseRacingManagementSystem/issues/31) | Hàn | Phase 3 | P0 | **Backlog** |
| **BE-REF-01**  | `[BE-REF-01] Trọng tài giám sát & vi phạm` | [Issue #32](https://github.com/dacmyan/HorseRacingManagementSystem/issues/32) | Hàn | Phase 4 | P1 | **Backlog** |
| **BE-RESULT-01**| `[BE-RESULT-01] Ghi nhận & Công bố kết quả đua` | [Issue #33](https://github.com/dacmyan/HorseRacingManagementSystem/issues/33) | Hàn | Phase 4 | P0 | **Backlog** |
| **BE-REF-02**  | `[BE-REF-02] Trọng tài lập biên bản & Báo cáo` | [Issue #34](https://github.com/dacmyan/HorseRacingManagementSystem/issues/34) | Hàn | Phase 4 | P3 | **Backlog** |
| **BE-BET-01**  | `[BE-BET-01] Khán giả đặt cược (Place Bets)` | [Issue #35](https://github.com/dacmyan/HorseRacingManagementSystem/issues/35) | Đắc | Phase 5 | P0 | **Backlog** |
| **BE-BET-02**  | `[BE-BET-02] Quản lý dự đoán (Predictions)` | [Issue #36](https://github.com/dacmyan/HorseRacingManagementSystem/issues/36) | Đắc | Phase 5 | P2 | **Backlog** |
| **BE-PAY-01**  | `[BE-PAY-01] Tự động trả thưởng đặt cược` | [Issue #37](https://github.com/dacmyan/HorseRacingManagementSystem/issues/37) | Đắc | Phase 5 | P0 | **Backlog** |
| **BE-PAY-02**  | `[BE-PAY-02] Trả thưởng giải đấu cho HorseOwner/Jockey` | [Issue #38](https://github.com/dacmyan/HorseRacingManagementSystem/issues/38) | Đắc | Phase 5 | P1 | **Backlog** |
| **Integration**| `[Integration] Tích hợp thông báo toàn hệ thống` | [Issue #39](https://github.com/dacmyan/HorseRacingManagementSystem/issues/39) | Đắc | Phase 5 | P1 | **Backlog** |

---

## 3. Phân Nhóm Theo Trạng Thái (Status)

### 3.1. Backlog (17 Tasks)
Đây là các task thuộc phase sau, có độ ưu tiên thấp hoặc bị block bởi các task khác, chưa nên thực hiện ngay:
1. `BE-USER-02` (Xem và cập nhật Profile cá nhân) - Blocked bởi `BE-USER-01`
2. `BE-USER-03` (Admin quản lý danh sách Users) - Blocked bởi `BE-USER-01`
3. `BE-RACE-02` (Lên lịch thi đấu) - Blocked bởi `BE-RACE-01`
4. `BE-HORSE-02` (Quản lý tài liệu và chỉ số ngựa) - Blocked bởi `BE-HORSE-01`
5. `BE-CONTRACT-01` (Gửi hợp đồng thuê nài ngựa) - Blocked bởi `BE-HORSE-01` & `BE-USER-01`
6. `BE-CONTRACT-02` (Phản hồi hợp đồng) - Blocked bởi `BE-CONTRACT-01`
7. `BE-REG-01` (Đăng ký ngựa tham gia Giải đấu) - Blocked bởi `BE-HORSE-01` & `BE-RACE-01`
8. `BE-RACE-04` (Gán trọng tài điều hành cuộc đua) - Blocked bởi `BE-RACE-02` & `BE-USER-01`
9. `BE-RACE-03` (Ghép cặp trận đấu) - Blocked bởi `BE-RACE-02`, `BE-REG-01` & `BE-CONTRACT-02`
10. `BE-REF-01` (Trọng tài giám sát & vi phạm) - Blocked bởi `BE-RACE-04` & `BE-RACE-03`
11. `BE-RESULT-01` (Ghi nhận & Công bố kết quả đua) - Blocked bởi `BE-REF-01` & `BE-RACE-03`
12. `BE-REF-02` (Trọng tài lập biên bản & Báo cáo) - Blocked bởi `BE-REF-01`
13. `BE-BET-01` (Khán giả đặt cược) - Blocked bởi `BE-WALLET-01` & `BE-RACE-03`
14. `BE-BET-02` (Quản lý dự đoán) - Blocked bởi `BE-RACE-02`
15. `BE-PAY-01` (Tự động trả thưởng đặt cược) - Blocked bởi `BE-BET-01` & `BE-RESULT-01`
16. `BE-PAY-02` (Trả thưởng giải đấu cho HorseOwner/Jockey) - Blocked bởi `BE-RESULT-01` & `BE-USER-01`
17. `Integration` (Tích hợp thông báo toàn hệ thống) - Blocked bởi `BE-NOTI-01` & các sự kiện Phase 3, 4, 5

### 3.2. Todo (6 Tasks)
Các task nền tảng hoặc có thể thực hiện song song ngay trong Sprint hiện tại (không có block hoặc block dễ xử lý):
1. `BE-AUTH-01` (Cải tiến Login & Register Validation) - Triều
2. `BE-USER-01` (Admin tạo tài khoản nghiệp vụ) - Triều
3. `BE-WALLET-01` (Giao dịch ví điện tử Nạp/Rút) - Đắc
4. `BE-NOTI-01` (Tạo và gửi thông báo hệ thống) - Đắc
5. `BE-HORSE-01` (Quản lý Ngựa đua - CRUD Horses) - Khang
6. `BE-RACE-01` (Quản lý Giải đấu - Tournaments) - Hàn

### 3.3. Doing (0 Tasks)
Chưa có task nào được chuyển sang Doing tại thời điểm khởi tạo này.

### 3.4. Done (0 Tasks)
Chưa có task nào hoàn thành.

---

## 4. Các điểm nghẽn (Block) và Lý do chi tiết

| Task bị block | Bị block bởi | Lý do cụ thể | Giải pháp tháo gỡ tạm thời (Mocking) |
| :--- | :--- | :--- | :--- |
| **BE-REG-01** | `BE-RACE-01` | Chủ ngựa không thể đăng ký ngựa tham gia nếu Admin chưa tạo Tournament trong DB. | Hàn seed sẵn một Tournament mẫu trong `DataSeeder` để Khang gọi API đăng ký ngựa. |
| **BE-RACE-03** | `BE-REG-01`<br>`BE-CONTRACT-02` | Không thể xếp làn chạy nếu ngựa chưa đăng ký vào giải đấu, hoặc Jockey chưa đồng ý ký hợp đồng hoạt động (`Active`). | Khang và Hàn thống nhất cấu trúc bảng `Registration` và `JockeyContract` sớm để map DB, sau đó mock bản ghi test thuật toán chia làn. |
| **BE-BET-01** | `BE-RACE-03` | Spectator chỉ cược được khi biết trận đấu có những ngựa nào chạy ở làn nào. | Đắc viết API nhận cược kiểm tra `RaceId` tồn tại, bỏ qua kiểm tra làn chạy chi tiết ở giai đoạn đầu. |
| **BE-REF-01** | `BE-RACE-04`<br>`BE-RACE-03` | Trọng tài chỉ có quyền phạt những ngựa/Jockey có tên trong danh sách chạy của trận đấu mà họ được phân công bắt chính. | Cấu hình mặc định bypass check phân quyền gán trọng tài hoặc mock admin làm trọng tài. |
| **BE-RESULT-01**| `BE-RACE-03` | Không thể nhập hạng 1, 2, 3 nếu trận đấu không có danh sách làn chạy (`RaceEntry`) thực tế. | Mock danh sách kết quả bằng text thuần thay vì khóa ngoại tới `RaceEntry` khi dev độc lập. |
| **BE-PAY-01** | `BE-RESULT-01` | Chỉ hệ thống tự động cộng tiền ví khi Admin công bố kết quả và đổi trạng thái trận đấu sang `Finished`. | Đắc viết API trigger thủ công `/api/admin/payouts/trigger/{raceId}` để test logic cộng tiền của mình độc lập. |
| **BE-PAY-02** | `BE-RESULT-01` | Chỉ phát giải thưởng giải đấu khi giải đấu kết thúc và tìm ra chú ngựa đoạt giải nhất chung cuộc. | Đắc tự tạo bản ghi kết quả thắng cuộc giải đấu bằng tay trong DB SQL Server để test. |

---

## 5. Đề xuất Task kéo vào Doing đầu tiên

### Kế hoạch hành động khẩn cấp cho từng member:
1. **Triều (Auth/User):** Kéo **`BE-AUTH-01` (Cải tiến Login & Register Validation)** vào **Doing** đầu tiên. Đây là điều kiện tiên quyết cho toàn bộ dự án để lấy JWT Token cấu hình Swagger Authorize.
2. **Khang (Horses/Contracts):** Sau khi Triều làm xong Auth, Khang kéo **`BE-HORSE-01` (CRUD Horses)** vào **Doing**. Chú ý phối hợp với Đắc để định nghĩa cấu trúc class thực thể `Horse.cs` chuẩn chỉnh.
3. **Hàn (Tournaments/Races):** Kéo **`BE-RACE-01` (CRUD Giải đấu)** vào **Doing** để cung cấp Tournament ID cho Khang.
4. **Đắc (Wallet/Betting):** Kéo **`BE-WALLET-01` (Giao dịch ví nạp/rút)** vào **Doing** để hoàn thiện API ví và chuẩn bị data seeder cho Spectator.

---

## 6. Ghi chú và hướng dẫn cho Team

- **Cập nhật trạng thái:** Khi bắt đầu code bất kỳ task nào, chủ động chuyển Issue trên GitHub Project từ `Todo` sang `Doing`. Khi hoàn thành PR và merge vào `main`, chuyển sang `Done`.
- **Thống nhất Database:** Đắc, Khang và Hàn cần họp thống nhất cấu trúc các bảng `JockeyContract`, `Registration`, và `RaceEntry` ngay trong tuần đầu tiên. Dù code logic chưa viết, nhưng các thực thể C# rỗng cần được tạo và Migration Database chung trước để Đắc không bị vỡ logic mock.
- **Git Branch:** Hãy checkout từ nhánh `main` mới nhất trước khi làm task mới, ví dụ: `feature/[tên-member]/[mã-task]`. Đảm bảo `dotnet build` chạy thành công ở local trước khi đẩy PR.
