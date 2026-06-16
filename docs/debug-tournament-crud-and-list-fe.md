# Debug Tournament CRUD And Frontend List

Báo cáo chi tiết quá trình kiểm tra, phát hiện lỗi và sửa lỗi cho chức năng CRUD và hiển thị danh sách Tournament (Giải đấu).

## 1. Mô tả lỗi
- Tournament đã có dữ liệu trong SQL Server nhưng frontend không hiển thị đúng danh sách giải đấu trên một số giao diện.
- Trình tạo giải đấu (Create Tournament) không hiển thị được ID giải đấu mới tạo trong thông báo thành công.
- Trong giao diện đăng ký thi đấu của chủ ngựa (Owner), ô nhập giải đấu yêu cầu người dùng tự gõ ID bằng tay do thiếu dropdown hiển thị danh sách giải đấu đang có.

## 2. Database Tournament data
- **Server**: `localhost`
- **Database**: `HorseRacingManagementSystem`
- **Dữ liệu hiện tại trong bảng `Tournament`**:
  - `TournamentId = 4`: `Giải Đua Vô Địch Quốc Gia 2026` | Status: `Active`
  - `TournamentId = 3`: `Hanoi Championship 2026` | Status: `Completed`
  - `TournamentId = 1`: `Giải đấu Khang Lẹo` | Status: `Upcoming`
  - *Và giải đấu mới được tạo thêm khi test*: `Giai Dua Ke Hoach 2026` | Status: `Upcoming` (ID = 5)

## 3. Backend Tournament endpoints
- `GET /api/Public/tournaments` (Lấy danh sách tất cả giải đấu - Public)
- `GET /api/Public/tournaments/{id}` (Lấy chi tiết giải đấu - Public)
- `POST /api/Admin/tournaments` (Tạo giải đấu mới - Quyền Admin)
- `PUT /api/Admin/tournaments/{id}` (Cập nhật giải đấu - Quyền Admin)
*(Lưu ý: Hệ thống hiện tại không định nghĩa endpoint Delete giải đấu).*

## 4. Backend CRUD test result
Bảng kết quả thử nghiệm trực tiếp trên API Backend (sử dụng PowerShell `Invoke-RestMethod`):

| Case | Endpoint | Expected | Actual | Pass/Fail |
| ---- | -------- | -------- | ------ | --------- |
| GET list | `GET /api/Public/tournaments` | Trả về HTTP 200, danh sách chứa các giải đấu từ database | Trả về HTTP 200 kèm danh sách 3 giải đấu ban đầu | Pass |
| GET detail | `GET /api/Public/tournaments/1` | Trả về HTTP 200, thông tin chi tiết giải đấu ID = 1 | Trả về HTTP 200 đúng thông tin giải đấu ID = 1 | Pass |
| POST create | `POST /api/Admin/tournaments` | Trả về HTTP 201, tạo thành công giải đấu và trả về đối tượng giải đấu | Trả về HTTP 201 cùng giải đấu mới với ID tự sinh | Pass |
| PUT update | `PUT /api/Admin/tournaments/1` | Trả về HTTP 200, cập nhật thành công | Trả về HTTP 200 kèm thông tin giải đấu đã sửa | Pass |

## 5. Frontend API request
- **Request URL**: `http://localhost:5001/api/public/tournaments`
- **Request Method**: `GET`
- **Response Status**: `200 OK`
- **Response Body**:
  ```json
  {
    "message": "Tournaments retrieved successfully",
    "result": [
      {
        "tournamentId": 1,
        "name": "Giải đấu Khang Lẹo",
        "startDate": "2026-07-13T08:00:00",
        "endDate": "2026-08-02T10:00:00",
        "status": "Upcoming",
        "rounds": [...]
      }
    ]
  }
  ```

## 6. Backend response structure
- API danh sách giải đấu trả về cấu trúc bọc trong thuộc tính `result` dạng mảng: `{ message: "...", result: [...] }`.
- API tạo mới giải đấu (`POST /api/Admin/tournaments`) trả về trực tiếp đối tượng giải đấu (DTO `TournamentResponse`) chứ không bọc qua trường `result`.

## 7. Frontend response mapping
Bảng kiểm tra so khớp ánh xạ phản hồi:

| Backend response field | FE đang đọc | Đúng/Sai | Cách sửa |
| ---------------------- | ----------- | -------- | -------- |
| `result` (GET list) | `data?.result` | Đúng | Giữ nguyên |
| `tournamentId` (POST) | `data?.result?.id` hoặc `data?.result?.tournamentId` | Sai | Đổi thành `data?.tournamentId ?? data?.result?.tournamentId` vì POST trả trực tiếp DTO không bọc `result` |
| `rounds` array (GET list) | `t.numberOfRounds` | Sai | Đổi thành `t.rounds?.length` để đếm động số vòng vì DTO Backend không có thuộc tính `numberOfRounds` |

## 8. Frontend filter/pagination/render check
- **Bộ lọc trạng thái (Status filters)**: Bộ lọc trạng thái trên màn hình Admin trước đây so sánh chính xác phân biệt chữ hoa thường (ví dụ: `t.status === 'Active'`). Điều này dẫn đến nguy cơ ẩn đi dữ liệu nếu database chứa chuỗi trạng thái có cách viết hoa khác (ví dụ: `active`, `upcoming`, `completed`). Đã sửa thành so sánh không phân biệt hoa thường bằng cách dùng `.toLowerCase()`.

## 9. Nguyên nhân chính
1. **Lỗi cổng kết nối (Port mismatch)**: Mặc định nếu chạy backend không chỉ định cổng, backend sẽ chạy trên cổng 5000, trong khi `.env` của frontend được thiết lập để kết nối cổng 5001. Do đó nếu chạy không đúng lệnh chỉ định cổng của backend, API sẽ bị lỗi kết nối.
2. **Lỗi phân tích ID khi tạo mới**: API `POST` tạo giải đấu trả về trực tiếp DTO thay vì bọc trong `result` khiến frontend không đọc được `newId` để hiển thị trong popup thành công.
3. **Lỗi hiển thị số vòng đua**: Frontend gọi trường `numberOfRounds` không tồn tại trong DTO phản hồi của backend.
4. **Thiếu Dropdown chọn giải đấu**: Trên màn hình đăng ký ngựa thi đấu của chủ sở hữu (`OwnerRegistrationsPage.tsx`), người dùng phải tự điền số ID giải đấu thay vì được chọn từ danh sách giải đấu có sẵn do frontend thiếu việc gọi API lấy danh sách.

## 10. File đã sửa
1. **[AdminTournamentsPage.tsx](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/pages/admin/AdminTournamentsPage.tsx)**:
   - Sửa logic tính toán `newId` khi tạo mới để tương thích với dữ liệu trả về trực tiếp từ API.
   - Sửa các so sánh lọc trạng thái thành không phân biệt chữ hoa chữ thường.
   - Chuyển hiển thị số vòng đấu từ trường tĩnh `numberOfRounds` thành đếm số phần tử của mảng `rounds`.
2. **[OwnerRegistrationsPage.tsx](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/pages/owner/OwnerRegistrationsPage.tsx)**:
   - Tích hợp thêm API lấy danh sách giải đấu (`getTournaments`).
   - Chuyển đổi ô nhập ID giải đấu thủ công bằng select dropdown hiển thị tên và ID giải đấu một cách trực quan.

## 11. Kết quả test lại
- Thực hiện chạy thử nghiệm luồng đầy đủ bằng Browser Subagent:
  1. Đăng nhập Admin -> Vào trang giải đấu -> Danh sách các giải đấu cũ hiển thị đầy đủ và chính xác với số vòng đấu được đếm đúng.
  2. Tạo giải đấu mới thành công và popup thông báo đã hiển thị chính xác mã giải đấu (ID = 5). Danh sách tự động tải lại và giải đấu mới hiển thị ngay lập tức.
  3. Đăng nhập Owner -> Vào trang đăng ký thi đấu -> Click nút Đăng ký ngựa -> Dropdown chọn giải đấu hoạt động hoàn hảo, hiển thị đầy đủ các giải đấu hiện có bao gồm giải đấu ID = 5 vừa tạo.

## 12. Lỗi còn lại nếu có
- Không có lỗi nào còn lại đối với luồng CRUD/Hiển thị danh sách giải đấu.
