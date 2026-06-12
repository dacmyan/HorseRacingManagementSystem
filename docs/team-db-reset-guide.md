# Hướng Dẫn Reset Local Database Sau Khi Đồng Bộ DB Schema Singular

Tài liệu này hướng dẫn cách dọn dẹp và thiết lập lại cơ sở dữ liệu local/dev của các thành viên trong dự án sau khi team merge các thay đổi cấu hình EF Core ánh xạ thực thể sang bảng số ít (singular).

## 1. Tại sao cần phải Reset Database Local?
Trước đây, file `AppDbContext.cs` bị thiếu cấu hình mapping `.ToTable()` cho một số thực thể. Do đó, EF Core đã tự động tạo ra các bảng số nhiều (plural) như `Users`, `Roles`, `Races`, `RaceEntries`,... dẫn đến tình trạng cơ sở dữ liệu bị trùng lặp bảng (duplicate tables) với các bảng số ít chuẩn (`AppUser`, `Role`, `Race`, `RaceEntry`,...).

Hiện tại, codebase đã được cập nhật:
* Sửa toàn bộ mapping trong `AppDbContext.cs` về dạng bảng số ít chuẩn.
* Thêm thực thể và bảng `RefereeReport` đầy đủ.
* Đồng bộ hóa tệp Migration mới.

Vì cơ sở dữ liệu local cũ của các bạn chứa cả hai nhóm bảng (nhiều bảng rỗng và một số bảng có dữ liệu test cũ), cách tốt nhất và nhanh nhất để làm sạch là **DROP DATABASE** cũ và **CREATE DATABASE** mới theo script được cung cấp sẵn.

---

## 2. Các bước Reset Database trên máy cá nhân

> [!CAUTION]
> **CẢNH BÁO MẤT DỮ LIỆU LOCAL**
> Việc chạy script dưới đây sẽ xóa hoàn toàn cơ sở dữ liệu `HorseRacingManagementSystem` hiện tại của các bạn và tạo lại bản sạch. Hãy chủ động backup dữ liệu test nếu các bạn có dữ liệu quan trọng tự tạo.
> **TUYỆT ĐỐI KHÔNG** chạy script này trên môi trường Production hoặc database dùng chung (Shared DB).

### Bước 1: Pull code mới nhất từ GitHub
Hãy checkout sang branch của team và thực hiện pull code mới nhất:
```bash
git pull
```

### Bước 2: Chạy SQL Script dựng lại Database sạch
Sử dụng công cụ **SQL Server Management Studio (SSMS)** hoặc công cụ SQL của bạn:
1. Mở file script SQL tại: [recreate-clean-database.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/recreate-clean-database.sql).
2. Chạy toàn bộ script này trên SQL Server instance local của bạn.
3. Script sẽ tự động:
   * Chuyển ngữ cảnh sang master và đóng các kết nối cũ tới database.
   * Xóa database cũ `HorseRacingManagementSystem`.
   * Tạo database mới sạch sẽ.
   * Tạo toàn bộ 24 bảng singular, index và khóa ngoại chuẩn xác.
   * Seed đầy đủ dữ liệu Role (`Admin`, `HorseOwner`, `Jockey`, `Referee`, `Spectator`) và các tài khoản test chuẩn (`admin`, `owner`, `jockey`, `referee`, `spectator` với mật khẩu mặc định là `123456`).
   * Ghi lịch sử Migration vào bảng `__EFMigrationsHistory` để EF Core không cố gắng chạy lại các lệnh Scaffold cũ.

*Lưu ý: Nếu muốn chạy bằng CLI qua công cụ `sqlcmd`, sử dụng lệnh:*
```bash
sqlcmd -S <TÊN_SERVER_CỦA_BẠN> -E -i docs/sql/recreate-clean-database.sql
```

### Bước 3: Build lại Project backend
Chạy lệnh sau tại thư mục `backend` để khôi phục các nuget packages và build lại toàn bộ giải pháp:
```bash
dotnet restore HorseRacing.sln
dotnet build HorseRacing.sln
```

---

## 3. Cách Verify thiết lập sau khi Reset thành công

### Kiểm tra các bảng trong SQL Server
Mở SQL Server và chạy query sau để đảm bảo không còn bảng số nhiều nào:
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Users', 'Roles', 'Horses', 'Tournaments', 'RaceEntries', 'RaceResults', 'WalletTransactions', 'Notifications');
```
*Kết quả mong muốn: Trả về 0 dòng.*

Chạy query sau để hiển thị toàn bộ bảng hiện có:
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
ORDER BY TABLE_NAME;
```
*Kết quả mong muốn: Chỉ xuất hiện các bảng ở dạng số ít chuẩn (Ví dụ: `AppUser`, `Role`, `Horse`, `Race`, `RefereeReport`, `WalletTransaction`,...).*

---

## 4. Chạy Backend và Kiểm tra Swagger / Health Check
Khởi động API Backend:
```bash
dotnet run --project src/HorseRacing.API -- --urls http://localhost:5001
```

Sau đó kiểm tra trên trình duyệt:
1. **Swagger UI:** `http://localhost:5001/swagger/index.html` (để kiểm tra OpenAPI Docs).
2. **Health Check DB:** `http://localhost:5001/api/health/db` (để kiểm tra xem backend đã kết nối thành công tới Database mới chưa). Kết quả mong muốn:
   ```json
   {"status":"success","message":"Database connected successfully"}
   ```
