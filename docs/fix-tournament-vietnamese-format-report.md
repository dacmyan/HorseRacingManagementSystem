# Fix Tournament Vietnamese Format Report

## 1. Mô tả lỗi
- Tên giải đấu trên giao diện Frontend bị lỗi hiển thị tiếng Việt (Mojibake): `Giáº£i Äua VÃ´ Äá»ch Quá»‘c Gia 2026`. Tên chuẩn mong muốn là `Giải Đua Vô Địch Quốc Gia 2026`.
- Ngày tháng hiển thị thô và chưa tối ưu hóa cho định dạng tiếng Việt: `08:00:00 1/8/2026`, mong muốn hiển thị đẹp hơn là `dd/mm/yyyy hh:mm` (ví dụ `01/08/2026 08:00`).

## 2. SQL data check
Chạy truy vấn kiểm tra trong SQL Server:
```sql
SELECT 
    TournamentId,
    Name,
    StartDate,
    EndDate,
    Status
FROM Tournament
ORDER BY TournamentId DESC;
```
Kết quả kiểm tra:
- Bản ghi `TournamentId = 4` có cột `Name` trong cơ sở dữ liệu bị lưu lỗi sẵn: `Giáº£i Äua VÃ´ Äá»‹ch Quá»‘c Gia 2026`.
- Bản ghi `TournamentId = 1` hiển thị đúng: `Giải đấu Khang Lẹo`.
=> Kết luận: Cột `Name` trong DB bị lỗi tiếng Việt ở một số bản ghi demo do dữ liệu được seed/chèn bị lỗi encoding từ trước.

## 3. Backend API response check
Gọi API danh sách giải đấu:
`GET http://localhost:5001/api/Public/tournaments`
Response JSON trước khi sửa:
```json
{
  "tournamentId": 4,
  "name": "Giáº£i Äua VÃ´ Äá»‹ch Quá»‘c Gia 2026",
  ...
}
```
Response sau khi chạy script sửa dữ liệu trong SQL Server:
```json
{
  "tournamentId": 4,
  "name": "Giải Đua Vô Địch Quốc Gia 2026",
  ...
}
```
=> Kết luận: Response của API Backend hiển thị đúng tiếng Việt sau khi dữ liệu trong DB được cập nhật chuẩn xác.

## 4. Frontend Network check
- Endpoint Frontend gọi: `GET http://localhost:5001/api/Public/tournaments` (qua api service client).
- Response Body nhận được ở Frontend Network khớp hoàn toàn với Backend API, đã nhận Unicode UTF-8 chính xác của tên giải đấu sau khi sửa.

## 5. Nguyên nhân chính
- **Lỗi hiển thị tiếng Việt**: Nằm ở **SQL seed/data**. Do dữ liệu demo cũ trong DB bị chèn sai encoding UTF-8 (có thể thiếu tiền tố `N` hoặc file script seed cũ không lưu ở chuẩn UTF-8). Schema của cột `Name` là `nvarchar(max)` hoàn toàn hỗ trợ Unicode tiếng Việt chuẩn.
- **Ngày tháng hiển thị chưa đẹp**: Nằm ở **Frontend date format**. Các file view của Frontend trước đây gọi trực tiếp `.toLocaleString()` của Javascript Date object dẫn đến định dạng hiển thị không nhất quán và thô.

## 6. File đã sửa
1. `docs/sql/fix-vietnamese-tournament-data.sql` (Tạo mới - Script sửa dữ liệu tiếng Việt bị lỗi).
2. `frontend-test/src/utils/format.js` (Thêm helper `formatDateTime`).
3. `frontend-test/src/pages/spectator/SpectatorTournamentsPage.tsx` (Áp dụng helper định dạng ngày tháng).
4. `frontend-test/src/pages/owner/OwnerTournamentsPage.tsx` (Áp dụng helper định dạng ngày tháng).
5. `frontend-test/src/pages/admin/AdminTournamentsPage.tsx` (Áp dụng helper định dạng ngày tháng).

## 7. SQL script sửa data
Tệp tin: [fix-vietnamese-tournament-data.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/fix-vietnamese-tournament-data.sql)
```sql
USE [HorseRacingManagementSystem];
GO

-- Safe update for corrupted Vietnamese tournament name
UPDATE [Tournament]
SET [Name] = N'Giải Đua Vô Địch Quốc Gia 2026'
WHERE [TournamentId] = 4;
GO
```

## 8. Date format fix
Thêm hàm helper `formatDateTime` trong [format.js](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend-test/src/utils/format.js):
```javascript
export function formatDateTime(value) {
  if (!value) return "—";
  const date = new Date(value);
  if (isNaN(date.getTime())) return "—";
  
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();
  const hours = String(date.getHours()).padStart(2, '0');
  const minutes = String(date.getMinutes()).padStart(2, '0');
  
  return `${day}/${month}/${year} ${hours}:${minutes}`;
}
```
Và sử dụng trong các page:
```javascript
{formatDateTime(t.startDate)}
{formatDateTime(t.endDate)}
```

## 9. Test result
- Dữ liệu trong database hiển thị chính xác: `Giải Đua Vô Địch Quốc Gia 2026`.
- API Backend trả về đúng Unicode UTF-8 của tên giải đấu.
- Giao diện Frontend (Spectator, Owner, Admin) hiển thị chuẩn tiếng Việt: `Giải Đua Vô Địch Quốc Gia 2026`.
- Thời gian bắt đầu và kết thúc hiển thị đẹp theo đúng định dạng `dd/mm/yyyy hh:mm` (Ví dụ: `01/06/2026 00:00`).

## 10. Remaining issues
- Không có lỗi nào chưa được xử lý. Tất cả các yêu cầu đã được đáp ứng trọn vẹn và an toàn.
