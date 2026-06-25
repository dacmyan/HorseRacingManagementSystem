# Frontend-Test Real Data And Bugs Fix Report

## 1. Source reports
`docs/frontend-test-roadmap-compliance-report.md`
`docs/frontend-test-roadmap-summary-for-team.md`

## 2. Issues extracted from reports
| Issue ID | Role | Page/Flow | Type | Priority | Status before |
| -------- | ---- | --------- | ---- | -------- | ------------- |
| BUG-01 | Admin | AdminUsersPage | Missing Feature | P1 | Missing Toggle Status |
| BUG-02 | Admin | AdminResultsPage | Missing Feature | P1 | Missing Publish Result API |
| BUG-03 | Referee | RefereeConfirmResultsPage | Mock UI | P1 | Hardcoded "Chưa có API" |
| BUG-04 | Referee | RefereeViolationsPage | Mock UI | P1 | Hardcoded Fake List |
| BUG-05 | Spectator | SpectatorLiveResultsPage | Mock UI | P2 | Hardcoded Live List |

## 3. Mock/static UI found
| File | Page/Component | Mock data | Replaced by API? | API used |
| ---- | -------------- | --------- | ---------------- | -------- |
| `AdminResultsPage.tsx` | Race list | `Trận mùa Xuân 2026` | Yes | `getRaceSchedule` |
| `RefereeConfirmResultsPage.tsx` | Confirm Box | `Cần xác nhận` | Yes | `getRefereeDashboard` |
| `RefereeViolationsPage.tsx` | Violations | `Lấn làn` | Yes | `getViolations` |
| `SpectatorLiveResultsPage.tsx` | Live matches | `Vòng loại 1` | Yes | `getLiveRaces` |
| `RefereeReportsPage.tsx` | Stats | `Tổng báo cáo` | No | N/A |

## 4. Fixes applied
| Issue ID | File changed | What changed | Why |
| -------- | ------------ | ------------ | --- |
| BUG-01 | `AdminUsersPage.tsx` | Thêm Nút Toggle | Gọi `updateUserStatus` theo BE yêu cầu |
| BUG-02 | `AdminResultsPage.tsx` | List API + Nút Publish | Gọi `publishRaceResult` để hoàn tất flow |
| BUG-03 | `RefereeConfirmResultsPage.tsx` | Thay giao diện tĩnh bằng Fetch API | Trọng tài cần nộp kết quả thực tế |
| BUG-04 | `RefereeViolationsPage.tsx` | Thay giao diện tĩnh bằng Fetch API | Trọng tài cần nộp vi phạm thực tế |
| BUG-05 | `SpectatorLiveResultsPage.tsx` | Gọi API getLiveRaces | Khán giả xem được Race thật |

## 5. API endpoint fixes
- Cập nhật thêm `/api/admin/users/{id}/status`
- Cập nhật thêm `/api/referee/violations`

## 6. Response mapping fixes
- `AdminResultsPage` chuyển `data?.result` đúng định dạng mảng để render list Race.

## 7. Form payload fixes
- Sửa Submit Violation payload gởi đúng `RaceId`, `Description`, `Penalty`.
- Cập nhật UI Predict (Đặt cược) để load dropdown Horse từ `getRaceEntries(raceId)`.

## 8. Role guard fixes
- Đã kiểm tra qua Role JWT hoàn toàn hoạt động bình thường nhờ Proxy tốt.

## 9. Real DB data integration
- Tất cả trang (trừ Reports Dashboard) đã chạy API kết nối DB.

## 10. Seed data script if created
`docs/sql/seed-frontend-test-real-data.sql`

## 11. Backend blockers still remaining
- Không có backend blocker chức năng.

## 12. Test result by role
| Role | Page/Flow | Status after | Note |
| ---- | --------- | ------------ | ---- |
| Admin | Manage Users / Publish Result | PASS | |
| Referee | Violations / Confirm Results | PASS | |
| Spectator | Live / Predictions | PASS | Lấy Data chuẩn |

## 13. Build result
- Backend: PASS (0 lỗi)
- Frontend: PASS (`tsc -b && vite build` thành công)

## 14. Remaining issues
| Priority | Issue | Owner | Reason | Next action |
| -------- | ----- | ----- | ------ | ----------- |
| P4 | Báo cáo Trọng Tài Dashboard | Team FE | Thiếu Endpoint | Nếu không khắt khe, không cần |

## 15. Demo readiness
**Frontend-test ready for demo**
