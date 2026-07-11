using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWithdrawalFieldsAndCheckType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WithdrawReason",
                table: "RaceEntry",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WithdrawTime",
                table: "RaceEntry",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckType",
                table: "MedicalCheckRecord",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FailReason",
                table: "MedicalCheckRecord",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WithdrawReason",
                table: "RaceEntry");

            migrationBuilder.DropColumn(
                name: "WithdrawTime",
                table: "RaceEntry");

            migrationBuilder.DropColumn(
                name: "CheckType",
                table: "MedicalCheckRecord");

            migrationBuilder.DropColumn(
                name: "FailReason",
                table: "MedicalCheckRecord");
        }
    }
}
