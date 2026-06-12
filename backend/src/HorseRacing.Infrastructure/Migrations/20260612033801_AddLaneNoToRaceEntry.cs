using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLaneNoToRaceEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_Horse_HorseId1",
                table: "RaceEntry");

            migrationBuilder.DropIndex(
                name: "IX_RaceEntry_HorseId1",
                table: "RaceEntry");

            migrationBuilder.DropIndex(
                name: "IX_RaceEntry_RaceId",
                table: "RaceEntry");

            migrationBuilder.DropColumn(
                name: "HorseId1",
                table: "RaceEntry");

            migrationBuilder.AddColumn<int>(
                name: "LaneNo",
                table: "RaceEntry",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_RaceId_LaneNo",
                table: "RaceEntry",
                columns: new[] { "RaceId", "LaneNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RaceEntry_RaceId_LaneNo",
                table: "RaceEntry");

            migrationBuilder.DropColumn(
                name: "LaneNo",
                table: "RaceEntry");

            migrationBuilder.AddColumn<int>(
                name: "HorseId1",
                table: "RaceEntry",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_HorseId1",
                table: "RaceEntry",
                column: "HorseId1");

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntry_RaceId",
                table: "RaceEntry",
                column: "RaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntry_Horse_HorseId1",
                table: "RaceEntry",
                column: "HorseId1",
                principalTable: "Horse",
                principalColumn: "Id");
        }
    }
}
