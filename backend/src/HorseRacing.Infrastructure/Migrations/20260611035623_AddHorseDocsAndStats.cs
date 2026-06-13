using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHorseDocsAndStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HorseId1",
                table: "RaceEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Horses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HealthStatus",
                table: "Horses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RegistrationId",
                table: "Horses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HorseDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HorseId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorseDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorseDocuments_Horses_HorseId",
                        column: x => x.HorseId,
                        principalTable: "Horses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorseStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HorseId = table.Column<int>(type: "int", nullable: false),
                    TotalRaces = table.Column<int>(type: "int", nullable: false),
                    TotalWins = table.Column<int>(type: "int", nullable: false),
                    TotalSecondPlaces = table.Column<int>(type: "int", nullable: false),
                    TotalThirdPlaces = table.Column<int>(type: "int", nullable: false),
                    AverageSpeed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorseStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorseStatistics_Horses_HorseId",
                        column: x => x.HorseId,
                        principalTable: "Horses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JockeyContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HorseId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    JockeyId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JockeyContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JockeyContracts_Horses_HorseId",
                        column: x => x.HorseId,
                        principalTable: "Horses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JockeyContracts_Users_JockeyId",
                        column: x => x.JockeyId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JockeyContracts_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    HorseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registrations_Horses_HorseId",
                        column: x => x.HorseId,
                        principalTable: "Horses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_RaceEntries_HorseId1",
                table: "RaceEntries",
                column: "HorseId1");

            migrationBuilder.CreateIndex(
                name: "IX_Horses_RegistrationId",
                table: "Horses",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_HorseDocuments_HorseId",
                table: "HorseDocuments",
                column: "HorseId");

            migrationBuilder.CreateIndex(
                name: "IX_HorseStatistics_HorseId",
                table: "HorseStatistics",
                column: "HorseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JockeyContracts_HorseId",
                table: "JockeyContracts",
                column: "HorseId");

            migrationBuilder.CreateIndex(
                name: "IX_JockeyContracts_JockeyId",
                table: "JockeyContracts",
                column: "JockeyId");

            migrationBuilder.CreateIndex(
                name: "IX_JockeyContracts_OwnerId",
                table: "JockeyContracts",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_HorseId",
                table: "Registrations",
                column: "HorseId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_TournamentId",
                table: "Registrations",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Horses_Registrations_RegistrationId",
                table: "Horses",
                column: "RegistrationId",
                principalTable: "Registrations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntries_Horses_HorseId1",
                table: "RaceEntries",
                column: "HorseId1",
                principalTable: "Horses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Horses_Registrations_RegistrationId",
                table: "Horses");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntries_Horses_HorseId1",
                table: "RaceEntries");

            migrationBuilder.DropTable(
                name: "HorseDocuments");

            migrationBuilder.DropTable(
                name: "HorseStatistics");

            migrationBuilder.DropTable(
                name: "JockeyContracts");

            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_RaceEntries_HorseId1",
                table: "RaceEntries");

            migrationBuilder.DropIndex(
                name: "IX_Horses_RegistrationId",
                table: "Horses");

            migrationBuilder.DropColumn(
                name: "HorseId1",
                table: "RaceEntries");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Horses");

            migrationBuilder.DropColumn(
                name: "HealthStatus",
                table: "Horses");

            migrationBuilder.DropColumn(
                name: "RegistrationId",
                table: "Horses");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEATglvte26OBE9xd/qWKA6OIR0/FpdZGiWJpiSyoEimQFkrHsR1R2M9MuSTW0Z5S3w==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEHQ3c4q/N3Z1wWBLk2KhVZK6kobOEpaXNqph7uZ5sXsnwVsVzu1GY/w4SyG4TgJQUw==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEAvcQRwm2FTrZSZ6PXIYZx8yRipbwVPIk/6NN0CxB76y3JKs+BbWlbYzphyKRlhOMg==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEOESdzbETNWYcgmcIuib6/nTOBVlZPEkPqRyxj4hn0J29FPBj9nQVNRUpS4oF5x/vg==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEG1NuO/n0jgVTwQoetXiTo1eLV9QRv1venSg8T730Bd9BJsA728X77MZDlX7+Uq9pQ==");
        }
    }
}
