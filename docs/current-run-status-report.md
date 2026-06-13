# Current Run Status Report

Tài liệu báo cáo hiện trạng khả năng chạy dự án (run status) và tích hợp (integration) giữa Frontend và Backend tại môi trường local.

## 1. Backend status
- **Build**: **Thành công** (0 Error, 45 Warnings liên quan đến cảnh báo bảo mật của một số package NuGet cũ như MailKit, MimeKit, AutoMapper và cảnh báo nullable reference).
- **Run local**: **Thành công**. Ứng dụng ASP.NET Core khởi chạy bình thường trên cổng `http://localhost:5001`.
- **Swagger**: **Hoạt động tốt** tại địa chỉ `http://localhost:5001/swagger`.

## 2. Database status
- **Kết nối**: **Thành công** (Database connected successfully).
- **Trạng thái cấu trúc bảng**: **Đúng chuẩn**. Toàn bộ các bảng trong SQL Server được khởi tạo chính xác ở định dạng số ít (**Singular**), ví dụ: `AppUser`, `Role`, `RaceEntry`, `WalletTransaction`, `Bet`, v.v. Không còn sự tồn tại của các bảng dạng số nhiều (Plural).
- **Seed dữ liệu mẫu**: **Thành công**. Hệ thống tự động seed các vai trò và 5 tài khoản kiểm thử chính cùng với các profile/wallet liên quan khi ứng dụng backend khởi chạy lần đầu.

## 3. Frontend status
- **Cài đặt thư viện**: **Thành công** (Sử dụng `npm.cmd install` trên Windows).
- **Build**: **Thành công** (Tạo ra bundle static files trong folder `frontend/dist`).
- **Lint**: **Thất bại** (Gặp 37 lỗi tĩnh liên quan đến quy tắc đặt kiểu dữ liệu `any` và việc gọi `setState` đồng bộ trong `useEffect` ở các trang spectator).

## 4. FE-BE integration status
- **Trạng thái**: **Chờ điều chỉnh cấu hình cổng (Pending Configuration)**.
- **Chi tiết kết nối**:
  - Khi chạy thử nghiệm tích hợp, các yêu cầu gọi API từ frontend qua relative path (ví dụ `/api/health/db`) trả về lỗi **`502 Bad Gateway`**.
  - **Nguyên nhân**: Cấu hình proxy target trong tệp `frontend/vite.config.ts` đang trỏ về cổng `http://localhost:5000`, trong khi backend đang được khởi chạy trên cổng `http://localhost:5001`.
  - **Khắc phục**: Khi đổi cổng proxy target sang `http://localhost:5001`, việc tích hợp và gọi API thông qua proxy local hoạt động trơn tru.

## 5. Lỗi còn lại

1. **Lệch cổng Proxy ở Frontend**: `vite.config.ts` trỏ target tới `http://localhost:5000` thay vì `http://localhost:5001`.
2. **Thiếu CORS ở Backend**: Backend chưa đăng ký policy CORS cho phép origin `http://localhost:5173`. Do đó nếu frontend gọi trực tiếp tới URL tuyệt đối `http://localhost:5001/api/...`, trình duyệt sẽ chặn request.
3. **Lỗi Lint ở Frontend**: 37 lỗi tĩnh cần được dọn dẹp để đảm bảo code quality.

## 6. Việc FE cần xử lý

1. **Sửa cấu hình Proxy local**:
   Cập nhật file [vite.config.ts](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/vite.config.ts) ở thuộc tính `server.proxy` để target trỏ tới `http://localhost:5001`.
2. **Khắc phục 37 lỗi lint**:
   - Thay thế các kiểu khai báo `any` thành các interfaces hoặc kiểu `unknown`.
   - Điều chỉnh hàm load dữ liệu ở `SpectatorNotificationsPage.tsx`, `SpectatorPredictionsPage.tsx`, và `SpectatorWalletPage.tsx` để tránh gọi setState đồng bộ trực tiếp trong `useEffect`.
3. **Tạo tệp môi trường local**:
   Tạo tệp `.env` kế thừa từ `.env.example` với giá trị `VITE_API_URL=/api`.

## 7. Việc BE cần xử lý

1. **Cấu hình CORS Policy**:
   Bổ sung CORS policy cho phép origin `http://localhost:5173` (giao diện dev) truy cập trực tiếp trong trường hợp không chạy qua proxy.
   - Thêm vào `Program.cs` trước `builder.Build()`:
     ```csharp
     builder.Services.AddCors(options => {
         options.AddPolicy("AllowFrontend", policy => {
             policy.WithOrigins("http://localhost:5173")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
         });
     });
     ```
   - Thêm vào sau `app.UseRouting()`:
     ```csharp
     app.UseCors("AllowFrontend");
     ```

## 8. Có thể run local chưa?

**Có thể chạy local được ngay lập tức** (chỉ cần điều chỉnh cổng proxy target của frontend sang `5001` hoặc cấu hình cổng chạy của backend sang `5000` để khớp cấu hình mặc định).

## 9. Nên bắt đầu từ đâu?

1. Đảm bảo dịch vụ SQL Server local đang chạy.
2. Khởi chạy Backend trên cổng 5001:
   ```bash
   cd backend
   dotnet run --project src/HorseRacing.API -- --urls http://localhost:5001
   ```
3. Mở một terminal mới, cập nhật tệp `frontend/vite.config.ts` chỉnh sửa proxy target sang `http://localhost:5001`.
4. Tạo tệp `frontend/.env` chứa dòng `VITE_API_URL=/api`.
5. Khởi chạy Frontend:
   ```bash
   cd frontend
   npm.cmd install  # Windows PowerShell
   npm.cmd run dev
   ```
6. Truy cập trình duyệt tại `http://localhost:5173` và thử đăng nhập bằng tài khoản `admin@gmail.com` / mật khẩu `123456`.
