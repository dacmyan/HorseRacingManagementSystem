using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRaceViolationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaceViolation_Race_RaceId",
                table: "RaceViolation");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RaceViolation",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "RaceEntryId",
                table: "RaceViolation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RefereeId",
                table: "RaceViolation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_RaceViolation_RaceEntryId",
                table: "RaceViolation",
                column: "RaceEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceViolation_RefereeId",
                table: "RaceViolation",
                column: "RefereeId");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceViolation_RaceEntry_RaceEntryId",
                table: "RaceViolation",
                column: "RaceEntryId",
                principalTable: "RaceEntry",
                principalColumn: "RaceEntryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceViolation_Race_RaceId",
                table: "RaceViolation",
                column: "RaceId",
                principalTable: "Race",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceViolation_RefereeProfile_RefereeId",
                table: "RaceViolation",
                column: "RefereeId",
                principalTable: "RefereeProfile",
                principalColumn: "RefereeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaceViolation_RaceEntry_RaceEntryId",
                table: "RaceViolation");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceViolation_Race_RaceId",
                table: "RaceViolation");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceViolation_RefereeProfile_RefereeId",
                table: "RaceViolation");

            migrationBuilder.DropIndex(
                name: "IX_RaceViolation_RaceEntryId",
                table: "RaceViolation");

            migrationBuilder.DropIndex(
                name: "IX_RaceViolation_RefereeId",
                table: "RaceViolation");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RaceViolation");

            migrationBuilder.DropColumn(
                name: "RaceEntryId",
                table: "RaceViolation");

            migrationBuilder.DropColumn(
                name: "RefereeId",
                table: "RaceViolation");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceViolation_Race_RaceId",
                table: "RaceViolation",
                column: "RaceId",
                principalTable: "Race",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
