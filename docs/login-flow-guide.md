# Login Flow Guide - Horse Racing Management System

## 1. Mục đích tài liệu
Tài liệu này được biên soạn nhằm giúp các thành viên trong đội ngũ phát triển dự án SWP391 hiểu rõ kiến trúc, luồng đi của dữ liệu, và cách thức hoạt động của chức năng **Đăng nhập (Login)** sử dụng mã xác thực JWT trong hệ thống. Tài liệu cũng hướng dẫn chi tiết cách chạy và kiểm thử chức năng này dưới local.

---

## 2. Tổng quan chức năng Login
* **Mục đích:** Xác thực danh tính người dùng (Jockey, Referee, Admin, Spectator, Owner) dựa trên email và mật khẩu. Khi đăng nhập thành công, hệ thống sẽ cấp một thẻ bài truy cập (JWT Access Token) để người dùng thực hiện các thao tác yêu cầu phân quyền tiếp theo.
* **Gửi request:** Client (Frontend/Swagger/Postman) gửi một yêu cầu HTTP POST chứa JSON gồm `email` và `password` tới API của Backend.
* **Kiểm tra User:** Backend truy vấn cơ sở dữ liệu SQL Server thông qua `AppDbContext` để tìm tài khoản có email trùng khớp.
* **Kiểm tra Password:** Mật khẩu trong DB đã được mã hóa (hashing). Backend sử dụng thư viện `PasswordHasher<AppUser>` để so khớp mật khẩu người dùng gửi lên với chuỗi băm lưu trữ.
* **Response trả về:** Nếu khớp, backend sinh ra một chuỗi JWT Access Token chứa các thông tin cơ bản (Claims) như ID, Email, Role và trả về HTTP 200 kèm thông tin user. Nếu sai mật khẩu hoặc không tìm thấy user, hệ thống trả về HTTP 401 Unauthorized.
* **Vai trò của JWT:** JWT (JSON Web Token) giúp bảo mật phi trạng thái (stateless). Máy chủ không cần lưu phiên đăng nhập (session), thay vào đó client sẽ gửi kèm Token này ở header `Authorization: Bearer <Token>` trong mỗi yêu cầu gửi lên sau này để chứng minh quyền truy cập.

---

## 3. API Login
### Endpoint:
```http
POST /api/auth/login
```

### Request Body mẫu:
```json
{
  "email": "admin@gmail.com",
  "password": "123456"
}
```

### Response thành công mẫu (HTTP 200 OK):
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
*(Lưu ý: Trường `refreshToken` hiện tại được gán mặc định là `null` do hệ thống chưa triển khai cơ chế làm mới Token).*

### Response thất bại mẫu (HTTP 401 Unauthorized):
```json
{
  "message": "Invalid email or password"
}
```

---

## 4. Danh sách file liên quan đến Login

| Tên File | Thêm mới / Chỉnh sửa | Vai trò | Vì sao cần |
| :--- | :--- | :--- | :--- |
| **LoginRequest.cs** | Chỉnh sửa | DTO (Data Transfer Object) nhận dữ liệu đầu vào | Chứa cấu trúc email và password gửi lên từ Client. |
| **AuthResponse.cs** | Chỉnh sửa | DTO định nghĩa cấu trúc dữ liệu trả về | Đảm bảo định dạng đầu ra chuẩn bao gồm Token và thông tin User. |
| **IAuthService.cs** | Thêm mới | Định nghĩa Interface cho dịch vụ xác thực | Tạo ranh giới lỏng lẻo (Loose Coupling) cho việc triển khai logic Đăng nhập. |
| **IUserRepository.cs**| Chỉnh sửa | Định nghĩa Interface truy vấn bảng người dùng | Định nghĩa cách thức tìm kiếm User theo Email hoặc ID mà không phụ thuộc DB cụ thể. |
| **IJwtTokenGenerator.cs** | Thêm mới | Định nghĩa Interface sinh chuỗi mã JWT | Trừu tượng hóa dịch vụ bảo mật sinh Token. |
| **AuthService.cs** | Chỉnh sửa | Xử lý logic xác thực và băm mật khẩu | Thực hiện tìm kiếm user, so khớp pass và gọi sinh token. |
| **UserRepository.cs** | Chỉnh sửa | Triển khai truy cập cơ sở dữ liệu thực tế | Chứa các truy vấn Entity Framework Core truy xuất bảng `Users`. |
| **JwtTokenGenerator.cs**| Thêm mới | Triển khai thuật toán mã hóa khóa JWT | Đọc cấu hình khóa bí mật từ file config, đính kèm claims và ký số SHA256. |
| **DependencyInjection.cs** | Chỉnh sửa | Đăng ký các dịch vụ tầng Infrastructure vào DI | Đăng ký UserRepository, DbContext và JwtTokenGenerator. |
| **ServiceExtensions.cs** | Chỉnh sửa | Đăng ký các dịch vụ tầng Application vào DI | Đăng ký AuthService để API có thể Inject vào Controller. |
| **AuthController.cs** | Thêm mới | Tiếp nhận yêu cầu HTTP đầu tiên từ Client | Tiếp nhận Request Body, gọi AuthService và trả kết quả HTTP thích hợp. |
| **Program.cs** | Chỉnh sửa | Thiết lập cấu hình ứng dụng, Pipeline & JWT | Khai báo xác thực Bearer token, thứ tự gọi Middleware và chạy ứng dụng. |
| **appsettings.json** | Chỉnh sửa | Cấu hình tham số JWT (Khóa, Issuer, Audience) | Lưu cấu hình bảo mật phục vụ mọi môi trường. |
| **appsettings.Development.json** | Chỉnh sửa | Cấu hình tham số JWT cho local development | Chứa connection string local tới SQL Server và cấu hình JWT. |
| **DataSeeder.cs** | Chỉnh sửa | Định nghĩa tài khoản Admin ban đầu | Tạo người dùng mặc định có mật khẩu mã hóa sẵn khi chạy Migration. |
| **AppDbContext.cs** | Chỉnh sửa | Cấu hình thực thể CSDL & Gọi Seed Data | Kết nối các Entity tới bảng tương ứng và cấu hình Delete behavior. |

---

## 5. Giải thích từng file

### LoginRequest.cs
* **Vị trí file:** `backend/src/HorseRacing.Application/Features/UserManagement/DTOs/LoginRequest.cs`
* **Mục đích:** Là lớp dữ liệu thô (DTO) dùng để giải mã JSON từ API Client gửi lên.
* **Tham gia bước nào:** Nhận request đầu vào ở `AuthController.Login()`.
* **Đoạn code quan trọng:**
  ```csharp
  public class LoginRequest
  {
      public string Email { get; set; } = string.Empty;
      public string Password { get; set; } = string.Empty;
  }
  ```
* **Ý nghĩa:** Chỉ bao gồm 2 thuộc tính cơ bản là Email và Password. 
* **Khi nào cần sửa:** Khi bạn muốn thêm các trường thông tin đăng nhập khác như mã OTP, reCAPTCHA hoặc đăng nhập bằng Username thay vì Email.

---

### AuthResponse.cs
* **Vị trí file:** `backend/src/HorseRacing.Application/Features/UserManagement/DTOs/AuthResponse.cs`
* **Mục đích:** Định nghĩa cấu trúc JSON chuẩn trả về cho phía Client.
* **Tham gia bước nào:** Trả về kết quả cuối cùng từ `AuthService.LoginAsync()` ra ngoài controller.
* **Đoạn code quan trọng:**
  ```csharp
  public class AuthResponse
  {
      public string Message { get; set; } = string.Empty;
      public AuthResult Result { get; set; } = null!;
  }
  ```
* **Ý nghĩa:** Bao bọc mã thông báo JWT (`accessToken`), token làm mới (`refreshToken`), và các claims cơ bản của người dùng (`UserDto`).
* **Khi nào cần sửa:** Khi frontend yêu cầu bổ sung thông tin hiển thị ngay sau khi đăng nhập (ví dụ: Avatar URL, số dư Wallet).

---

### IAuthService.cs
* **Vị trí file:** `backend/src/HorseRacing.Application/Features/UserManagement/Interfaces/IAuthService.cs`
* **Mục đích:** Định nghĩa hợp đồng (contract) của dịch vụ xác thực.
* **Tham gia bước nào:** Làm cầu nối giao tiếp Dependency Injection cho `AuthController`.
* **Đoạn code quan trọng:**
  ```csharp
  public interface IAuthService
  {
      Task<AuthResponse?> LoginAsync(LoginRequest request);
  }
  ```
* **Ý nghĩa:** Khai báo phương thức đăng nhập bất đồng bộ nhận `LoginRequest` và trả về `AuthResponse` (hoặc null nếu lỗi).
* **Khi nào cần sửa:** Khi cần thêm nghiệp vụ như đăng ký tài khoản (Register), đổi mật khẩu, hoặc xác thực đa yếu tố.

---

### IUserRepository.cs
* **Vị trí file:** `backend/src/HorseRacing.Application/Features/UserManagement/Interfaces/IUserRepository.cs`
* **Mục đích:** Định nghĩa giao diện truy xuất dữ liệu đối với thực thể AppUser.
* **Tham gia bước nào:** Được `AuthService` gọi để truy vấn thông tin User từ cơ sở dữ liệu.
* **Đoạn code quan trọng:**
  ```csharp
  public interface IUserRepository
  {
      Task<AppUser?> GetByEmailAsync(string email);
      Task AddAsync(AppUser user);
      Task SaveChangesAsync();
  }
  ```
* **Ý nghĩa:** Định nghĩa các phương thức lấy AppUser bằng Email, thêm AppUser và lưu thay đổi vào cơ sở dữ liệu.
* **Khi nào cần sửa:** Khi cần thêm các nghiệp vụ truy vấn phức tạp liên quan đến người dùng (tìm kiếm nâng cao, lấy danh sách theo Role).

---

### IJwtTokenGenerator.cs
* **Vị trí file:** `backend/src/HorseRacing.Application/Common/Interfaces/IJwtTokenGenerator.cs`
* **Mục đích:** Giao diện sinh chuỗi JWT.
* **Tham gia bước nào:** Được `AuthService` gọi để lấy chuỗi Token sau khi người dùng khớp mật khẩu.
* **Đoạn code quan trọng:**
  ```csharp
  public interface IJwtTokenGenerator
  {
      string GenerateToken(AppUser user);
  }
  ```
* **Ý nghĩa:** Cho phép tầng Application yêu cầu sinh Token mà không cần quan tâm tầng Infrastructure dùng thuật toán mã hóa nào.
* **Khi nào cần sửa:** Hầu như không cần sửa trừ khi có yêu cầu thay đổi tham số đầu vào.

---

### AuthService.cs
* **Vị trí file:** `backend/src/HorseRacing.Application/Features/UserManagement/Services/AuthService.cs`
* **Mục đích:** Chứa logic nghiệp vụ cốt lõi của việc Đăng nhập.
* **Tham gia bước nào:** Thực hiện xử lý chính từ sau khi nhận request tới trước khi trả response.
* **Đoạn code quan trọng:**
  ```csharp
  var user = await _userRepository.GetByEmailAsync(request.Email);
  if (user == null) return null;

  var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
  if (result == PasswordVerificationResult.Failed) return null;

  var token = _jwtTokenGenerator.GenerateToken(user);
  ```
* **Ý nghĩa:** 
  1. Tìm kiếm user theo email thông qua Repository.
  2. Dùng `PasswordHasher<AppUser>` để so sánh mật khẩu thô của client với chuỗi băm.
  3. Gọi JwtTokenGenerator sinh mã token và trả về DTO kết quả.
* **Khi nào cần sửa:** Khi thay đổi logic đăng nhập (ví dụ: khóa tài khoản nếu nhập sai 5 lần, hoặc tích hợp thêm phân quyền động).

---

### UserRepository.cs
* **Vị trí file:** `backend/src/HorseRacing.Infrastructure/Repositories/UserRepository.cs`
* **Mục đích:** Thực hiện các truy vấn dữ liệu thực tế thông qua Entity Framework Core.
* **Tham gia bước nào:** Truy vấn CSDL SQL Server ở tầng lưu trữ.
* **Đoạn code quan trọng:**
  ```csharp
  public async Task<AppUser?> GetByEmailAsync(string email)
  {
      return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
  }
  ```
* **Ý nghĩa:** Sử dụng DbContext để lấy bản ghi đầu tiên có Email khớp từ bảng `Users`.
* **Khi nào cần sửa:** Khi muốn tối ưu hóa truy vấn CSDL hoặc thay đổi cách thức lấy dữ liệu (ví dụ: dùng Dapper thay cho EF Core).

---

### JwtTokenGenerator.cs
* **Vị trí file:** `backend/src/HorseRacing.Infrastructure/ExternalServices/JwtTokenGenerator.cs`
* **Mục đích:** Thực hiện sinh chuỗi token JWT thực tế.
* **Tham gia bước nào:** Sinh chuỗi mã ở cuối luồng xử lý của AuthService.
* **Đoạn code quan trọng:**
  ```csharp
  var jwtSettings = _configuration.GetSection("Jwt");
  var secretKey = jwtSettings["Key"];
  var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
  var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
  ```
* **Ý nghĩa:** 
  1. Đọc cấu hình từ `IConfiguration`.
  2. Tạo khóa mã hóa đối xứng từ Secret Key.
  3. Ký số số hóa dựa trên thuật toán HMAC-SHA256 để chống giả mạo token.
* **Khi nào cần sửa:** Khi muốn thay đổi các Claim mặc định lưu trong Token (ví dụ: thêm Username, thêm quyền hạn chi tiết).

---

### AuthController.cs
* **Vị trí file:** `backend/src/HorseRacing.API/Controllers/AuthController.cs`
* **Mục đích:** Tiếp nhận và trả phản hồi API bên ngoài.
* **Tham gia bước nào:** Điểm bắt đầu và kết thúc của yêu cầu HTTP từ client.
* **Đoạn code quan trọng:**
  ```csharp
  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest request)
  {
      var response = await _authService.LoginAsync(request);
      if (response == null)
      {
          return Unauthorized(new { message = "Invalid email or password" });
      }
      return Ok(response);
  }
  ```
* **Ý nghĩa:** Nếu `LoginAsync` trả về `null` (không khớp user/pass), nó trả về mã lỗi HTTP 401. Nếu thành công, trả về HTTP 200 kèm DTO Login.
* **Khi nào cần sửa:** Khi muốn bổ sung validation ở đầu vào (ví dụ kiểm tra định dạng email) hoặc điều chỉnh cấu trúc mã lỗi trả về.

---

### Program.cs
* **Vị trí file:** `backend/src/HorseRacing.API/Program.cs`
* **Mục đích:** Khởi tạo ứng dụng và thiết lập Middleware.
* **Tham gia bước nào:** Cấu hình Middleware chạy trước và sau khi truy cập API.
* **Đoạn code quan trọng:**
  ```csharp
  builder.Services.AddAuthentication(options => { ... }).AddJwtBearer(options => { ... });
  ...
  app.UseAuthentication();
  app.UseAuthorization();
  ```
* **Ý nghĩa:** Kích hoạt hệ thống xác thực JWT Bearer của ASP.NET Core, kiểm tra tính hợp lệ của Token trước khi cho phép vào các API được bảo vệ.
* **Khi nào cần sửa:** Khi muốn đổi phương thức xác thực hoặc tùy chỉnh tham số kiểm tra token (ví dụ: tắt kiểm tra thời gian hết hạn khi phát triển).

---

### DataSeeder.cs
* **Vị trí file:** `backend/src/HorseRacing.Infrastructure/Persistence/DataSeeder.cs`
* **Mục đích:** Đảm bảo hệ thống luôn có sẵn tài khoản kiểm thử khi khởi động dự án.
* **Tham gia bước nào:** Chạy trong quá trình EF Migration cập nhật DB (`dotnet ef database update`).
* **Đoạn code quan trọng:**
  ```csharp
  var hasher = new PasswordHasher<AppUser>();
  var admin = new AppUser { Id = 1, Email = "admin@gmail.com", Role = "Admin" ... };
  admin.PasswordHash = hasher.HashPassword(admin, "123456");
  modelBuilder.Entity<AppUser>().HasData(admin);
  ```
* **Ý nghĩa:** Khởi tạo dữ liệu cứng cho tài khoản Admin ban đầu, mật khẩu `123456` được băm tự động bằng thuật toán bảo mật cao rồi lưu vào DB.
* **Khi nào cần sửa:** Khi cần thêm các tài khoản kiểm thử cho các Role khác (Jockey, Referee, Spectator) để test phân quyền.

---

## 6. Luồng Login từ đầu đến cuối
Luồng đi của dữ liệu được diễn ra theo tuần tự sau:

1. **Client (Postman/Swagger/Web UI)** gửi yêu cầu POST dạng JSON đến địa chỉ: `/api/auth/login`.
2. **AuthController** tiếp nhận request body và chuyển đổi JSON thành đối tượng **LoginRequest DTO**.
3. **AuthController** gọi hàm `_authService.LoginAsync(request)` từ tầng Application.
4. **AuthService** gọi `_userRepository.GetByEmailAsync(request.Email)` để truy vấn thông tin người dùng.
5. **UserRepository** kết nối qua **AppDbContext** để tìm bản ghi có email tương ứng trong bảng `Users` trên SQL Server.
6. **AuthService** nhận kết quả người dùng:
   - Nếu không có, dừng luồng đăng nhập và trả về `null`.
   - Nếu có, gọi `PasswordHasher.VerifyHashedPassword()` để kiểm tra độ khớp mật khẩu.
7. Nếu mật khẩu không trùng khớp, trả về `null`.
8. Nếu mật khẩu trùng khớp, **AuthService** gọi `_jwtTokenGenerator.GenerateToken(user)` để tạo một chuỗi mã JWT Token hợp lệ chứa các Claim thông tin của user.
9. **AuthService** đóng gói Token và thông tin User cơ bản vào đối tượng **AuthResponse DTO** rồi trả về cho Controller.
10. **AuthController** nhận kết quả:
    - Nếu kết quả là `null`: Trả về lỗi `401 Unauthorized` kèm thông điệp `"Invalid email or password"`.
    - Nếu kết quả hợp lệ: Trả về HTTP `200 OK` kèm theo toàn bộ dữ liệu JSON chứa Access Token cho Client.

---

## 7. Sơ đồ flow dạng text
```text
POST /api/auth/login (Client gửi JSON)
        |
        v
AuthController.Login(LoginRequest)  <-- Nhận DTO đầu vào
        |
        v
AuthService.LoginAsync(LoginRequest) <-- Logic xác thực chính
        |
        +---> UserRepository.GetByEmailAsync()
        |           |
        |           v
        |     AppDbContext (Query SQL Server) <-- Lấy AppUser từ bảng Users
        |
        v
PasswordHasher.VerifyHashedPassword() <-- So khớp mật khẩu đã băm
        |
        v (Mật khẩu đúng)
JwtTokenGenerator.GenerateToken()    <-- Tạo chuỗi JWT Access Token từ appsettings.json
        |
        v
Trả về AuthResponse DTO
        |
        v
AuthController trả về HTTP 200 OK (hoặc HTTP 401 nếu sai user/pass)
```

---

## 8. Giải thích JWT trong project này
* **JWT là gì:** JWT (JSON Web Token) là định dạng token bảo mật gồm 3 phần phân cách bởi dấu chấm: `Header.Payload.Signature`. Nó lưu trữ thông tin dưới dạng JSON mã hóa Base64 và được ký số để đảm bảo tính toàn vẹn dữ liệu.
* **Access token là gì:** Là chuỗi token JWT dùng để đại diện cho phiên làm việc của người dùng. Mỗi khi truy cập API cần bảo mật, Client phải gửi token này lên.
* **Nơi sinh Token:** Được sinh ra tại lớp `JwtTokenGenerator` nằm ở tầng Infrastructure.
* **Secret Key:** Lấy từ cấu hình bảo mật ở phần `"Jwt:Key"` trong file `appsettings.json`. Nó có độ dài đủ an toàn (hơn 256 bits) để sử dụng thuật toán HMAC-SHA256.
* **Issuer (Nhà phát hành):** `HorseRacingAPI` (Mô tả máy chủ phát hành token).
* **Audience (Người sử dụng):** `HorseRacingSpectator` (Mô tả đối tượng được phép sử dụng token).
* **ExpiresInMinutes:** Thời hạn hiệu lực của token. Được thiết lập là `60` phút. Sau 60 phút token sẽ bị mất hiệu lực và client cần đăng nhập lại.
* **Sử dụng ở Frontend:** Sau khi lưu Token vào LocalStorage hoặc Cookie, Frontend sẽ gán nó vào header của mọi yêu cầu HTTP tiếp theo:
  ```http
  Authorization: Bearer <accessToken>
  ```
* **Tại sao không nên hard-code key:** Nếu hard-code key trực tiếp trong code C#, khi kẻ xấu lấy được mã nguồn sẽ dễ dàng sinh ra các Token giả mạo có quyền Admin để tấn công hệ thống. Việc lưu key ở `appsettings.json` giúp quản lý bảo mật và thay đổi linh hoạt theo từng môi trường chạy (Development/Production).

---

## 9. Tài khoản test Login
Hệ thống đã cấu hình seed data sẵn tài khoản Admin dùng để test chức năng như sau:
* **Email:** `admin@gmail.com`
* **Mật khẩu thô:** `123456`
* **Role:** `Admin`
* **Vị trí cấu hình seeder:** Nằm tại [DataSeeder.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/DataSeeder.cs).
* **Password Hashing:** Có. Mật khẩu không bao giờ được lưu dạng chữ thường mà được băm bằng thuật toán `PasswordHasher` tiêu chuẩn.
* **Thời điểm chạy seeder:** Khi bạn thực thi lệnh `database update` lần đầu tiên của Entity Framework Core.
* **Cách kiểm tra trực quan:** Bạn mở SQL Server truy vấn bảng `Users` sẽ thấy bản ghi:
  ```sql
  SELECT * FROM Users WHERE Email = 'admin@gmail.com';
  ```

---

## 10. Cách test Login
Hãy thực hiện theo đúng các bước sau đây để chạy thử nghiệm:

### Bước 1: Khởi động Backend
Mở Git Bash tại thư mục `backend/` của dự án và chạy:
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project src/HorseRacing.API --urls "http://localhost:5001"
```

### Bước 2: Mở Swagger UI trên trình duyệt
Truy cập địa chỉ: [http://localhost:5001/swagger](http://localhost:5001/swagger)

### Bước 3: Thực thi đăng nhập
1. Tìm api màu xanh lá `POST /api/Auth/login`.
2. Click **Try it out**.
3. Điền JSON Request Body:
   ```json
   {
     "email": "admin@gmail.com",
     "password": "123456"
   }
   ```
4. Click **Execute**.

### Bước 4: Đọc kết quả
* **Thành công:** Trả về mã HTTP `200 OK` và hiển thị chi tiết Token cùng thông tin User.
* **Sai mật khẩu:** Trả về mã HTTP `401 Unauthorized` kèm JSON: `{"message": "Invalid email or password"}`.
* **Sai Email:** Trả về tương tự như sai mật khẩu (để bảo mật thông tin tài khoản, tránh việc hacker quét dò tìm các email tồn tại trong hệ thống).
* **Lỗi Server (HTTP 500):** Cần kiểm tra cửa sổ log Console của ứng dụng xem có lỗi kết nối database hay lỗi cấu hình JWT hay không.

---

## 11. Các lỗi thường gặp khi test Login

| Tên Lỗi | Nguyên nhân | Cách xử lý |
| :--- | :--- | :--- |
| **HTTP ERROR 404** khi vào Swagger | Dự án đang chạy mặc định ở môi trường `Production`, làm ẩn Swagger UI. | Đảm bảo chạy lệnh `export ASPNETCORE_ENVIRONMENT=Development` trước khi `dotnet run`. |
| **401 Unauthorized** | Sai email hoặc sai mật khẩu. | Đảm bảo nhập đúng tài khoản: `admin@gmail.com` / `123456`. |
| **500 Internal Server Error** | Lỗi runtime phát sinh bên trong ứng dụng. | Xem chi tiết log hiển thị trên màn hình console terminal của dotnet để gỡ lỗi. |
| **Cannot connect database** | SQL Server local chưa bật hoặc database chưa được tạo. | Kiểm tra dịch vụ SQL Server đang chạy và bạn đã tạo db `HorseRacingManagementSystem`. |
| **JWT config missing** | Thiếu cấu hình `Jwt:Key` trong appsettings.json. | Đảm bảo không thay đổi hoặc xóa phần `"Jwt"` trong file cấu hình json. |
| **Locked file / Port in use** | Có tiến trình dotnet khác đang chạy ẩn và giữ file `.dll`/port. | Chạy lệnh `taskkill -F -IM dotnet.exe` trong cmd/terminal để giải phóng. |

---

## 12. Nên đọc file nào trước để hiểu Login?
Để tiếp cận một cách dễ hiểu nhất, hãy đọc các file theo thứ tự từ dễ đến khó sau đây:

1. [LoginRequest.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/DTOs/LoginRequest.cs) (Hiểu cấu trúc dữ liệu gửi lên).
2. [AuthResponse.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/DTOs/AuthResponse.cs) (Hiểu cấu trúc dữ liệu trả về).
3. [AuthController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AuthController.cs) (Xem điểm tiếp nhận HTTP API).
4. [IAuthService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/Interfaces/IAuthService.cs) (Xem hợp đồng nghiệp vụ).
5. [AuthService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/Services/AuthService.cs) (Xem luồng xử lý so khớp và băm mật khẩu).
6. [IJwtTokenGenerator.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Common/Interfaces/IJwtTokenGenerator.cs) và [JwtTokenGenerator.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/ExternalServices/JwtTokenGenerator.cs) (Tìm hiểu cách tạo Token JWT).
7. [IUserRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/Interfaces/IUserRepository.cs) và [UserRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/UserRepository.cs) (Cách viết Repository lấy dữ liệu).
8. [DataSeeder.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/DataSeeder.cs) (Xem cách tạo tài khoản test mã hóa).
9. [Program.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Program.cs) (Cách đăng ký middleware bảo mật JWT).

---

## 13. Login hiện tại đã làm được gì và còn thiếu gì?

### Đã làm được:
* Đăng ký Authentication và cấu hình Middleware JWT Bearer hoàn chỉnh trong `Program.cs`.
* Chức năng đăng nhập hoạt động tốt, mã hóa và đối sánh mật khẩu bảo mật qua `PasswordHasher`.
* Đã tự động sinh JWT Access Token chứa các Claims thông tin người dùng thực tế (`Id`, `Email`, `Username`, `Role`).
* Tự động seed tài khoản admin mẫu vào Database khi chạy migration.
* Sửa lỗi khóa ngoại cascade delete của cơ sở dữ liệu SQL Server.

### Còn thiếu và các bước tiếp theo cần triển khai:
* **Register (Đăng ký tài khoản):** Chưa làm API đăng ký để người dùng tạo tài khoản mới tự do.
* **Get Current Profile:** Chưa làm API cho phép lấy thông tin chi tiết của user hiện tại bằng việc giải mã Token gửi kèm.
* **Role Authorization (Phân quyền API):** Chưa áp dụng từ khóa `[Authorize(Roles = "Admin")]` lên các Controller chức năng cụ thể để kiểm tra thực tế token có chặn được truy cập trái phép không.
* **Refresh Token:** Hiện tại `refreshToken` trả về `null`. Cần lưu Refresh Token vào Database để hỗ trợ gia hạn phiên đăng nhập khi Access Token (60 phút) hết hạn mà không cần bắt user nhập lại mật khẩu.
* **Logout / Change Password:** Chưa hỗ trợ vô hiệu hóa token và cập nhật lại chuỗi băm mật khẩu mới.
