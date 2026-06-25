# Báo cáo: Fix lỗi Spectator Betting & Thêm dữ liệu 50 Ngựa

## 1. Mục tiêu
- Thêm 50 con ngựa test và đảm bảo mỗi race thuộc tournament chỉ được có tối đa 12 ngựa.
- Sửa lỗi đặt cược để chỉ cho phép đặt cược ở các race sắp diễn ra (`Scheduled` hoặc `Live`), và tournament chưa kết thúc.
- Cập nhật UI/UX phía Frontend để hiển thị cảnh báo nếu không thể đặt cược, thay vì chỉ ẩn hoàn toàn form đặt cược.

## 2. Các thay đổi Backend
- **TournamentRepository & ITournamentRepository**: Thêm phương thức `AddRacesAsync` và `AddRaceEntriesAsync` để hỗ trợ batch insert.
- **TournamentService**: Thêm logic `GenerateRacesForTournamentAsync` tự động lấy tất cả các ngựa đã đăng ký (Approved) của một tournament, chia nhỏ thành các chặng đua tối đa 12 ngựa/chặng (dùng `GroupBy`).
- **BettingService**: Bổ sung validation kiểm tra trạng thái của Tournament và Race để ngăn chặn đặt cược ở cấp độ API nếu trạng thái không hợp lệ (ví dụ: `Finished`, `Cancelled`).
- **PublicController/AdminController**: Expose API `POST /api/public/tournaments/{id}/generate-races` hoặc `POST /api/admin/tournaments/{id}/generate-races` để kích hoạt việc chia chặng.

## 3. Các thay đổi Frontend
- **SpectatorRaceDetailPage.tsx**:
  - Lấy trạng thái của cả Race và Tournament để quyết định biến `isBettingAllowed`.
  - Thay vì ẩn hoàn toàn form đặt cược khi không hợp lệ, form vẫn hiển thị nhưng các ô input bị disable, kèm theo dòng cảnh báo màu đỏ giải thích lý do cụ thể (Tournament đã kết thúc hoặc Race không trong thời gian cho phép).

## 4. Dữ liệu Test (SQL Seed)
- Tạo file `docs/sql/seed-50-horses-and-tournament.sql`.
- Script này tạo ra một chủ ngựa test, một Tournament mẫu (`Giải Đua Siêu Cấp 50 Ngựa 2026`), 1 vòng đua, sau đó sinh ra 50 con ngựa và tự động chèn vào bảng `Registration` cho Tournament nói trên.

## 5. Kết quả kiểm tra
- Đã chạy thành công lệnh `dotnet build` và `npm run build` không lỗi.
- Đã chạy script tạo 50 ngựa trên database thật.
- Test endpoint generate races bằng cURL: Trả về 5 race được tạo (4 race đầu mỗi race 12 ngựa, race cuối 2 ngựa - chính xác 50 ngựa chia đều).
- Flow UI đặt cược đã có validation đầy đủ.
