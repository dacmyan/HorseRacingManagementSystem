\# Global Rules



Business Rule là nguồn sự thật cao nhất.



Thứ tự ưu tiên:



Business Rule

>

Database

>

Backend

>

Frontend



Không được sửa Business Rule nếu không có yêu cầu.



Không tự thêm chức năng.



Không tự đổi tên Entity.



Không tự đổi Route.



Không tự Refactor.



Không tối ưu nếu chưa được yêu cầu.



Mọi thay đổi Database phải đồng bộ:



\- Entity

\- Migration

\- DbContext

\- SSMS

\- Azure SQL



Không được làm mất dữ liệu.



Không Drop Database.



Không Drop Table.



Ưu tiên:



ALTER TABLE



EF Migration



Seed



Nếu phát hiện Business Rule và code mâu thuẫn:



Sửa code.



Không sửa Business.

