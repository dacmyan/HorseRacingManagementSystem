using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRuntimeUserSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "JockeyProfile",
                keyColumn: "JockeyId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RefereeProfile",
                keyColumn: "RefereeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Wallet",
                keyColumn: "WalletId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AppUser",
                keyColumn: "UserId",
                keyValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppUser",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "PasswordHash", "RoleId", "Status", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 9, 0, 0, 0, 0, DateTimeKind.Utc), "admin@gmail.com", "Admin", "AQAAAAIAAYagAAAAEL3MNWtRhvfQxwwwIqhshbFLU07YLYFvXEUG66I7640i3DJ4x1GfHVWHTyt5JhDkvg==", 1, "Active", "admin" },
                    { 2, new DateTime(2026, 6, 9, 0, 0, 0, 0, DateTimeKind.Utc), "owner@gmail.com", "HorseOwner", "AQAAAAIAAYagAAAAEGyQeVxp9OVePd4lUW2yUnlaIslc2Hb1UqZouwu1xjOlZgLmdErHbODNrBy9QAk7Fg==", 2, "Active", "owner" },
                    { 3, new DateTime(2026, 6, 9, 0, 0, 0, 0, DateTimeKind.Utc), "jockey@gmail.com", "Jockey", "AQAAAAIAAYagAAAAEG7QrTR2HaVVSpLEoFY6nOXiEX/NozyjPG00YA6FAebngMyKqLiQ72/SXsHxSwGLmA==", 3, "Active", "jockey" },
                    { 4, new DateTime(2026, 6, 9, 0, 0, 0, 0, DateTimeKind.Utc), "referee@gmail.com", "Referee", "AQAAAAIAAYagAAAAEAdQI0nKh2I0WQ5tIG7x7PcS/FtpxA2nTEzJmsOIfzFlBOZM0COQr0/xSZll9vhcwQ==", 4, "Active", "referee" },
                    { 5, new DateTime(2026, 6, 9, 0, 0, 0, 0, DateTimeKind.Utc), "spectator@gmail.com", "Spectator", "AQAAAAIAAYagAAAAELF89zW+xZNN798lwcDM2PkiAZJVpjK8/S+59Yst+SzV3QlCb5IuBvLXib+/BFmcqA==", 5, "Active", "spectator" }
                });

            migrationBuilder.InsertData(
                table: "JockeyProfile",
                columns: new[] { "JockeyId", "ExperienceYears", "RankingPoint", "Status", "UserId" },
                values: new object[] { 1, 3, 100, "Active", 3 });

            migrationBuilder.InsertData(
                table: "RefereeProfile",
                columns: new[] { "RefereeId", "ExperienceYears", "LicenseNumber", "Status", "UserId" },
                values: new object[] { 1, 5, "LIC-REF-001", "Active", 4 });

            migrationBuilder.InsertData(
                table: "Wallet",
                columns: new[] { "WalletId", "Balance", "UserId" },
                values: new object[] { 1, 0m, 5 });
        }
    }
}
