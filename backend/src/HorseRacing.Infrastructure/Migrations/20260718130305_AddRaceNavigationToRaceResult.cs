using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRaceNavigationToRaceResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RaceResult_RaceId",
                table: "RaceResult",
                column: "RaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceResult_Race_RaceId",
                table: "RaceResult",
                column: "RaceId",
                principalTable: "Race",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaceResult_Race_RaceId",
                table: "RaceResult");

            migrationBuilder.DropIndex(
                name: "IX_RaceResult_RaceId",
                table: "RaceResult");
        }
    }
}
