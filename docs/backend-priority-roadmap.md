# Backend Priority Roadmap - Horse Racing Management System

## 1. Mục đích tài liệu

Tài liệu **Backend Priority Roadmap** này được biên soạn bởi Senior Backend Lead & Project Manager nhằm mục đích sắp xếp lại toàn bộ các task phát triển Backend của dự án **Horse Racing Management System (SWP391)**. 

Thay vì phân chia và thực hiện các task độc lập, rời rạc theo từng thành viên (Member-based) dẫn đến tình trạng block lẫn nhau hoặc phải viết code mock (giả định), tài liệu này sẽ tổ chức các task theo **trình tự ưu tiên (Priority)**, **mối quan hệ phụ thuộc (Dependency)**, và **luồng nghiệp vụ thực tế (Feature Flow)**. 

Tài liệu này sẽ giúp:
* **Cả team backend (Triều, Khang, Hàn, Đắc)** biết rõ mình cần làm gì trước, làm gì sau, và khi nào có thể tích hợp code của mình với các thành viên khác.
* **Project Manager** dễ dàng theo dõi tiến độ, điều phối tài nguyên và quản lý các rủi ro liên quan đến nghẽn cổ chai (bottleneck).
* **Đảm bảo tính thống nhất** của kiến trúc Clean Architecture và cấu trúc Database trong suốt quá trình phát triển.

---

## 2. Vấn đề của file chia task hiện tại

Trong tài liệu chia task cũ (`docs/backend-task-division-4-members.md`), các task được chia theo chiều dọc cho 4 thành viên:
* **Triều**: Phụ trách Auth, User, Role.
* **Khang**: Phụ trách Horse, Contract, Registration.
* **Hàn**: Phụ trách Tournaments, Races, Referee, Results.
* **Đắc**: Phụ trách Spectator, Betting, Wallet, Notifications.

Cách chia này đang gặp phải các vấn đề nghiêm trọng sau:
1. **Chia tách quá rời rạc**: Mỗi người tập trung làm module của mình mà không chú ý đến việc đầu vào của mình là đầu ra của người khác.
2. **Sự phụ thuộc dữ liệu (Database Dependency)**: 
   * Quyết định đặt cược của Spectator (`BE-BET-01` do Đắc làm) yêu cầu thông tin về trận đấu đang mở cổng cược (`BE-RACE-02` do Hàn làm) và làn chạy được phân bổ (`BE-RACE-03` do Hàn làm).
   * Thuật toán trả thưởng cược (`BE-PAY-01` do Đắc làm) và trả thưởng giải đấu (`BE-PAY-02` do Đắc làm) bị block hoàn toàn bởi kết quả trận đua (`BE-RESULT-01` do Hàn làm).
   * Quy trình đăng ký giải đấu (`BE-REG-01` do Khang làm) bắt buộc phải có thông tin giải đấu được tạo trước (`BE-RACE-01` do Hàn làm).
   * Quy trình ghép làn chạy (`BE-RACE-03` do Hàn làm) bắt buộc phải có thông tin đăng ký (`BE-REG-01` do Khang làm) và hợp đồng của Jockey (`BE-CONTRACT-02` do Khang làm).
3. **Hiện tượng code giả (Mocking code)**: Do Đắc triển khai module Wallet & Betting rất nhanh nhưng các thực thể như `Registration.cs`, `JockeyContract.cs`, `Round.cs`, `RaceRefereeAssignment.cs` từ Khang và Hàn đang là **file rỗng (0 bytes)**, dẫn đến việc Đắc phải code giả định cấu trúc DB. Khi Khang và Hàn bắt tay vào viết Code thực tế, cấu trúc DB thay đổi sẽ làm vỡ code của Đắc, gây mất thời gian sửa lỗi (refactoring) cực kỳ lớn.
4. **Không có luồng chạy thử (End-to-End Demo)**: Team khó lòng thực hiện được một buổi demo hoàn chỉnh (MVP) vì mỗi người chỉ làm xong một góc nhỏ không khớp nối được với nhau.

---

## 3. Nguyên tắc sắp xếp task

Để giải quyết các vấn đề trên, roadmap này tuân thủ 10 nguyên tắc cốt lõi sau:

1. **Auth & Seed Data là số 1**: Hệ thống phân quyền (Auth/User/Role) và seed data mặc định phải hoạt động ổn định trước tiên để làm nền tảng cho việc kiểm thử Swagger và phân quyền API ở các module sau.
2. **Dữ liệu gốc (Master Data) đi trước**: Các thực thể độc lập như Ngựa (`Horse`), Giải đấu (`Tournament`) phải được CRUD thành công trước khi phát triển các tính năng liên kết chúng.
3. **Quy trình Hợp đồng song song với Đăng ký giải**: Hợp đồng thuê Jockey (`JockeyContract`) và Đăng ký giải đấu (`Registration`) có thể làm song song sau khi có Ngựa và Giải đấu, nhưng phải hoàn thành trước khi lập lịch chi tiết.
4. **Lịch thi đấu & Xếp làn phải có đủ đầu vào**: Không thể gán làn chạy (`RaceEntry`) nếu chưa có danh sách ngựa đăng ký giải đấu (`Registration`) và hợp đồng nài ngựa (`JockeyContract`) ở trạng thái hoạt động (`Active`).
5. **Ví điện tử làm nền tảng tài chính**: Ví (`Wallet`) và nạp/rút tiền (`BE-WALLET-01`) phải được hoàn thiện ở mức cơ bản để Spectator có tiền thực hiện đặt cược.
6. **Đặt cược đi sau Lịch thi đấu**: Chỉ cho phép Spectator đặt cược (`BE-BET-01`) khi đã có lịch thi đấu (`BE-RACE-02`) và danh sách làn chạy (`RaceEntry` từ `BE-RACE-03`).
7. **Kết quả đua kích hoạt Trả thưởng**: Trọng tài cập nhật kết quả và Admin công bố kết quả (`BE-RESULT-01`) là điều kiện bắt buộc (trigger) để chạy nghiệp vụ Payout đặt cược (`BE-PAY-01`) và Payout giải đấu (`BE-PAY-02`).
8. **Báo cáo và Vi phạm của Trọng tài có thể làm sau**: Các báo cáo vi phạm (`BE-REF-01`) và biên bản (`BE-REF-02`) không trực tiếp block luồng cược/trả thưởng chính nên có thể xếp ưu tiên thấp hơn (Phase 4).
9. **Thông báo hệ thống (Notification) làm khung xương trước**: Viết service gửi thông báo cơ bản (`BE-NOTI-01`) trước, sau đó tích hợp chức năng tự động bắn thông báo vào các sự kiện nghiệp vụ (nạp tiền, cược thành công, trả thưởng) ở các phase sau.
10. **Không làm task khi thiếu dữ liệu đầu vào**: Bất kỳ thành viên nào cố tình làm task nâng cao khi task dependency chưa hoàn thành sẽ bị yêu cầu dừng lại để hỗ trợ thành viên khác giải quyết bottleneck.

---

## 4. Dependency map tổng quan

Dưới đây là sơ đồ mối quan hệ phụ thuộc giữa các Task ID thật của dự án:

```text
[Phase 1: Foundation]
BE-AUTH-01 (Cải tiến Login/Register & Khởi tạo Ví mặc định)
   ├── BE-USER-01 (Admin tạo tài khoản nghiệp vụ: Jockey, Owner, Referee)
   │     ├── BE-USER-02 (Cập nhật Profile cá nhân & Chi tiết Jockey/Referee)
   │     └── BE-USER-03 (Admin quản lý danh sách Users & Trạng thái hoạt động)
   ├── BE-WALLET-01 (Giao dịch ví điện tử: Nạp/Rút/Lịch sử)
   └── BE-NOTI-01 (Khung xương thông báo hệ thống - Basic)

[Phase 2: Core Master Data]
BE-AUTH-01 & BE-USER-01
   ├── BE-HORSE-01 (CRUD Ngựa đua của HorseOwner)
   │     └── BE-HORSE-02 (Quản lý tài liệu sức khỏe & chỉ số ngựa - Thấp)
   └── BE-RACE-01 (Admin tạo Giải đấu Tournament & các Vòng đấu Round)
         └── BE-RACE-02 (Lên lịch các cuộc đua Races trong từng Vòng đấu)

[Phase 3: Registration & Assignment Flow]
BE-HORSE-01 & BE-USER-01
   └── BE-CONTRACT-01 (Owner gửi hợp đồng Jockey Contract)
         └── BE-CONTRACT-02 (Jockey phản hồi hợp đồng)
              └── BE-RACE-03 (Lane Assignment - Xếp cặp chạy & Làn chạy) <-- [Cần BE-RACE-02 & BE-REG-01]

BE-HORSE-01 & BE-RACE-01
   └── BE-REG-01 (Owner đăng ký ngựa tham gia Tournament)
         └── BE-RACE-03 (Lane Assignment - Xếp cặp chạy & Làn chạy)

BE-RACE-02 & BE-USER-01
   └── BE-RACE-04 (Admin gán Trọng tài vào trận đua)
         └── BE-REF-01 (Trọng tài kiểm tra & ghi nhận Vi phạm) <-- [Cần BE-RACE-03 để biết đối tượng phạt]
              ├── BE-REF-02 (Trọng tài lập báo cáo tổng kết - Thấp)
              └── BE-RESULT-01 (Ghi nhận & Công bố kết quả đua) <-- [Cần BE-RACE-03]

[Phase 4 & 5: Betting, Operation & Financial Flow]
BE-WALLET-01 & BE-RACE-03
   └── BE-BET-01 (Spectator đặt cược vào làn chạy/ngựa trong trận đấu)
         └── BE-PAY-01 (Tự động trả thưởng đặt cược) <-- [Cần BE-RESULT-01]

BE-RACE-02
   └── BE-BET-02 (Spectator tham gia dự đoán minigame)

BE-RESULT-01 & BE-USER-01 & BE-WALLET-01
   └── BE-PAY-02 (Admin duyệt phát giải thưởng Tournament cho Owner/Jockey)
```

---

## 5. Roadmap theo Phase

Để dự án đi đúng hướng, toàn bộ backend sẽ được chia thành **5 Phase** phát triển tuần tự. Mỗi phase tập trung vào một nhóm chức năng có tính liên kết cao:

| Phase | Mục tiêu | Task ID | Task name | Owner | Dependency | Có thể làm song song với | Output cần có | Ưu tiên |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Phase 1** | **Foundation** (Xây dựng nền tảng xác thực, tài khoản và ví cơ bản) | `BE-AUTH-01` | Cải tiến Login & Register Validation | Triều | Không | `BE-WALLET-01` (Khung xương) | API Login/Register hoạt động đúng validation; đăng ký Spectator tự động tạo ví rỗng trong DB. | **P0** (Critical) |
| | | `BE-USER-01` | Admin tạo tài khoản nghiệp vụ | Triều | `BE-AUTH-01` | `BE-WALLET-01` | Admin tạo được tài khoản Jockey, Referee, Owner kèm bản ghi profile tương ứng. | **P0** (Critical) |
| | | `BE-WALLET-01` | Giao dịch ví điện tử (Nạp/Rút) | Đắc | `BE-AUTH-01` | `BE-USER-01` | API nạp/rút tiền ví hoạt động, ghi log transaction chính xác. | **P0** (Critical) |
| | | `BE-NOTI-01` | Tạo và gửi thông báo hệ thống | Đắc | `BE-AUTH-01` | `BE-USER-02`, `BE-USER-03` | API lấy thông báo và đánh dấu đã đọc. Đã có hàm helper gửi thông báo. | **P1** (High) |
| | | `BE-USER-02` | Xem và cập nhật Profile cá nhân | Triều | `BE-USER-01` | `BE-NOTI-01`, `BE-USER-03` | API lấy và cập nhật profile cá nhân của mọi role. | **P2** (Medium) |
| | | `BE-USER-03` | Admin quản lý danh sách Users | Triều | `BE-USER-01` | `BE-NOTI-01`, `BE-USER-02` | API danh sách user phân trang và khóa/mở khóa tài khoản. | **P2** (Medium) |
| **Phase 2** | **Core Master Data** (Khởi tạo dữ liệu nền về Ngựa, Giải đấu, Lịch đua) | `BE-HORSE-01` | Quản lý Ngựa đua (CRUD Horses) | Khang | `BE-AUTH-01`, `BE-USER-01` | `BE-RACE-01` | API CRUD ngựa thuộc sở hữu của HorseOwner. Tệp `Horse.cs` được map đầy đủ. | **P0** (Critical) |
| | | `BE-RACE-01` | Quản lý Giải đấu (Tournaments) | Hàn | `BE-AUTH-01` | `BE-HORSE-01` | API CRUD Tournament và Round. Hiện thực hóa tệp `Round.cs` và `Tournament.cs`. | **P0** (Critical) |
| | | `BE-RACE-02` | Lên lịch thi đấu (Scheduling Races) | Hàn | `BE-RACE-01` | `BE-HORSE-02` | API tạo cuộc đua và xem lịch thi đấu công khai. | **P1** (High) |
| | | `BE-HORSE-02` | Quản lý tài liệu và chỉ số ngựa | Khang | `BE-HORSE-01` | `BE-RACE-02` | Hiện thực hóa `HorseDocument` và `HorseStatistic` trống để lưu hồ sơ ngựa. | **P3** (Low) |
| **Phase 3** | **Registration & Assignment Flow** (Quy trình kết nối Jockey, Ngựa và lập lịch chi tiết) | `BE-CONTRACT-01`| Gửi hợp đồng thuê nài ngựa | Khang | `BE-HORSE-01`, `BE-USER-01` | `BE-REG-01` | Hiện thực hóa `JockeyContract.cs`. API gửi hợp đồng từ Owner tới Jockey ở trạng thái `Pending`. | **P1** (High) |
| | | `BE-CONTRACT-02`| Phản hồi hợp đồng (Jockey Action) | Khang | `BE-CONTRACT-01` | `BE-REG-01` | API duyệt/từ chối hợp đồng. Hợp đồng chuyển sang `Active`. | **P1** (High) |
| | | `BE-REG-01` | Đăng ký ngựa tham gia Giải đấu | Khang | `BE-HORSE-01`, `BE-RACE-01` | `BE-CONTRACT-01`, `BE-CONTRACT-02` | Hiện thực thực thể `Registration.cs` trống. API Owner đăng ký ngựa vào giải đấu đang mở. | **P1** (High) |
| | | `BE-RACE-04` | Gán trọng tài điều hành cuộc đua | Hàn | `BE-RACE-02`, `BE-USER-01` | `BE-REG-01` | Hiện thực `RaceRefereeAssignment.cs`. API gán trọng tài cho trận đấu. | **P2** (Medium) |
| | | `BE-RACE-03` | Ghép cặp trận đấu (Lane Assignment) | Hàn | `BE-RACE-02`, `BE-REG-01`, `BE-CONTRACT-02` | Không | API xếp ngựa và Jockey vào làn chạy (`RaceEntry`). Kiểm tra hợp đồng Jockey phải `Active`. | **P0** (Critical) |
| **Phase 4** | **Race Operation & Result** (Vận hành trận đua, ghi nhận vi phạm và kết quả) | `BE-REF-01` | Trọng tài giám sát & vi phạm | Hàn | `BE-RACE-04`, `BE-RACE-03` | `BE-BET-02` | API ghi nhận lỗi vi phạm của ngựa/Jockey trong làn chạy. | **P1** (High) |
| | | `BE-RESULT-01`| Ghi nhận & Công bố kết quả đua | Hàn | `BE-REF-01`, `BE-RACE-03` | `BE-REF-02` | API nhập kết quả thứ hạng và API Publish kết quả của Admin (đổi trạng thái trận thành `Finished`). | **P0** (Critical) |
| | | `BE-REF-02` | Trọng tài lập biên bản & Báo cáo | Hàn | `BE-REF-01` | `BE-RESULT-01` | Hiện thực `RefereeReport.cs` trống. API viết báo cáo trận đấu. | **P3** (Low) |
| **Phase 5** | **Betting & Financial Flow** (Ví điện tử, đặt cược, trả thưởng và gửi thông báo) | `BE-BET-01` | Khán giả đặt cược (Place Bets) | Đắc | `BE-WALLET-01`, `BE-RACE-03` | `BE-BET-02` | Hiện thực `Bet.cs` trống. API đặt cược trừ tiền ví, tính odds Pari-Mutuel động và bắn thông báo. | **P0** (Critical) |
| | | `BE-BET-02` | Quản lý dự đoán (Predictions) | Đắc | `BE-RACE-02` | `BE-BET-01` | API dự đoán minigame miễn phí và xem thống kê dự đoán đám đông. | **P2** (Medium) |
| | | `BE-PAY-01` | Tự động trả thưởng đặt cược | Đắc | `BE-BET-01`, `BE-RESULT-01` | `BE-PAY-02` | Tự động cộng tiền ví khán giả thắng cược khi Admin Publish kết quả. | **P0** (Critical) |
| | | `BE-PAY-02` | Trả thưởng giải đấu cho Owner/Jockey | Đắc | `BE-RESULT-01`, `BE-USER-01`, `BE-WALLET-01` | `BE-PAY-01` | Hiện thực `Prize` và `TournamentPrizePayout` trống. API Admin duyệt chi thưởng 70/30. | **P1** (High) |
| | | **Integration** | Tích hợp thông báo toàn hệ thống | Đắc | `BE-NOTI-01` + Các sự kiện Phase 3, 4, 5 | Không | Toàn bộ các hành động nạp/rút, cược, trúng thưởng, có lời mời hợp đồng đều tự động gửi thông báo. | **P1** (High) |

---

## 6. Sắp xếp lại task của từng member theo thứ tự nên làm

Mỗi thành viên cần tuân thủ nghiêm ngặt thứ tự triển khai task cá nhân dưới đây để tránh tự block bản thân và block chéo thành viên khác:

### 6.1. Thứ tự làm việc của Triều (Auth, User & Role)

| Thứ tự | Task ID | Task name | Làm ngay hay chờ | Phụ thuộc task nào | Lý do |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | **BE-AUTH-01** | Cải tiến Login & Register Validation | **Làm ngay** | Không | Đây là nền tảng bảo mật của toàn bộ hệ thống API. Cần làm ngay để team có JWT token kiểm thử Swagger. |
| 2 | **BE-USER-01** | Admin tạo tài khoản nghiệp vụ | **Làm ngay** | `BE-AUTH-01` | Cần có API này để tạo tài khoản Owner, Jockey, Referee cho Khang và Hàn chạy test luồng nghiệp vụ của họ. |
| 3 | **BE-USER-02** | Xem và cập nhật Profile cá nhân | Chờ | `BE-USER-01` | Cần có tài khoản Jockey/Referee được tạo kèm hồ sơ trống từ `BE-USER-01` thì mới có data để GET/PUT profile. |
| 4 | **BE-USER-03** | Admin quản lý danh sách Users | Chờ | `BE-USER-01` | Chỉ làm sau khi hệ thống đã có nhiều user được tạo để kiểm thử tính năng phân trang và khóa tài khoản. |

### 6.2. Thứ tự làm việc của Khang (Horse Owner, Horses & Contracts)

| Thứ tự | Task ID | Task name | Làm ngay hay chờ | Phụ thuộc task nào | Lý do |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | **BE-HORSE-01** | Quản lý Ngựa đua (CRUD Horses) | **Làm ngay** | `BE-AUTH-01`, `BE-USER-01` | Ngựa là thực thể chính. Cần làm ngay sau khi Triều cấp tài khoản HorseOwner để lưu thông tin ngựa vào DB. |
| 2 | **BE-CONTRACT-01** | Gửi hợp đồng thuê nài ngựa (Jockey Contract) | Chờ | `BE-HORSE-01`, `BE-USER-01` | Phải hiện thực hóa thực thể `JockeyContract.cs` trước. Yêu cầu hệ thống phải có ngựa và tài khoản Jockey đang active. |
| 3 | **BE-CONTRACT-02** | Phản hồi hợp đồng (Jockey Action) | Chờ | `BE-CONTRACT-01` | Chỉ Jockey được chỉ định mới phản hồi được hợp đồng đã gửi ở bước trước. |
| 4 | **BE-REG-01** | Đăng ký ngựa tham gia Giải đấu | Chờ | `BE-HORSE-01`, `BE-RACE-01` | Phải hiện thực hóa thực thể `Registration.cs`. Cần giải đấu của Hàn đã tạo (`BE-RACE-01`) để đăng ký ngựa vào. |
| 5 | **BE-HORSE-02** | Quản lý tài liệu và chỉ số ngựa | Chờ (Ưu tiên thấp) | `BE-HORSE-01` | Phải hiện thực hóa `HorseDocument` và `HorseStatistic`. Task này không block ai nên có thể làm sau cùng. |

### 6.3. Thứ tự làm việc của Hàn (Tournaments, Races, Referee & Results)

| Thứ tự | Task ID | Task name | Làm ngay hay chờ | Phụ thuộc task nào | Lý do |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | **BE-RACE-01** | Quản lý Giải đấu (Tournaments) | **Làm ngay** | `BE-AUTH-01` | Giải đấu là thực thể chính quản lý trận đấu. Cần làm sớm để Khang lấy Tournament ID đăng ký ngựa (`BE-REG-01`). |
| 2 | **BE-RACE-02** | Lên lịch thi đấu (Scheduling Races) | **Làm ngay** | `BE-RACE-01` | Cần tạo trận đấu (`Race`) và Round để chuẩn bị ghép làn chạy. |
| 3 | **BE-RACE-04** | Gán trọng tài điều hành cuộc đua | Chờ | `BE-RACE-02`, `BE-USER-01` | Cần có trận đấu (`Race`) và tài khoản trọng tài (`Referee` từ Triều) để gán việc. |
| 4 | **BE-RACE-03** | Ghép cặp trận đấu (Lane Assignment) | **Chờ gấp** | `BE-RACE-02`, `BE-REG-01`, `BE-CONTRACT-02` | **Task cực kỳ quan trọng**. Kết nối Ngựa, Jockey (hợp đồng Active), Giải đấu và Trận đấu vào làn chạy (`RaceEntry`). Quyết định đầu vào cho việc đặt cược và kết quả. |
| 5 | **BE-REF-01** | Trọng tài giám sát & vi phạm | Chờ | `BE-RACE-04`, `BE-RACE-03` | Chỉ trọng tài được gán mới được phạt các ngựa có tên trong danh sách chạy của trận đó. |
| 6 | **BE-RESULT-01** | Ghi nhận & Công bố kết quả đua | Chờ | `BE-REF-01`, `BE-RACE-03` | Kết quả xếp hạng dựa trên danh sách làn chạy. Cần công bố kết quả để kích hoạt tính năng trả thưởng của Đắc. |
| 7 | **BE-REF-02** | Trọng tài lập biên bản & Báo cáo | Chờ (Ưu tiên thấp) | `BE-REF-01` | Biên bản báo cáo không trực tiếp ảnh hưởng đến luồng trả thưởng nên có thể hoàn thiện sau. |

### 6.4. Thứ tự làm việc của Đắc (Spectators, Betting, Wallet & Notification)

*Lưu ý: Đắc đã hoàn thành khung xương và code logic cho Module 4 ở local. Thứ tự dưới đây hướng dẫn Đắc cách **tích hợp (integration) và kiểm thử thực tế** trên cơ sở dữ liệu thật khi các thành viên khác bàn giao code.*

| Thứ tự | Task ID | Task name | Làm ngay hay chờ | Phụ thuộc task nào | Lý do |
| :---: | :--- | :--- | :--- | :--- | :--- |
| 1 | **BE-WALLET-01** | Giao dịch ví điện tử (Nạp/Rút) | **Làm ngay** | `BE-AUTH-01` | Wallet đã có sẵn khung xương trong DB. Cần hoàn thiện API nạp/rút để spectator có số dư thử nghiệm. |
| 2 | **BE-NOTI-01** | Tạo và gửi thông báo hệ thống | **Làm ngay** | `BE-AUTH-01` | Hiện thực hóa thực thể `Notification.cs`. Cần viết hàm helper bắn thông báo để sẵn sàng tích hợp vào các module khác. |
| 3 | **BE-BET-02** | Quản lý dự đoán (Predictions) | Chờ | `BE-RACE-02` | Dự đoán miễn phí chỉ cần có lịch thi đấu (`Race`) của Hàn là có thể chạy thử nghiệm được ngay. |
| 4 | **BE-BET-01** | Khán giả đặt cược (Place Bets) | **Chờ tích hợp** | `BE-WALLET-01`, `BE-RACE-03` | Cần Hàn hoàn thành xếp làn chạy (`BE-RACE-03`) để Spectator cược vào ngựa chạy ở làn cụ thể. Đắc cần bỏ mock dữ liệu làn chạy. |
| 5 | **BE-PAY-01** | Tự động trả thưởng đặt cược | **Chờ tích hợp** | `BE-BET-01`, `BE-RESULT-01` | Kích hoạt tự động khi Admin công bố kết quả trận đua (`BE-RESULT-01` của Hàn). Đắc cần kết nối service trả thưởng vào API Publish của Hàn. |
| 6 | **BE-PAY-02** | Trả thưởng giải đấu cho Owner/Jockey | **Chờ tích hợp** | `BE-RESULT-01`, `BE-USER-01` | Đắc hiện thực hóa thực thể `Prize` và `TournamentPrizePayout`. API này chỉ chạy được khi đã xác định được người thắng cuộc giải đấu và có ví của Jockey/Owner. |

---

## 7. Task nào nên làm song song

Để tối ưu hóa thời gian dự án SWP391 và tránh việc cả team phải ngồi chờ một người, các nhóm task sau có thể được phân công làm song song:

| Cặp task có thể làm song song | Điều kiện song song | Vì sao song song được? |
| :--- | :--- | :--- |
| `BE-HORSE-01` (Khang) <br>và `BE-RACE-01` (Hàn) | Triều đã hoàn thành `BE-AUTH-01` và seed đầy đủ Role/User Admin và Owner mẫu. | Khang làm việc trên thực thể `Horse` thuộc module Quản lý Ngựa, Hàn làm việc trên thực thể `Tournament`/`Round` thuộc module Giải đấu. Hai module này độc lập, không chung bảng DB nên không sợ conflict. |
| `BE-WALLET-01` (Đắc) <br>và `BE-HORSE-01` (Khang) / `BE-RACE-01` (Hàn) | Triều hoàn tất `BE-AUTH-01` cấp JWT Token Spectator. | Giao dịch ví điện tử của Spectator hoàn toàn độc lập với thông tin ngựa hay giải đấu ở giai đoạn khai báo ban đầu. |
| `BE-CONTRACT-01` (Khang) <br>và `BE-RACE-02` (Hàn) | Đã có thực thể `Horse` (Khang) và `Tournament` (Hàn) trong cơ sở dữ liệu. | Hợp đồng thuê Jockey và việc lên lịch trận đua sơ bộ không phụ thuộc lẫn nhau. Khang làm việc với Jockey và Owner, Hàn làm việc với Admin lập lịch. |
| `BE-NOTI-01` (Đắc) <br>và các task CRUD ở Phase 2 | Đắc đã khởi tạo bảng `Notifications` thành công. | Đắc viết khung dịch vụ thông báo trong khi Khang và Hàn đang hoàn thiện dữ liệu master ngựa/giải đấu. |

---

## 8. Task nào bị block

Đây là các điểm nghẽn (bottleneck) nghiêm trọng nhất trong dự án. Thành viên phụ trách task block bắt buộc phải hoàn thành đúng hạn, nếu không toàn bộ luồng phía sau sẽ dừng hoạt động:

| Task bị block | Bị block bởi | Lý do bị block | Giải pháp làm tạm trước (Mocking) |
| :--- | :--- | :--- | :--- |
| **BE-REG-01** (Khang - Đăng ký giải) | `BE-RACE-01` (Hàn - Tạo giải đấu) | Không thể đăng ký ngựa tham gia giải đấu nếu Admin chưa tạo Tournament trong cơ sở dữ liệu. | Hàn có thể seed sẵn một Tournament mẫu trong `DataSeeder` để Khang dùng test API đăng ký ngựa. |
| **BE-RACE-03** (Hàn - Làn chạy) | `BE-REG-01` (Khang - Đăng ký ngựa) <br>`BE-CONTRACT-02` (Khang - Duyệt hợp đồng) | Không thể xếp làn chạy cho ngựa nếu chủ ngựa chưa đăng ký vào giải đấu, hoặc Jockey chưa đồng ý ký hợp đồng hoạt động (`Active`). | Khang và Hàn thống nhất cấu trúc bảng `Registration` và `JockeyContract`. Khang viết class thực thể C# trước để tạo DB, Hàn có thể mock bản ghi trong DB để chạy thử thuật toán chia làn. |
| **BE-BET-01** (Đắc - Đặt cược) | `BE-RACE-03` (Hàn - Xếp làn chạy) | Spectator chỉ cược được khi biết cuộc đua có những ngựa nào chạy ở làn nào. | Đắc có thể viết API nhận cược với validation đơn giản là `RaceId` tồn tại, chưa cần check chi tiết làn chạy. |
| **BE-REF-01** (Hàn - Lỗi vi phạm) | `BE-RACE-04` (Hàn - Gán trọng tài) <br>`BE-RACE-03` (Hàn - Xếp làn chạy) | Trọng tài chỉ phạt được ngựa/Jockey tham gia trận đua đó nếu họ được gán quyền và trận đấu có danh sách làn chạy thực tế. | Mock trọng tài chính là admin để bypass check phân quyền gán trọng tài. |
| **BE-RESULT-01** (Hàn - Kết quả) | `BE-RACE-03` (Hàn - Xếp làn chạy) | Không thể xếp hạng 1, 2, 3 cho ngựa nếu trận đấu không có danh sách làn chạy (`RaceEntry`) tham gia ban đầu. | Mock danh sách ngựa chiến thắng bằng text thuần thay vì tham chiếu khóa ngoại tới bảng `RaceEntry` (Tuy nhiên cần sửa lại đúng quan hệ DB khi tích hợp). |
| **BE-PAY-01** (Đắc - Trả thưởng cược) | `BE-RESULT-01` (Hàn - Công bố kết quả) | Không thể tính toán ai thắng cược và chia tiền nếu Admin chưa thực hiện Publish kết quả trận đấu để chuyển trạng thái cuộc đua sang `Finished`. | Đắc tự viết một API trigger giả lập `/api/admin/payouts/trigger/{raceId}` để tự test luồng trả thưởng của mình mà không cần chờ API Publish của Hàn. (Đắc hiện đã làm điều này ở `AdminController.cs`). |
| **BE-PAY-02** (Đắc - Trả giải đấu) | `BE-RESULT-01` (Hàn - Công bố kết quả) | Chỉ phát giải đấu khi giải đấu kết thúc và tìm ra chú ngựa đoạt giải nhất chung cuộc. | Đắc có thể tự tạo bản ghi `RaceResult` thắng cuộc bằng tay trong DB SQL Server để test API phát giải thưởng. |

---

## 9. MVP tối thiểu để demo sớm

Để chứng minh hệ thống hoạt động tốt với giảng viên hướng dẫn môn SWP391 và đảm bảo tiến độ, team sẽ thực hiện tích hợp và demo theo 5 mốc MVP lũy tiến sau:

### Demo 1: Auth & Role (Hoàn thành Phase 1)
* **Luồng chạy**: 
  1. Người dùng đăng ký tài khoản Spectator -> hệ thống tự động tạo ví balance = 0.
  2. Khán giả đăng nhập -> nhận JWT Token chứa vai trò `Spectator`.
  3. Admin đăng nhập -> nhận token `Admin`.
  4. Admin dùng token gọi API `POST /api/admin/accounts` tạo thành công tài khoản `HorseOwner`, `Jockey`, `Referee` mới trong hệ thống.
  5. Test thử tính năng phân quyền: Jockey dùng token của mình gọi API Admin tạo tài khoản -> hệ thống trả về `403 Forbidden` (Đạt yêu cầu bảo mật).

### Demo 2: Horse + Tournament (Hoàn thành Phase 2)
* **Luồng chạy**:
  1. HorseOwner đăng nhập -> Gọi API `POST /api/horses` tạo mới chú ngựa "Red Lightning".
  2. Admin đăng nhập -> Gọi API `POST /api/tournaments` tạo giải đấu "Hanoi Derby 2026" và thiết lập 3 vòng đấu (Rounds).
  3. Admin gọi API `POST /api/races` lên lịch trận đua "Race 1 - Qualifying" thuộc vòng 1.
  4. Mọi người (Public) gọi API `GET /api/races/schedule` thấy trận đấu sắp diễn ra.

### Demo 3: Registration + Race Entry (Hoàn thành Phase 3)
* **Luồng chạy**:
  1. HorseOwner gọi API `POST /api/jockey-contracts` gửi lời mời thuê Jockey "Nguyễn Văn A" điều khiển ngựa "Red Lightning".
  2. Jockey đăng nhập -> Gọi API phản hồi đồng đồng ý -> Hợp đồng chuyển sang `Active`.
  3. HorseOwner gọi API `POST /api/registrations` đăng ký ngựa "Red Lightning" vào giải đấu "Hanoi Derby 2026".
  4. Admin đóng cổng đăng ký, gọi API `POST /api/races/{id}/entries` gán ngựa "Red Lightning" và Jockey "Nguyễn Văn A" chạy ở Làn số 2 của trận "Race 1".

### Demo 4: Betting + Wallet (Hoàn thành Phase 5 - Phần Đặt cược)
* **Luồng chạy**:
  1. Spectator nạp 500,000đ vào ví qua API nạp tiền giả lập.
  2. Spectator xem lịch sử giao dịch ví thấy có bản ghi nạp tiền.
  3. Spectator xem lịch thi đấu và thông tin làn chạy của trận "Race 1" -> Thấy ngựa "Red Lightning" ở làn số 2.
  4. Spectator gọi API `POST /api/bets` cược 100,000đ vào ngựa "Red Lightning". Số dư ví giảm còn 400,000đ. Ghi nhận giao dịch cược và gửi thông báo cược thành công.

### Demo 5: Result + Payout (Hoàn thành Phase 4 & Phase 5 - Phần Trả thưởng)
* **Luồng chạy**:
  1. Trọng tài gọi API `POST /api/referee/results` cập nhật ngựa "Red Lightning" về Nhất.
  2. Admin duyệt và gọi API `POST /api/admin/races/{id}/publish` công bố kết quả trận đấu. Trạng thái cuộc đua chuyển sang `Finished`.
  3. Hệ thống tự động trigger luồng Payout cược: Tính toán Odds Pari-Mutuel động và cộng tiền thắng cược vào ví Spectator. Spectator nhận được thông báo trúng cược và số dư ví tăng lên.
  4. Admin gọi API `POST /api/admin/payouts/prizes` phân phối giải thưởng giải đấu cho HorseOwner sở hữu ngựa và Jockey điều khiển ngựa vô địch.

---

## 10. Sprint plan đề xuất

Kế hoạch phát triển backend được đề xuất chia làm **4 Sprint (Mỗi Sprint kéo dài 1 tuần)**:

| Sprint | Thời gian | Mục tiêu | Task cần hoàn thành | Phụ trách | Kế hoạch tích hợp & Demo |
| :---: | :---: | :--- | :--- | :---: | :--- |
| **Sprint 1** | Tuần 1 | Ổn định nền tảng Auth, khởi tạo ví mặc định, hoàn thiện thực thể DB và các API tài khoản. | * `BE-AUTH-01`: Validation Login/Register<br>* `BE-USER-01`: Admin tạo tài khoản nghiệp vụ<br>* `BE-WALLET-01`: Giao dịch ví (Nạp/Rút)<br>* `BE-NOTI-01`: Cơ chế thông báo hệ thống (Khung xương) | Triều, Đắc | **Demo 1**: Test toàn bộ tính năng đăng nhập, đăng ký và phân quyền các vai trò trên Swagger. |
| **Sprint 2** | Tuần 2 | Tạo dữ liệu gốc ngựa, giải đấu, lịch đua và cập nhật hồ sơ cá nhân. | * `BE-HORSE-01`: CRUD Ngựa đua<br>* `BE-RACE-01`: CRUD Giải đấu & Vòng đấu<br>* `BE-RACE-02`: Lịch thi đấu (Races)<br>* `BE-USER-02`: Xem/cập nhật Profile<br>* `BE-USER-03`: Quản lý danh sách User | Khang, Hàn, Triều | **Demo 2**: Tạo thành công ngựa, giải đấu và lên lịch đua sơ bộ. |
| **Sprint 3** | Tuần 3 | Hoàn thành quy trình ký hợp đồng thuê Jockey, đăng ký giải đấu và xếp làn chạy chi tiết. | * `BE-CONTRACT-01`: Gửi hợp đồng thuê Jockey<br>* `BE-CONTRACT-02`: Duyệt hợp đồng<br>* `BE-REG-01`: Đăng ký giải đấu<br>* `BE-RACE-04`: Gán trọng tài cuộc đua<br>* `BE-RACE-03`: Ghép cặp làn chạy (`RaceEntry`) | Khang, Hàn | **Demo 3**: Đăng ký ngựa vào giải đấu và xếp làn chạy chi tiết (Kết nối thành công Khang và Hàn). |
| **Sprint 4** | Tuần 4 | Vận hành cuộc đua, nhập kết quả và kích hoạt luồng tự động trả thưởng, cược và thông báo. | * `BE-REF-01`: Ghi nhận lỗi vi phạm<br>* `BE-RESULT-01`: Nhập & Publish kết quả<br>* `BE-BET-01`: Khán giả đặt cược thực tế<br>* `BE-BET-02`: Minigame dự đoán<br>* `BE-PAY-01`: Trả thưởng cược tự động<br>* `BE-PAY-02`: Trả giải đấu 70/30 | Hàn, Đắc | **Demo 4 & 5**: Khán giả đặt cược tiền ví thật -> chạy đua -> công bố kết quả -> tự động cộng tiền ví trả thưởng và gửi thông báo chúc mừng. |

---

## 11. Checklist trước khi một task được bắt đầu

Trước khi kéo một task từ cột "To Do" sang "In Progress", thành viên phụ trách phải tự tích đủ các điều kiện sau:

* [ ] **Kiểm tra Dependency**: Tất cả các Task ID phụ thuộc (ghi trong cột Dependency của Phase) đã được merge thành công vào nhánh chính `main`.
* [ ] **Đồng bộ Database**: Thực thể Entity (C# class) liên quan đến task đã được định nghĩa đầy đủ và cấu hình khóa ngoại trong `AppDbContext.cs`.
* [ ] **Cập nhật Migrations**: Đã chạy lệnh `dotnet ef database update` ở máy local để đồng bộ DB mới nhất, không bị lỗi sai cấu hình bảng.
* [ ] **Bypass Auth**: Swagger Authorize đã hoạt động đúng với role của API sắp viết (nếu API yêu cầu phân quyền).
* [ ] **Đồng bộ code mới**: Đã checkout sang nhánh mới từ nhánh `main` mới nhất (`git pull origin main`).
* [ ] **Xác nhận ranh giới code**: Đã đọc phần giới hạn file cần sửa (Mục 9 trong file chia task cũ) để đảm bảo không viết đè lên file của người khác.

---

## 12. Checklist khi task hoàn thành

Một task được coi là hoàn thành và sẵn sàng tạo PR merge vào `main` khi:

* [ ] **Biên dịch không lỗi**: Chạy lệnh `dotnet build` tại thư mục root của backend thành công, không có bất kỳ Warning/Error đỏ nào.
* [ ] **Đúng chuẩn Swagger**: API hiển thị chính xác trên giao diện Swagger UI với đầy đủ HTTP Method (GET/POST/PUT/DELETE) và DTO body mẫu.
* [ ] **Test phân quyền (Authorization)**:
  * Gọi API không truyền Token -> Trả về `401 Unauthorized`.
  * Dùng Token của Role không được phép -> Trả về `403 Forbidden`.
  * Dùng đúng token -> Trả về kết quả nghiệp vụ thành công.
* [ ] **Validation đầu vào**: Đã cấu hình FluentValidation hoặc kiểm tra thủ công. Truyền dữ liệu trống hoặc sai định dạng (ví dụ: cược tiền âm, rút tiền quá số dư) trả về đúng mã lỗi `400 Bad Request` kèm thông điệp rõ ràng.
* [ ] **Bảo mật dữ liệu**: API không trả ra các thông tin nhạy cảm của User (PasswordHash, mã bảo mật ví...).
* [ ] **Đúng mã trạng thái RESTful**: Trả về `200 OK` (lấy/sửa thành công), `201 Created` (tạo mới thành công), `404 Not Found` (không thấy bản ghi).
* [ ] **Không có log/comment rác**: Đã xóa toàn bộ code thử nghiệm, `Console.WriteLine` dư thừa và các chú thích rác trước khi commit.
* [ ] **Push code an toàn**: Đã merge nhánh `main` mới nhất vào branch của mình ở local, giải quyết hết conflict (nếu có), sau đó mới push lên GitHub và tạo Pull Request.

---

## 13. Đề xuất sửa lại file task division cũ

Để file chia task cũ (`docs/backend-task-division-4-members.md`) trở nên hữu ích và đồng bộ với roadmap mới này, chúng ta nên thực hiện các cải tiến sau:

1. **Thêm phần "Priority Roadmap Overview" ở đầu file**: Dẫn link tới file roadmap mới này để người đọc nắm được luồng tổng quan trước khi đi vào chi tiết task của từng người.
2. **Bổ sung các cột thông tin mới vào bảng task chi tiết của từng member**:
   * **Phase**: Cho biết task đó thuộc Phase mấy (từ 1 đến 5).
   * **Blocked By**: Ghi rõ mã Task ID cụ thể đang block task này (ví dụ: `BE-BET-01` bị block bởi `BE-RACE-03`).
   * **Can Start When**: Điều kiện cụ thể để bắt đầu code (ví dụ: "Khi thực thể `JockeyContract` đã được Khang định nghĩa và cập nhật DB").
   * **Integration Point**: Chỉ ra API/Service của thành viên khác mà task này sẽ gọi hoặc tích hợp chung (ví dụ: `BE-PAY-01` sẽ tích hợp vào hàm `PublishRaceResultAsync` của Hàn).
3. **Thêm cột "Status"**: Để cập nhật trạng thái thực tế của task (`To Do`, `In Progress`, `Done`, `Blocked`).

---

## 14. Báo cáo kế hoạch tích hợp và hành động tiếp theo

### 14.1. Phân tích trạng thái hiện tại (Bottleneck lớn nhất)
* **Thành viên Đắc (Ví, Cược, Trả thưởng)** đã hoàn thành phần lớn logic code của Module 4 ở local. Tuy nhiên, các service của Đắc đang gọi đến các Entity mock/rỗng của Khang và Hàn.
* **Bottleneck lớn nhất**: Khang và Hàn đang có các file Entity rỗng trong DB. Việc này khiến toàn bộ phần tích hợp cược và trả thưởng bị treo.

### 14.2. Khuyến nghị hành động khẩn cấp
1. **Triều (Auth)**: Cần hoàn thiện ngay API Admin tạo tài khoản (`BE-USER-01`) để Khang và Hàn có tài khoản test.
2. **Khang (Horses/Contracts) & Hàn (Tournaments/Races)**:
   * **Nhiệm vụ đầu tiên**: Không cần viết API Service vội. Khang phải định nghĩa ngay cấu trúc các class thực thể: `JockeyContract.cs`, `Registration.cs`. Hàn định nghĩa ngay `Round.cs` và `RaceRefereeAssignment.cs`.
   * **Migration DB**: Đưa các thuộc tính DB này vào `AppDbContext.cs`, tạo migration mới để cập nhật Database chung của team. Việc này giúp Đắc xóa bỏ code mock và kết nối trực tiếp vào các bảng DB thật.
3. **Đắc (Wallet/Betting)**:
   * Tập trung ổn định và test kỹ API Ví (`BE-WALLET-01`) với DB thực tế.
   * Hỗ trợ Khang và Hàn định nghĩa các mối quan hệ khóa ngoại (Foreign Keys) trong database để tránh xung đột khi map bảng.
4. **Hàn (Races)**: Sau khi map xong thực thể, ưu tiên viết API Lập lịch đua (`BE-RACE-02`) và Xếp làn chạy (`BE-RACE-03`) để Đắc có dữ liệu đầu vào chạy luồng đặt cược thật.

Mọi thành viên hãy checkout nhánh tính năng của mình từ `main` mới nhất, phối hợp chặt chẽ trên kênh chat chung để thực hiện tích hợp trơn tru theo đúng Sprint Plan đã đề ra!
