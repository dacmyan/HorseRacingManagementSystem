using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHorseStatsAndRaceEntryPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FinishPosition",
                table: "RaceEntry",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinishTime",
                table: "RaceEntry",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageTime",
                table: "Horse",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RecentAverageTime",
                table: "Horse",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WinRate",
                table: "Horse",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RaceEntryId",
                table: "Bet",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bet_RaceEntryId",
                table: "Bet",
                column: "RaceEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bet_RaceEntry_RaceEntryId",
                table: "Bet",
                column: "RaceEntryId",
                principalTable: "RaceEntry",
                principalColumn: "RaceEntryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bet_RaceEntry_RaceEntryId",
                table: "Bet");

            migrationBuilder.DropIndex(
                name: "IX_Bet_RaceEntryId",
                table: "Bet");

            migrationBuilder.DropColumn(
                name: "FinishPosition",
                table: "RaceEntry");

            migrationBuilder.DropColumn(
                name: "FinishTime",
                table: "RaceEntry");

            migrationBuilder.DropColumn(
                name: "AverageTime",
                table: "Horse");

            migrationBuilder.DropColumn(
                name: "RecentAverageTime",
                table: "Horse");

            migrationBuilder.DropColumn(
                name: "WinRate",
                table: "Horse");

            migrationBuilder.DropColumn(
                name: "RaceEntryId",
                table: "Bet");
        }
    }
}
