# Backend FE Blockers - Fix Report

## Overview
This document summarizes the fixes applied to resolve the 11 backend blockers reported by the frontend team. All missing endpoints, missing fields, and database schema issues have been addressed.

## 1. Database & Migrations
- **Status:** All 11 migrations, including `AddPredictionFeature`, `AddRefereeReport`, `AddLaneNoToRaceEntry`, and `AddRefereeAssignmentFeature`, were already applied to the database. The database is up-to-date and no migrations were pending.
- **Entity Types:** The `Prediction` entity was verified to use `long` for `RaceId` and `RaceEntryId`. This was preserved to avoid breaking the existing database schema, as `Race` and `RaceEntry` also use `long` for their primary keys.
- **Missing Tables:** The `Prediction`, `RaceEntry` (with `LaneNo`), `RefereeReport`, and `RaceRefereeAssignment` tables exist and map correctly to the updated entities.

## 2. API Endpoints Implemented

### Admin Registration Approve/Reject
- **Endpoints Added:**
  - `PUT /api/admin/registrations/{id}/approve`
  - `PUT /api/admin/registrations/{id}/reject`
- **Implementation:** Added `ReviewRegistrationAsync` to `IRegistrationService` and `RegistrationService`. The Admin controller now securely updates the registration status to either 'Approved' or 'Rejected'.

### Public Live Race API
- **Endpoint Added:** `GET /api/public/races/live`
- **Implementation:** Added to `PublicController` with `[AllowAnonymous]`. It queries the `Races` table for statuses `Live`, `Running`, `InProgress`, or `Ongoing` and returns essential race information (RaceId, Name, TournamentName, StartTime, Status).

### Admin Activity Log
- **Endpoint Added:** `GET /api/admin/activity-log`
- **Implementation:** Added a lightweight query in `AdminController` that aggregates recent records from Users, Registrations, Bets, Notifications, and WalletTransactions. The results are unified into a standard format (`{ Type, Title, Description, CreatedAt }`), sorted by `CreatedAt` descending, and capped at the top 50 entries.

### Owner Dashboard API
- **Endpoint Added:** `GET /api/owner/dashboard`
- **Implementation:** Added to `OwnerController`. It calculates metrics specific to the authenticated owner:
  - `HorseCount`: Total horses owned.
  - `RegistrationCount`: Total tournament registrations for the owner's horses.
  - `ActiveRaceCount`: Races currently live/running where the owner's horses are participating.
  - `UpcomingRaceCount`: Scheduled races where the owner's horses are participating.
  - `TotalPrizeAmount`: Total sum of all tournament prize payouts awarded to the owner.

### Admin Dropdown Options
- **Endpoints Added:**
  - `GET /api/admin/users/options`
  - `GET /api/admin/horses/options`
- **Implementation:** Added to `AdminController`. These provide lightweight arrays of `{ Id, Label, Extra }` for frontend dropdowns (e.g., when filing referee reports or assigning users).

## 3. DTO Enhancements
- **BetTicketResponse:** Added `PotentialPayout` (decimal), `ActualPayout` (decimal?), and `PayoutStatus` (string?) to provide clear financial outcomes to the frontend.
- **BettingService:** Updated `PlaceBetAsync` and `GetMyBetsAsync` to automatically compute `PotentialPayout` (`Amount * Odds`) and determine `PayoutStatus` and `ActualPayout` based on the bet's status (Pending, Won, Lost, PaidOut).

## 4. Test Data Seed Script
- **Script Created:** `docs/sql/seed-be-fe-blockers-test-data.sql`
- **Details:** Contains `IF NOT EXISTS` guarded INSERT statements to safely generate dummy data for:
  - Pending Registrations
  - RaceEntries with LaneNo
  - RaceRefereeAssignments
  - RaceViolations
  - RefereeReports
  - Predictions

## Verification
- **Build Status:** `dotnet build` succeeds with 0 errors.
- **Review:** All required logic for the 11 blockers is now integrated directly into the Clean Architecture layers (`API`, `Application`, `Domain`). The FE team should now be unblocked.
