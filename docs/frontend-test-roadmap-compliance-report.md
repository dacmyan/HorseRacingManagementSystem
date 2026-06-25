# Báo cáo Kiểm tra Tính tuân thủ Roadmap của Frontend

**Dự án**: Horse Racing Management System (SWP391)
**Ngày kiểm tra**: Hôm nay
**Mục tiêu**: Kiểm tra folder `frontend-test` có tuân thủ đúng theo `backend-priority-roadmap.md` và `backend-task-division-4-members.md` hay không.

---

## 1. Kết quả kiểm tra hạ tầng (Infrastructure)

* **[PASS] Vite Proxy Configuration**: Đã sửa cấu hình proxy trong `vite.config.ts` để trỏ tới `127.0.0.1:5001`. Hiện tại frontend đã gọi thành công các API backend mà không bị dính lỗi CORS hay nhận nhầm file HTML.
* **[PASS] Môi trường (Env)**: Việc cấu hình `.env` chứa `VITE_API_URL=/api` hoạt động tốt với Proxy.
* **[PASS] Đăng nhập & Điều hướng Role**: Frontend đã tích hợp đúng JWT và phân quyền điều hướng người dùng sau khi login theo từng vai trò: `Admin`, `Spectator`, `HorseOwner`, `Jockey`, `Referee`.

---

## 2. Kết quả kiểm tra theo Roadmap & Task Division

Việc đối chiếu được thực hiện dựa trên 5 Phase của hệ thống backend.

### Phase 1: Foundation (Auth, User, Wallet)
* **[PASS] BE-AUTH-01 (Login/Register)**: Frontend gọi đúng `/api/auth/login` và `/api/auth/register`. Xử lý lưu Token, parse Role chính xác.
* **[PASS] BE-USER-01 (Admin tạo tài khoản)**: Frontend (trang `AdminUsersPage.tsx`) gọi đúng API `POST /api/admin/accounts` kèm theo logic ẩn/hiện trường thông tin đặc thù (`licenseNumber`) cho Jockey/Referee.
* **[PASS] BE-WALLET-01 (Ví điện tử)**: Khán giả gọi đúng API Nạp/Rút (`/spectator/wallet/deposit` & `withdraw`) và xem lịch sử ví.
* **[MISSING] BE-USER-03 (Admin thay đổi trạng thái user)**: Roadmap backend yêu cầu API `PUT /api/admin/users/{id}/status` nhưng frontend tại màn hình Quản lý Người dùng hoàn toàn không có giao diện/nút chức năng để Admin khóa/mở khóa tài khoản (chỉ render static label).
* **[WARN] Route Mismatch**: Backend cung cấp `GET /api/admin/accounts` (và `frontend-test` cũng sử dụng endpoint này). Tuy nhiên, tài liệu Task Division ghi là `GET /api/admin/users`. Đây không phải là bug FE, chỉ là tên đường dẫn lúc code Backend có sai lệch nhỏ so với document.

### Phase 2: Core Master Data (Ngựa, Giải đấu, Lịch đua)
* **[PASS] BE-HORSE-01 (CRUD Ngựa đua)**: Trang `OwnerHorsesPage.tsx` đã tích hợp đầy đủ.
* **[PASS] BE-RACE-01 (Giải đấu)**: Admin gọi chuẩn `POST /admin/tournaments` và xem danh sách giải qua `GET /public/tournaments`.
* **[PASS] BE-RACE-02 (Lên lịch đua)**: Admin đã tạo và xem được trận đua qua API.

### Phase 3: Registration & Assignment Flow
* **[PASS] BE-CONTRACT-01 & 02 (Ký hợp đồng)**: Trang của Owner gửi hợp đồng `POST /jockey-contracts` và trang của Jockey duyệt hợp đồng hoạt động khớp luồng.
* **[PASS] BE-REG-01 (Đăng ký giải)**: Owner đăng ký ngựa thành công.
* **[PASS] BE-RACE-03 (Lane Assignment)**: Admin gán ngựa và Jockey vào làn qua `POST /admin/races/{raceId}/entries`.
* **[PASS] BE-RACE-04 (Gán trọng tài)**: Admin phân công qua `POST /admin/races/{raceId}/referees`.

### Phase 4: Race Operation & Result (Hoạt động trận đua & Kết quả)
Đây là Phase **CHƯA ĐƯỢC TÍCH HỢP HOÀN THIỆN** trên Frontend.

* **[MISSING] BE-RESULT-01 (Nhập & Publish kết quả)**: 
  * Trang `RefereeConfirmResultsPage.tsx` **chưa gọi API backend**. Frontend đang để mock UI với comment: `TODO: BE chưa có API danh sách kết quả cuộc đua cần xác nhận`. 
  * Backend thực chất **đã cung cấp** API nhập kết quả (`POST /api/referee/results`) và công bố kết quả của Admin (`POST /api/admin/races/{raceId}/publish`), nhưng cả `RefereeConfirmResultsPage` (Trọng tài) và `AdminResultsPage` (Admin) đều chưa gọi các endpoint này.
* **[MISSING] BE-REF-01 (Ghi nhận vi phạm)**: Trang `RefereeViolationsPage.tsx` hiện tại chỉ là mock UI tĩnh. Việc ghi nhận vi phạm (`POST /api/referee/violations`) chưa được gọi.

### Phase 5: Betting & Financial Flow
* **[PASS] BE-BET-01 (Đặt cược)**: Khán giả đã cược được tiền ví vào các cuộc đua đang diễn ra.
* **[PASS] BE-PAY-02 (Trả giải đấu)**: Admin đã có UI để gọi `POST /admin/payouts/prizes` (chia 70/30).
* **[PASS] BE-PAY-01 (Trigger trả thưởng cược)**: Admin có UI `Trigger` để gọi `POST /admin/payouts/trigger/{raceId}`. *Lưu ý: Theo Roadmap luồng trả cược nên tự động chạy khi Publish kết quả, nhưng frontend đang cung cấp thêm nút Trigger thủ công (có thể dành cho debug).*

---

## 3. Tổng kết Lỗi (Bug) & Điểm thiếu (Missing) cần bổ sung

Dưới đây là các phần Frontend cần bổ sung để đáp ứng đầy đủ yêu cầu Backend:

1. **Admin Users Page**: Cần thêm Action (Nút Toggle) gọi API `PUT /api/admin/users/{id}/status` để khóa/mở khóa tài khoản.
2. **Admin Results Page**: Cần bổ sung danh sách "Kết quả chờ công bố" và nút **Publish** gọi API `POST /api/admin/races/{raceId}/publish`. 
3. **Referee Violations Page**: Bỏ mock data, thay thế bằng hàm gọi API lấy danh sách vi phạm, xử lý form "Ghi nhận vi phạm" kết nối với endpoint `POST /api/referee/violations`.
4. **Referee Confirm Results Page**: Bỏ màn hình "Chưa có dữ liệu" mock, kết nối form cập nhật thứ hạng trận đua vào API `POST /api/referee/results`.

---

**Kết luận chung**: Khung sườn (`Phase 1, 2, 3, 5`) của `frontend-test` được code cực kỳ tỉ mỉ và chạy rất chuẩn với Backend. Lỗi nghiêm trọng `Unexpected token <` lúc đầu hoàn toàn là do cấu hình proxy sai. `Phase 4` là phần duy nhất frontend bị bỏ dở (hiện đang dùng Mock UI), cần phải được tích hợp bổ sung.
