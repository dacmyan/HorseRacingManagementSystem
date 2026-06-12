# Hướng Dẫn Seeding Dữ Liệu và Sửa Lỗi PendingModelChanges

Tài liệu này giải thích cấu trúc seeding dữ liệu mới của hệ thống và hướng dẫn các thành viên trong team cách cập nhật môi trường local sau khi pull code mới nhất.

---

## 1. Vấn Đề Gặp Phải (PendingModelChanges)
Trước đây, trong `DataSeeder.cs`, chúng ta sử dụng `HasData` của EF Core để seed tĩnh các tài khoản test (`admin`, `owner`, `jockey`, `referee`, `spectator`) kèm theo việc băm mật khẩu:
```csharp
var hasher = new PasswordHasher<AppUser>();
user.PasswordHash = hasher.HashPassword(user, "123456");
```
Do `PasswordHasher` tạo salt ngẫu nhiên mỗi lần chạy, EF Core nhận thấy model snapshot của database bị thay đổi liên tục so với trạng thái biên dịch trước đó. Điều này gây ra lỗi `PendingModelChanges` cho đồng đội khi build/chạy ứng dụng hoặc cố gắng tạo migration mới.

---

## 2. Giải Pháp: Tách Biệt Tĩnh và Động (Runtime Seeding)

Chúng ta đã tách cơ chế seeding thành 2 phần độc lập:

1. **Static Seeding (EF Core `HasData`):**
   - Chỉ giữ lại seed dữ liệu tĩnh không bao giờ thay đổi ngẫu nhiên, cụ thể là bảng `Role` (`Admin`, `HorseOwner`, `Jockey`, `Referee`, `Spectator`).
   - Tệp sửa đổi: [DataSeeder.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/DataSeeder.cs).

2. **Runtime Seeding (`DatabaseSeeder`):**
   - Chuyển toàn bộ việc khởi tạo tài khoản test (`AppUser`), hồ sơ (`JockeyProfile`, `RefereeProfile`), và ví (`Wallet`) sang thời điểm khởi chạy ứng dụng (Application Startup).
   - Tệp mới: [DatabaseSeeder.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/DatabaseSeeder.cs).
   - Dữ liệu mật khẩu sẽ được băm động tại runtime một lần duy nhất khi tạo user.
   - Cơ chế hoạt động: Khi ứng dụng khởi chạy, `DatabaseSeeder` sẽ kiểm tra xem tài khoản đã tồn tại trong database (qua `Email` hoặc `Username`) hay chưa. Nếu chưa tồn tại, nó mới thực hiện thêm mới tài khoản và các dữ liệu liên quan. Nếu đã tồn tại, nó sẽ bỏ qua để tránh ghi đè dữ liệu test hiện tại của bạn.

---

## 3. Cách Cấu Hình Trong Codebase

* **Đăng ký Service:** `DatabaseSeeder` được đăng ký dưới dạng Scoped Service trong [DependencyInjection.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/DependencyInjection.cs).
* **Khởi chạy khi Startup:** Trong [Program.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Program.cs), trước lệnh `app.Run()`, một scope tạm thời được tạo ra để giải quyết dịch vụ `DatabaseSeeder` và gọi phương thức `SeedAsync()`.

---

## 4. Hướng Dẫn Dành Cho Thành Viên Team Khi Pull Code Mới

Khi bạn pull code mới từ branch `fix/db-singular-table-mapping` (hoặc sau khi branch này được merge vào `main`), bạn hãy làm theo các bước sau để cập nhật local database:

### Bước 1: Pull code mới nhất
```bash
git pull
```

### Bước 2: Cập nhật Migration cho Database
Chúng ta đã thêm migration `FixRuntimeUserSeeding` để dọn dẹp các bản ghi seed tĩnh của user cũ khỏi EF Core tracking. Hãy cập nhật database local của bạn:
```bash
dotnet ef database update --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API
```

*(Lưu ý: Nếu bạn muốn reset toàn bộ database local về trạng thái sạch hoàn toàn, hãy sử dụng hướng dẫn trong [team-db-reset-guide.md](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/team-db-reset-guide.md)).*

### Bước 3: Chạy ứng dụng
Khởi động ứng dụng backend:
```bash
dotnet run --project src/HorseRacing.API
```
Tại thời điểm ứng dụng khởi chạy, hệ thống sẽ tự động chèn các tài khoản test sau nếu chúng chưa có trong DB của bạn:
* **Admin:** `admin` / `123456`
* **HorseOwner:** `owner` / `123456`
* **Jockey:** `jockey` / `123456` (Tự tạo JockeyProfile)
* **Referee:** `referee` / `123456` (Tự tạo RefereeProfile)
* **Spectator:** `spectator` / `123456` (Tự tạo Wallet với số dư = 0)
