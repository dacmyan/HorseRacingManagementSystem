# Tóm tắt Kiểm tra Frontend-test (Gửi Team)

**Chào Team,**

Bộ phận QA đã hoàn tất quá trình Full-stack test luồng tích hợp giữa `frontend-test` và `backend`. Nhìn chung, tiến độ tích hợp rất đáng khen ngợi. Tuy nhiên vẫn còn một số điểm cần các bạn lưu ý và hoàn thiện.

### ✅ Những phần đã chạy tốt (Pass)
* **Khắc phục lỗi Đăng nhập**: Vấn đề Frontend lỗi JSON khi login đã được khắc phục hoàn toàn. Nguyên nhân do Vite Proxy không bắt được IPv6 của .NET. Hiện tại toàn bộ Request đã được proxy thẳng về Backend. Token & Phân quyền các Role chạy ổn định.
* **Module Tài khoản & Ví (Triều, Đắc)**: Đăng nhập, đăng ký, Admin thêm tài khoản, Khán giả Nạp/rút tiền, Đặt cược chạy đúng.
* **Module Dữ liệu & Đăng ký (Hàn, Khang)**: Tạo Ngựa, Giải đấu, Ký hợp đồng, Đăng ký giải và Gán làn chạy (`RaceEntry`) đều hoạt động trơn tru.

### ❌ Những điểm còn thiếu (Cần bổ sung)
Frontend hiện đang dùng giao diện giả (Mock UI) ở một số luồng thuộc **Phase 4 (Race Operation)** do lúc code frontend chưa có API. Bây giờ Backend đã có API, các bạn Frontend cần vào tích hợp:

1. **Trang Xác nhận kết quả của Trọng tài (`RefereeConfirmResultsPage`)**:
   * *Hiện tại*: Đang báo "Chưa có API".
   * *Cần làm*: Gọi API `POST /api/referee/results` để trọng tài nộp kết quả xếp hạng.
2. **Trang Vi phạm của Trọng tài (`RefereeViolationsPage`)**:
   * *Hiện tại*: Đang là Mock UI tĩnh.
   * *Cần làm*: Tích hợp form vi phạm gọi API `POST /api/referee/violations`.
3. **Trang Kết quả của Admin (`AdminResultsPage`)**:
   * *Cần làm*: Bổ sung chức năng "Công bố kết quả" gọi API `POST /api/admin/races/{raceId}/publish` để kích hoạt luồng trả thưởng tự động của Đắc.
4. **Trang Quản lý Người dùng của Admin (`AdminUsersPage`)**:
   * *Cần làm*: Bổ sung nút "Khóa/Mở Khóa" gọi API cập nhật trạng thái User.

**👉 Hành động tiếp theo:** Các thành viên phụ trách UI của Referee và Admin Results nhận lại file và ưu tiên gắn API cho các trang trên nhé. Chúc team hoàn thành dự án xuất sắc!
