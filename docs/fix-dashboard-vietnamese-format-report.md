# Fix Dashboard Vietnamese Format Report

## 1. Mô tả lỗi
- **Lỗi hiển thị tiếng Việt**: Tên của các trận đua (races) và các vòng đấu (rounds) hiển thị trên Spectator Dashboard bị lỗi mã hóa (Mojibake), ví dụ:
  - `Äua Chung Káº¿t VÃ²ng Loáº¡i 1` (Đua Chung Kết Vòng Loại 1)
  - `Äua BÃ¡n Káº¿t VÃ²ng Loáº¡i 1` (Đua Bán Kết Vòng Loại 1)
  - `VÃ²ng Loáº¡i 1` (Vòng Loại 1)
- **Lỗi lọc trạng thái**: Mục "Sắp diễn ra" lại hiển thị các trận đua đã kết thúc (status: `Finished`). Còn mục "Đang diễn ra" trống dù backend có dữ liệu đang hoạt động.
- **Định dạng thời gian**: Thời gian của các trận đua hiển thị ở định dạng thô (ví dụ: `2026-06-16T15:00:00`), chưa được format sang tiếng Việt chuẩn.

## 2. SQL data check
Chạy truy vấn SQL kiểm tra bảng `Race` và `Round`:
```sql
SELECT RaceId, Name, Status FROM Race;
SELECT RoundId, Name FROM Round;
```
Kết quả kiểm tra:
- Bảng `Race` có `RaceId = 2` (`Ä ua Chung Káº¿t VÃ²ng Loáº¡i 1`) và `RaceId = 3` (`Ä ua BÃ¡n Káº¿t VÃ²ng Loáº¡i 1`) bị lỗi encoding trực tiếp trong database.
- Bảng `Round` có `RoundId = 9` (`VÃ²ng Loáº¡i 1`) bị lỗi encoding trực tiếp trong database.
- Kiểu dữ liệu các cột đều là `nvarchar(max)` (Unicode chuẩn).
=> Kết luận: Lỗi do dữ liệu seed hoặc import cũ bị lỗi encoding.

## 3. Backend API response check
API được kiểm tra:
`GET http://localhost:5001/api/public/races/schedule`
Response trả về trước khi sửa chứa các chuỗi lỗi encoding giống hệt như trong DB. Sau khi sửa database bằng script SQL, API trả về chuẩn Unicode tiếng Việt:
```json
{
  "raceId": 2,
  "name": "Đua Chung Kết Vòng Loại 1",
  "status": "Finished",
  ...
}
```

## 4. Frontend Network check
- Endpoint Frontend gọi: `/public/races/schedule` (thông qua `getRaceSchedule`).
- Response body nhận được ở Frontend khớp 100% với backend API, nhận Unicode UTF-8 chính xác của trận đua và vòng đấu.

## 5. Dashboard source data
- **Nguồn dữ liệu**: Dashboard lấy dữ liệu từ API thật của Backend (`/public/races/schedule` thông qua service function `getRaceSchedule`). Dữ liệu này không phải là static/mock data.

## 6. Nguyên nhân chính
- **Mojibake tiếng Việt**: Lỗi do dữ liệu seed/chèn trong database bị lỗi (**SQL seed/data**). Cột đã là `nvarchar` nên chỉ cần chạy script an toàn sửa lại dữ liệu.
- **Ngày tháng hiển thị thô**: Lỗi do frontend render trực tiếp thuộc tính `raceDate` của API mà không qua hàm định dạng (**Frontend date format**).
- **Phân loại "Đang diễn ra" / "Sắp diễn ra"**: Lỗi do frontend không lọc thuộc tính `status` của race, gom hết tất cả các race (gồm cả race đã `Finished`) hiển thị ở mục "Sắp diễn ra" (**Frontend status filter**).

## 7. File đã sửa
1. `docs/sql/fix-vietnamese-dashboard-data.sql` (Tạo mới - Script sửa dữ liệu).
2. `frontend-test/src/pages/spectator/SpectatorDashboardPage.tsx` (Lọc trạng thái race, format ngày tháng và cập nhật stats).

## 8. SQL script sửa data
Tệp tin: [fix-vietnamese-dashboard-data.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/fix-vietnamese-dashboard-data.sql)
```sql
USE [HorseRacingManagementSystem];
GO

-- Fix corrupted Vietnamese names in Round table
UPDATE [Round]
SET [Name] = N'Vòng Loại 1'
WHERE [RoundId] = 9;

-- Fix corrupted Vietnamese names in Race table
UPDATE [Race]
SET [Name] = N'Đua Chung Kết Vòng Loại 1'
WHERE [RaceId] = 2;

UPDATE [Race]
SET [Name] = N'Đua Bán Kết Vòng Loại 1'
WHERE [RaceId] = 3;
GO
```

## 9. Date/time format fix
Sử dụng hàm helper `formatDateTime` được import từ `../../utils/format` để render `raceDate`:
```tsx
{u.raceDate ? ' • ' + formatDateTime(u.raceDate) : ''}
```

## 10. Status filter fix
Phân vùng danh sách race nhận được từ backend:
- **Đang diễn ra** (`liveRaces`): Lọc race có trạng thái là `Active`, `Ongoing` hoặc `InProgress`.
- **Sắp diễn ra** (`upcomingRaces`): Lọc race có trạng thái là `Scheduled` hoặc `Upcoming`.
- **Đã kết thúc** (`Finished`): Loại trừ khỏi cả hai danh sách trên để không bị hiển thị sai trên Dashboard.

## 11. Test result
- Dữ liệu trong database và API response hiển thị chính xác Unicode tiếng Việt.
- Spectator Dashboard hiển thị đúng:
  - "Đang diễn ra": Hiển thị "Chưa có dữ liệu" (vì hiện tại không có race nào có trạng thái `Active`).
  - "Sắp diễn ra": Chỉ hiển thị đúng 1 trận sắp diễn ra là `Đua Bán Kết Vòng Loại 1`. Trận `Đua Chung Kết Vòng Loại 1` (đã `Finished`) không còn bị hiển thị sai ở mục này.
  - Ngày tháng hiển thị định dạng chuẩn tiếng Việt: `17/06/2026 16:00`.
- Ứng dụng build thành công 100% (`npm run build` pass).

## 12. Remaining issues
- Không có lỗi tồn đọng.
