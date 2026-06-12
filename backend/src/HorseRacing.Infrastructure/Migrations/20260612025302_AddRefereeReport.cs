using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefereeReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefereeReport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RaceId = table.Column<long>(type: "bigint", nullable: false),
                    RefereeId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedUserId = table.Column<int>(type: "int", nullable: true),
                    ReportedHorseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefereeReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefereeReport_AppUser_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "AppUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefereeReport_Horse_ReportedHorseId",
                        column: x => x.ReportedHorseId,
                        principalTable: "Horse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefereeReport_Race_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Race",
                        principalColumn: "RaceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RefereeReport_RefereeProfile_RefereeId",
                        column: x => x.RefereeId,
                        principalTable: "RefereeProfile",
                        principalColumn: "RefereeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEL3MNWtRhvfQxwwwIqhshbFLU07YLYFvXEUG66I7640i3DJ4x1GfHVWHTyt5JhDkvg==");

            migrationBuilder.UpdateData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEGyQeVxp9OVePd4lUW2yUnlaIslc2Hb1UqZouwu1xjOlZgLmdErHbODNrBy9QAk7Fg==");

            migrationBuilder.UpdateData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEG7QrTR2HaVVSpLEoFY6nOXiEX/NozyjPG00YA6FAebngMyKqLiQ72/SXsHxSwGLmA==");

            migrationBuilder.UpdateData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEAdQI0nKh2I0WQ5tIG7x7PcS/FtpxA2nTEzJmsOIfzFlBOZM0COQr0/xSZll9vhcwQ==");

            migrationBuilder.UpdateData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 5,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAELF89zW+xZNN798lwcDM2PkiAZJVpjK8/S+59Yst+SzV3QlCb5IuBvLXib+/BFmcqA==");

            migrationBuilder.CreateIndex(
                name: "IX_RefereeReport_RaceId",
                table: "RefereeReport",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_RefereeReport_RefereeId",
                table: "RefereeReport",
                column: "RefereeId");

            migrationBuilder.CreateIndex(
                name: "IX_RefereeReport_ReportedHorseId",
                table: "RefereeReport",
                column: "ReportedHorseId");

            migrationBuilder.CreateIndex(
                name: "IX_RefereeReport_ReportedUserId",
                table: "RefereeReport",
                column: "ReportedUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefereeReport");

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
        }
    }
}
