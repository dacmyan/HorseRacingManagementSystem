# RaceEntry LaneNo Audit

Tài liệu này báo cáo kết quả kiểm tra cột `LaneNo` và các cấu hình liên quan đối với bảng `RaceEntry` trong cơ sở dữ liệu và dự án hiện tại.

---

## 1. Database đang kiểm tra
* **Kết nối mặc định:** Server `localhost` (SQL Server), database `HorseRacingManagementSystem`.
* **Người dùng kết nối:** `sa`

---

## 2. Kết quả kiểm tra bảng RaceEntry
Chạy câu lệnh SQL kiểm tra các cột thực tế của bảng `RaceEntry`:
```sql
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'RaceEntry'
ORDER BY ORDINAL_POSITION;
```

### Kết quả trả về từ SQL Server:
| TABLE_NAME | COLUMN_NAME | DATA_TYPE | IS_NULLABLE | CHARACTER_MAXIMUM_LENGTH |
| :--- | :--- | :--- | :--- | :--- |
| RaceEntry | Id | int | NO | NULL |
| RaceEntry | RaceId | bigint | NO | NULL |
| RaceEntry | HorseId | int | NO | NULL |
| RaceEntry | JockeyId | int | NO | NULL |
| RaceEntry | Status | nvarchar | NO | -1 |
| RaceEntry | HorseId1 | int | YES | NULL |

---

## 3. RaceEntry có LaneNo chưa?
* **Chưa.** Bảng `RaceEntry` trong database thực tế hiện **không** có cột `LaneNo`.

---

## 4. Kiểu dữ liệu và nullable
* Do cột `LaneNo` chưa tồn tại trong database thực tế nên không có kiểu dữ liệu.
* *Kỳ vọng thiết kế:* `LaneNo INT NOT NULL`.

---

## 5. Unique constraint RaceId + LaneNo
* Chạy query kiểm tra các unique constraints của bảng `RaceEntry`:
  ```sql
  SELECT tc.CONSTRAINT_NAME, tc.TABLE_NAME, kcu.COLUMN_NAME
  FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
  JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
  WHERE tc.TABLE_NAME = 'RaceEntry' AND tc.CONSTRAINT_TYPE = 'UNIQUE';
  ```
* **Kết quả:** Trả về `0 rows affected` (Chưa có bất kỳ Unique Constraint nào được định nghĩa trên bảng `RaceEntry`).
* *Kỳ vọng thiết kế:* Unique Constraint `UQ_RaceEntry_Race_Lane` trên hai cột `RaceId` và `LaneNo`.

---

## 6. Có bảng duplicate RaceEntries không?
* Chạy query kiểm tra:
  ```sql
  SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('RaceEntry', 'RaceEntries');
  ```
* **Kết quả:** Chỉ trả về `RaceEntry` (1 dòng). Hệ thống **không bị trùng lặp** bảng `RaceEntries` số nhiều.
* **Số dòng hiện tại của bảng RaceEntry:** 0 dòng.

---

## 7. Kết quả kiểm tra Entity RaceEntry
* **Tệp tin:** [RaceEntry.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Tournaments/RaceEntry.cs)
* **Trạng thái ban đầu:** Thiếu hoàn toàn thuộc tính `LaneNo`.
* **Trạng thái hiện tại (Đã sửa):** Đã bổ sung thành công thuộc tính:
  ```csharp
  public int LaneNo { get; set; }
  ```

---

## 8. Kết quả kiểm tra DTO/API
* Hiện tại trong dự án, **chưa có** các DTO liên quan như `CreateRaceEntryRequest`, `UpdateRaceEntryRequest` hay `RaceEntryResponse`. 
* File repository `IRaceEntryRepository.cs` là file rỗng (0-byte) và chưa được triển khai.
* Chưa có API/Controller nào xử lý trực tiếp việc tạo hoặc phân làn cho `RaceEntry`.

---

## 9. Kết quả kiểm tra AppDbContext
* **Tệp tin:** [AppDbContext.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/AppDbContext.cs)
* **Trạng thái ban đầu:** Thiếu cấu hình Unique Index trên Fluent API và gặp lỗi quan hệ đơn hướng (unidirectional) dẫn đến việc tự động sinh ra cột shadow `HorseId1` trong database.
* **Trạng thái hiện tại (Đã sửa):** 
  * Đã bổ sung cấu hình Unique Index cho cặp cột `RaceId` + `LaneNo`:
    ```csharp
    entity.HasIndex(x => new { x.RaceId, x.LaneNo }).IsUnique();
    ```
  * Sửa lỗi quan hệ của `Horse` thành quan hệ hai chiều (bidirectional) để loại bỏ cột shadow `HorseId1` bị thừa:
    ```csharp
    entity.HasOne(re => re.Horse)
        .WithMany(h => h.RaceEntries)
        .HasForeignKey(re => re.HorseId)
        .OnDelete(DeleteBehavior.Restrict);
    ```

---

## 10. Kết quả kiểm tra Migration
* Các migration cũ trước đây **không** có thông tin tạo cột `LaneNo` hay tạo unique index tương ứng.
* **Đã tạo thành công Migration mới:** `20260612033801_AddLaneNoToRaceEntry`
  * Migration này sẽ thực hiện:
    1. Drop khóa ngoại và cột shadow `HorseId1` thừa trong database.
    2. Thêm cột `LaneNo` kiểu `int` với thuộc tính `nullable: false`.
    3. Tạo Unique Index `IX_RaceEntry_RaceId_LaneNo` trên hai cột `RaceId` và `LaneNo`.

---

## 11. Lỗi phát hiện

| Mức độ | Vị trí | Vấn đề | Cách sửa |
| :--- | :--- | :--- | :--- |
| **High** | Database & Entity `RaceEntry` | Thiếu cột `LaneNo` ở cả cơ sở dữ liệu thực tế và lớp Entity trong code C#. | Đã thêm thuộc tính `LaneNo` vào Entity và tạo Migration để bổ sung cột vào DB. |
| **Medium** | `AppDbContext.cs` mapping | Thiếu định nghĩa Unique Index/Constraint `RaceId` + `LaneNo` trong Fluent API. | Đã cấu hình `.HasIndex(x => new { x.RaceId, x.LaneNo }).IsUnique();` |
| **Low** | `AppDbContext.cs` mapping | Cấu hình mối quan hệ của `Horse` bị đơn hướng gây ra việc EF tự sinh cột shadow `HorseId1` trong DB. | Đã chuyển sang cấu hình hai chiều với `.WithMany(h => h.RaceEntries)`. |

---

## 12. Kết luận
```text
RaceEntry thiếu LaneNo / mapping chưa đồng bộ
```
*(Hiện tại lỗi này đã được khắc phục hoàn toàn trong mã nguồn và đã có sẵn tệp Migration `AddLaneNoToRaceEntry` chờ chạy lệnh update để cập nhật cấu trúc database thực tế).*
