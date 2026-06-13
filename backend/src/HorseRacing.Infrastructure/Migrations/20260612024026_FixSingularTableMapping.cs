using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixSingularTableMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Horses_HorseId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Races_RaceId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Users_UserId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_HorseDocuments_Horses_HorseId",
                table: "HorseDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_Horses_Registrations_RegistrationId",
                table: "Horses");

            migrationBuilder.DropForeignKey(
                name: "FK_Horses_Users_OwnerId",
                table: "Horses");

            migrationBuilder.DropForeignKey(
                name: "FK_HorseStatistics_Horses_HorseId",
                table: "HorseStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_JockeyContracts_Horses_HorseId",
                table: "JockeyContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_JockeyContracts_Users_JockeyId",
                table: "JockeyContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_JockeyContracts_Users_OwnerId",
                table: "JockeyContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_JockeyProfiles_Users_UserId",
                table: "JockeyProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Payouts_Bets_BetId",
                table: "Payouts");

            migrationBuilder.DropForeignKey(
                name: "FK_Prizes_Tournaments_TournamentId",
                table: "Prizes");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntries_Horses_HorseId",
                table: "RaceEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntries_Horses_HorseId1",
                table: "RaceEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntries_Races_RaceId",
                table: "RaceEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntries_Users_JockeyId",
                table: "RaceEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceRefereeAssignments_Races_RaceId",
                table: "RaceRefereeAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceRefereeAssignments_RefereeProfiles_RefereeId",
                table: "RaceRefereeAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Races_Rounds_RoundId",
                table: "Races");

            migrationBuilder.DropForeignKey(
                name: "FK_RefereeProfiles_Users_UserId",
                table: "RefereeProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Horses_HorseId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Tournaments_TournamentId",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId",
                table: "Rounds");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPrizePayouts_Tournaments_TournamentId",
                table: "TournamentPrizePayouts");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPrizePayouts_Users_UserId",
                table: "TournamentPrizePayouts");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_WalletId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Violations_Races_RaceId",
                table: "Violations");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_Users_UserId",
                table: "Wallets");

            migrationBuilder.RenameTable(
                name: "Tournaments",
                newName: "Tournament");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournament");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Violations",
                table: "Violations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentPrizePayouts",
                table: "TournamentPrizePayouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rounds",
                table: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Registrations",
                table: "Registrations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefereeProfiles",
                table: "RefereeProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Races",
                table: "Races");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RaceResults",
                table: "RaceResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RaceRefereeAssignments",
                table: "RaceRefereeAssignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RaceEntries",
                table: "RaceEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prizes",
                table: "Prizes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Predictions",
                table: "Predictions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payouts",
                table: "Payouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JockeyProfiles",
                table: "JockeyProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JockeyContracts",
                table: "JockeyContracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HorseStatistics",
                table: "HorseStatistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Horses",
                table: "Horses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HorseDocuments",
                table: "HorseDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bets",
                table: "Bets");



            migrationBuilder.RenameTable(
                name: "Wallets",
                newName: "Wallet");

            migrationBuilder.RenameTable(
                name: "Violations",
                newName: "RaceViolation");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "AppUser");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "WalletTransaction");

            migrationBuilder.RenameTable(
                name: "TournamentPrizePayouts",
                newName: "TournamentPrizePayout");

            migrationBuilder.RenameTable(
                name: "Rounds",
                newName: "Round");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Role");

            migrationBuilder.RenameTable(
                name: "Registrations",
                newName: "Registration");

            migrationBuilder.RenameTable(
                name: "RefereeProfiles",
                newName: "RefereeProfile");

            migrationBuilder.RenameTable(
                name: "Races",
                newName: "Race");

            migrationBuilder.RenameTable(
                name: "RaceResults",
                newName: "RaceResult");

            migrationBuilder.RenameTable(
                name: "RaceRefereeAssignments",
                newName: "RaceRefereeAssignment");

            migrationBuilder.RenameTable(
                name: "RaceEntries",
                newName: "RaceEntry");

            migrationBuilder.RenameTable(
                name: "Prizes",
                newName: "Prize");

            migrationBuilder.RenameTable(
                name: "Predictions",
                newName: "Prediction");

            migrationBuilder.RenameTable(
                name: "Payouts",
                newName: "Payout");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "Notification");

            migrationBuilder.RenameTable(
                name: "JockeyProfiles",
                newName: "JockeyProfile");

            migrationBuilder.RenameTable(
                name: "JockeyContracts",
                newName: "JockeyContract");

            migrationBuilder.RenameTable(
                name: "HorseStatistics",
                newName: "HorseStatistic");

            migrationBuilder.RenameTable(
                name: "Horses",
                newName: "Horse");

            migrationBuilder.RenameTable(
                name: "HorseDocuments",
                newName: "HorseDocument");

            migrationBuilder.RenameTable(
                name: "Bets",
                newName: "Bet");

            migrationBuilder.RenameIndex(
                name: "IX_Wallets_UserId",
                table: "Wallet",
                newName: "IX_Wallet_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Violations_RaceId",
                table: "RaceViolation",
                newName: "IX_RaceViolation_RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_RoleId",
                table: "AppUser",
                newName: "IX_AppUser_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_WalletId",
                table: "WalletTransaction",
                newName: "IX_WalletTransaction_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPrizePayouts_UserId",
                table: "TournamentPrizePayout",
                newName: "IX_TournamentPrizePayout_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPrizePayouts_TournamentId",
                table: "TournamentPrizePayout",
                newName: "IX_TournamentPrizePayout_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Rounds_TournamentId",
                table: "Round",
                newName: "IX_Round_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Registrations_TournamentId",
                table: "Registration",
                newName: "IX_Registration_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Registrations_HorseId",
                table: "Registration",
                newName: "IX_Registration_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_RefereeProfiles_UserId",
                table: "RefereeProfile",
                newName: "IX_RefereeProfile_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Races_RoundId",
                table: "Race",
                newName: "IX_Race_RoundId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceRefereeAssignments_RefereeId",
                table: "RaceRefereeAssignment",
                newName: "IX_RaceRefereeAssignment_RefereeId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceRefereeAssignments_RaceId",
                table: "RaceRefereeAssignment",
                newName: "IX_RaceRefereeAssignment_RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceEntries_RaceId",
                table: "RaceEntry",
                newName: "IX_RaceEntry_RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceEntries_JockeyId",
                table: "RaceEntry",
                newName: "IX_RaceEntry_JockeyId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceEntries_HorseId1",
                table: "RaceEntry",
                newName: "IX_RaceEntry_HorseId1");

            migrationBuilder.RenameIndex(
                name: "IX_RaceEntries_HorseId",
                table: "RaceEntry",
                newName: "IX_RaceEntry_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_Prizes_TournamentId",
                table: "Prize",
                newName: "IX_Prize_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Payouts_BetId",
                table: "Payout",
                newName: "IX_Payout_BetId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "Notification",
                newName: "IX_Notification_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_JockeyProfiles_UserId",
                table: "JockeyProfile",
                newName: "IX_JockeyProfile_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_JockeyContracts_OwnerId",
                table: "JockeyContract",
                newName: "IX_JockeyContract_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_JockeyContracts_JockeyId",
                table: "JockeyContract",
                newName: "IX_JockeyContract_JockeyId");

            migrationBuilder.RenameIndex(
                name: "IX_JockeyContracts_HorseId",
                table: "JockeyContract",
                newName: "IX_JockeyContract_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_HorseStatistics_HorseId",
                table: "HorseStatistic",
                newName: "IX_HorseStatistic_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_Horses_RegistrationId",
                table: "Horse",
                newName: "IX_Horse_RegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_Horses_OwnerId",
                table: "Horse",
                newName: "IX_Horse_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_HorseDocuments_HorseId",
                table: "HorseDocument",
                newName: "IX_HorseDocument_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_Bets_UserId",
                table: "Bet",
                newName: "IX_Bet_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Bets_RaceId",
                table: "Bet",
                newName: "IX_Bet_RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_Bets_HorseId",
                table: "Bet",
                newName: "IX_Bet_HorseId");



            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournament",
                table: "Tournament",
                column: "TournamentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wallet",
                table: "Wallet",
                column: "WalletId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RaceViolation",
                table: "RaceViolation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUser",
                table: "AppUser",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WalletTransaction",
                table: "WalletTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentPrizePayout",
                table: "TournamentPrizePayout",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Round",
                table: "Round",
                column: "RoundId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Role",
                table: "Role",
                column: "RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Registration",
                table: "Registration",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefereeProfile",
                table: "RefereeProfile",
                column: "RefereeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Race",
                table: "Race",
                column: "RaceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RaceResult",
                table: "RaceResult",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RaceRefereeAssignment",
                table: "RaceRefereeAssignment",
                column: "AssignmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RaceEntry",
                table: "RaceEntry",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prize",
                table: "Prize",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prediction",
                table: "Prediction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payout",
                table: "Payout",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notification",
                table: "Notification",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JockeyProfile",
                table: "JockeyProfile",
                column: "JockeyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JockeyContract",
                table: "JockeyContract",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HorseStatistic",
                table: "HorseStatistic",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Horse",
                table: "Horse",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HorseDocument",
                table: "HorseDocument",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bet",
                table: "Bet",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEDQl5GtU6lXMcfTRWSHnVz0AYO/DcesuZdc26L/lMBXERd4y/Ry6KFBnnz3OcVLaiA==");

            migrationBuilder.UpdateData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEIU26C1yHTr+reVJwGZJlfXXlzLjVh3ejJo2RFDSgGRU3CQn2cp7nJJIfJ/m6XAm8Q==");

            migrationBuilder.UpdateData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEETNsom+QeZLwGVXPatXiCXnefira62DVHwdLXgHMsgdrwu7Xy3znwO8r72lMU5Mog==");

            migrationBuilder.UpdateData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEN/0ABNONq8lx6xDeeREIStGpg1ENtlTUY2BLUaNLzyO6hnHvqnoA4Qd+WqRgJadgg==");

            migrationBuilder.UpdateData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 5,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEJKV8UzjVkRV+m8NEvHJi0leRC9j/XUkH19a15TyFSoRWPiDhJfEqjAl28C89Ikcbg==");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUser_Role_RoleId",
                table: "AppUser",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bet_AppUser_UserId",
                table: "Bet",
                column: "UserId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bet_Horse_HorseId",
                table: "Bet",
                column: "HorseId",
                principalTable: "Horse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bet_Race_RaceId",
                table: "Bet",
                column: "RaceId",
                principalTable: "Race",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Horse_AppUser_OwnerId",
                table: "Horse",
                column: "OwnerId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Horse_Registration_RegistrationId",
                table: "Horse",
                column: "RegistrationId",
                principalTable: "Registration",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HorseDocument_Horse_HorseId",
                table: "HorseDocument",
                column: "HorseId",
                principalTable: "Horse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HorseStatistic_Horse_HorseId",
                table: "HorseStatistic",
                column: "HorseId",
                principalTable: "Horse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JockeyContract_AppUser_JockeyId",
                table: "JockeyContract",
                column: "JockeyId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JockeyContract_AppUser_OwnerId",
                table: "JockeyContract",
                column: "OwnerId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JockeyContract_Horse_HorseId",
                table: "JockeyContract",
                column: "HorseId",
                principalTable: "Horse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JockeyProfile_AppUser_UserId",
                table: "JockeyProfile",
                column: "UserId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_AppUser_UserId",
                table: "Notification",
                column: "UserId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payout_Bet_BetId",
                table: "Payout",
                column: "BetId",
                principalTable: "Bet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prize_Tournament_TournamentId",
                table: "Prize",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Race_Round_RoundId",
                table: "Race",
                column: "RoundId",
                principalTable: "Round",
                principalColumn: "RoundId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntry_AppUser_JockeyId",
                table: "RaceEntry",
                column: "JockeyId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntry_Horse_HorseId",
                table: "RaceEntry",
                column: "HorseId",
                principalTable: "Horse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntry_Horse_HorseId1",
                table: "RaceEntry",
                column: "HorseId1",
                principalTable: "Horse",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntry_Race_RaceId",
                table: "RaceEntry",
                column: "RaceId",
                principalTable: "Race",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceRefereeAssignment_Race_RaceId",
                table: "RaceRefereeAssignment",
                column: "RaceId",
                principalTable: "Race",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceRefereeAssignment_RefereeProfile_RefereeId",
                table: "RaceRefereeAssignment",
                column: "RefereeId",
                principalTable: "RefereeProfile",
                principalColumn: "RefereeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceViolation_Race_RaceId",
                table: "RaceViolation",
                column: "RaceId",
                principalTable: "Race",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefereeProfile_AppUser_UserId",
                table: "RefereeProfile",
                column: "UserId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_Horse_HorseId",
                table: "Registration",
                column: "HorseId",
                principalTable: "Horse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_Tournament_TournamentId",
                table: "Registration",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Round_Tournament_TournamentId",
                table: "Round",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPrizePayout_AppUser_UserId",
                table: "TournamentPrizePayout",
                column: "UserId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPrizePayout_Tournament_TournamentId",
                table: "TournamentPrizePayout",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallet_AppUser_UserId",
                table: "Wallet",
                column: "UserId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_Wallet_WalletId",
                table: "WalletTransaction",
                column: "WalletId",
                principalTable: "Wallet",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUser_Role_RoleId",
                table: "AppUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Bet_AppUser_UserId",
                table: "Bet");

            migrationBuilder.DropForeignKey(
                name: "FK_Bet_Horse_HorseId",
                table: "Bet");

            migrationBuilder.DropForeignKey(
                name: "FK_Bet_Race_RaceId",
                table: "Bet");

            migrationBuilder.DropForeignKey(
                name: "FK_Horse_AppUser_OwnerId",
                table: "Horse");

            migrationBuilder.DropForeignKey(
                name: "FK_Horse_Registration_RegistrationId",
                table: "Horse");

            migrationBuilder.DropForeignKey(
                name: "FK_HorseDocument_Horse_HorseId",
                table: "HorseDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_HorseStatistic_Horse_HorseId",
                table: "HorseStatistic");

            migrationBuilder.DropForeignKey(
                name: "FK_JockeyContract_AppUser_JockeyId",
                table: "JockeyContract");

            migrationBuilder.DropForeignKey(
                name: "FK_JockeyContract_AppUser_OwnerId",
                table: "JockeyContract");

            migrationBuilder.DropForeignKey(
                name: "FK_JockeyContract_Horse_HorseId",
                table: "JockeyContract");

            migrationBuilder.DropForeignKey(
                name: "FK_JockeyProfile_AppUser_UserId",
                table: "JockeyProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_AppUser_UserId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_Payout_Bet_BetId",
                table: "Payout");

            migrationBuilder.DropForeignKey(
                name: "FK_Prize_Tournament_TournamentId",
                table: "Prize");

            migrationBuilder.DropForeignKey(
                name: "FK_Race_Round_RoundId",
                table: "Race");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_AppUser_JockeyId",
                table: "RaceEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_Horse_HorseId",
                table: "RaceEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_Horse_HorseId1",
                table: "RaceEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_Race_RaceId",
                table: "RaceEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceRefereeAssignment_Race_RaceId",
                table: "RaceRefereeAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceRefereeAssignment_RefereeProfile_RefereeId",
                table: "RaceRefereeAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceViolation_Race_RaceId",
                table: "RaceViolation");

            migrationBuilder.DropForeignKey(
                name: "FK_RefereeProfile_AppUser_UserId",
                table: "RefereeProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_Horse_HorseId",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_Tournament_TournamentId",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_Round_Tournament_TournamentId",
                table: "Round");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPrizePayout_AppUser_UserId",
                table: "TournamentPrizePayout");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPrizePayout_Tournament_TournamentId",
                table: "TournamentPrizePayout");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallet_AppUser_UserId",
                table: "Wallet");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_Wallet_WalletId",
                table: "WalletTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournament",
                table: "Tournament");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WalletTransaction",
                table: "WalletTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wallet",
                table: "Wallet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentPrizePayout",
                table: "TournamentPrizePayout");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Round",
                table: "Round");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Role",
                table: "Role");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Registration",
                table: "Registration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefereeProfile",
                table: "RefereeProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RaceViolation",
                table: "RaceViolation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RaceResult",
                table: "RaceResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RaceRefereeAssignment",
                table: "RaceRefereeAssignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RaceEntry",
                table: "RaceEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Race",
                table: "Race");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prize",
                table: "Prize");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prediction",
                table: "Prediction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payout",
                table: "Payout");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notification",
                table: "Notification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JockeyProfile",
                table: "JockeyProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JockeyContract",
                table: "JockeyContract");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HorseStatistic",
                table: "HorseStatistic");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HorseDocument",
                table: "HorseDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Horse",
                table: "Horse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bet",
                table: "Bet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUser",
                table: "AppUser");



            migrationBuilder.RenameTable(
                name: "WalletTransaction",
                newName: "Transactions");

            migrationBuilder.RenameTable(
                name: "Wallet",
                newName: "Wallets");

            migrationBuilder.RenameTable(
                name: "TournamentPrizePayout",
                newName: "TournamentPrizePayouts");

            migrationBuilder.RenameTable(
                name: "Round",
                newName: "Rounds");

            migrationBuilder.RenameTable(
                name: "Role",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "Registration",
                newName: "Registrations");

            migrationBuilder.RenameTable(
                name: "RefereeProfile",
                newName: "RefereeProfiles");

            migrationBuilder.RenameTable(
                name: "RaceViolation",
                newName: "Violations");

            migrationBuilder.RenameTable(
                name: "RaceResult",
                newName: "RaceResults");

            migrationBuilder.RenameTable(
                name: "RaceRefereeAssignment",
                newName: "RaceRefereeAssignments");

            migrationBuilder.RenameTable(
                name: "RaceEntry",
                newName: "RaceEntries");

            migrationBuilder.RenameTable(
                name: "Race",
                newName: "Races");

            migrationBuilder.RenameTable(
                name: "Prize",
                newName: "Prizes");

            migrationBuilder.RenameTable(
                name: "Prediction",
                newName: "Predictions");

            migrationBuilder.RenameTable(
                name: "Payout",
                newName: "Payouts");

            migrationBuilder.RenameTable(
                name: "Notification",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "JockeyProfile",
                newName: "JockeyProfiles");

            migrationBuilder.RenameTable(
                name: "JockeyContract",
                newName: "JockeyContracts");

            migrationBuilder.RenameTable(
                name: "HorseStatistic",
                newName: "HorseStatistics");

            migrationBuilder.RenameTable(
                name: "HorseDocument",
                newName: "HorseDocuments");

            migrationBuilder.RenameTable(
                name: "Horse",
                newName: "Horses");

            migrationBuilder.RenameTable(
                name: "Bet",
                newName: "Bets");

            migrationBuilder.RenameTable(
                name: "AppUser",
                newName: "Users");

            migrationBuilder.RenameIndex(
                name: "IX_WalletTransaction_WalletId",
                table: "Transactions",
                newName: "IX_Transactions_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_Wallet_UserId",
                table: "Wallets",
                newName: "IX_Wallets_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPrizePayout_UserId",
                table: "TournamentPrizePayouts",
                newName: "IX_TournamentPrizePayouts_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPrizePayout_TournamentId",
                table: "TournamentPrizePayouts",
                newName: "IX_TournamentPrizePayouts_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Round_TournamentId",
                table: "Rounds",
                newName: "IX_Rounds_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Registration_TournamentId",
                table: "Registrations",
                newName: "IX_Registrations_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Registration_HorseId",
                table: "Registrations",
                newName: "IX_Registrations_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_RefereeProfile_UserId",
                table: "RefereeProfiles",
                newName: "IX_RefereeProfiles_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceViolation_RaceId",
                table: "Violations",
                newName: "IX_Violations_RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceRefereeAssignment_RefereeId",
                table: "RaceRefereeAssignments",
                newName: "IX_RaceRefereeAssignments_RefereeId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceRefereeAssignment_RaceId",
                table: "RaceRefereeAssignments",
                newName: "IX_RaceRefereeAssignments_RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceEntry_RaceId",
                table: "RaceEntries",
                newName: "IX_RaceEntries_RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceEntry_JockeyId",
                table: "RaceEntries",
                newName: "IX_RaceEntries_JockeyId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceEntry_HorseId1",
                table: "RaceEntries",
                newName: "IX_RaceEntries_HorseId1");

            migrationBuilder.RenameIndex(
                name: "IX_RaceEntry_HorseId",
                table: "RaceEntries",
                newName: "IX_RaceEntries_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_Race_RoundId",
                table: "Races",
                newName: "IX_Races_RoundId");

            migrationBuilder.RenameIndex(
                name: "IX_Prize_TournamentId",
                table: "Prizes",
                newName: "IX_Prizes_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_Payout_BetId",
                table: "Payouts",
                newName: "IX_Payouts_BetId");

            migrationBuilder.RenameIndex(
                name: "IX_Notification_UserId",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_JockeyProfile_UserId",
                table: "JockeyProfiles",
                newName: "IX_JockeyProfiles_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_JockeyContract_OwnerId",
                table: "JockeyContracts",
                newName: "IX_JockeyContracts_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_JockeyContract_JockeyId",
                table: "JockeyContracts",
                newName: "IX_JockeyContracts_JockeyId");

            migrationBuilder.RenameIndex(
                name: "IX_JockeyContract_HorseId",
                table: "JockeyContracts",
                newName: "IX_JockeyContracts_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_HorseStatistic_HorseId",
                table: "HorseStatistics",
                newName: "IX_HorseStatistics_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_HorseDocument_HorseId",
                table: "HorseDocuments",
                newName: "IX_HorseDocuments_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_Horse_RegistrationId",
                table: "Horses",
                newName: "IX_Horses_RegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_Horse_OwnerId",
                table: "Horses",
                newName: "IX_Horses_OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Bet_UserId",
                table: "Bets",
                newName: "IX_Bets_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Bet_RaceId",
                table: "Bets",
                newName: "IX_Bets_RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_Bet_HorseId",
                table: "Bets",
                newName: "IX_Bets_HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_AppUser_RoleId",
                table: "Users",
                newName: "IX_Users_RoleId");



            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets",
                column: "WalletId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentPrizePayouts",
                table: "TournamentPrizePayouts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rounds",
                table: "Rounds",
                column: "RoundId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Registrations",
                table: "Registrations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefereeProfiles",
                table: "RefereeProfiles",
                column: "RefereeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Violations",
                table: "Violations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RaceResults",
                table: "RaceResults",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RaceRefereeAssignments",
                table: "RaceRefereeAssignments",
                column: "AssignmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RaceEntries",
                table: "RaceEntries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Races",
                table: "Races",
                column: "RaceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prizes",
                table: "Prizes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Predictions",
                table: "Predictions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payouts",
                table: "Payouts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JockeyProfiles",
                table: "JockeyProfiles",
                column: "JockeyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JockeyContracts",
                table: "JockeyContracts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HorseStatistics",
                table: "HorseStatistics",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HorseDocuments",
                table: "HorseDocuments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Horses",
                table: "Horses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bets",
                table: "Bets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournament",
                column: "TournamentId");

            migrationBuilder.RenameTable(
                name: "Tournament",
                newName: "Tournaments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEBJni0PXeYtMwPxuCquVlJp/vfZ3T5NnXbL4ITA+wICO09HwDpr8wKiZe/SHw7zHkg==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEI/YtzgdjI6CI7pmJwDWxYUsgvNE0VR1INfQBA/HJrBpt4H/1jRb9TsnuI3ay69gdQ==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEFzjyabBrnWWObzTRPf2QN1AOsPQA+iBv+MMBSsnI/l9WZm49fn2/+X0qHIelygN0w==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEB8SFpN5wOe/nlVsq/Su4b3pvp8aPd1qHOdXKQWPj+98Ca9A9W3ieFp9C05tYbk3Gg==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEBhri9xP2iah7DDqBdF6EqjQJdezvKu5jRXQ5fSaSCvMZxQ2E4ErUcb1W8vQRsSTJQ==");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Horses_HorseId",
                table: "Bets",
                column: "HorseId",
                principalTable: "Horses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Races_RaceId",
                table: "Bets",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Users_UserId",
                table: "Bets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HorseDocuments_Horses_HorseId",
                table: "HorseDocuments",
                column: "HorseId",
                principalTable: "Horses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Horses_Registrations_RegistrationId",
                table: "Horses",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Horses_Users_OwnerId",
                table: "Horses",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HorseStatistics_Horses_HorseId",
                table: "HorseStatistics",
                column: "HorseId",
                principalTable: "Horses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JockeyContracts_Horses_HorseId",
                table: "JockeyContracts",
                column: "HorseId",
                principalTable: "Horses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JockeyContracts_Users_JockeyId",
                table: "JockeyContracts",
                column: "JockeyId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JockeyContracts_Users_OwnerId",
                table: "JockeyContracts",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JockeyProfiles_Users_UserId",
                table: "JockeyProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payouts_Bets_BetId",
                table: "Payouts",
                column: "BetId",
                principalTable: "Bets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prizes_Tournaments_TournamentId",
                table: "Prizes",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntries_Horses_HorseId",
                table: "RaceEntries",
                column: "HorseId",
                principalTable: "Horses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntries_Horses_HorseId1",
                table: "RaceEntries",
                column: "HorseId1",
                principalTable: "Horses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntries_Races_RaceId",
                table: "RaceEntries",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntries_Users_JockeyId",
                table: "RaceEntries",
                column: "JockeyId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceRefereeAssignments_Races_RaceId",
                table: "RaceRefereeAssignments",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceRefereeAssignments_RefereeProfiles_RefereeId",
                table: "RaceRefereeAssignments",
                column: "RefereeId",
                principalTable: "RefereeProfiles",
                principalColumn: "RefereeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Races_Rounds_RoundId",
                table: "Races",
                column: "RoundId",
                principalTable: "Rounds",
                principalColumn: "RoundId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefereeProfiles_Users_UserId",
                table: "RefereeProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Horses_HorseId",
                table: "Registrations",
                column: "HorseId",
                principalTable: "Horses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Tournaments_TournamentId",
                table: "Registrations",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rounds_Tournaments_TournamentId",
                table: "Rounds",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPrizePayouts_Tournaments_TournamentId",
                table: "TournamentPrizePayouts",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPrizePayouts_Users_UserId",
                table: "TournamentPrizePayouts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_WalletId",
                table: "Transactions",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Violations_Races_RaceId",
                table: "Violations",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_Users_UserId",
                table: "Wallets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
