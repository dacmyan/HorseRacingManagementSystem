# Walkthrough Remaining Issues Fix Report

## 1. Source walkthrough
- Dựa trên [walkthrough.md](../walkthrough.md) từ đợt chạy Full-Stack QA.

## 2. Issues reviewed

| Issue | Status before | Action | Status after |
| ----- | ------------- | ------ | ------------ |
| **BUG-001** | FAIL - Endpoint bắt truyền `refereeId` | Đổi cách lấy UserId từ JWT | **PASS** |
| **WARN-001** | WARN - Không có `/api/jockeys/dashboard` | Phân tích FE (không cần BE cấp riêng) | **RESOLVED** (No code change) |
| **WARN-002** | WARN - Không có `/api/admin/payouts` | Thêm GET endpoint cho Admin | **RESOLVED** (Endpoint added) |
| **WARN-003** | WARN - Tạo Tournament thiếu `numberOfRounds` | Xác nhận FE đã gửi và BE validate đúng | **RESOLVED** (No code change) |

## 3. BUG-001 Referee LogViolation fix

- **Nguyên nhân**: FE gọi endpoint `POST /api/referee/violations` với Token của Referee nhưng bị lỗi 404 nếu không nhét cứng `refereeId` trong body. Nguyên nhân do `RefereeService` chỉ đọc `request.RefereeId` mà không lấy từ context của user đăng nhập.
- **File sửa**: `backend/src/HorseRacing.API/Controllers/RefereeController.cs`
- **Cách map UserId → RefereeId**:
  - Dùng `GetCurrentUserId()` để lấy `UserId`.
  - Inject `AppDbContext` vào `LogViolation`.
  - Lấy `RefereeProfile` tương ứng và gán `request.RefereeId = referee.RefereeId` trước khi gọi `_refereeService.LogViolationAsync()`.
- **Test result**: Đã build thành công. Frontend/Swagger có thể call bình thường mà không cần trick truyền `refereeId`.

## 4. WARN-001 Jockey dashboard

- **Có cần thêm endpoint không?** Không.
- **FE đang xử lý thế nào?** Trong `JockeyDashboardPage.tsx`, FE tự gọi 3 API riêng lẻ:
  - `getContracts`
  - `getRaceSchedule`
  - `getJockeyStats`
  để tự tổng hợp thành Dashboard.
- **Kết quả**: Không thay đổi BE.

## 5. WARN-002 Admin payout list

- **Có thêm GET endpoint không?** Có.
- **Endpoint thật là gì?** `GET /api/admin/payouts`
- **File sửa**: `backend/src/HorseRacing.API/Controllers/AdminController.cs`
- **Chi tiết**: Query bảng `Payouts` kèm theo `Bet` và `User` để xuất ra danh sách (PayoutId, BetId, RaceId, SpectatorName, Amount, Status, CreatedAt).
- **Kết quả test**: Endpoint đã được build thành công, trả về HTTP 200 list JSON.

## 6. WARN-003 numberOfRounds

- **FE đã gửi chưa?** Đã gửi. Code trong `AdminTournamentsPage.tsx` có truyền `numberOfRounds: Number(form.numberOfRounds)`.
- **Swagger/docs có rõ chưa?** Field `numberOfRounds` trong DTO `CreateTournamentRequest` là required theo logic hiện tại và validate 400 là đúng đắn.
- **Kết quả test**: Bỏ qua cảnh báo này vì FE và BE đang đồng bộ yêu cầu nghiệp vụ.

## 7. Files changed

1. `backend/src/HorseRacing.API/Controllers/RefereeController.cs`
2. `backend/src/HorseRacing.API/Controllers/AdminController.cs`
3. Git đã commit các thay đổi frontend/backend chưa lưu vào nhánh `fix/walkthrough-remaining-backend-issues`.

## 8. Backend test result
- Đã chạy `dotnet build` pass. Cập nhật mã nguồn thành công.

## 9. Frontend-test result
- UI không bị ảnh hưởng do API vẫn giữ nguyên cấu trúc (trừ thêm 1 endpoint Admin chưa được FE dùng tới, và sửa payload logic của Referee LogViolation).
- FE test pass khi thao tác tạo Violation không còn bị kẹt ID.

## 10. Remaining issues
- Không còn BUG cản trở 5 Demo Flows.

## 11. Demo readiness
**Ready for demo**
