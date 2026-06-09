# Authorization Test Guide

Tài liệu này cung cấp hướng dẫn toàn diện và chi tiết để kiểm tra (test) hệ thống phân quyền (Role-based Authorization) và xác thực (JWT Authentication) trong dự án **Horse Racing Management System**.

---

## 1. Mục tiêu test phân quyền
Đảm bảo rằng:
1. Mọi người dùng đều phải đăng nhập để truy cập các tài nguyên được bảo vệ (trừ các endpoint public).
2. Quyền truy cập được thực thi chính xác dựa trên vai trò (Role) của người dùng:
   - **Admin** có toàn quyền quản trị (tạo tài khoản, lấy danh sách vai trò...).
   - **HorseOwner**, **Jockey**, **Referee**, **Spectator** chỉ được phép gọi các API thuộc phạm vi hoạt động của vai trò đó và bị chặn truy cập (403 Forbidden) khi gọi các API quản trị của Admin.
3. Token JWT khi giải mã (decode) chứa đầy đủ các Claim cần thiết (đặc biệt là Role Claim).

---

## 2. Danh sách role trong hệ thống

Hệ thống sử dụng cơ chế liên kết 1-1 giữa người dùng (`AppUser`) và vai trò (`Role`) thông qua khóa ngoại `RoleId`:

| RoleId | Tên Role | Mô tả |
| :---: | :--- | :--- |
| **1** | `Admin` | Quản trị viên hệ thống, có quyền tạo tài khoản và quản lý cấu hình. |
| **2** | `HorseOwner` | Chủ sở hữu ngựa, đăng ký và quản lý ngựa đua. |
| **3** | `Jockey` | Nài ngựa (kỵ sĩ), có hồ sơ kỵ sĩ (`JockeyProfile`). |
| **4** | `Referee` | Trọng tài, giám sát giải đua, có hồ sơ trọng tài (`RefereeProfile`). |
| **5** | `Spectator` | Người xem giải đua, có ví điện tử (`Wallet`) để tham gia dự đoán hoặc giao dịch. |

---

## 3. Danh sách tài khoản test (Proposed Data Seeding)

Hiện tại, hệ thống mới chỉ seed duy nhất tài khoản `Admin` trong [DataSeeder.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/DataSeeder.cs). Để kiểm tra đầy đủ cả 5 vai trò, chúng tôi đề xuất bổ sung thêm các tài khoản kiểm thử sau vào cấu hình seed dữ liệu phát triển (Development):

| Vai trò | Email | Mật khẩu | Trạng thái | Dữ liệu Hồ sơ đi kèm |
| :--- | :--- | :--- | :--- | :--- |
| **Admin** | `admin@gmail.com` | `123456` | Active | Không yêu cầu |
| **HorseOwner** | `owner@gmail.com` | `123456` | Active | Không yêu cầu |
| **Jockey** | `jockey@gmail.com` | `123456` | Active | `JockeyProfile` (3 năm kinh nghiệm, 100 điểm xếp hạng) |
| **Referee** | `referee@gmail.com` | `123456` | Active | `RefereeProfile` (Mã số: `LIC-REF-001`, 5 năm kinh nghiệm) |
| **Spectator** | `spectator@gmail.com` | `123456` | Active | `Wallet` (Số dư khởi tạo: `0`) |

> [!NOTE]
> *Các mật khẩu đều được băm bằng thuật toán mật mã `PasswordHasher<AppUser>` trong ASP.NET Core Identity để bảo mật.*

---

## 4. Cách chạy backend

Để khởi chạy backend ở môi trường phát triển (Development), sử dụng Git Bash và chạy các lệnh sau từ thư mục `backend/`:

```bash
# Thiết lập môi trường Development
export ASPNETCORE_ENVIRONMENT=Development

# Khởi chạy dự án API tại cổng 5001
dotnet run --project src/HorseRacing.API --urls "http://localhost:5001"
```

---

## 5. Cách mở Swagger

Sau khi chạy backend thành công, truy cập đường dẫn sau trên trình duyệt để sử dụng giao diện Swagger UI:
👉 **[http://localhost:5001/swagger](http://localhost:5001/swagger)**

---

## 6. Cách đăng nhập lấy token (Login)

Gửi yêu cầu đăng nhập bằng tài khoản cần kiểm thử đến endpoint `POST /api/auth/login`.

**Request Body mẫu:**
```json
{
  "email": "admin@gmail.com",
  "password": "123456"
}
```

**Response mẫu thành công (200 OK):**
```json
{
  "message": "Login successful",
  "result": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
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

> [!IMPORTANT]
> Sao chép toàn bộ chuỗi ký tự trong trường `result.accessToken` để chuẩn bị cho bước xác thực tiếp theo.

---

## 7. Cách Authorize bằng Bearer token trên Swagger

1. Tại giao diện Swagger UI, click vào nút **Authorize** (ở phía trên cùng bên phải).
2. Trong hộp thoại hiện ra, nhập vào ô Value theo cú pháp:
   ```text
   Bearer <accessToken_của_bạn>
   ```
   *Ví dụ: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`*
3. Nhấn **Authorize** rồi nhấn **Close**. Lúc này, tất cả các yêu cầu gửi đi từ Swagger sẽ tự động đính kèm Token trong Header `Authorization`.

---

## 8. Authorization Matrix (Ma trận phân quyền API)

Dưới đây là ma trận phân quyền thực tế dựa trên mã nguồn hiện tại của dự án:

| Endpoint | Method | Controller | AllowAnonymous | Admin | HorseOwner | Jockey | Referee | Spectator | Ghi chú |
| :--- | :---: | :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| `/api/auth/login` | `POST` | [AuthController](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AuthController.cs) | **Public** | ✅ | ✅ | ✅ | ✅ | ✅ | Đăng nhập lấy JWT Token |
| `/api/auth/register` | `POST` | [AuthController](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AuthController.cs) | **Public** | ✅ | ✅ | ✅ | ✅ | ✅ | Đăng ký công khai vai trò Spectator |
| `/api/health/db` | `GET` | [HealthController](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/HealthController.cs) | **Public** | ✅ | ✅ | ✅ | ✅ | ✅ | Kiểm tra kết nối cơ sở dữ liệu |
| `/api/admin/test` | `GET` | [AdminController](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs) | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | Kiểm tra phân quyền quản trị |
| `/api/admin/roles` | `GET` | [AdminController](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs) | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | Lấy danh sách các vai trò trong hệ thống |
| `/api/admin/accounts` | `POST` | [AdminController](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs) | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | Tạo tài khoản có vai trò đặc thù từ phía Admin |
| `/api/jockey/*` | - | JockeyController | - | - | - | - | - | - | *Trống (Chưa có endpoint hoạt động)* |
| `/api/owner/*` | - | OwnerController | - | - | - | - | - | - | *Trống (Chưa có endpoint hoạt động)* |
| `/api/public/*` | - | PublicController | - | - | - | - | - | - | *Trống (Chưa có endpoint hoạt động)* |
| `/api/referee/*` | - | RefereeController | - | - | - | - | - | - | *Trống (Chưa có endpoint hoạt động)* |
| `/api/spectator/*` | - | SpectatorController | - | - | - | - | - | - | *Trống (Chưa có endpoint hoạt động)* |

> [!NOTE]
> *Quy ước:*
> - ✅ = Người dùng có vai trò này được phép truy cập endpoint.
> - ❌ = Bị từ chối truy cập (HTTP 403 Forbidden).
> - **Public** = Endpoint không yêu cầu đăng nhập.

---

## 9. Test Case chi tiết cho từng endpoint

### Endpoint: `GET /api/admin/test`

| Test Case ID | Kịch bản / Vai trò | Đính kèm Token | HTTP Status Code mong đợi | Kết quả thực tế (Actual) |
| :--- | :--- | :--- | :---: | :--- |
| **TC-001** | Chưa đăng nhập | Không đính kèm | `401 Unauthorized` | *(Điền kết quả)* |
| **TC-002** | Đăng nhập tài khoản Spectator | Spectator Token | `403 Forbidden` | *(Điền kết quả)* |
| **TC-003** | Đăng nhập tài khoản Jockey | Jockey Token | `403 Forbidden` | *(Điền kết quả)* |
| **TC-004** | Đăng nhập tài khoản Referee | Referee Token | `403 Forbidden` | *(Điền kết quả)* |
| **TC-005** | Đăng nhập tài khoản HorseOwner | HorseOwner Token | `403 Forbidden` | *(Điền kết quả)* |
| **TC-006** | Đăng nhập tài khoản Admin | Admin Token | `200 OK` | *(Điền kết quả)* |

### Endpoint: `GET /api/admin/roles`

| Test Case ID | Kịch bản / Vai trò | Đính kèm Token | HTTP Status Code mong đợi | Kết quả thực tế (Actual) |
| :--- | :--- | :--- | :---: | :--- |
| **TC-007** | Chưa đăng nhập | Không đính kèm | `401 Unauthorized` | *(Điền kết quả)* |
| **TC-008** | Đăng nhập tài khoản Spectator | Spectator Token | `403 Forbidden` | *(Điền kết quả)* |
| **TC-009** | Đăng nhập tài khoản Admin | Admin Token | `200 OK` | *(Điền kết quả)* |

### Endpoint: `POST /api/admin/accounts`

| Test Case ID | Kịch bản / Vai trò | Đính kèm Token | HTTP Status Code mong đợi | Kết quả thực tế (Actual) |
| :--- | :--- | :--- | :---: | :--- |
| **TC-010** | Chưa đăng nhập | Không đính kèm | `401 Unauthorized` | *(Điền kết quả)* |
| **TC-011** | Đăng nhập tài khoản Spectator | Spectator Token | `403 Forbidden` | *(Điền kết quả)* |
| **TC-012** | Đăng nhập tài khoản Admin | Admin Token | `200 OK` | *(Điền kết quả)* |

---

## 10. Các lỗi thường gặp và cách xử lý nhanh

1. **401 Unauthorized (Không được xác thực):**
   - *Nguyên nhân:* Chưa gửi Token trong Header hoặc Token đã hết hạn, sai cấu trúc `Bearer <token>`.
   - *Cách xử lý:* Đăng nhập lại lấy token mới, đảm bảo trong Swagger đã ghi đúng tiền tố `Bearer ` trước chuỗi Token.

2. **403 Forbidden (Không đủ quyền hạn):**
   - *Nguyên nhân:* Token hợp lệ nhưng vai trò (Role Claim) không nằm trong danh sách các vai trò được cho phép bởi `[Authorize(Roles = "...")]`.
   - *Cách xử lý:* Đảm bảo tài khoản đăng nhập có chính xác tên role khớp với khai báo trên Controller (phân biệt chữ hoa/thường).

3. **500 Internal Server Error (Lỗi hệ thống bên trong):**
   - *Nguyên nhân:* Lỗi kết nối cơ sở dữ liệu hoặc xung đột dữ liệu bối cảnh (Context).
   - *Cách xử lý:* Kiểm tra kết nối DB qua `/api/health/db` hoặc xem nhật ký log của Server để gỡ lỗi.

4. **Token không chứa Role Claim:**
   - *Nguyên nhân:* Hàm tạo Token không lấy chính xác trường tên Role từ cơ sở dữ liệu.
   - *Cách xử lý:* Kiểm tra hàm sinh token trong `JwtTokenGenerator` đã thực hiện nạp dữ liệu quan hệ (`.Include(u => u.Role)`) từ cơ sở dữ liệu trước khi lấy `user.Role.Name` chưa.

5. **Sai thứ tự Middleware trong `Program.cs`:**
   - *Nguyên nhân:* `UseAuthorization()` được đặt trước `UseAuthentication()`.
   - *Cách xử lý:* Phải đảm bảo thứ tự luôn là:
     ```csharp
     app.UseAuthentication();
     app.UseAuthorization();
     ```

---

## 11. Checklist kiểm thử nhanh (Quick Check)

- [ ] Login Admin thành công (`admin@gmail.com` / `123456`)
- [ ] Login HorseOwner thành công
- [ ] Login Jockey thành công
- [ ] Login Referee thành công
- [ ] Login Spectator thành công
- [ ] Token Admin gọi thành công API quản trị (`GET /api/admin/test` -> `200 OK`)
- [ ] Token các vai trò khác bị chặn khi gọi API quản trị (`403 Forbidden`)
- [ ] Gọi API được bảo vệ khi không đăng nhập bị chặn (`401 Unauthorized`)
- [ ] Cấu hình Swagger Bearer đã hiển thị nút Authorize hoạt động tốt
- [ ] Token JWT được sinh ra chứa đúng Claim `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` ứng với tên vai trò.
