# Debug Horse Creation Error

## 1. Mô tả lỗi
Khi người dùng thuộc vai trò `HorseOwner` thực hiện thêm ngựa mới trên Frontend, hệ thống hiển thị thông báo lỗi chung:
```text
An error occurred during horse creation
```
Phía giao diện không hiển thị thêm thông tin chi tiết. Giao dịch tạo ngựa bị hủy bỏ và không lưu được thông tin vào cơ sở dữ liệu.

## 2. Frontend request
* **Request URL:** `http://localhost:5173/api/horses` (Vite dev server proxy chuyển tới `http://localhost:5001/api/horses`)
* **Request Method:** `POST`
* **Headers:**
  - `Content-Type: application/json`
  - `Authorization: Bearer <JWT_token>`
* **Request Body:**
  ```json
  {
    "name": "Red Lightning",
    "breed": "Thoroughbred",
    "age": 4,
    "gender": "Male"
  }
  ```

## 3. Backend endpoint
* **Controller:** `OwnerController` trong project `HorseRacing.API`
* **Route:** `[HttpPost("horses")]` mapped to `CreateHorse([FromBody] RegisterHorseRequest request)`
* **Quyền truy cập:** Yêu cầu đăng nhập với role `HorseOwner` (`[Authorize(Roles = "HorseOwner")]`)

## 4. Request DTO mapping

| Field FE gửi | Field BE DTO cần | Khớp không | Ghi chú |
| ------------ | ---------------- | ---------- | ------- |
| `name`       | `Name` (string)  | Khớp       | Bắt buộc |
| `breed`      | `Breed` (string) | Khớp       | Bắt buộc |
| `age`        | `Age` (int)      | Khớp       | Bắt buộc |
| `gender`     | `Gender` (string)| Khớp       | Bắt buộc |

Request DTO map khớp hoàn toàn giữa cấu trúc payload gửi từ Frontend và class `RegisterHorseRequest` trên Backend.

## 5. Authorization/Role check
* Giao diện Frontend gửi chính xác header `Authorization: Bearer <token>`.
* Token được trích xuất từ tài khoản có `RoleId = 2` (tương ứng với role `HorseOwner` trong bảng `Role`).
* Việc xác thực phân quyền qua middleware diễn ra thành công (Backend không trả về HTTP 401 hay 403).

## 6. OwnerId check
* `OwnerId` được Backend tự động lấy ra từ JWT token Claims qua phương thức helper `GetCurrentUserId()` trong `OwnerController.cs`.
* Email đăng nhập của owner: `owner@gmail.com`
* `UserId` tương ứng trong bảng `AppUser` là `2`.
* `OwnerId` được gán chính xác bằng `2` tại Business Logic Service.

## 7. Database Horse schema check
Cấu trúc thực tế của bảng `Horse` trong cơ sở dữ liệu SQL Server:
```sql
COLUMN_NAME     DATA_TYPE    IS_NULLABLE
-----------------------------------------
Id              int          NO (Primary Key, Identity)
Name            nvarchar     NO
Age             int          NO
Breed           nvarchar     NO
OwnerId         int          NO
Gender          nvarchar     NO
HealthStatus    nvarchar     NO
```
* Bảng `Horse` không có cột `RegistrationId`.
* Liên kết khóa ngoại: `Horse.OwnerId -> AppUser.UserId`.

## 8. Exception thật từ backend
Khi thực hiện lưu thay đổi trong `CreateHorseAsync` thông qua EF Core, database ném ra ngoại lệ:
```text
fail: Microsoft.EntityFrameworkCore.Update[10000]
      An exception occurred in the database while saving changes for context type 'HorseRacing.Infrastructure.Persistence.AppDbContext'.
      Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
       ---> Microsoft.Data.SqlClient.SqlException (0x80131904): Invalid column name 'RegistrationId'.
         at Microsoft.Data.SqlClient.SqlCommand.<>c.<ExecuteDbDataReaderAsync>b__195_0(Task`1 result)
         ...
         at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
```
Mã SQL thực tế do EF Core sinh ra và cố gắng thực thi:
```sql
INSERT INTO [Horse] ([Age], [Breed], [Gender], [HealthStatus], [Name], [OwnerId], [RegistrationId])
OUTPUT INSERTED.[Id]
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6);
```

## 9. Nguyên nhân chính
Trong entity class `Registration.cs` (thuộc tầng Domain), có khai báo một thuộc tính không sử dụng:
```csharp
public ICollection<Horse> Horses { get; set; } = new List<Horse>();
```
Do khai báo này, EF Core tự động suy luận ra một quan hệ một-nhiều bổ sung giữa `Registration` và `Horse` (coi là một `Registration` liên kết tới nhiều `Horse` và cần một cột khóa ngoại trên bảng `Horse`). Hệ quả là EF Core sinh ra shadow property `RegistrationId` trên thực thể `Horse` và tìm kiếm cột `RegistrationId` trong bảng `Horse` khi truy vấn hoặc thực hiện lệnh `INSERT`.
Vì bảng `Horse` thực tế trong database không có cột `RegistrationId`, SQL Server báo lỗi `Invalid column name 'RegistrationId'`.

## 10. File đã sửa
* [Registration.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/Registration.cs): Xóa bỏ khai báo thuộc tính dư thừa `public ICollection<Horse> Horses { get; set; }`.

## 11. Kết quả test lại
* Dự án backend build thành công không lỗi.
* Thực hiện tạo ngựa mới "Red Lightning" (4 tuổi, đực, Thoroughbred) trên Frontend thành công.
* Dữ liệu được ghi nhận chính xác vào database bảng `Horse` và `HorseStatistic` với `OwnerId = 2`.
  - Kết quả truy vấn database:
    ```text
    Id   OwnerId  Email            Name           Age  Breed         HealthStatus
    -----------------------------------------------------------------------------
    1    2        owner@gmail.com  Red Lightning  4    Thoroughbred  Healthy
    ```

## 12. Lỗi còn lại nếu có
Không có. Luồng tạo ngựa chạy hoàn hảo từ Frontend đến Database.
