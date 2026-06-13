# Debug Admin Create Jockey Account Not Showing

## 1. Mô tả lỗi
Sau khi Admin tạo tài khoản Jockey mới thành công, tài khoản này không hiển thị trong danh sách Accounts trên giao diện Frontend. Ngay cả các tài khoản Jockey đã có sẵn từ trước trong hệ thống (như `jockey@gmail.com`) cũng hoàn toàn biến mất khỏi danh sách người dùng.

## 2. Backend create account API
* **Endpoint:** `POST /api/admin/accounts`
* **Controller:** `AdminController.cs` (phương thức `CreateAccount`)
* **Logic:** Khi tạo Jockey, API kiểm tra tính hợp lệ của dữ liệu, kiểm tra trùng lặp email, băm mật khẩu bằng `PasswordHasher<AppUser>`, lưu thông tin vào bảng `AppUser` với `RoleId` tương ứng của Jockey, và cuối cùng tạo một bản ghi `JockeyProfile` trống để liên kết.
* **Đánh giá:** Giai đoạn lưu trữ dữ liệu hoàn toàn chính xác. Hệ thống đã lưu thành công tài khoản Jockey vào cơ sở dữ liệu kèm theo profile tương ứng.

## 3. Database check
Truy vấn SQL thực tế xác minh trạng thái trong cơ sở dữ liệu:
```sql
-- Kiểm tra User Jockey mới
SELECT u.UserId, u.FullName, u.Email, u.RoleId, r.Name AS RoleName, u.Status 
FROM AppUser u JOIN Role r ON u.RoleId = r.RoleId 
WHERE r.Name = 'Jockey';
```
* **Kết quả:**
  * User cũ `jockey@gmail.com` (RoleId = 3, Status = 'Active').
  * User mới vừa tạo `Test Jockey 99` (jockey99@gmail.com, RoleId = 3, Status = 'Active').
* **Profile:** Cả hai tài khoản Jockey đều có bản ghi tương ứng trong bảng `JockeyProfile` với trạng thái `Active`.

## 4. Backend get accounts API
* **Vấn đề cốt lõi ở Backend:**
  * Hệ thống ban đầu **hoàn toàn chưa có API lấy danh sách tài khoản / người dùng** cho quản trị viên (`GET /api/admin/accounts`).
  * `IAdminService` và `AdminService` chỉ có phương thức tạo tài khoản và lấy danh sách role, hoàn toàn thiếu đi logic truy vấn danh sách người dùng.

* **Giải pháp khắc phục:**
  1. Thêm phương thức `GetAllUsersAsync()` trong `IUserRepository` và `UserRepository` để nạp dữ liệu tất cả tài khoản kèm theo thông tin `Role`.
  2. Định nghĩa DTO `AccountResponseDto` chứa các thông tin hiển thị cần thiết (`UserId`, `FullName`, `Email`, `RoleName`, `Status`, `CreatedAt`).
  3. Hiện thực hóa logic nghiệp vụ `GetAccountsAsync()` trong `IAdminService` và `AdminService`.
  4. Bổ sung endpoint `GET /api/admin/accounts` trong `AdminController.cs` để trả về danh sách người dùng đầy đủ cho Frontend.

## 5. Frontend API call
* **Tệp tin:** `frontend/src/api/adminService.js`
* **Vấn đề:** Chưa định nghĩa phương thức gọi API danh sách tài khoản.
* **Giải pháp khắc phục:** Đã bổ sung hàm `getAccounts`:
  ```javascript
  export const getAccounts = () => api.get('/admin/accounts');
  ```

## 6. Frontend response mapping
* **Tệp tin:** `frontend/src/pages/admin/AdminUsersPage.tsx`
* **Vấn đề:** Trang quản lý tài khoản của admin ban đầu chỉ là một khung giao diện trống (UI Placeholder). Không hề khai báo state chứa danh sách người dùng, không có hiệu ứng nạp dữ liệu (`useEffect` gọi API) và bảng hiển thị được hardcode là "Chưa có dữ liệu" với số lượng kết quả luôn bằng `0`.
* **Giải pháp khắc phục:** 
  * Cấu hình state `accounts` để quản lý danh sách người dùng.
  * Hiện thực hàm `fetchAccounts()` gọi tới `getAccounts()` của API và cập nhật danh sách người dùng.
  * Gọi `fetchAccounts()` ngay sau khi component được mount.

## 7. Frontend filter/refetch
* **Cải tiến:**
  * **Đồng bộ hóa sau khi tạo mới:** Sau khi tạo tài khoản mới thành công qua Modal, ứng dụng tự động kích hoạt lại hàm `fetchAccounts()` để cập nhật danh sách tức thì mà không cần tải lại trang.
  * **Lọc (Filter) và tìm kiếm (Search):** Áp dụng bộ lọc client-side cho ô tìm kiếm (tìm theo Họ tên/Email) và bộ lọc theo Role (Tất cả, Admin, Horse Owner, Jockey, Referee, Spectator).
  * **Đếm số lượng (Counts):** Tính toán động số lượng người dùng của từng nhóm vai trò thay vì hiển thị cứng `0`.

## 8. Nguyên nhân chính
1. **Backend thiếu API:** Chưa có API `GET /api/admin/accounts` để cung cấp dữ liệu tài khoản người dùng.
2. **Frontend chưa tích hợp:** Trang quản trị `AdminUsersPage.tsx` chỉ là khung giao diện tĩnh chưa có kết nối API, chưa có bảng hiển thị và chưa quản lý state danh sách người dùng.

## 9. File đã sửa
1. **DTO:** [AccountResponseDto.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/DTOs/AccountResponseDto.cs) (Mới)
2. **Repository:** [IUserRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/Interfaces/IUserRepository.cs) & [UserRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/UserRepository.cs) (Bổ sung `GetAllUsersAsync`)
3. **Application Services:** [IAdminService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/Interfaces/IAdminService.cs) & [AdminService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/Services/AdminService.cs) (Hiện thực `GetAccountsAsync`)
4. **API Controller:** [AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs) (Bổ sung endpoint `GET accounts`)
5. **Frontend API Client:** [adminService.js](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/api/adminService.js) (Thêm hàm `getAccounts`)
6. **Frontend UI Page:** [AdminUsersPage.tsx](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/pages/admin/AdminUsersPage.tsx) (Tích hợp nạp dữ liệu, thống kê động, bộ lọc và dựng bảng hiển thị chi tiết)

## 10. Kết quả test lại
* Thử nghiệm End-to-End đã được chạy tự động thông qua subagent trình duyệt và cho kết quả tuyệt vời:
  1. Admin đăng nhập và truy cập thành công vào trang quản lý người dùng.
  2. Danh sách nạp đầy đủ các Jockey cũ (`jockey@gmail.com`) cùng các vai trò khác.
  3. Tạo mới Jockey `Test Jockey 99` thành công. Modal đóng và danh sách lập tức tự động cập nhật hiển thị tài khoản mới này ở trạng thái `Active`.
  4. Bộ lọc hoạt động mượt mà, công cụ tìm kiếm lọc chính xác tên `Test Jockey 99`.

![Video ghi lại quá trình test hệ thống](/C:/Users/Dac's Laptop/.gemini/antigravity-ide/brain/211f7803-b05e-440e-b637-07541fee5184/admin_jockey_test_1781338261688.webp)

## 11. Lỗi còn lại nếu có
* Không có lỗi phát sinh thêm. Toàn bộ hệ thống chạy mượt mà và đồng bộ.
