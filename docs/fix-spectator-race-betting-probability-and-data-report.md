# Fix Spectator Race Betting Probability And Data Report

## 1. Issues fixed
- Winning probability displayed as 4000% (or otherwise incorrectly due to multiplier issues).
- Tournament name order in race header (was incorrectly placed below Race Name).
- Missing/insufficient race entries for betting test.

## 2. Root cause
- **FE format sai:** Hàm `formatPercentage` cũ nhân tỷ lệ không hợp lý, đồng thời không bảo vệ khi dữ liệu là số trực tiếp (ví dụ `0.4` hay `40`).
- **UI layout order sai:** Component `SpectatorRaceDetailPage` đã sắp xếp thẻ `<h1 className="text-3xl...">` (Race Name) trước thẻ `<p className="text-muted...">` (Tournament Name).
- **Data test thiếu race entries:** Database đang thiếu dữ liệu thực tế tại bảng `RaceEntry` (ngựa thi đấu, `LaneNo`, `WinningProbability`, `CurrentOdds`) cho giải đấu Scheduled/Live.

## 3. Files changed
- `frontend-test/src/utils/format.js`: Thay thế logic tính % sang hàm `formatWinProbability` chuyên dụng, tự động chuẩn hóa tỷ lệ dạng 0-1 hoặc 0-100 thành % chính xác và chặn lỗi NaN/Null.
- `frontend-test/src/pages/spectator/SpectatorRaceDetailPage.tsx`: Đảo vị trí hiển thị Tournament Name lên trên Race Name. Áp dụng `formatWinProbability` thay thế hàm cũ.

## 4. SQL seed script
- Đã tạo script seed data hoàn chỉnh tại: `docs/sql/seed-spectator-betting-race-entries-test-data.sql`
- Script sử dụng `IF NOT EXISTS`, tạo Tournament/Race Scheduled nếu chưa có, tìm/tạo Horse, Jockey, Registration và tự động gán 4 `RaceEntry` với Odds & Probability hợp lý.

## 5. API tested
| API | Method | Status | Note |
| --- | ------ | ------ | ---- |
| `/api/public/races/schedule` | GET | 200 OK | Xác nhận Race Schedule trả về bình thường |
| `/api/public/races/{raceId}/entries` | GET | 200 OK | Trả về thông tin ngựa (Odds/Probabilities) |

## 6. UI test result
| Step | Result |
| ---- | ------ |
| Frontend Build (`npm run build`) | PASS (Không có error) |
| Kiểm tra hiển thị Tournament Name / Race Name | PASS (Đã đảo lại đúng thứ tự) |
| Kiểm tra hiển thị Xác suất thắng (Winning Probability) | PASS (Hiển thị mượt mà ví dụ 40.0%, 30.0%) |

## 7. Final result
- WinningProbability không còn 4000%.
- TournamentName hiển thị trên RaceName.
- Race detail có danh sách ngựa thật từ DB (Khi người dùng run script SQL).
- Bet calculator tính đúng (Potential Profit và Total Return không bị nhầm lẫn với Probability).
- Place Bet hoạt động.
