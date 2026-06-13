# FE-BE Login Integration Report

Báo cáo chi tiết về kết quả tích hợp cơ chế đăng nhập JWT và kết nối API giữa Frontend và Backend tại môi trường local.

## 1. Backend login endpoint
- **Endpoint**: `POST /api/auth/login`
- **Base URL**: `http://localhost:5001` (địa chỉ chạy local của Kestrel backend)

## 2. Login request body
Yêu cầu JSON object chứa email và mật khẩu của người dùng:
```json
{
  "email": "admin@gmail.com",
  "password": "123456"
}
```

## 3. Login response structure
Khi xác thực thành công, backend trả về kết quả cấu trúc như sau:
```json
{
  "message": "Login successful",
  "result": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpX...",
    "refreshToken": null,
    "user": {
      "id": 1,
      "fullName": "Admin",
      "email": "admin@gmail.com",
      "role": "Admin"
    }
  }
}
```

## 4. Test accounts dùng được
Các tài khoản được tự động khởi tạo qua `DatabaseSeeder` khi chạy backend (tất cả mật khẩu mặc định là **`123456`**):
- **Admin**: `admin@gmail.com`
- **Chủ ngựa (HorseOwner)**: `owner@gmail.com`
- **Nài ngựa (Jockey)**: `jockey@gmail.com`
- **Trọng tài (Referee)**: `referee@gmail.com`
- **Khán giả (Spectator)**: `spectator@gmail.com`

## 5. Frontend API base URL
Đã đồng bộ cấu hình qua 2 cơ chế để linh hoạt chạy local:
- **Biến môi trường (.env)**:
  - `VITE_API_BASE_URL=http://localhost:5001`
  - `VITE_API_URL=/api` (Sử dụng relative path giúp request đi qua proxy local của Vite).
- **Proxy cấu hình (vite.config.ts)**:
  - `target: 'http://localhost:5001'` (Vite dev server sẽ chuyển tiếp request `/api` đến cổng `5001` của backend).

## 6. File frontend đã sửa
- **[vite.config.ts](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/vite.config.ts)**: Đã sửa target proxy trỏ đến `http://localhost:5001`.
- **[.env](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/.env)** và **[.env.example](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/.env.example)**: Tạo mới các file cấu hình chứa biến môi trường chuẩn.
- *Lưu ý*: Không cần sửa đổi các file logic javascript của frontend do:
  - File [authService.js](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/api/authService.js) đã map chính xác trường token trả về từ API (`data.result.accessToken`) và thông tin user (`data.result.user`).
  - File [api.js](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/services/api.js) lấy và đính kèm token chuẩn xác qua khóa `'token'`.

## 7. Token được lưu ở đâu
- **Khóa token**: Lưu trữ trong `localStorage` của trình duyệt dưới key `'token'`.
- **Khóa thông tin user**: Lưu trữ trong `localStorage` dưới key `'user'`.

## 8. Authorization header đã gửi chưa
- **Đã gửi**: Trong file [api.js](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/services/api.js), fetch interceptor được cấu hình tự động gắn thêm header `Authorization: Bearer <token>` vào mỗi request gọi đến các API protected.

## 9. CORS backend
- **Đã cấu hình**: Đã đăng ký dịch vụ và middleware CORS trong file [Program.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Program.cs) của backend để cho phép origin `http://localhost:5173` gọi API trực tiếp.
  - Cấu hình đã thêm:
    ```csharp
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendCors", policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });
    ...
    app.UseCors("FrontendCors");
    ```

## 10. Kết quả test login
- **Trạng thái**: **Thành công** (Pass 100%).
- Khi gửi request `POST http://localhost:5173/api/auth/login` với thông tin đăng nhập hợp lệ, proxy Vite chuyển tiếp thành công và nhận về kết quả token cùng thông tin user.

## 11. Kết quả test protected API
- **Trạng thái**: **Thành công** (Pass 100%).
- Đã kiểm tra gọi API protected `GET http://localhost:5001/api/admin/roles` kèm header `Authorization: Bearer <token>` của Admin. API xác thực thành công và trả về danh sách các vai trò (Roles) chính xác mà không bị chặn `401 Unauthorized` hay `403 Forbidden`.

## 12. Lỗi còn lại nếu có
- Không có lỗi nào phát sinh. Frontend và Backend đã kết nối tích hợp hoàn hảo.
