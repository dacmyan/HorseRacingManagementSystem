-- Script kiểm tra toàn bộ khóa ngoại (Foreign Keys) trong database
-- Để lập thứ tự xóa bảng rỗng an toàn mà không vi phạm ràng buộc FK.

SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS ChildTable,
    OBJECT_NAME(fk.referenced_object_id) AS ParentTable
FROM sys.foreign_keys fk
ORDER BY ParentTable, ChildTable;
