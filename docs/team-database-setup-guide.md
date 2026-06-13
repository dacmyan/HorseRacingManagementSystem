# Team Database Setup Guide

Tài liệu này hướng dẫn cách thiết lập, đồng bộ hóa và quản lý cơ sở dữ liệu local/dev SQL Server dành cho các thành viên trong dự án.

---

## 1. Git push có push database không?

> [!IMPORTANT]
> **Git KHÔNG push cơ sở dữ liệu SQL Server local của bạn lên GitHub.**
> Git là hệ thống quản lý phiên bản mã nguồn, nó chỉ theo dõi và đẩy các tệp tin văn bản (code C#, cấu hình JSON, script SQL, migrations, tài liệu markdown). Nó không thể trực tiếp đồng bộ các file dữ liệu nhị phân đang chạy trên SQL Server local instance của bạn.

Vì vậy, khi bạn thay đổi cấu trúc bảng hoặc thêm dữ liệu ở database local của mình, các thành viên khác **sẽ không thấy** những thay đổi này trừ khi bạn định nghĩa chúng dưới dạng **EF Core Migrations** hoặc **SQL Scripts** và push các file đó lên GitHub.

---

## 2. Những file liên quan đến database NÊN push lên Git

Để chia sẻ thay đổi cấu trúc hoặc dữ liệu mẫu cho team, bạn chỉ nên commit các file sau:
1. **EF Core Migration Files:** Các file nằm trong thư mục `Migrations/` (bao gồm tệp `.cs`, `.Designer.cs` và snapshot `AppDbContextModelSnapshot.cs`). Đây là cơ chế chính thức và chuẩn mực nhất để đồng bộ cấu trúc DB trong .NET Core.
2. **SQL Scripts phục vụ môi trường Local:** Các script khởi tạo nhanh database, tạo bảng sạch hoặc seed dữ liệu tĩnh dùng chung (nếu được lưu trữ trong thư mục `docs/sql/` và không chứa thông tin bảo mật nhạy cảm).
3. **Mã nguồn Runtime Seeding:** Các file code C# cấu hình seeding lúc khởi động app (như `DatabaseSeeder.cs`, `DataSeeder.cs`).
4. **Tài liệu hướng dẫn:** Các file `.md` hướng dẫn setup.

---

## 3. Những file TUYỆT ĐỐI KHÔNG được push lên Git

> [!WARNING]
> **CẢNH BÁO BẢO MẬT & XUNG ĐỘT PHIÊN BẢN**
> Việc đẩy các file dữ liệu vật lý hoặc file chứa password thật lên Git có thể gây lộ lọt thông tin hoặc làm hỏng repository. Hãy chắc chắn bỏ qua các file sau:

* **File sao lưu / Dữ liệu vật lý:** các file `.bak` (Backup), `.mdf` (Primary Data File), `.ldf` (Transaction Log File). Các file này rất nặng và mang tính chất local cá nhân.
* **Connection String chứa thông tin thật:** `appsettings.json` hoặc các file config chứa mật khẩu thật của SQL Server sản xuất (Production). Chỉ nên dùng cấu hình mặc định localhost với Integrated Security (Windows Authentication) hoặc User Secrets cho môi trường local.
* **Các file bí mật môi trường:** `.env`, `.env.local`, `appsettings.Development.local.json`.

---

## 4. Cách các thành viên trong team tạo Database Local

Dự án hiện tại hỗ trợ **2 cách độc lập** để dựng lại database local của bạn. Bạn chỉ cần chọn **một trong hai** cách dưới đây:

### Cách A: Dùng EF Core Migration (Khuyến nghị cho Developer)

Đây là cách chuẩn nhất khi làm việc với EF Core, giúp bạn theo dõi lịch sử thay đổi schema dễ dàng.

```bash
# 1. Di chuyển vào thư mục backend
cd backend

# 2. Khôi phục các gói nuget và build dự án
dotnet restore HorseRacing.sln
dotnet build HorseRacing.sln

# 3. Chạy cập nhật database bằng EF Core CLI
dotnet ef database update --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API
```

### Cách B: Dùng SQL Script (Khởi tạo nhanh / Sạch hoàn toàn)

Nếu bạn gặp lỗi migration hoặc muốn dựng lại database sạch chính xác 100% giống thiết kế DB, bạn có thể chạy file script có sẵn.

* **File Script nguồn:** [recreate-clean-database.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/recreate-clean-database.sql)
* **Cách thực hiện:**
  1. Mở file script bằng **SQL Server Management Studio (SSMS)** hoặc **Azure Data Studio**.
  2. Kết nối tới SQL Server instance local của bạn và chạy toàn bộ script.
  3. Script sẽ tự động xóa database cũ `HorseRacingManagementSystem` (nếu có), tạo lại database mới, dựng đầy đủ các bảng ở dạng số ít chuẩn (singular) và chèn dữ liệu Roles cùng các tài khoản thử nghiệm.

*Lưu ý: Nếu muốn chạy qua Command Line:*
```bash
sqlcmd -S <TEN_SERVER_LOCAL> -E -i docs/sql/recreate-clean-database.sql
```

---

## 5. Cách kiểm tra Database đã thiết lập đúng hay chưa

Sau khi chạy xong một trong hai cách trên, hãy mở SQL Server local của bạn và chạy câu lệnh SQL sau để kiểm tra:

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
ORDER BY TABLE_NAME;
```

### Kết quả mong muốn:
* **Chỉ hiển thị các bảng ở dạng SỐ ÍT (Singular):**
  ```text
  AppUser
  Role
  Race
  RaceEntry
  RaceResult
  Wallet
  WalletTransaction
  Notification
  ...
  ```
* **TUYỆT ĐỐI KHÔNG xuất hiện các bảng ở dạng SỐ NHIỀU (Plural) sau:**
  ```text
  Users
  Roles
  Races
  RaceEntries
  RaceResults
  Wallets
  WalletTransactions
  Notifications
  ```

---

## 6. Cách chạy Backend và Xác thực

Sau khi cấu hình DB hoàn tất, hãy khởi động API Backend:

```bash
cd backend
dotnet run --project src/HorseRacing.API -- --urls http://localhost:5001
```

Kiểm tra trạng thái kết nối trên trình duyệt:
1. **Swagger UI (Xem danh sách API):** [http://localhost:5001/swagger](http://localhost:5001/swagger)
2. **Health Check DB (Kiểm tra kết nối DB):** [http://localhost:5001/api/health/db](http://localhost:5001/api/health/db)
   * Kết quả mong muốn:
     ```json
     {
       "status": "success",
       "message": "Database connected successfully"
     }
     ```

---

## 7. Kết luận & Các bước cần làm sau khi Pull Code mới nhất

Mỗi lần bạn pull code mới từ nhánh chính (`main`), hãy thực hiện lần lượt các bước sau:

1. **Pull Code:** `git pull`
2. **Build Code:** `dotnet build` tại thư mục `backend`.
3. **Cập nhật Database (chọn 1 trong 2):**
   * Chạy lệnh cập nhật: `dotnet ef database update --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API`
   * Hoặc chạy SQL Script: [recreate-clean-database.sql](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/sql/recreate-clean-database.sql) nếu muốn dọn sạch DB hoàn toàn.
4. **Chạy App:** `dotnet run --project src/HorseRacing.API`
   * *Lưu ý:* Cơ chế **Runtime Seeding** tích hợp trong dự án sẽ tự động kiểm tra và chèn các tài khoản test (`admin`, `owner`, `jockey`, `referee`, `spectator` với mật khẩu mặc định `123456`) cùng ví, hồ sơ tương ứng vào DB local của bạn khi khởi chạy ứng dụng lần đầu mà không cần tác động thủ công.
