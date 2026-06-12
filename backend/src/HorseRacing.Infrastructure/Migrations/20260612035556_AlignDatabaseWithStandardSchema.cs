using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignDatabaseWithStandardSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JockeyContract_AppUser_OwnerId",
                table: "JockeyContract");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_AppUser_JockeyId",
                table: "RaceEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_Horse_HorseId",
                table: "RaceEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_Race_RaceId",
                table: "RaceEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RefereeReport_Race_RaceId",
                table: "RefereeReport");

            migrationBuilder.DropForeignKey(
                name: "FK_RefereeReport_RefereeProfile_RefereeId",
                table: "RefereeReport");

            migrationBuilder.DropTable(
                name: "Prediction");

            migrationBuilder.DropIndex(
                name: "IX_Round_TournamentId",
                table: "Round");

            migrationBuilder.DropIndex(
                name: "IX_Registration_TournamentId",
                table: "Registration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefereeReport",
                table: "RefereeReport");

            migrationBuilder.DropIndex(
                name: "IX_RefereeReport_RefereeId",
                table: "RefereeReport");

            migrationBuilder.DropIndex(
                name: "IX_RaceRefereeAssignment_RaceId",
                table: "RaceRefereeAssignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RaceEntry",
                table: "RaceEntry");

            migrationBuilder.DropIndex(
                name: "IX_Prize_TournamentId",
                table: "Prize");

            migrationBuilder.DropIndex(
                name: "IX_JockeyContract_OwnerId",
                table: "JockeyContract");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RefereeReport");

            migrationBuilder.DropColumn(
                name: "RefereeId",
                table: "RefereeReport");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RaceEntry");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "JockeyContract");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "WalletTransaction",
                newName: "TransactionId");

            migrationBuilder.RenameColumn(
                name: "RaceId",
                table: "RefereeReport",
                newName: "AssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_RefereeReport_RaceId",
                table: "RefereeReport",
                newName: "IX_RefereeReport_AssignmentId");

            migrationBuilder.RenameColumn(
                name: "HorseId",
                table: "RaceEntry",
                newName: "RegistrationId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceEntry_HorseId",
                table: "RaceEntry",
                newName: "IX_RaceEntry_RegistrationId");

            migrationBuilder.RenameColumn(
                name: "Rank",
                table: "Prize",
                newName: "RankPosition");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "JockeyContract",
                newName: "ContractId");

            migrationBuilder.AddColumn<int>(
                name: "BetId",
                table: "WalletTransaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewayTransactionId",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayoutId",
                table: "WalletTransaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrizePayoutId",
                table: "WalletTransaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReportId",
                table: "RefereeReport",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "ViolationNote",
                table: "RefereeReport",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "JockeyId",
                table: "RaceEntry",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<long>(
                name: "RaceEntryId",
                table: "RaceEntry",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentOdds",
                table: "RaceEntry",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WinningProbability",
                table: "RaceEntry",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TournamentId",
                table: "JockeyContract",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefereeReport",
                table: "RefereeReport",
                column: "ReportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RaceEntry",
                table: "RaceEntry",
                column: "RaceEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransaction_BetId",
                table: "WalletTransaction",
                column: "BetId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransaction_PayoutId",
                table: "WalletTransaction",
                column: "PayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransaction_PrizePayoutId",
                table: "WalletTransaction",
                column: "PrizePayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Round_TournamentId_RoundNumber",
                table: "Round",
                columns: new[] { "TournamentId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registration_TournamentId_HorseId",
                table: "Registration",
                columns: new[] { "TournamentId", "HorseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaceRefereeAssignment_RaceId_RefereeId",
                table: "RaceRefereeAssignment",
                columns: new[] { "RaceId", "RefereeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_RaceId_RegistrationId",
                table: "RaceEntry",
                columns: new[] { "RaceId", "RegistrationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prize_TournamentId_RankPosition",
                table: "Prize",
                columns: new[] { "TournamentId", "RankPosition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JockeyContract_TournamentId_HorseId_JockeyId",
                table: "JockeyContract",
                columns: new[] { "TournamentId", "HorseId", "JockeyId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JockeyContract_Tournament_TournamentId",
                table: "JockeyContract",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntry_JockeyProfile_JockeyId",
                table: "RaceEntry",
                column: "JockeyId",
                principalTable: "JockeyProfile",
                principalColumn: "JockeyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntry_Race_RaceId",
                table: "RaceEntry",
                column: "RaceId",
                principalTable: "Race",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntry_Registration_RegistrationId",
                table: "RaceEntry",
                column: "RegistrationId",
                principalTable: "Registration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefereeReport_RaceRefereeAssignment_AssignmentId",
                table: "RefereeReport",
                column: "AssignmentId",
                principalTable: "RaceRefereeAssignment",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_Bet_BetId",
                table: "WalletTransaction",
                column: "BetId",
                principalTable: "Bet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_Payout_PayoutId",
                table: "WalletTransaction",
                column: "PayoutId",
                principalTable: "Payout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransaction_TournamentPrizePayout_PrizePayoutId",
                table: "WalletTransaction",
                column: "PrizePayoutId",
                principalTable: "TournamentPrizePayout",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JockeyContract_Tournament_TournamentId",
                table: "JockeyContract");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_JockeyProfile_JockeyId",
                table: "RaceEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_Race_RaceId",
                table: "RaceEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_Registration_RegistrationId",
                table: "RaceEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_RefereeReport_RaceRefereeAssignment_AssignmentId",
                table: "RefereeReport");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_Bet_BetId",
                table: "WalletTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_Payout_PayoutId",
                table: "WalletTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransaction_TournamentPrizePayout_PrizePayoutId",
                table: "WalletTransaction");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransaction_BetId",
                table: "WalletTransaction");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransaction_PayoutId",
                table: "WalletTransaction");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransaction_PrizePayoutId",
                table: "WalletTransaction");

            migrationBuilder.DropIndex(
                name: "IX_Round_TournamentId_RoundNumber",
                table: "Round");

            migrationBuilder.DropIndex(
                name: "IX_Registration_TournamentId_HorseId",
                table: "Registration");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefereeReport",
                table: "RefereeReport");

            migrationBuilder.DropIndex(
                name: "IX_RaceRefereeAssignment_RaceId_RefereeId",
                table: "RaceRefereeAssignment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RaceEntry",
                table: "RaceEntry");

            migrationBuilder.DropIndex(
                name: "IX_RaceEntry_RaceId_RegistrationId",
                table: "RaceEntry");

            migrationBuilder.DropIndex(
                name: "IX_Prize_TournamentId_RankPosition",
                table: "Prize");

            migrationBuilder.DropIndex(
                name: "IX_JockeyContract_TournamentId_HorseId_JockeyId",
                table: "JockeyContract");

            migrationBuilder.DropColumn(
                name: "BetId",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "GatewayTransactionId",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "PayoutId",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "PrizePayoutId",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "RefereeReport");

            migrationBuilder.DropColumn(
                name: "ViolationNote",
                table: "RefereeReport");

            migrationBuilder.DropColumn(
                name: "RaceEntryId",
                table: "RaceEntry");

            migrationBuilder.DropColumn(
                name: "CurrentOdds",
                table: "RaceEntry");

            migrationBuilder.DropColumn(
                name: "WinningProbability",
                table: "RaceEntry");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "JockeyContract");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "WalletTransaction",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AssignmentId",
                table: "RefereeReport",
                newName: "RaceId");

            migrationBuilder.RenameIndex(
                name: "IX_RefereeReport_AssignmentId",
                table: "RefereeReport",
                newName: "IX_RefereeReport_RaceId");

            migrationBuilder.RenameColumn(
                name: "RegistrationId",
                table: "RaceEntry",
                newName: "HorseId");

            migrationBuilder.RenameIndex(
                name: "IX_RaceEntry_RegistrationId",
                table: "RaceEntry",
                newName: "IX_RaceEntry_HorseId");

            migrationBuilder.RenameColumn(
                name: "RankPosition",
                table: "Prize",
                newName: "Rank");

            migrationBuilder.RenameColumn(
                name: "ContractId",
                table: "JockeyContract",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RefereeReport",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "RefereeId",
                table: "RefereeReport",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "JockeyId",
                table: "RaceEntry",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RaceEntry",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "JockeyContract",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefereeReport",
                table: "RefereeReport",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RaceEntry",
                table: "RaceEntry",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Prediction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PredictedWinner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RaceId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prediction", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Round_TournamentId",
                table: "Round",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_TournamentId",
                table: "Registration",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_RefereeReport_RefereeId",
                table: "RefereeReport",
                column: "RefereeId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceRefereeAssignment_RaceId",
                table: "RaceRefereeAssignment",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Prize_TournamentId",
                table: "Prize",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_JockeyContract_OwnerId",
                table: "JockeyContract",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_JockeyContract_AppUser_OwnerId",
                table: "JockeyContract",
                column: "OwnerId",
                principalTable: "AppUser",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_RaceEntry_Race_RaceId",
                table: "RaceEntry",
                column: "RaceId",
                principalTable: "Race",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefereeReport_Race_RaceId",
                table: "RefereeReport",
                column: "RaceId",
                principalTable: "Race",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefereeReport_RefereeProfile_RefereeId",
                table: "RefereeReport",
                column: "RefereeId",
                principalTable: "RefereeProfile",
                principalColumn: "RefereeId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
