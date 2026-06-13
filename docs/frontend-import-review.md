# Frontend Import Review

## 1. Repo frontend đã import
- **Repository URL**: `https://github.com/tuanphat0909/Horse-Tournament-Management.git`

## 2. Folder import
- **Folder**: [frontend](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend) (nằm ở thư mục gốc của monorepo)

## 3. Cách xử lý nested .git
- **Mô tả**: Sau khi clone repository frontend, thư mục ẩn `.git` riêng của repository này đã được phát hiện bên trong thư mục `frontend`.
- **Cách xử lý**: Đã xóa thư mục `.git` này bằng lệnh PowerShell:
  ```powershell
  Remove-Item -Recurse -Force frontend/.git
  ```
  Điều này đảm bảo thư mục `frontend` trở thành thư mục thông thường trong monorepo chứ không phải là submodule hay nested repository, giúp Git của dự án cha theo dõi file chính xác.

## 4. Package manager và scripts
- **Package Manager khuyến nghị**: `npm` (dựa trên sự hiện diện của file `package-lock.json` được clone về).
- **Các script cấu hình trong package.json**:
  - `dev`: `vite`
  - `build`: `tsc -b && vite build`
  - `lint`: `eslint .`
  - `preview`: `vite preview`
  *(Không có script `test`)*
- **Thư viện chính**: React 19.2.6, React Router DOM 7.17.0, Tailwind CSS 4.3.0, TypeScript 6.0.2, Vite 8.0.12.

## 5. Kết quả npm install
- **Trạng thái**: **Thành công** (`npm.cmd install` đã tải và cài đặt thành công 180 packages mà không gặp lỗi dependency nào, 0 lỗ hổng bảo mật).
- **Lưu ý trên Windows**: Cần sử dụng `npm.cmd` thay vì `npm` do chính sách Execution Policy của PowerShell trên Windows ngăn chặn chạy tệp `.ps1` của npm.

## 6. Kết quả build/lint/test
- **npm run build**: **Thành công** (`tsc -b && vite build` biên dịch TypeScript và đóng gói thành công, tạo ra thư mục `frontend/dist`).
- **npm run lint**: **Thất bại** (37 errors, 0 warnings).
  - *Lỗi @typescript-eslint/no-explicit-any*: Sử dụng kiểu `any` trong code TypeScript.
  - *Lỗi react-hooks/set-state-in-effect*: Gọi `setState` đồng bộ bên trong `useEffect` (gọi hàm `load()` / `loadAll()` khởi tạo state đồng bộ như `setLoading(true)`).
- **npm test**: Không áp dụng (không có script test).

### Bảng chi tiết lỗi lint quan trọng:

| File | Lỗi | Nguyên nhân | Cách sửa đề xuất |
| ---- | --- | ----------- | ---------------- |
| [SpectatorNotificationsPage.tsx](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/pages/spectator/SpectatorNotificationsPage.tsx#L48) | `react-hooks/set-state-in-effect` | Gọi hàm `load()` đồng bộ trong `useEffect`. Hàm `load` có lệnh setState đồng bộ ở đầu (`setLoading(true)`). | Sử dụng `setTimeout(() => load(), 0)` hoặc tách riêng logic gọi API để tránh setState đồng bộ, hoặc cấu hình tắt rule trong `eslint.config.js`. |
| [SpectatorPredictionsPage.tsx](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/pages/spectator/SpectatorPredictionsPage.tsx#L57) | `react-hooks/set-state-in-effect` | Gọi hàm `load()` đồng bộ trong `useEffect`. | Tương tự như trên. |
| [SpectatorWalletPage.tsx](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/pages/spectator/SpectatorWalletPage.tsx#L71) | `react-hooks/set-state-in-effect` | Gọi hàm `loadAll()` đồng bộ trong `useEffect`. | Tương tự như trên. |
| Nhiều file source `.tsx` / `.ts` | `@typescript-eslint/no-explicit-any` | Gán kiểu `any` cho các biến hoặc tham số. | Định nghĩa interfaces rõ ràng cho DTOs/Models trả về từ backend hoặc dùng `unknown`. |

## 7. Kiểm tra conflict với monorepo
- **Git / File conflict**: Không có. Thư mục `frontend/` nằm hoàn toàn biệt lập với `backend/`.
- **Cấu hình `.gitignore`**: Tệp `.gitignore` ở thư mục gốc monorepo đã được định cấu hình đầy đủ để ignore các thư mục `node_modules/`, `dist/`, `.env`, `.env.local`, và `.vite/`. Không có file rác frontend nào bị add nhầm vào git index của monorepo.
- **Xung đột Port**:
  - Frontend Dev Server: Cổng `5173` (`http://localhost:5173`).
  - Backend API: Cổng `5000` (`http://localhost:5000`).
  - *Kết luận*: Không có xung đột cổng (hai ứng dụng chạy ở cổng riêng biệt).

## 8. Kiểm tra API base URL
- Trong [api.js](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend/src/services/api.js):
  ```javascript
  const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';
  ```
- Nếu không cấu hình biến môi trường `VITE_API_URL`, ứng dụng frontend sẽ gửi yêu cầu trực tiếp đến địa chỉ tuyệt đối `http://localhost:5000/api`. Điều này sẽ dẫn đến lỗi **CORS** do backend chưa cấu hình CORS cho phép origin của frontend (`http://localhost:5173`).
- **Cách giải quyết**:
  - Đã tạo file `.env` và `.env.example` trong thư mục [frontend](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/frontend) với cấu hình:
    ```env
    VITE_API_URL=/api
    ```
  - Khi sử dụng relative path `/api`, Vite dev server sẽ bắt các request này và chuyển tiếp qua server proxy (được định cấu hình trong `vite.config.ts` để trỏ về `http://localhost:5000`). Điều này giúp tránh hoàn toàn lỗi CORS khi phát triển ở local.

## 9. Kiểm tra API compatibility với backend
Tất cả 30 endpoint API mà frontend gọi trong các service `frontend/src/api/*.js` đều khớp hoàn hảo 100% với các Route & Method HTTP tương ứng được khai báo trong các Controller của ASP.NET Core backend.

| Frontend API gọi | Backend có chưa | Ghi chú |
| ---------------- | --------------- | ------- |
| `GET /api/admin/roles` | Có | Khớp với [AdminController.GetRoles](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs#L45) |
| `POST /api/admin/accounts` | Có | Khớp với [AdminController.CreateAccount](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs#L56) |
| `POST /api/admin/tournaments` | Có | Khớp với [AdminController.CreateTournament](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs#L122) |
| `POST /api/admin/races` | Có | Khớp với [AdminController.CreateRace](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs#L141) |
| `POST /api/admin/payouts/prizes` | Có | Khớp với [AdminController.DistributeTournamentPrizes](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs#L78) |
| `POST /api/admin/payouts/trigger/{raceId}` | Có | Khớp với [AdminController.TriggerBetPayout](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs#L100) |
| `POST /api/auth/login` | Có | Khớp với [AuthController.Login](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AuthController.cs#L18) |
| `POST /api/auth/register` | Có | Khớp với [AuthController.Register](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AuthController.cs#L30) |
| `GET /api/jockeys/contracts` | Có | Khớp với [JockeyController.GetMyContracts](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/JockeyController.cs#L33) |
| `PUT /api/jockeys/contracts/{id}/respond` | Có | Khớp với [JockeyController.RespondToContract](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/JockeyController.cs#L48) |
| `GET /api/horses/my-horses` | Có | Khớp với [OwnerController.GetMyHorses](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs#L64) |
| `POST /api/horses` | Có | Khớp với [OwnerController.CreateHorse](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs#L45) |
| `GET /api/horses/{id}` | Có | Khớp với [OwnerController.GetHorseById](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs#L79) |
| `PUT /api/horses/{id}` | Có | Khớp với [OwnerController.UpdateHorse](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs#L98) |
| `DELETE /api/horses/{id}` | Có | Khớp với [OwnerController.DeleteHorse](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs#L121) |
| `POST /api/registrations` | Có | Khớp với [OwnerController.RegisterHorse](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs#L205) |
| `GET /api/registrations/my-registrations` | Có | Khớp với [OwnerController.GetMyRegistrations](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs#L228) |
| `POST /api/jockey-contracts` | Có | Khớp với [OwnerController.CreateContract](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs#L167) |
| `GET /api/jockey-contracts/my-proposals` | Có | Khớp với [OwnerController.GetMyProposedContracts](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/OwnerController.cs#L190) |
| `GET /api/public/rankings/jockeys` | Có | Khớp với [PublicController.GetJockeyRankings](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/PublicController.cs#L49) |
| `GET /api/public/rankings/horses` | Có | Khớp với [PublicController.GetHorseRankings](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/PublicController.cs#L78) |
| `GET /api/public/races/schedule` | Có | Khớp với [PublicController.GetPublicRaceSchedule](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/PublicController.cs#L157) |
| `GET /api/public/notifications` | Có | Khớp với [PublicController.GetMyNotifications](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/PublicController.cs#L121) |
| `PUT /api/public/notifications/{id}/read` | Có | Khớp với [PublicController.MarkNotificationAsRead](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/PublicController.cs#L137) |
| `POST /api/spectator/wallet/deposit` | Có | Khớp với [SpectatorController.Deposit](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs#L39) |
| `POST /api/spectator/wallet/withdraw` | Có | Khớp với [SpectatorController.Withdraw](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs#L58) |
| `GET /api/spectator/wallet/balance` | Có | Khớp với [SpectatorController.GetWalletBalance](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs#L96) |
| `GET /api/spectator/wallet/history` | Có | Khớp với [SpectatorController.GetWalletHistory](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs#L81) |
| `POST /api/spectator/bets` | Có | Khớp với [SpectatorController.PlaceBet](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs#L111) |
| `GET /api/spectator/bets/my-bets` | Có | Khớp với [SpectatorController.GetMyBets](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs#L134) |

## 10. CORS/Port cần chú ý
- **Backend CORS**: File `Program.cs` của backend API chưa cấu hình CORS Middleware. Để gọi API thành công từ frontend chạy độc lập không thông qua Vite proxy dev server, cần bổ sung cấu hình CORS trên backend (cho phép origin `http://localhost:5173`).
- **Proxy hoạt động**: Ở môi trường dev local, nếu chạy frontend qua Vite dev server và cấu hình `.env` với `VITE_API_URL=/api`, các cuộc gọi API sẽ đi vòng qua cổng của Vite `http://localhost:5173/api/...` và proxy an toàn đến `http://localhost:5000/api/...`, giúp vượt qua giới hạn CORS thành công.

## 11. File cần sửa nếu có
- Không cần sửa đổi code logic hiện tại. Tất cả cấu hình chạy local đã được thiết lập sẵn sàng thông qua các tệp cấu hình `.env` mới tạo ở thư mục `frontend/`.

## 12. Kết luận
```text
Frontend import OK but needs API config
```
*Lý do*: Quá trình import và cài đặt dependencies diễn ra thành công tốt đẹp, build pass 100%. Mức độ tương thích API giữa frontend và backend là tuyệt đối (30/30 API đều khớp hoàn hảo). Cần áp dụng file cấu hình `.env` (đã tạo) để frontend gọi API qua cổng proxy 5000 không bị dính lỗi CORS ở môi trường local.
