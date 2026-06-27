# Run Local Guide

Tài liệu này hướng dẫn cách cài đặt và chạy dự án Horse Racing Management System dưới máy local cho các thành viên trong nhóm phát triển.

## 1. Yêu cầu cài đặt

Trước khi bắt đầu, hãy đảm bảo máy tính của bạn đã cài đặt các công cụ sau:
- **.NET SDK** (phiên bản 8.0 trở lên)
- **SQL Server** (LocalDB hoặc SQL Server Express/Enterprise) đang chạy trên cổng 1433
- **Node.js** (phiên bản 18+ hoặc 20+ khuyến nghị LTS)
- **Git**
- **EF Core CLI** (nếu muốn làm việc với database migration):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## 2. Clone/Pull code

1. Sử dụng Git để clone/pull phiên bản code mới nhất của dự án.
2. Dự án được thiết lập dưới dạng Monorepo, có cấu trúc thư mục như sau:
   - `backend/`: Mã nguồn dự án API ASP.NET Core (.NET 8.0).
   - `frontend/`: Mã nguồn dự án giao diện React + Vite + TypeScript.
   - `docs/`: Thư mục tài liệu của dự án.

## 3. Setup database

1. Đảm bảo SQL Server của bạn đang hoạt động.
2. Kiểm tra chuỗi kết nối (connection string) trong tệp [appsettings.Development.json](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/appsettings.Development.json) của backend:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=HorseRacingManagementSystem;User Id=sa;Password=12345;TrustServerCertificate=True;"
   }
   ```
   *Lưu ý: Nếu SQL Server của bạn sử dụng tài khoản/mật khẩu khác hoặc chạy cổng khác, hãy sửa đổi chuỗi kết nối này cho phù hợp với máy local của bạn. Không commit mật khẩu production lên Git.*
3. Cơ sở dữ liệu sử dụng định dạng tên bảng dạng số ít (**Singular** - ví dụ `AppUser`, `Role`, `RaceEntry`, `WalletTransaction`).
4. Khởi chạy Database:
   - Khi chạy ứng dụng backend lần đầu tiên, hệ thống sẽ tự động tạo cơ sở dữ liệu (nếu chưa có) và tự động chạy seed dữ liệu mẫu qua class `DatabaseSeeder`.
   - Nếu bạn muốn cập nhật database qua Migration, di chuyển vào thư mục `backend/` và chạy:
     ```bash
     dotnet ef database update --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API
     ```

## 4. Run backend

1. Di chuyển vào thư mục backend:
   ```bash
   cd backend
   ```
2. Thực hiện restore các packages và build dự án:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Chạy backend API trên cổng mặc định (ví dụ: cổng 5001):
   ```bash
   dotnet run --project src/HorseRacing.API -- --urls http://localhost:5000
   ```
   *(Nếu cổng 5001 đã bị chương trình khác chiếm dụng, bạn có thể chạy trên cổng 5002 bằng cách thay URL ở cuối: `--urls http://localhost:5002`)*

## 5. Run frontend

1. Di chuyển vào thư mục frontend:
   ```bash
   cd ../frontend
   ```
2. Cài đặt dependencies (Node Modules):
   - Trên **Windows PowerShell**:
     ```powershell
     npm.cmd install
     ```
   - Trên **CMD** hoặc các hệ điều hành khác (Linux/macOS):
     ```bash
     npm install
     ```
3. Tạo cấu hình tệp môi trường `.env`:
   - Sao chép tệp cấu hình mẫu `.env.example` thành tệp `.env`.
   - Mở file `.env` và thiết lập biến môi trường như sau để đi qua cổng proxy dev của Vite:
     ```env
     VITE_API_URL=/api
     ```
4. Cấu hình cổng proxy trong [vite.config.ts](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/vite.config.ts):
   - Mở tệp `vite.config.ts` và đảm bảo thuộc tính `server.proxy` trỏ đúng cổng mà backend API của bạn đang chạy (cổng `5001` thay vì cổng `5000` mặc định):
     ```typescript
     server: {
       proxy: {
         '/api': {
           target: 'http://localhost:5001', // Đảm bảo khớp với cổng backend đang chạy (ví dụ 5001)
           changeOrigin: true,
         },
       },
     }
     ```
5. Chạy frontend dev server:
   - Trên **PowerShell**: `npm.cmd run dev`
   - Trên **CMD/Bash**: `npm run dev`
   - Giao diện frontend sẽ chạy tại địa chỉ: `http://localhost:5173`

## 6. Test Swagger

Khi backend đã khởi động, bạn có thể truy cập tài liệu hướng dẫn và thử nghiệm các API trực tiếp tại:
`http://localhost:5001/swagger`

## 7. Test Login

Dữ liệu mẫu đã được seed tự động các tài khoản thử nghiệm tương ứng với 5 vai trò trong hệ thống (Tất cả mật khẩu mặc định là **`123456`**):

| Vai trò | Email đăng nhập | Mật khẩu | Ghi chú |
| ------- | --------------- | -------- | ------- |
| **Admin** (Quản trị viên) | `admin@gmail.com` | `123456` | Có toàn quyền quản lý, tạo giải đấu, vòng đua, tài khoản |
| **HorseOwner** (Chủ ngựa) | `owner@gmail.com` | `123456` | Quản lý ngựa, đề xuất nài ngựa, đăng ký đua |
| **Jockey** (Nài ngựa) | `jockey@gmail.com` | `123456` | Xem hợp đồng đề xuất và phản hồi |
| **Referee** (Trọng tài) | `referee@gmail.com` | `123456` | Xem lịch và kết quả, báo cáo vi phạm |
| **Spectator** (Khán giả) | `spectator@gmail.com` | `123456` | Có ví, nạp/rút tiền, đặt cược các trận đấu |

## 8. Lỗi thường gặp và cách xử lý

- **Lỗi 404 khi vào root URL backend (`http://localhost:5001/`)**: Đây là bình thường vì backend không định cấu hình landing page. Sử dụng địa chỉ `/swagger` để kiểm tra.
- **Lỗi 502 Bad Gateway khi gọi API từ frontend**:
  - *Nguyên nhân*: Cấu hình proxy target trong `vite.config.ts` (mặc định là `http://localhost:5000`) lệch cổng so với cổng chạy thực của backend (ví dụ bạn đang chạy cổng `5001`).
  - *Cách sửa*: Sửa target trong `vite.config.ts` khớp với cổng chạy thực của backend.
- **Lỗi CORS (Cross-Origin Resource Sharing)**:
  - *Nguyên nhân*: Gọi trực tiếp API tuyệt đối (ví dụ `http://localhost:5001/api/...`) từ trình duyệt mà backend chưa bật CORS middleware.
  - *Cách sửa*: Đảm bảo dùng relative path `/api` và để Vite proxy lo việc chuyển tiếp trung gian. Hoặc bổ sung cấu hình cho phép origin `http://localhost:5173` trong tệp `Program.cs` của backend.
- **Lỗi "Policy restriction" khi chạy lệnh npm**:
  - *Nguyên nhân*: PowerShell trên Windows chặn chạy các tập tin script `.ps1`.
  - *Cách sửa*: Dùng `npm.cmd` thay vì `npm` (ví dụ `npm.cmd install`, `npm.cmd run dev`).
- **Database chép đè hoặc lỗi singular/plural**:
  - Cơ sở dữ liệu chuẩn bắt buộc phải là dạng số ít (singular). Nếu bạn đang gặp lỗi các bảng dạng số nhiều (`Users`, `Roles`), hãy drop database cục bộ bằng SQL Server Management Studio rồi chạy dự án backend để EF tự sinh lại schema chuẩn.
