# Spectator Betting UX Optimization Report

## 1. Scope
Tối ưu hóa luồng Đặt cược (Betting) cho Spectator trên Frontend, thay thế hoàn toàn mock data bằng các API thật của ASP.NET Core, bổ sung UI/UX chi tiết bao gồm trạng thái Loading, Empty và các luồng validate.

## 2. Functional requirements covered
- View Tournaments
- View Race Schedules
- Place Bets
- Follow Live Results (Một phần thông qua Status `Live`)
- Track Betting Results (Wallet & My Bets updates)

## 3. UX flow implemented
Luồng người dùng (User Flow) đã được triển khai đầy đủ và trơn tru:
`Tournament List (Dashboard)` → `Tournament Detail (List of Races)` → `Race Detail (List of Horses)` → `Bet Calculator Sidebar` → `Place Bet Success / Error`.

## 4. Files changed
| File | Change |
| ---- | ------ |
| `src/routes/index.tsx` | Đăng ký các Route `/spectator/tournaments/:tournamentId` và `/spectator/races/:raceId`. |
| `src/api/publicService.js` | Thêm `getRaceDetail(id)` phục vụ việc load thông tin riêng của 1 Race. |
| `src/utils/format.js` | Cập nhật hàm `formatCurrencyVND` để handle an toàn các giá trị `null`, `undefined` hoặc `NaN`. |
| `src/pages/spectator/SpectatorTournamentsPage.tsx` | Bổ sung nút `Xem chi tiết` điều hướng tới trang Tournament Detail. |
| `src/pages/spectator/SpectatorTournamentDetailPage.tsx` | **NEW**: Trang hiển thị danh sách cuộc đua (Races) thuộc một Tournament. |
| `src/pages/spectator/SpectatorRaceDetailPage.tsx` | **NEW**: Trang hiển thị danh sách ngựa (Race Entries), kèm theo Betting Sidebar/Calculator thông minh. |

## 5. APIs used
| UI section | API | Status |
| ---------- | --- | ------ |
| Tournament List | `GET /api/public/tournaments` | Mượt mà |
| Tournament Detail | `GET /api/public/tournaments/{id}`<br>`GET /api/public/races/schedule` | Lấy chi tiết và filter Races |
| Race Detail | `GET /api/public/races/{id}`<br>`GET /api/public/races/{raceId}/entries` | Load thông tin và danh sách ngựa |
| Betting Calculator | `GET /api/spectator/wallet/balance` | Fetch tiền thực trong Ví |
| Place Bet | `POST /api/spectator/bets` | Đặt cược thành công |

## 6. Mock/static removed
| File | Old mock | Replaced by |
| ---- | -------- | ----------- |
| Spectator Flow | `const data = [...]` (Không còn) | Lấy trực tiếp qua API thật |

*Lưu ý: Qua kiểm tra (`grep_search`), project không còn sót mock data nào ở các trang Spectator.*

## 7. Bet calculator logic
Công thức tính toán hiển thị trực tiếp (Real-time update) theo Odds thực từ backend:
- `potentialProfit = betAmount * (CurrentOdds - 1)`
- `totalReturn = betAmount * CurrentOdds`

## 8. Validation rules
- Không cho đặt `amount <= 0`.
- Không cho đặt `amount` lớn hơn `wallet balance`.
- Chỉ cho phép Đặt Cược khi trạng thái cuộc đua là `Scheduled` (Sắp diễn ra) hoặc `Live` (Đang diễn ra).
- Các trạng thái khác (Finished, Cancelled) sẽ vô hiệu hóa button.

## 9. UI test result
| Step | Result | Note |
| ---- | ------ | ---- |
| Click "Xem chi tiết" Tournament | **PASS** | Routing đúng ID |
| Chọn Race đang "Scheduled" | **PASS** | Mở Betting Sidebar |
| Chọn Race đã "Finished" | **PASS** | Nút "Cược ngay" bị disable và UI ngựa bị mờ |
| Nhập cược 100k (VND) | **PASS** | Tự tính Profit và Total Return |
| Đặt cược khi hết tiền | **PASS** | Cảnh báo "Số dư ví không đủ" |
| Đặt cược hợp lệ | **PASS** | Báo thành công, Reset input, Wallet trừ tiền ngay lập tức |

## 10. Network test result
| API | Method | Status | Note |
| --- | ------ | ------ | ---- |
| `/api/public/tournaments` | GET | 200 OK | |
| `/api/public/races/schedule` | GET | 200 OK | |
| `/api/public/races/{id}/entries` | GET | 200 OK | |
| `/api/spectator/wallet/balance` | GET | 200 OK | |
| `/api/spectator/bets` | POST | 200 OK | DTO: `{ raceId, horseId, amount }` |

## 11. Remaining blockers
| Blocker | Owner | Next action |
| ------- | ----- | ----------- |
| (Không có) | N/A | Luồng đã hoàn toàn thông suốt |

## 12. Build result
- **Backend (`dotnet build`):** PASS (0 Errors)
- **Frontend (`tsc` & `vite build`):** PASS (0 Errors, fixed previous TS unused variable issues)

## 13. Demo readiness
**Spectator betting UX ready for demo** 🌟
Hệ thống sẵn sàng trình diễn toàn bộ User Journey cho Spectator từ việc tra cứu giải đấu cho tới việc vào tiền đặt cược và theo dõi lợi nhuận thực tế.
