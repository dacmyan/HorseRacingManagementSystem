# Đắc Task Implementation Report

## 1. Scope
This report documents the implementation of the following backend tasks for Đắc:
- **Part A:** Automate Bet Payout when the Admin publishes a race result.
- **Part B:** Implement Prediction / Minigame features.
- **Part C:** Resolve the unit test project dependency issues with AutoMapper and the test runner.
- **Part D:** Database migration and compatibility alignment.

## 2. Decision Confirmed
- **Auto Bet Payout:** Approved and fully automated upon race result publication.
- **Prediction Feature:** Re-designed and created in compliance with the singular database naming scheme (`Prediction`).
- **AutoMapper Test Fix:** Resolved directly in `HorseRacing.Tests.csproj` by registering `AutoMapper`, `AutoMapper.Extensions.Microsoft.DependencyInjection`, and the missing `Microsoft.NET.Test.Sdk` references.
- **ID/FK Type:** Restored standard `int` mapping for keys, with exceptions made only for referenced keys that were already established as `bigint`/`long` in the original database tables (`RaceId` and `RaceEntryId`).

## 3. Files Changed
- **Model / Database Mapping:**
  - `[NEW]` [Prediction.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Domain/Entities/Financials/Prediction.cs)
  - `[MODIFY]` [AppDbContext.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Persistence/AppDbContext.cs)
- **Business Logic & Automation:**
  - `[MODIFY]` [RaceResultService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/OfficiatingAndResults/Services/RaceResultService.cs)
  - `[NEW]` [IPredictionRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Interfaces/IPredictionRepository.cs)
  - `[NEW]` [PredictionRepository.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Repositories/PredictionRepository.cs)
  - `[NEW]` [IPredictionService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Interfaces/IPredictionService.cs)
  - `[NEW]` [PredictionService.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/Services/PredictionService.cs)
- **API & Registrations:**
  - `[MODIFY]` [SpectatorController.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Controllers/SpectatorController.cs)
  - `[NEW]` [PredictionDtos.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Application/Features/BettingEngine/DTOs/PredictionDtos.cs)
  - `[MODIFY]` [ServiceExtensions.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.API/Extensions/ServiceExtensions.cs)
  - `[MODIFY]` [DependencyInjection.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/DependencyInjection.cs)
- **Tests & Migrations:**
  - `[MODIFY]` [HorseRacing.Tests.csproj](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/tests/HorseRacing.Tests/HorseRacing.Tests.csproj)
  - `[MODIFY]` [RaceResultServiceTests.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/tests/HorseRacing.Tests/Unit/RaceResultServiceTests.cs)
  - `[NEW]` [20260614094032_AddPredictionFeature.cs](file:///d:/FPTUni/FU_SU2026/SWP391/HorseRacingManagementSystem/backend/src/HorseRacing.Infrastructure/Migrations/20260614094032_AddPredictionFeature.cs) (auto-generated migration file)

## 4. Auto Bet Payout Implementation
- Integrated `_betPayoutService.ProcessPayoutAsync(raceId)` directly inside `PublishResultAsync(long raceId)` in `RaceResultService.cs`.
- The action is idempotent: it only runs once when the race status changes to `Finished`. If called multiple times, the underlying service detects that there are no remaining `Pending` bets on the race and exits safely.
- Added try-catch logs so that any errors in processing payouts or notifications do not crash or abort the publishing transaction, keeping the published race results intact.

## 5. Prediction Implementation
- **Rules applied:**
  - Predictions do not deduct money from user's wallet.
  - Spectators can submit one prediction per scheduled race.
  - Upon publishing result, the winning horse's `RaceEntryId` is used to evaluate predictions. Correct predictions award `Point = 1` and mark status as `Evaluated`. Incorrect ones mark `Point = 0` and status as `Evaluated`.
  - Generates notifications automatically when predictions are submitted and evaluated.
  - Ensures a user can only query their own predictions.

## 6. AutoMapper Test Fix
- Resolved the missing assembly problem in the test project by directly registering the package references in `tests/HorseRacing.Tests/HorseRacing.Tests.csproj`.
- Added `Microsoft.NET.Test.Sdk` version `17.8.0` to establish the correct dependency mapping for target framework `.NET 10.0`.
- Cleaned and rebuilt the solution, restoring 100% of unit tests functionality.

## 7. Migration Created
- Migration `AddPredictionFeature` was generated successfully under `HorseRacing.Infrastructure`.
- Applied successfully to the local database, introducing the `Prediction` table with foreign keys mapped to `AppUser`, `Race`, and `RaceEntry`.
- Unique index `(UserId, RaceId)` was configured on the `Prediction` table to enforce single-prediction rule database-wide.

## 8. API Added/Updated
### Spectator
- `POST /api/Spectator/predictions`: Create a prediction.
- `GET /api/Spectator/predictions/my-predictions`: Retrieve all predictions submitted by the logged-in spectator.
- `GET /api/Spectator/predictions/race/{raceId}`: Retrieve the logged-in spectator's prediction for the specified race.

## 9. Database Tables Affected
- **Table created:** `Prediction` (singular mapping).
- **Columns:**
  - `PredictionId` (INT, PK, Identity)
  - `UserId` (INT, FK -> AppUser)
  - `RaceId` (BIGINT, FK -> Race)
  - `RaceEntryId` (BIGINT, FK -> RaceEntry)
  - `PredictedAt` (DATETIME2)
  - `Status` (NVARCHAR(MAX))
  - `IsCorrect` (BIT, NULL)
  - `Point` (INT)

## 10. int/bigint Compatibility Notes
- To align with existing database types in the codebase, `RaceId` and `RaceEntryId` fields in `Prediction` are mapped as `bigint` / `long` (because the original `Race` and `RaceEntry` entities were already defined with `long` type).
- The primary key `PredictionId` and user association `UserId` are strictly mapped as `int` (to follow the team's standard `int` key conventions).

## 11. Test Results
- All unit tests compile and run cleanly.
- Running `dotnet test` returns **Passed: 9, Failed: 0, Skipped: 0**.

## 12. Known Risks
- Mismatched foreign keys might occur if team members manually change types of primary keys in `Race` or `RaceEntry` from `bigint` to `int` in the future. If such a change is made, the foreign keys in the `Prediction` table and entity must be altered to `int` accordingly.

## 13. Remaining Work
- Implement React frontend interface for spectators to place predictions.

## 14. How to Test Manually
1. Register/Login as a `Spectator`.
2. Retrieve scheduled races, select a race, and place a prediction by sending the `RaceId` and `RaceEntryId`.
3. Submit a race result via Referee/Admin.
4. Publish the result as an Admin.
5. Check `my-predictions` or the database to verify the status has transitioned to `Evaluated` and that `IsCorrect` and `Point` are updated correctly.
