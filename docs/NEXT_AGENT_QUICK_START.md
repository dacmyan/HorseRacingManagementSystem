# NEXT AGENT QUICK START

## 1. Current branch
`fix/frontend-test-real-data-and-roadmap-bugs`

## 2. Current status
**Ready for next session**. Toàn bộ mock UI chính ở FE đã được thay thế bằng gọi API thật tới BE. Các luồng Core (Auth, Registration, Result, Violation, Bet, Payout) đều đã có thể Demo. Build BE & FE Pass.

## 3. What was fixed
- **BE**: Bổ sung các API còn thiếu (`PUT /api/admin/users/{id}/status`, `GET /api/referee/violations`).
- **FE SDK**: Viết thêm logic cho `adminService`, `refereeService`, `publicService`.
- **UI**: Thay thế UI cứng thành Data thật ở: `AdminUsersPage`, `AdminResultsPage`, `RefereeConfirmResultsPage`, `RefereeViolationsPage`, `SpectatorLiveResultsPage`, `SpectatorPredictionsPage`.
- **DB**: Tạo file seed `docs/sql/seed-frontend-test-real-data.sql` để có data cho các trạng thái Race Live, Completed, Published.

## 4. What remains
- **Mock UI**: `RefereeReportsPage` (Phần thống kê dashboard) vẫn là Mock do chưa cấp API. (Priority: P4)
- **Git**: Nhánh chưa được commit và merge.

## 5. First 5 commands to run
```bash
git status
git branch
cd backend && dotnet run --project src/HorseRacing.API -- --urls http://localhost:5001
cd ../frontend-test && npm run dev
# Mở docs/NEXT_AGENT_HANDOFF.md để đọc chi tiết
```

## 6. Top 5 issues to fix next
1. Commit branch hiện tại và merge về main nếu đã test Pass hết ở browser.
2. Thêm UX handling (toast message / validation form chi tiết) ở Frontend.
3. Nếu cần thiết, thêm endpoint cho `RefereeReportsPage` để xóa nốt Mock cuối cùng.

## 7. Test accounts
* **Admin**: admin@gmail.com / 123456
* **HorseOwner**: owner@gmail.com / 123456
* **Jockey**: jockey@gmail.com / 123456
* **Referee**: referee@gmail.com / 123456
* **Spectator**: spectator@gmail.com / 123456

## 8. Important URLs
* **Backend Swagger**: http://localhost:5001/swagger
* **Health DB**: http://localhost:5001/api/health/db
* **Frontend-test**: http://localhost:5173
