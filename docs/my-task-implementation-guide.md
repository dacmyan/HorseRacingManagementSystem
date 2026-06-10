# My Backend Task Implementation Guide

## 1. Mục đích tài liệu

Tài liệu này được biên soạn nhằm hướng dẫn và giải thích chi tiết toàn bộ phần công việc backend đã được triển khai cho **Module 4: Spectator Betting, Financials & Notifications** thuộc dự án **Horse Racing Management System**. 

Tài liệu sẽ giúp bạn hiểu rõ:
* Module này làm nhiệm vụ gì và bạn đóng vai trò gì.
* Danh sách các tệp tin đã được tạo mới hoặc chỉnh sửa trong project.
* Giải thích chi tiết mã nguồn của từng tệp tin quan trọng (theo từng tầng API/Application/Domain/Infrastructure).
* Luồng xử lý nghiệp vụ của hệ thống.
* Cách phân quyền (Role authorization) cho các API.
* Thiết kế database và các mối quan hệ.
* Hướng dẫn chi tiết cách chạy dự án và kiểm thử (test) từng API bằng Swagger.
* Các lỗi thường gặp và cách khắc phục khi vận hành.

---

## 2. Task của tôi là gì?

Dựa vào tài liệu phân chia công việc [backend-task-division-4-members.md](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/docs/backend-task-division-4-members.md), phần nhiệm vụ của tôi được xác định như sau:

*   **Tên thành viên phụ trách**: **Đắc** (Member 4)
*   **Module phụ trách**: **Spectators, Betting, Wallet & Notification** (Khán giả, Đặt cược, Ví điện tử & Thông báo)
*   **Các chức năng chính**:
    1.  **Giao dịch ví điện tử**: Cho phép Spectator nạp/rút tiền giả lập và xem lịch sử giao dịch ví.
    2.  **Đặt cược (Place Bets)**: Cho phép Spectator đặt cược tiền vào ngựa đua trong trận đấu sắp diễn ra (`Scheduled`).
    3.  **Dự đoán (Predictions)**: Cho phép Spectator tham gia minigame dự đoán ngựa thắng cuộc không mất phí và xem tỷ lệ dự đoán từ đám đông.
    4.  **Tự động trả thưởng đặt cược**: Hệ thống tự động tính toán tỷ lệ odds cược theo công thức Pari-Mutuel và trả thưởng tiền thắng vào ví khán giả khi trận đấu đổi sang trạng thái `"Finished"`.
    5.  **Trả thưởng giải đấu (Prize Payout)**: Cho phép Admin phát giải thưởng của giải đấu cho Horse Owner (Chủ ngựa - nhận 70%) và Jockey (Nài ngựa - nhận 30%) của chú ngựa vô địch giải.
    6.  **Thông báo hệ thống**: Tạo và gửi thông báo khi có các giao dịch tài chính hoặc kết quả trả thưởng, cho phép người dùng xem danh sách thông báo và đánh dấu đã đọc.
*   **Các bảng database liên quan**: `Wallets`, `Transactions` (`WalletTransaction`), `Bets`, `Payouts`, `Predictions`, `Prizes`, `TournamentPrizePayouts`, và `Notifications`.
*   **Các role được phép tương tác**:
    *   `Spectator`: Người đặt cược, thực hiện nạp/rút, dự đoán, nhận trả thưởng cược.
    *   `Admin`: Phê duyệt trả giải thưởng giải đấu, trigger cược thủ công nếu cần.
    *   `Public` / `All Roles`: Xem xếp hạng Jockey, Horse, nhận thông báo cá nhân.

---

## 3. Tóm tắt những gì đã làm

Hệ thống đã được thiết kế và cài đặt theo đúng mô hình **Clean Architecture (Onion Architecture)** phân tách rõ ràng các layer:

*   **API (Presentation Layer)**:
    *   Tạo mới [SpectatorController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs) với các API dành riêng cho Spectator.
    *   Tạo mới [PublicController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/PublicController.cs) với các API công khai cho bảng xếp hạng và thông báo cá nhân.
    *   Cập nhật [AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs) với các API trả thưởng giải đấu và trigger cược của Admin.
*   **Application Layer**:
    *   **DTOs**: Khởi tạo và thiết kế các request/response DTOs phục vụ truyền nhận dữ liệu an toàn, không lộ thông tin nhạy cảm.
    *   **Interfaces**: Định nghĩa các service interface và repository interface độc lập, giúp tầng Application không bị phụ thuộc vào EF Core.
    *   **Services**: Viết toàn bộ mã nguồn xử lý logic nghiệp vụ ví điện tử, cược, thuật toán odds Pari-Mutuel, phân chia giải thưởng và quản lý thông báo.
*   **Domain Layer**:
    *   Định nghĩa đầy đủ thuộc tính cho các thực thể domain: `Bet`, `Payout`, `Prize`, `TournamentPrizePayout`, `Notification`.
*   **Infrastructure Layer**:
    *   **AppDbContext**: Khai báo các `DbSet<>` mới và cấu hình mối quan hệ khóa ngoại (Foreign Keys) chặt chẽ trong `OnModelCreating`.
    *   **Repositories**: Viết mã nguồn triển khai các repository để query dữ liệu thông qua EF Core.
    *   **Dependency Injection**: Đăng ký các repository mới vào file [DependencyInjection.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/DependencyInjection.cs) và đăng ký các service mới vào file [ServiceExtensions.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Extensions/ServiceExtensions.cs).
*   **Database Migration**:
    *   Tạo thành công migration `AddBetPayoutPrizeNotification` và thực thi cập nhật cấu trúc database cục bộ.

---

## 4. Danh sách file đã tạo mới hoặc chỉnh sửa

Dưới đây là danh sách chi tiết các file thuộc phạm vi Module 4:

| File | Thêm mới hay chỉnh sửa | Vai trò | Vì sao cần file này |
| :--- | :--- | :--- | :--- |
| **Domain Layer** | | | |
| [Bet.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Bet.cs) | Chỉnh sửa (từ file rỗng) | Entity | Định nghĩa thông tin đặt cược của khán giả. |
| [Payout.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Payout.cs) | Chỉnh sửa (từ file rỗng) | Entity | Lưu lịch sử trả thưởng đặt cược thành công. |
| [Prize.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Prize.cs) | Chỉnh sửa (từ file rỗng) | Entity | Lưu cấu hình giải thưởng của giải đấu (Tournament). |
| [TournamentPrizePayout.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/TournamentPrizePayout.cs) | Chỉnh sửa (từ file rỗng) | Entity | Lưu lịch sử phát thưởng giải đấu cho HorseOwner/Jockey. |
| [Notification.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Notifications/Notification.cs) | Chỉnh sửa (từ file rỗng) | Entity | Lưu trữ thông báo hệ thống gửi đến người dùng. |
| **Application Layer** | | | |
| *DTOs* | | | |
| [DepositRequest.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/DTOs/DepositRequest.cs) | Chỉnh sửa (từ rỗng) | DTO Request | Nhận số tiền nạp từ client. |
| [WithdrawRequest.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/DTOs/WithdrawRequest.cs) | Chỉnh sửa (từ rỗng) | DTO Request | Nhận số tiền rút từ client. |
| [WalletBalanceResponse.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/DTOs/WalletBalanceResponse.cs) | Chỉnh sửa (từ rỗng) | DTO Response | Trả về thông tin số dư ví hiện tại. |
| [TransactionHistoryResponse.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/DTOs/TransactionHistoryResponse.cs) | Chỉnh sửa (từ rỗng) | DTO Response | Trả về dòng lịch sử giao dịch ví. |
| [PlaceBetRequest.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/DTOs/PlaceBetRequest.cs) | Chỉnh sửa (từ rỗng) | DTO Request | Nhận thông tin đặt cược từ khán giả. |
| [BetTicketResponse.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/DTOs/BetTicketResponse.cs) | Chỉnh sửa (từ rỗng) | DTO Response | Trả về vé cược chi tiết của khán giả. |
| [PredictionManagementRequest.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/DTOs/PredictionManagementRequest.cs) | Chỉnh sửa (từ rỗng) | DTO Request | Nhận thông tin dự đoán minigame. |
| [PredictionStatsResponse.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/DTOs/PredictionStatsResponse.cs) | Tạo mới | DTO Response | Trả về thống kê tỷ lệ dự đoán đám đông. |
| [UpdateOddsRequest.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/DTOs/UpdateOddsRequest.cs) | Chỉnh sửa (từ rỗng) | DTO Request | Nhận thông tin cập nhật odds cược. |
| [PrizePayoutRequest.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/DTOs/PrizePayoutRequest.cs) | Tạo mới | DTO Request | Hỗ trợ Admin truyền thông tin giải thưởng giải đấu. |
| [NotificationResponse.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/Notifications/DTOs/NotificationResponse.cs) | Chỉnh sửa (từ rỗng) | DTO Response | Trả về chi tiết thông báo của người dùng. |
| [SendNotificationRequest.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/Notifications/DTOs/SendNotificationRequest.cs) | Chỉnh sửa (từ rỗng) | DTO Request | Gửi yêu cầu tạo thông báo thủ công. |
| [HorseRankingResponse.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/HorseManagement/DTOs/HorseRankingResponse.cs) | Tạo mới | DTO Response | Trả về bảng xếp hạng ngựa thắng nhiều nhất. |
| [JockeyRankingResponse.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/UserManagement/DTOs/JockeyRankingResponse.cs) | Tạo mới | DTO Response | Trả về bảng xếp hạng nài ngựa theo ranking point. |
| *Interfaces* | | | |
| [IWalletService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Interfaces/IWalletService.cs) | Tạo mới | Interface | Định nghĩa phương thức nghiệp vụ ví điện tử. |
| [IBettingService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Interfaces/IBettingService.cs) | Tạo mới | Interface | Định nghĩa phương thức đặt cược. |
| [IPredictionService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Interfaces/IPredictionService.cs) | Tạo mới | Interface | Định nghĩa phương thức dự đoán minigame. |
| [IBetPayoutService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Interfaces/IBetPayoutService.cs) | Tạo mới | Interface | Định nghĩa phương thức tự động trả thưởng cược. |
| [IPrizePayoutService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Interfaces/IPrizePayoutService.cs) | Tạo mới | Interface | Định nghĩa phương thức phát giải thưởng giải đấu. |
| [INotificationService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/Notifications/Interfaces/INotificationService.cs) | Tạo mới | Interface | Định nghĩa phương thức gửi/nhận thông báo. |
| [IBetRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Interfaces/IBetRepository.cs) | Chỉnh sửa (từ rỗng) | Interface Repo | Định nghĩa các câu truy vấn thực thể cược, lịch đấu. |
| [IWalletRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Interfaces/IWalletRepository.cs) | Tạo mới | Interface Repo | Định nghĩa truy cập dữ liệu ví. |
| [IWalletTransactionRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Interfaces/IWalletTransactionRepository.cs) | Tạo mới | Interface Repo | Định nghĩa truy cập lịch sử giao dịch. |
| [IPayoutRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Interfaces/IPayoutRepository.cs) | Chỉnh sửa (từ rỗng) | Interface Repo | Định nghĩa chèn dữ liệu payout cược. |
| [IPrizeRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Interfaces/IPrizeRepository.cs) | Chỉnh sửa (từ rỗng) | Interface Repo | Định nghĩa truy cập cấu hình & lịch sử giải thưởng. |
| [INotificationRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/Notifications/Interfaces/INotificationRepository.cs) | Chỉnh sửa (từ rỗng) | Interface Repo | Định nghĩa truy cập lịch sử thông báo hệ thống. |
| *Services* | | | |
| [WalletService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Services/WalletService.cs) | Chỉnh sửa (từ rỗng) | Service | Thực hiện nghiệp vụ nạp/rút tiền, cập nhật ví. |
| [BettingService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Services/BettingService.cs) | Chỉnh sửa (từ rỗng) | Service | Xử lý việc đặt cược và tính odds động Pari-Mutuel. |
| [PredictionService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Services/PredictionService.cs) | Chỉnh sửa (từ rỗng) | Service | Thực hiện ghi nhận và thống kê tỷ lệ dự đoán minigame. |
| [BetPayoutService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Services/BetPayoutService.cs) | Chỉnh sửa (từ rỗng) | Service | Xử lý tính toán tiền thắng cược và phát ví tự động. |
| [PrizePayoutService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Services/PrizePayoutService.cs) | Chỉnh sửa (từ rỗng) | Service | Xử lý phát giải thưởng cho HorseOwner và Jockey. |
| [NotificationService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/Notifications/Services/NotificationService.cs) | Chỉnh sửa (từ rỗng) | Service | Xử lý truy vấn thông báo hệ thống và đánh dấu đã đọc. |
| **Infrastructure Layer** | | | |
| [AppDbContext.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/AppDbContext.cs) | Chỉnh sửa | Persistence | Khai báo các `DbSet` mới và liên kết cấu hình DB. |
| [DependencyInjection.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/DependencyInjection.cs) | Chỉnh sửa | DI Registration | Đăng ký các repository của Module 4 vào DI Container. |
| [BetRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/BetRepository.cs) | Chỉnh sửa (từ rỗng) | Repository | Triển khai query dữ liệu cược, lịch đua qua EF Core. |
| [WalletRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/WalletRepository.cs) | Tạo mới | Repository | Triển khai quản lý ví điện tử qua EF Core. |
| [WalletTransactionRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/WalletTransactionRepository.cs) | Tạo mới | Repository | Triển khai quản lý giao dịch ví qua EF Core. |
| [PayoutRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/PayoutRepository.cs) | Tạo mới | Repository | Triển khai lưu thông tin payout cược qua EF Core. |
| [PrizeRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/PrizeRepository.cs) | Tạo mới | Repository | Triển khai lưu trữ lịch sử trả thưởng giải đấu qua EF. |
| [NotificationRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/NotificationRepository.cs) | Tạo mới | Repository | Triển khai quản lý thông báo hệ thống qua EF Core. |
| **API Layer** | | | |
| [SpectatorController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs) | Chỉnh sửa (từ rỗng) | Controller | Cung cấp endpoints nạp/rút/cược/dự đoán của Spectator. |
| [PublicController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/PublicController.cs) | Chỉnh sửa (từ rỗng) | Controller | Endpoints xem xếp hạng và thông báo cho người dùng. |
| [AdminController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/AdminController.cs) | Chỉnh sửa | Controller | Endpoint phát thưởng giải và trigger trả thưởng cược. |
| [ServiceExtensions.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Extensions/ServiceExtensions.cs) | Chỉnh sửa | DI Registration | Đăng ký các service của Module 4 vào DI Container. |

---

## 5. Giải thích từng file

### [Bet.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Bet.cs)
*   **Vị trí**: `src/HorseRacing.Domain/Entities/Financials/Bet.cs`
*   **Layer**: Domain
*   **Mục đích**: Định nghĩa cấu trúc dữ liệu của một vé đặt cược.
*   **Chức năng tham gia**: Đặt cược (`BE-BET-01`), trả thưởng đặt cược tự động (`BE-PAY-01`).
*   **Đoạn code quan trọng**:
    ```csharp
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public int RaceId { get; set; }
    public Race? Race { get; set; }
    public int HorseId { get; set; }
    public Horse? Horse { get; set; }
    public decimal Amount { get; set; }
    public decimal Odds { get; set; }
    public string Status { get; set; } = "Pending";
    ```
*   **Ý nghĩa**: Lưu trữ thông tin ai đặt (`UserId`), trận đấu nào (`RaceId`), ngựa nào (`HorseId`), đặt bao nhiêu (`Amount`), tỷ lệ ăn tiền lúc đặt (`Odds`) và trạng thái (`Status` gồm `"Pending"`, `"Won"`, `"Lost"`, `"Refunded"`).
*   **Khi nào cần sửa**: Khi bạn muốn mở rộng hình thức cược (ví dụ: cược top 3 thay vì chỉ cược ngựa vô địch).

---

### [Payout.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Payout.cs)
*   **Vị trí**: `src/HorseRacing.Domain/Entities/Financials/Payout.cs`
*   **Layer**: Domain
*   **Mục đích**: Định nghĩa bản ghi lịch sử trả thưởng đặt cược.
*   **Chức năng tham gia**: Trả thưởng tự động đặt cược (`BE-PAY-01`).
*   **Đoạn code quan trọng**:
    ```csharp
    public int Id { get; set; }
    public int BetId { get; set; }
    public Bet? Bet { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    ```
*   **Ý nghĩa**: Liên kết trực tiếp với vé cược thắng (`BetId`), lưu trữ số tiền thực tế được phát thưởng (`Amount`) và thời gian thực hiện.
*   **Khi nào cần sửa**: Khi bạn muốn thêm thông tin thuế hay phí giao dịch khấu trừ vào phần thưởng.

---

### [Prize.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Prize.cs)
*   **Vị trí**: `src/HorseRacing.Domain/Entities/Financials/Prize.cs`
*   **Layer**: Domain
*   **Mục đích**: Cấu hình giải thưởng của một giải đấu.
*   **Chức năng tham gia**: Trả thưởng giải đấu (`BE-PAY-02`).
*   **Đoạn code quan trọng**:
    ```csharp
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public Tournament? Tournament { get; set; }
    public int Rank { get; set; }
    public decimal Amount { get; set; }
    public decimal OwnerPercentage { get; set; }
    public decimal JockeyPercentage { get; set; }
    ```
*   **Ý nghĩa**: Một giải đấu (`TournamentId`) có nhiều hạng giải thưởng (`Rank` từ 1 đến 3). Mỗi giải thưởng quy định tổng tiền (`Amount`) và tỷ lệ phần trăm phân chia cho Chủ ngựa (`OwnerPercentage`) và Nài ngựa (`JockeyPercentage`).
*   **Khi nào cần sửa**: Khi cần thay đổi cấu trúc chia giải thưởng hoặc thêm phân phối cho ban tổ chức.

---

### [TournamentPrizePayout.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/TournamentPrizePayout.cs)
*   **Vị trí**: `src/HorseRacing.Domain/Entities/Financials/TournamentPrizePayout.cs`
*   **Layer**: Domain
*   **Mục đích**: Lưu trữ thông tin phát tiền giải thưởng thực tế.
*   **Chức năng tham gia**: Trả thưởng giải đấu (`BE-PAY-02`).
*   **Ý nghĩa**: Lưu vết xem ai (`UserId`), với vai trò gì (`Role` là `"HorseOwner"` hoặc `"Jockey"`) đã nhận được số tiền giải thưởng bao nhiêu (`Amount`) từ giải đấu nào (`TournamentId`).
*   **Khi nào cần sửa**: Khi cần tích hợp hệ thống xuất hóa đơn hoặc báo cáo thuế thu nhập cho chủ ngựa/nài ngựa.

---

### [Notification.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Notifications/Notification.cs)
*   **Vị trí**: `src/HorseRacing.Domain/Entities/Notifications/Notification.cs`
*   **Layer**: Domain
*   **Mục đích**: Thực thể lưu trữ thông tin thông báo hệ thống.
*   **Chức năng tham gia**: Hệ thống thông báo thời gian thực (`BE-NOTI-01`).
*   **Đoạn code quan trọng**:
    ```csharp
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    ```
*   **Ý nghĩa**: Mỗi người dùng (`UserId`) có một danh sách các thông báo với nội dung văn bản (`Message`). Thuộc tính `IsRead` dùng để đánh dấu xem thông báo đó đã được đọc trên UI hay chưa.
*   **Khi nào cần sửa**: Khi bạn muốn mở rộng phân loại thông báo (Ví dụ: thông báo hệ thống, thông báo tài chính, thông báo giải đấu) để hiển thị các icon khác nhau trên frontend.

---

### [AppDbContext.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/AppDbContext.cs)
*   **Vị trí**: `src/HorseRacing.Infrastructure/Persistence/AppDbContext.cs`
*   **Layer**: Infrastructure
*   **Mục đích**: Quản lý kết nối cơ sở dữ liệu và cấu hình thực thể Entity Framework Core.
*   **Chức năng tham gia**: Tất cả chức năng trong module.
*   **Đoạn code quan trọng**:
    ```csharp
    public DbSet<Bet> Bets { get; set; }
    public DbSet<Payout> Payouts { get; set; }
    // ...
    modelBuilder.Entity<Bet>(entity =>
    {
        entity.HasKey(b => b.Id);
        entity.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
        entity.HasOne(b => b.Race).WithMany().HasForeignKey(b => b.RaceId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(b => b.Horse).WithMany().HasForeignKey(b => b.HorseId).OnDelete(DeleteBehavior.Restrict);
    });
    ```
*   **Ý nghĩa**: Khai báo các bảng dữ liệu mới trong database và cấu hình mối quan hệ khóa ngoại (Foreign Keys). Sử dụng `DeleteBehavior.Restrict` cho `Race` và `Horse` để ngăn chặn việc xóa nhầm trận đấu hoặc ngựa khi đã có dữ liệu cược của khách hàng.
*   **Khi nào cần sửa**: Khi cần thay đổi cấu trúc bảng, đổi tên bảng trong DB hoặc thay đổi các quy tắc cascade delete.

---

### [BetRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/BetRepository.cs)
*   **Vị trí**: `src/HorseRacing.Infrastructure/Repositories/BetRepository.cs`
*   **Layer**: Infrastructure
*   **Mục đích**: Thực hiện các câu lệnh SQL thông qua EF Core để truy vấn thực thể cược, giải đấu, và lịch đấu.
*   **Chức năng tham gia**: Cược, dự đoán và bảng xếp hạng.
*   **Đoạn code quan trọng**:
    ```csharp
    public async Task<Race?> GetFinalRaceInTournamentAsync(int tournamentId)
    {
        return await _context.Races
            .Where(r => r.TournamentId == tournamentId && r.Status == "Finished")
            .OrderByDescending(r => r.ScheduledTime)
            .FirstOrDefaultAsync();
    }
    ```
*   **Ý nghĩa**: Định nghĩa cách lấy trận chung kết (hoặc trận kết thúc cuối cùng) của giải đấu nhằm tìm ra chú ngựa chiến thắng chung cuộc để trao thưởng giải đấu.
*   **Khi nào cần sửa**: Khi cần tối ưu hóa hiệu năng câu truy vấn (Eager loading bằng `.Include` hoặc dùng các câu query SQL thuần).

---

### [WalletService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Services/WalletService.cs)
*   **Vị trí**: `src/HorseRacing.Application/Features/FinancialRewards/Services/WalletService.cs`
*   **Layer**: Application
*   **Mục đích**: Xử lý logic nạp tiền, rút tiền và lấy lịch sử giao dịch ví.
*   **Chức năng tham gia**: Quản lý ví điện tử (`BE-WALLET-01`).
*   **Đoạn code quan trọng**:
    ```csharp
    public async Task<WalletBalanceResponse> WithdrawAsync(int userId, WithdrawRequest request)
    {
        if (request.Amount <= 0) throw new ArgumentException("Withdrawal amount must be greater than zero.");
        var wallet = await GetOrCreateWalletAsync(userId);
        if (wallet.Balance < request.Amount) throw new InvalidOperationException("Insufficient balance.");
        
        wallet.Balance -= request.Amount;
        // Ghi transaction âm tiền...
        // Tạo thông báo...
    }
    ```
*   **Ý nghĩa**: Đảm bảo số tiền rút phải hợp lệ (> 0) và số dư ví phải đủ để rút tiền, tránh việc ví bị âm tiền. Tự động lưu vết giao dịch âm tiền và đẩy thông báo cho người dùng.
*   **Khi nào cần sửa**: Khi tích hợp với cổng thanh toán online thật như VNPay hoặc PayOS.

---

### [BettingService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Services/BettingService.cs)
*   **Vị trí**: `src/HorseRacing.Application/Features/BettingEngine/Services/BettingService.cs`
*   **Layer**: Application
*   **Mục đích**: Thực hiện chức năng đặt cược của khán giả và tính toán odds động.
*   **Chức năng tham gia**: Đặt cược (`BE-BET-01`).
*   **Đoạn code quan trọng**:
    ```csharp
    public async Task<decimal> CalculateCurrentOddsAsync(int raceId, int horseId)
    {
        var bets = await _betRepository.GetByRaceIdAsync(raceId);
        var activeBets = bets.Where(b => b.Status != "Refunded").ToList();
        var totalRaceBets = activeBets.Sum(b => b.Amount);
        var totalHorseBets = activeBets.Where(b => b.HorseId == horseId).Sum(b => b.Amount);

        if (totalRaceBets == 0) return 2.0m;
        if (totalHorseBets == 0) return Math.Max((totalRaceBets * 0.9m) / 100m, 3.5m);

        return Math.Max((totalRaceBets * 0.9m) / totalHorseBets, 1.1m);
    }
    ```
*   **Ý nghĩa**: Cài đặt thuật toán tính tỷ lệ odds Pari-Mutuel động. Lấy tổng tiền cược của cả trận đấu đua ngựa (trừ đi 10% phí tổ chức) chia cho tổng tiền cược vào chính chú ngựa đó. Điều này đảm bảo tỷ lệ odds biến động tự nhiên theo thị trường đặt cược.
*   **Khi nào cần sửa**: Khi bạn muốn thay đổi mức phí cắt phế của nhà cái (House take-out rate, hiện tại là `10%` hay `0.9m`).

---

### [BetPayoutService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Services/BetPayoutService.cs)
*   **Vị trí**: `src/HorseRacing.Application/Features/FinancialRewards/Services/BetPayoutService.cs`
*   **Layer**: Application
*   **Mục đích**: Tự động trả thưởng đặt cược cho khán giả.
*   **Chức năng tham gia**: Trả thưởng tự động đặt cược (`BE-PAY-01`).
*   **Đoạn code quan trọng**:
    ```csharp
    if (bet.HorseId == winningHorse.Id)
    {
        decimal payoutAmount = Math.Round(bet.Amount * bet.Odds, 2);
        bet.Status = "Won";
        // Cộng tiền ví...
        // Tạo giao dịch BetWon...
    }
    else
    {
        bet.Status = "Lost";
    }
    ```
*   **Ý nghĩa**: Quét tất cả vé cược `"Pending"` của trận đấu. Nếu cược đúng ngựa thắng thì tính toán số tiền được nhận (`Amount * Odds`), chuyển tiền vào ví, đổi trạng thái cược thành `"Won"` và tạo thông báo trúng cược. Ngược lại thì đổi trạng thái cược thành `"Lost"`.
*   **Khi nào cần sửa**: Khi cần thay đổi cơ chế trả thưởng (ví dụ: trả thưởng cược cho cả ngựa về nhì và về ba).

---

### [PrizePayoutService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/FinancialRewards/Services/PrizePayoutService.cs)
*   **Vị trí**: `src/HorseRacing.Application/Features/FinancialRewards/Services/PrizePayoutService.cs`
*   **Layer**: Application
*   **Mục đích**: Tính toán và phân bổ giải thưởng của giải đấu cho Chủ ngựa và Nài ngựa.
*   **Chức năng tham gia**: Trả giải thưởng giải đấu (`BE-PAY-02`).
*   **Đoạn code quan trọng**:
    ```csharp
    decimal ownerAmount = Math.Round(firstPrize.Amount * (firstPrize.OwnerPercentage / 100m), 2);
    decimal jockeyAmount = Math.Round(firstPrize.Amount * (firstPrize.JockeyPercentage / 100m), 2);
    // Cộng ví Chủ ngựa và Nài ngựa...
    ```
*   **Ý nghĩa**: Từ giải thưởng hạng 1 của giải đấu, tính toán phần tiền thưởng cho chủ ngựa (mặc định 70%) và nài ngựa điều khiển chú ngựa đó (mặc định 30%), cộng tiền vào ví của họ và gửi thông báo chúc mừng.
*   **Khi nào cần sửa**: Khi luật của giải đua thay đổi tỷ lệ ăn chia giải thưởng giữa Chủ ngựa và Nài ngựa.

---

### [SpectatorController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs)
*   **Vị trí**: `src/HorseRacing.API/Controllers/SpectatorController.cs`
*   **Layer**: API
*   **Mục đích**: Tiếp nhận các API request từ Client và gọi Service tương ứng xử lý.
*   **Chức năng tham gia**: Các tính năng ví, cược, dự đoán của Spectator.
*   **Đoạn code quan trọng**:
    ```csharp
    [HttpPost("bets")]
    [Authorize(Roles = "Spectator")]
    public async Task<IActionResult> PlaceBet([FromBody] PlaceBetRequest request)
    {
        var userId = GetCurrentUserId();
        var response = await _bettingService.PlaceBetAsync(userId, request);
        return Ok(new { message = "Bet placed successfully", result = response });
    }
    ```
*   **Ý nghĩa**: Endpoint `POST api/spectator/bets` yêu cầu quyền truy cập của role `Spectator`. Tự động trích xuất `UserId` từ JWT token để đảm bảo khán giả không thể đặt cược hộ cho người khác.
*   **Khi nào cần sửa**: Khi bạn thay đổi đường dẫn URL hoặc đổi định dạng JSON truyền lên từ frontend.

---

## 6. Luồng xử lý chức năng

Mô tả luồng xử lý thực tế của chức năng **Đặt cược (Place Bet)**:

```text
Khán giả gửi yêu cầu cược (gồm RaceId, HorseId, Amount) cùng JWT Token lên endpoint POST /api/spectator/bets
        |
        v
SpectatorController chặn và kiểm tra Role = "Spectator" qua JWT Token
        |
        v
SpectatorController lấy UserId từ JWT Claims và truyền dữ liệu cho BettingService.PlaceBetAsync()
        |
        v
BettingService gọi BetRepository lấy thông tin Race và kiểm tra trạng thái có phải "Scheduled" (chưa chạy) hay không
        |
        v
BettingService kiểm tra số dư ví thông qua WalletRepository. Nếu không đủ tiền -> Trả lỗi 400 BadRequest
        |
        v
BettingService gọi thuật toán CalculateCurrentOddsAsync() để tính Odds động Pari-Mutuel tại thời điểm cược
        |
        v
BettingService trừ tiền ví, tạo WalletTransaction (Type = "BetPlaced"), và chèn bản ghi Bet mới vào Database
        |
        v
BettingService đẩy một bản ghi Notification mới lưu vào cơ sở dữ liệu để thông báo cược thành công
        |
        v
BettingService trả về DTO BetTicketResponse (vé cược chứa Odds cố định và mã vé cược)
        |
        v
SpectatorController nhận vé cược và trả về HTTP Status Code 200 OK kèm theo JSON kết quả cược cho client
```

---

## 7. Danh sách API đã làm

Dưới đây là danh sách các API đã được cài đặt thành công trong Module 4:

| API | Method | Role được phép gọi | Request body | Response chính | Mục đích |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `/api/spectator/wallet/deposit` | `POST` | `Spectator` | `DepositRequest` | `WalletBalanceResponse` | Nạp tiền vào ví điện tử. |
| `/api/spectator/wallet/withdraw` | `POST` | `Spectator` | `WithdrawRequest` | `WalletBalanceResponse` | Rút tiền từ ví điện tử. |
| `/api/spectator/wallet/history` | `GET` | `Spectator` | Không | Danh sách `TransactionHistoryResponse` | Xem lịch sử nạp/rút/cược/thưởng ví. |
| `/api/spectator/wallet/balance` | `GET` | `Spectator` | Không | `WalletBalanceResponse` | Xem số dư ví hiện tại. |
| `/api/spectator/bets` | `POST` | `Spectator` | `PlaceBetRequest` | `BetTicketResponse` | Đặt cược vào một chú ngựa trong trận đấu. |
| `/api/spectator/bets/my-bets` | `GET` | `Spectator` | Không | Danh sách `BetTicketResponse` | Xem danh sách các vé cược đã đặt. |
| `/api/spectator/predictions` | `POST` | `Spectator` | `PredictionManagementRequest` | `PredictionStatsResponse` | Đưa ra dự đoán không mất phí. |
| `/api/spectator/predictions/stats/{raceId}` | `GET` | `Spectator` | Không | `PredictionStatsResponse` | Xem thống kê dự đoán đám đông của trận đấu. |
| `/api/public/rankings/jockeys` | `GET` | `AllowAnonymous` | Không | Danh sách `JockeyRankingResponse` | Xem bảng xếp hạng Jockey theo rank points. |
| `/api/public/rankings/horses` | `GET` | `AllowAnonymous` | Không | Danh sách `HorseRankingResponse` | Xem bảng xếp hạng ngựa theo số trận thắng. |
| `/api/public/notifications` | `GET` | Đã đăng nhập (All) | Không | Danh sách `NotificationResponse` | Xem danh sách thông báo hệ thống của cá nhân. |
| `/api/public/notifications/{id}/read` | `PUT` | Đã đăng nhập (All) | Không | `Ok` message | Đánh dấu thông báo đã đọc. |
| `/api/admin/payouts/prizes` | `POST` | `Admin` | `PrizePayoutRequest` | `Ok` message | Chia tiền giải đấu cho Jockey/Owner vô địch. |
| `/api/admin/payouts/trigger/{raceId}`| `POST` | `Admin` | Không | `Ok` message | Trigger thủ công tính cược & trả thưởng cho trận. |

---

## 8. Phân quyền trong task này

Phân quyền hệ thống được thiết lập chặt chẽ bằng thuộc tính `[Authorize]` và `[Authorize(Roles = "...")]` của ASP.NET Core:

*   **API Public**: Các API xem bảng xếp hạng trong `PublicController.cs` (`GET rankings/jockeys` và `GET rankings/horses`) được gắn `[AllowAnonymous]`. Khách vãng lai chưa đăng nhập vẫn gọi được.
*   **API Yêu cầu Đăng nhập (All Roles)**: Các API xem thông báo (`GET /api/public/notifications`) và đọc thông báo (`PUT /api/public/notifications/{id}/read`) chỉ yêu cầu `[Authorize]`. Bất kỳ người dùng nào có Token JWT hợp lệ đều gọi được.
*   **API Giới hạn Role cụ thể**:
    *   Các endpoint trong `SpectatorController` được phân quyền bằng `[Authorize(Roles = "Spectator")]`. Chỉ tài khoản khán giả mới được đặt cược, nạp/rút tiền.
    *   Các endpoint trong `AdminController` được phân quyền bằng `[Authorize(Roles = "Admin")]`. Chỉ quản trị viên mới được phân phối giải thưởng giải đấu và trigger cược.
*   **Các mã lỗi bảo mật thường gặp**:
    *   **Không truyền Token**: Trả về lỗi `401 Unauthorized` (chưa đăng nhập).
    *   **Truyền sai Token/Sai Role** (ví dụ: dùng token Jockey gọi API đặt cược): Trả về lỗi `403 Forbidden` (không có quyền truy cập).
    *   **Đúng token & role**: Trả về `200 OK` hoặc `201 Created` kèm dữ liệu.

---

## 9. Database liên quan

Dưới đây là các bảng dữ liệu mà Module 4 tác động trực tiếp:

| Bảng | Dùng để làm gì trong task này | Thao tác |
| :--- | :--- | :--- |
| `Wallets` | Lưu số dư tài khoản của Spectator, Owner, Jockey. | Read, Insert (khi đăng ký), Update (khi nạp/rút/cược/thưởng) |
| `Transactions` | Lưu lịch sử dòng tiền nạp, rút, cược, thưởng ví. | Insert, Read |
| `Bets` | Lưu thông tin các vé cược khán giả đã đặt. | Insert, Read, Update (trạng thái Won/Lost khi kết thúc đua) |
| `Payouts` | Lưu trữ lịch sử hệ thống phát tiền thưởng cược thành công. | Insert, Read |
| `Predictions` | Lưu trữ minigame dự đoán miễn phí của khán giả. | Insert, Update (nếu đổi dự đoán trước giờ đua), Read |
| `Prizes` | Lưu trữ các cấu hình tiền thưởng giải đấu được Admin thiết lập. | Insert, Read |
| `TournamentPrizePayouts` | Lưu lịch sử phát thưởng giải đấu cho Jockey/Owner. | Insert, Read |
| `Notifications` | Lưu trữ các thông báo hệ thống được gửi cho từng user. | Insert, Read, Update (khi đánh dấu đã đọc) |

**Mối quan hệ khóa ngoại quan trọng**:
*   `Bets` liên kết với `Users` (`UserId`), `Races` (`RaceId`), và `Horses` (`HorseId`). Nhằm mục đích xác định chính xác ai cược trận nào cho ngựa nào.
*   `Payouts` liên kết khóa ngoại cascade với `Bets` (`BetId`). Khi vé cược bị xóa, bản ghi payout liên quan sẽ tự động bị xóa theo.

---

## 10. Cách test bằng Swagger

### Bước 1: Chạy backend
Sử dụng Git Bash tại thư mục root của backend (`backend/`):
```bash
dotnet run --project src/HorseRacing.API -- --urls http://localhost:5001
```

### Bước 2: Mở Swagger trên trình duyệt
Truy cập link: `http://localhost:5001/swagger/index.html`

### Bước 3: Đăng nhập lấy Token
Gọi API `POST /api/auth/login`.

**Tài khoản Spectator mẫu**:
```json
{
  "email": "spectator@gmail.com",
  "password": "123456"
}
```
**Tài khoản Admin mẫu**:
```json
{
  "email": "admin@gmail.com",
  "password": "123456"
}
```

### Bước 4: Click Authorize trên Swagger
Sao chép chuỗi `accessToken` nhận được từ API đăng nhập, nhấn nút **Authorize** ở góc phải phía trên của trang Swagger, nhập:
```text
Bearer <accessToken_của_bạn>
```
Nhấn **Authorize** để xác thực.

### Bước 5: Test các ca kiểm thử (Test Cases)

| Test case | API | Method | Token/Role | Body mẫu | Expected status | Expected result |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **TC01 - Xem số dư** | `/api/spectator/wallet/balance` | `GET` | Spectator Token | Không | `200 OK` | Trả về balance = 0.00 (mặc định ban đầu). |
| **TC02 - Nạp tiền** | `/api/spectator/wallet/deposit` | `POST` | Spectator Token | `{"amount": 100000}` | `200 OK` | Balance tăng lên thành 100000.00. |
| **TC03 - Rút tiền quá số dư**| `/api/spectator/wallet/withdraw` | `POST` | Spectator Token | `{"amount": 200000}` | `400 Bad Request` | Trả về báo lỗi "Insufficient balance." |
| **TC04 - Đặt cược hợp lệ** | `/api/spectator/bets` | `POST` | Spectator Token | `{"raceId": 1, "horseId": 1, "amount": 10000}` | `200 OK` | Tạo vé cược thành công, balance giảm đi 10000. |
| **TC05 - Dự đoán** | `/api/spectator/predictions` | `POST` | Spectator Token | `{"raceId": 1, "predictedWinner": "Owner's Horse"}` | `200 OK` | Đưa ra dự đoán thành công và trả về tỷ lệ đám đông. |
| **TC06 - Trigger trả thưởng**| `/api/admin/payouts/trigger/1` | `POST` | Admin Token | Không | `200 OK` | Hệ thống tính toán và tự động cộng tiền cho Spectator thắng. |
| **TC07 - Phát giải thưởng** | `/api/admin/payouts/prizes` | `POST` | Admin Token | `{"tournamentId": 1, "firstPlacePrize": 10000}` | `200 OK` | Phân phối 70% giải cho HorseOwner và 30% cho Jockey. |
| **TC08 - Xem bảng xếp hạng**| `/api/public/rankings/horses` | `GET` | Không cần token | Không | `200 OK` | Trả về danh sách ngựa được sắp xếp theo số trận thắng. |

---

## 11. Lỗi thường gặp khi test task này

| Lỗi | Nguyên nhân | Cách xử lý |
| :--- | :--- | :--- |
| **401 Unauthorized** | Chưa click Authorize trong Swagger hoặc Token JWT đã hết hạn (hạn dùng mặc định là 60 phút). | Đăng nhập lại qua API `/api/auth/login`, lấy Token mới và cấu hình lại Authorize trong Swagger. |
| **403 Forbidden** | Sử dụng sai role (Ví dụ dùng Token của Jockey/Referee đi gọi API nạp tiền hoặc đặt cược của Spectator). | Thực hiện đăng nhập đúng tài khoản có vai trò `Spectator` (ví dụ: `spectator@gmail.com`) để test. |
| **400 Bad Request (Insufficient balance)** | Khách hàng thực hiện rút tiền hoặc đặt cược số tiền lớn hơn số dư ví hiện có. | Gọi API nạp tiền (`/wallet/deposit`) để tăng số dư ví trước khi thực hiện đặt cược/rút tiền. |
| **400 Bad Request (Race status is not Scheduled)** | Cố tình đặt cược hoặc dự đoán vào một trận đấu đã/đang diễn ra (Status là `Running`, `Finished` hoặc `Completed`). | Chỉ cược vào những trận đua có trạng thái là `"Scheduled"` trong DB. |
| **500 Internal Error (Winner not found)** | Khi chạy trigger cược hoặc trả thưởng giải đấu nhưng trận đua đó chưa cập nhật kết quả kết quả `RaceResult` hoặc tên ngựa không tồn tại. | Đảm bảo bảng `RaceResults` đã có bản ghi kết quả hợp lệ tương ứng với `RaceId` trước khi chạy trigger. |

---

## 12. Task đã hoàn thành và task còn thiếu

| Task | Trạng thái | Ghi chú |
| :--- | :--- | :--- |
| **BE-WALLET-01 (Giao dịch ví)** | **Done** | Nạp tiền, rút tiền, xem lịch sử giao dịch hoạt động chuẩn RESTful và cập nhật ví chính xác. |
| **BE-BET-01 (Đặt cược)** | **Done** | Kiểm tra trạng thái trận đấu "Scheduled", trừ ví và tính toán Odds động Pari-Mutuel chính xác. |
| **BE-BET-02 (Dự đoán)** | **Done** | Lưu trữ dự đoán minigame không mất phí và trả thống kê tỷ lệ đám đông cho khán giả/admin. |
| **BE-PAY-01 (Tính trả thưởng cược)** | **Done** | Chạy trigger tự động phát tiền cho tất cả vé cược thắng dựa trên tỷ lệ Odds lúc đặt cược. |
| **BE-PAY-02 (Trả thưởng giải đấu)** | **Done** | Chia tiền thưởng giải đấu cho HorseOwner (70%) và Jockey (30%) của chú ngựa vô địch trận chung kết. |
| **BE-NOTI-01 (Thông báo hệ thống)** | **Done** | Tự động tạo và lưu trữ thông báo khi nạp/rút/cược/thưởng giải đấu. Đọc thông báo cá nhân hoạt động tốt. |

*Ghi chú*: Module 4 không bị block bởi bất kỳ thành viên nào. Các logic kiểm tra trạng thái Race, lấy thông tin Horse/Jockey đều được viết các câu query linh hoạt trong `BetRepository.cs` để tránh bị block khi module của người khác chưa hoàn thành.

---

## 13. Tôi nên đọc file nào trước?

Để dễ dàng nắm bắt logic code của Module 4, bạn nên đọc các tệp tin theo thứ tự từ tầng ngoài vào tầng cốt lõi như sau:

1.  **Entities (Domain Layer)**: Đọc [Bet.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Bet.cs) để hiểu cấu trúc một bản ghi cược lưu những gì trong database.
2.  **DTOs (Application Layer)**: Đọc [PlaceBetRequest.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/DTOs/PlaceBetRequest.cs) và [BetTicketResponse.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/DTOs/BetTicketResponse.cs) để thấy dữ liệu truyền đi/nhận lại giữa Client và Server.
3.  **Interfaces (Application Layer)**: Xem [IBettingService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Interfaces/IBettingService.cs) và [IBetRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Interfaces/IBetRepository.cs) để hiểu định nghĩa các nghiệp vụ sẽ được triển khai.
4.  **Service Implementation (Application Layer)**: Hạt nhân của module. Đọc [BettingService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Services/BettingService.cs) để xem thuật toán odds Pari-Mutuel và quy trình kiểm tra nghiệp vụ cược.
5.  **Repository (Infrastructure Layer)**: Xem [BetRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/BetRepository.cs) để biết cách truy xuất dữ liệu từ DB SQL Server.
6.  **Controllers (API Layer)**: Xem [SpectatorController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs) để biết API tiếp nhận yêu cầu như thế nào.
7.  **Dependency Injection**: Đọc [ServiceExtensions.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Extensions/ServiceExtensions.cs) để biết cách đăng ký Service chạy thật trong ASP.NET.

---

## 14. Lệnh kiểm tra sau khi code

Mỗi lần cập nhật hoặc thay đổi code, hãy chạy các lệnh sau trong Git Bash tại thư mục root backend (`backend/`):

```bash
# Phục hồi các gói thư viện NuGet NuGet
dotnet restore

# Biên dịch thử toàn bộ mã nguồn
dotnet build

# Chạy thử backend tại cổng 5001
dotnet run --project src/HorseRacing.API -- --urls http://localhost:5001
```

Nếu bạn thay đổi thuộc tính của Entity (ví dụ: thêm cột vào bảng Bet) và cần tạo migration cập nhật database:
```bash
# Tạo migration mới
dotnet ef migrations add <Ten_Migration_Cua_Ban> --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API

# Cập nhật migration vào Database local
dotnet ef database update --project src/HorseRacing.Infrastructure --startup-project src/HorseRacing.API
```

---

## 15. Kết luận

*   **Những gì đã hoàn thành**: Toàn bộ Module 4 đã được lập trình hoàn tất, cấu hình database trơn tru, và chạy mượt mà trên môi trường local.
*   **Có thể kiểm thử những gì**: Có thể test toàn bộ quy trình: Nạp tiền ví -> Đăng nhập lấy Token -> Đặt cược cược ngựa đua -> Dự đoán minigame đám đông -> Trigger trả thưởng cược tự động của Admin -> Chia giải thưởng giải đấu -> Đọc danh sách thông báo hệ thống -> Xem bảng xếp hạng Jockey/Horse công khai.
*   **Bước tiếp theo nên làm**: Bạn hãy chạy backend local lên, đăng nhập thử bằng các tài khoản seeder có sẵn và gọi thử từng API trên Swagger theo đúng hướng dẫn ở **Mục 10** để cảm nhận luồng chạy thực tế của dự án. Điều này sẽ rất hữu ích khi bạn thuyết trình demo bài SWP391 trước hội đồng!
*   **Cần hỏi team**: Thống nhất với team xem khi Member 3 (Hàn) làm xong API công bố kết quả trận đấu (`POST /api/admin/races/{id}/publish`), Hàn sẽ chủ động gọi trực tiếp hàm `IBetPayoutService.ProcessPayoutAsync(raceId)` bên trong API đó để kích hoạt cược tự động mà không cần chạy trigger thủ công nữa.
