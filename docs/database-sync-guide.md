# Hướng dẫn Đồng bộ Cơ sở dữ liệu và Dữ liệu mẫu (Database Synchronization Guide)

Tài liệu này hướng dẫn cách thức đồng bộ cấu trúc bảng (schema) và dữ liệu mẫu (seed data) trong quá trình làm việc nhóm bằng Git cho dự án **Horse Racing Management System**.

---

## 1. Nguyên tắc cốt lõi
* **Git KHÔNG đồng bộ cơ sở dữ liệu vật lý**: Git chỉ đồng bộ mã nguồn bao gồm các tệp Migrations (`.cs`) và mã nguồn Seeder.
* **Không chỉnh sửa trực tiếp trên cơ sở dữ liệu (SSMS / SQL Server)**: Mọi thay đổi về cấu trúc bảng (thêm/sửa/xóa cột, bảng, khóa ngoại) bắt buộc phải thực hiện bằng mã nguồn C# và tạo Migration.
* **Tự động hóa**: Khi bạn khởi chạy ứng dụng backend trong môi trường **Development**, hệ thống sẽ tự động chạy lệnh áp dụng Migration (nếu có) và chạy Seeder để đảm bảo dữ liệu mẫu luôn sẵn sàng.

---

## 2. Thiết lập cơ sở dữ liệu lần đầu (Khi pull code về lần đầu)

Sau khi clone hoặc pull mã nguồn mới nhất từ nhánh chung về máy local của bạn, hãy thực hiện các bước sau:

### Bước 2.1: Cấu hình appsettings local
1. Vào thư mục `backend/src/HorseRacing.API/`.
2. Copy tệp `appsettings.example.json` thành `appsettings.Development.json`.
3. Mở tệp `appsettings.Development.json` mới tạo và chỉnh sửa chuỗi kết nối (`DefaultConnection`) cho khớp với SQL Server cục bộ trên máy bạn:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=HorseRacingManagementSystem;User Id=sa;Password=Mật_Khẩu_Của_Bạn;TrustServerCertificate=True;"
   }
   ```
   *Lưu ý: Tệp `appsettings.Development.json` đã được đưa vào `.gitignore` để không bị đẩy lên Git, tránh làm đè cấu hình của các thành viên khác.*

### Bước 2.2: Chạy Migration để tạo Database và Bảng
Mở Terminal ở thư mục gốc dự án (hoặc thư mục `backend/`) và chạy lệnh sau để tự tạo cơ sở dữ liệu và toàn bộ cấu trúc bảng chuẩn:
```bash
dotnet ef database update --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API
```

### Bước 2.3: Khởi chạy Backend và Tự động Seed Data
Chạy dự án backend API:
```bash
dotnet run --project src/HorseRacing.API
```
* **Khi khởi chạy**: Hệ thống sẽ tự động áp dụng các Migration mới (nếu có).
* **Seeder**:
  - `DataSeeder` (Chạy ở mọi môi trường): Tự động tạo các vai trò mặc định (`Admin`, `HorseOwner`, `Jockey`, `Referee`, `Spectator`) và tài khoản Admin mẫu (`admin@gmail.com` / mật khẩu `123456`).
  - `DemoDataSeeder` (Chỉ chạy ở môi trường **Development**): Tự động thêm dữ liệu thử nghiệm bao gồm: 14 ngựa đua, Giải Đua Ngựa Mùa Đông 2026, 2 vòng đua thử nghiệm (Pre, Final), các lượt đua (Pre Race 1, Pre Race 2), đăng ký ngựa đua, hợp đồng nài ngựa, phân làn đua và phân công trọng tài.

---

## 3. Quy trình cập nhật Database khi pull code mới về
Mỗi khi bạn thực hiện `git pull` để nhận code mới nhất từ các thành viên khác, cấu trúc cơ sở dữ liệu có thể đã thay đổi. Hãy làm theo các bước sau để đồng bộ:

1. **Build dự án để kiểm tra lỗi biên dịch**:
   ```bash
   dotnet build
   ```
2. **Áp dụng các Migration mới vào Database của bạn**:
   ```bash
   dotnet ef database update --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API
   ```
3. **Chạy dự án backend**: Hệ thống sẽ khởi động và tự cập nhật thêm dữ liệu mẫu nếu có.

---

## 4. Cách thêm hoặc sửa đổi bảng (Tạo Migration mới)
Khi bạn cần thay đổi cấu trúc bảng (ví dụ: thêm cột mới vào bảng `Horse`):

1. **Sửa Entity trong mã nguồn**: Cập nhật thuộc tính của class Entity tương ứng ở project `HorseRacing.Domain` hoặc cấu hình Fluent API trong `AppDbContext.cs`.
2. **Tạo Migration mới bằng CLI**:
   Mở terminal trong thư mục `backend/` và chạy lệnh sau (thay `TenMigrationNganGon` bằng tên mô tả thay đổi, viết CamelCase):
   ```bash
   dotnet ef migrations add TenMigrationNganGon --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API
   ```
3. **Kiểm tra tệp Migration vừa sinh ra**: Xem trong thư mục `src/HorseRacing.Infrastructure/Migrations/` để đảm bảo lệnh `Up` và `Down` tạo đúng cấu trúc bạn mong muốn.
4. **Cập nhật database cục bộ của bạn**:
   ```bash
   dotnet ef database update --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API
   ```
5. **Commit và Push**: Đẩy cả file thực thể entity đã sửa và các file migration (`.cs` và `.Designer.cs`) mới sinh lên Git để đồng nghiệp có thể cập nhật.

---

## 5. Quy tắc viết Seed Data an toàn
Khi thêm dữ liệu mẫu trong `DataSeeder.cs` hoặc `DemoDataSeeder.cs`, bắt buộc phải tuân thủ:
* **Kiểm tra sự tồn tại (Idempotency)**: Luôn kiểm tra xem bản ghi đã tồn tại trong DB chưa trước khi gọi lệnh `Add`. Ví dụ:
  ```csharp
  if (!await _context.Horses.AnyAsync(h => h.Name == "Secretariat"))
  {
      _context.Horses.Add(new Horse { Name = "Secretariat", ... });
  }
  ```
* **Không sử dụng dữ liệu ngẫu nhiên (Random)**: Tránh dùng `Random` sinh dữ liệu nhạy cảm hoặc băm mật khẩu ngẫu nhiên mỗi lần start, vì nó sẽ gây nhiễu và làm sai lệch trạng thái kiểm tra.
* **Hash mật khẩu động bằng mã nguồn**: Sử dụng `PasswordHasher<AppUser>` để hash mật khẩu an toàn lúc chạy, mật khẩu mặc định cho môi trường phát triển là **`123456`**.

---

## 6. Cách Reset Database khi bị lỗi hoặc lệch Schema
Nếu cơ sở dữ liệu local của bạn bị lỗi dữ liệu hoặc lệch schema nghiêm trọng không thể giải quyết bằng update thông thường:

1. **Xóa (Drop) database cũ**:
   ```bash
   dotnet ef database drop --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API
   ```
   *Nhấn `y` để xác nhận xóa.*
2. **Chạy lại từ đầu để dựng lại database sạch**:
   ```bash
   dotnet ef database update --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API
   ```
3. **Chạy backend**: Dự án sẽ chạy lại toàn bộ seeder từ đầu giúp bạn có một database sạch và đầy đủ dữ liệu mẫu.

---

## 7. Các lệnh CLI thường dùng (Chạy từ thư mục backend)

| Lệnh | Mô tả |
| --- | --- |
| `dotnet build` | Biên dịch toàn bộ dự án |
| `dotnet ef database update --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API` | Cập nhật database local lên phiên bản Migration mới nhất |
| `dotnet ef migrations add <Tên> --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API` | Tạo một Migration mới dựa trên thay đổi của Entity |
| `dotnet ef migrations remove --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API` | Xóa Migration cuối cùng chưa được đẩy lên database |
| `dotnet ef database drop --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API` | Xóa hoàn toàn database local |

---

## 8. Khắc phục sự cố thường gặp (Troubleshooting)

### 8.1. Lỗi tiến trình chiếm dụng tệp tin (File lock)
* **Thông báo lỗi**: `Could not copy ... because it is being used by another process.`
* **Cách sửa**: Do backend API đang chạy nền hoặc hot-reload chưa tắt hẳn làm khóa tệp `.dll`. Chạy lệnh sau trong PowerShell để tắt tiến trình bị kẹt:
  ```powershell
  Stop-Process -Name "HorseRacing.API" -ErrorAction SilentlyContinue
  ```

### 8.2. Lỗi chèn khóa trùng lặp khi chạy Seed Data (Duplicate Key / Identity)
* **Thông báo lỗi**: `Cannot insert duplicate key row...`
* **Cách sửa**: Bản ghi bạn cố chèn đã tồn tại hoặc seeder của bạn bị thiếu bước kiểm tra `.AnyAsync()`. Hãy chắc chắn đã bao bọc lệnh chèn trong block kiểm tra tồn tại.

### 8.3. Lỗi `dotnet-ef` không nhận diện được lệnh
* **Thông báo lỗi**: `Could not execute because the specified command or file was not found.`
* **Cách sửa**: Dự án cấu hình dotnet-ef cục bộ dưới dạng local tool. Hãy khôi phục lại công cụ bằng cách chạy lệnh sau tại thư mục gốc dự án:
  ```bash
  dotnet tool restore
  ```
