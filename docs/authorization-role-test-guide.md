# Authorization Role Test Guide

Tài liệu này cung cấp hướng dẫn chi tiết để kiểm tra (test) hệ thống phân quyền (Role-based Authorization) và xác thực (JWT Authentication) đối với tất cả 5 vai trò hệ thống trong **Horse Racing Management System**.

---

## 1. Mục tiêu
- Xác minh rằng cả 5 vai trò hệ thống (`Admin`, `HorseOwner`, `Jockey`, `Referee`, `Spectator`) đều có thể đăng nhập thành công.
- Đảm bảo token JWT được tạo ra chứa đầy đủ các Claim thông tin người dùng và Claim vai trò chính xác.
- Kiểm tra tính bảo mật của các API: chỉ cho phép người dùng có vai trò phù hợp truy cập, trả về đúng mã trạng thái HTTP mong đợi (`200 OK`, `401 Unauthorized`, `403 Forbidden`).

---

## 2. Role chuẩn của hệ thống
Hệ thống sử dụng các tên vai trò chuẩn hóa sau (phân biệt chữ hoa/chữ thường):
- `Admin` - Quản trị viên hệ thống.
- `HorseOwner` - Chủ sở hữu ngựa.
- `Jockey` - Nài ngựa (kỵ sĩ).
- `Referee` - Trọng tài.
- `Spectator` - Người xem giải đua.

---

## 3. Tài khoản test từng role
Các tài khoản sau đã được seed mặc định trong cơ sở dữ liệu phát triển:

| Vai trò | Email | Mật khẩu | Trạng thái | Dữ liệu hồ sơ đi kèm |
| :--- | :--- | :--- | :--- | :--- |
| **Admin** | `admin@gmail.com` | `123456` | Active | Không có |
| **HorseOwner** | `owner@gmail.com` | `123456` | Active | Không có |
| **Jockey** | `jockey@gmail.com` | `123456` | Active | `JockeyProfile` (3 năm kinh nghiệm, 100 điểm xếp hạng) |
| **Referee** | `referee@gmail.com` | `123456` | Active | `RefereeProfile` (Mã số: `LIC-REF-001`, 5 năm kinh nghiệm) |
| **Spectator** | `spectator@gmail.com` | `123456` | Active | `Wallet` (Số dư: `0`) |

---

## 4. Cách chạy backend
Để chạy backend trong Git Bash, sử dụng lệnh sau từ thư mục `backend/`:

```bash
dotnet run --project src/HorseRacing.API -- --urls http://localhost:5001
```

---

## 5. Cách login lấy token
Gửi yêu cầu POST đến endpoint `/api/auth/login`.

**Request Body mẫu:**
```json
{
  "email": "jockey@gmail.com",
  "password": "123456"
}
```

**Response mẫu (200 OK):**
```json
{
  "message": "Login successful",
  "result": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 3,
      "fullName": "Jockey",
      "email": "jockey@gmail.com",
      "role": "Jockey"
    }
  }
}
```

---

## 6. Cách Authorize trên Swagger
1. Truy cập **[http://localhost:5001/swagger](http://localhost:5001/swagger)**.
2. Sao chép chuỗi `result.accessToken` từ phản hồi đăng nhập.
3. Click vào nút **Authorize** ở góc trên cùng bên phải giao diện Swagger.
4. Nhập vào hộp thoại theo định dạng:
   ```text
   Bearer <accessToken>
   ```
   *Ví dụ: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`*
5. Nhấn **Authorize** rồi nhấn **Close**.

---

## 7. Authorization Test Matrix

| Endpoint | Không token | Admin | HorseOwner | Jockey | Referee | Spectator |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: |
| `GET /api/auth-test/admin` | `401` | `200` | `403` | `403` | `403` | `403` |
| `GET /api/auth-test/horse-owner` | `401` | `403` | `200` | `403` | `403` | `403` |
| `GET /api/auth-test/jockey` | `401` | `403` | `403` | `200` | `403` | `403` |
| `GET /api/auth-test/referee` | `401` | `403` | `403` | `403` | `200` | `403` |
| `GET /api/auth-test/spectator` | `401` | `403` | `403` | `403` | `403` | `200` |
| `GET /api/auth-test/authenticated` | `401` | `200` | `200` | `200` | `200` | `200` |

---

## 8. Test case chi tiết từng role

### Case 1: Thử nghiệm không gửi token (Chưa đăng nhập)
- **Endpoint:** Gọi bất kỳ API nào ở bảng trên.
- **Header:** Không có `Authorization`.
- **Expected Status Code:** `401 Unauthorized`.
- **Expected Response:** Trống hoặc thông điệp lỗi xác thực.
- **Actual result:** `401 Unauthorized`

### Case 2: Thử nghiệm gửi token sai role
- **Endpoint:** `GET /api/auth-test/admin`
- **Header:** Đính kèm token của **Spectator** (`spectator@gmail.com`).
- **Expected Status Code:** `403 Forbidden`.
- **Expected Response:** Trống hoặc thông điệp từ chối quyền truy cập.
- **Actual result:** `403 Forbidden`

### Case 3: Thử nghiệm gửi token đúng role
- **Endpoint:** `GET /api/auth-test/jockey`
- **Header:** Đính kèm token của **Jockey** (`jockey@gmail.com`).
- **Expected Status Code:** `200 OK`.
- **Expected Response:**
  ```json
  {
    "message": "Jockey authorization successful",
    "role": "Jockey"
  }
  ```
- **Actual result:** `200 OK`

---

## 9. Cách hiểu lỗi 401, 403, 404, 500

- **401 Unauthorized:** Yêu cầu chưa được đính kèm token xác thực hoặc token không hợp lệ (sai cấu trúc, hết hạn, chữ ký không khớp). Hãy thực hiện đăng nhập lại và đính kèm token Bearer đúng cú pháp.
- **403 Forbidden:** Người dùng đã xác thực thành công nhưng vai trò hiện tại (Role Claim trong token) không nằm trong danh sách được phép của endpoint.
- **404 Not Found:** Đường dẫn URL API không tồn tại hoặc sai chính tả. Hãy kiểm tra lại URL.
- **500 Internal Server Error:** Lỗi phát sinh từ phía máy chủ (ví dụ: mất kết nối cơ sở dữ liệu, lỗi logic trong mã nguồn). Hãy kiểm tra log server để biết thêm chi tiết.

---

## 10. Checklist test nhanh (Quick Check)

- [ ] Có role Admin
- [ ] Có role HorseOwner
- [ ] Có role Jockey
- [ ] Có role Referee
- [ ] Có role Spectator
- [ ] Login Admin thành công
- [ ] Login HorseOwner thành công
- [ ] Login Jockey thành công
- [ ] Login Referee thành công
- [ ] Login Spectator thành công
- [ ] Token Admin chứa role Admin
- [ ] Token HorseOwner chứa role HorseOwner
- [ ] Token Jockey chứa role Jockey
- [ ] Token Referee chứa role Referee
- [ ] Token Spectator chứa role Spectator
- [ ] Admin gọi API Admin được 200
- [ ] HorseOwner gọi API HorseOwner được 200
- [ ] Jockey gọi API Jockey được 200
- [ ] Referee gọi API Referee được 200
- [ ] Spectator gọi API Spectator được 200
- [ ] Không token bị 401
- [ ] Sai role bị 403
