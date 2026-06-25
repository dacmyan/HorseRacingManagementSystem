# Tóm tắt Test Functional Requirements (Gửi Team)

**Ngày**: 2026-06-25 | **Branch**: `fix/frontend-test-real-data-and-roadmap-bugs`

---

## 🏆 Kết quả Overall

| Metric | Kết quả |
|--------|---------|
| Tổng functions test | **51** |
| ✅ PASS | **39 (76.5%)** |
| ⚠️ PARTIAL | **9 (17.6%)** |
| ❌ FAIL | **0 (0%)** |
| 🔒 BLOCKED (thiếu API) | **3 (5.9%)** |
| Build Backend | ✅ PASS |
| Build Frontend | ✅ PASS |
| Login 5 roles | ✅ PASS |

---

## ✅ Role nào pass nhiều nhất

1. **Admin** — 12/14 PASS, 2 PARTIAL (Dashboard stats, Dashboard activity log)
2. **Spectator** — 8/10 PASS, 2 PARTIAL (Predictions list/create)
3. **Jockey** — 7/9 PASS, 1 PARTIAL, 1 BLOCKED (Assigned horses page)
4. **Horse Owner** — 7/10 PASS, 2 PARTIAL, 1 BLOCKED
5. **Race Referee** — 5/8 PASS, 2 PARTIAL, 1 BLOCKED (Handle violation update)

---

## ⚠️ Role nào còn lỗi / cần bổ sung

### Admin
- ❌ Dashboard stats cards còn TODO (thống kê users/races/bets tổng hợp)

### Spectator
- ❌ Predictions: Backend `SpectatorController` thiếu `GET/POST /api/spectator/predictions`

### Jockey
- ❌ Không có trang "Assigned Horses" (ngựa được phân công cưỡi)

### Referee
- ❌ Không có `PUT /api/referee/violations/{id}` để handle/resolve vi phạm
- ⚠️ Horse Check API có nhưng FE chưa dùng

### Horse Owner
- ⚠️ Dashboard activity section còn TODO

---

## 🚀 Chức năng demo được ngay

| Luồng | Mô tả |
|-------|-------|
| ✅ Auth | Login/Logout 5 roles với JWT, redirect đúng dashboard |
| ✅ Public | Xem Tournaments, Race Schedule, Rankings không cần đăng nhập |
| ✅ Owner Flow | Đăng ký ngựa → Mời Jockey (Contract) → Đăng ký giải đấu |
| ✅ Jockey Flow | Nhận lời mời → Accept/Reject → Xem lịch & stats |
| ✅ Admin Full | Tạo Tournament/Race → Approve Registration → Assign Referee → Publish |
| ✅ Spectator Wallet | Nạp tiền → Đặt cược → Xem cược → Reward notification |
| ✅ Referee Full | Xem assigned races → Ghi vi phạm → Nộp kết quả |
| ✅ Payout | Trigger bet payout → Prize distribution (Admin) |

---

## ❌ Chức năng thiếu backend

| Chức năng | API cần thêm |
|-----------|-------------|
| Spectator Predictions | `GET/POST /api/spectator/predictions` |
| Admin Dashboard Stats | `GET /api/admin/dashboard` (aggregate) |
| Referee Handle Violation | `PUT /api/referee/violations/{id}` |

---

## 🎨 Mock UI còn lại

| Page | Mock chưa xóa |
|------|---------------|
| `AdminDashboardPage` | Stats cards (user count, race count...) |
| `AdminRefereesPage` | 2 section "Danh sách cuộc đua" TODO |
| `RefereeReportsPage` | Stats summary section |
| `OwnerDashboardPage` | Activity section |

---

## 📋 Thứ tự fix tiếp theo (ưu tiên cao → thấp)

1. **P2**: Thêm `GET /api/spectator/predictions` + `POST /api/spectator/predictions` vào SpectatorController
2. **P3**: Thêm `GET /api/admin/dashboard` aggregate endpoint
3. **P3**: Thêm `PUT /api/referee/violations/{id}` để resolve vi phạm
4. **P3**: Tích hợp `referee/races/{id}/horse-checks` vào FE Referee
5. **P4**: Xóa mock stats trên Dashboard pages, thay bằng API thật

---

## 🗃️ Seed Data

```text
docs/sql/seed-functional-requirements-test-data.sql
```
Đã chạy thành công. Có đủ data để test tất cả flows:
- 5 Race với đủ statuses: Live, Scheduled, Completed, Published, Finished
- 15 Horses, 11 Registrations, 8 Bets (Pending/Won/Lost)
- 7 Violations, 5 Reports, 4 Results, 5 Predictions, 29 Notifications

---

## 📂 Report đầy đủ

```text
docs/functional-requirements-fullstack-test-report.md
```

👉 **Mức độ sẵn sàng demo**: Partially Ready — Tất cả 7 luồng core đều demo được. Chỉ còn Predictions và Dashboard stats chưa hoàn chỉnh.
