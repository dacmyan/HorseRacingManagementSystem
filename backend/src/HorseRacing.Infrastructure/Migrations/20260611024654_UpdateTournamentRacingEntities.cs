using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTournamentRacingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Races_RaceId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_Prizes_Tournaments_TournamentId",
                table: "Prizes");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntries_Races_RaceId",
                table: "RaceEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Races_Tournaments_TournamentId",
                table: "Races");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPrizePayouts_Tournaments_TournamentId",
                table: "TournamentPrizePayouts");

            migrationBuilder.DropForeignKey(
                name: "FK_Violations_Races_RaceId",
                table: "Violations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Races",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Races_TournamentId",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Races");

            migrationBuilder.RenameColumn(
                name: "TournamentId",
                table: "Races",
                newName: "MaxLanes");

            migrationBuilder.RenameColumn(
                name: "ScheduledTime",
                table: "Races",
                newName: "RaceDate");

            migrationBuilder.RenameColumn(
                name: "Distance",
                table: "Races",
                newName: "DistanceMeter");

            migrationBuilder.AlterColumn<long>(
                name: "RaceId",
                table: "Violations",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Tournaments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Tournaments",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<long>(
                name: "TournamentId",
                table: "Tournaments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "TournamentId",
                table: "TournamentPrizePayouts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Races",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<long>(
                name: "RaceId",
                table: "Races",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<long>(
                name: "RoundId",
                table: "Races",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "RaceId",
                table: "RaceResults",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "RaceId",
                table: "RaceEntries",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "TournamentId",
                table: "Prizes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "RaceId",
                table: "Predictions",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "RaceId",
                table: "Bets",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments",
                column: "TournamentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Races",
                table: "Races",
                column: "RaceId");

            migrationBuilder.CreateTable(
                name: "RaceRefereeAssignments",
                columns: table => new
                {
                    AssignmentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RaceId = table.Column<long>(type: "bigint", nullable: false),
                    RefereeId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceRefereeAssignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_RaceRefereeAssignments_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "RaceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RaceRefereeAssignments_RefereeProfiles_RefereeId",
                        column: x => x.RefereeId,
                        principalTable: "RefereeProfiles",
                        principalColumn: "RefereeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    RoundId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.RoundId);
                    table.ForeignKey(
                        name: "FK_Rounds_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEOCrttfaCVXXKv/Wy+gaYr7tVyJOwyV+HSs32fMKAssoSs0sVJHPz/k+M+vWi3bKQQ==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEHwZ6VVoS037mIguxw+jqz7ak90xeEVNXLgyh4UPKuJyhPLZptFsDBXqipJGT6gLOw==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEGghxh7vAiSQ5BaGlvd975fc4KUzEYzwSkskU45lcryGMmmSQ+tiPQMMJ/lvm769yw==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEKpXVcz+pbPeqnCDD6zooCAAG41O/p0QbcisCuTVKBY3NuuKEbxrFad4Wf+2tURsMw==");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEI7KFJe27AeVB7P+vqMsLmvFp3PBKSTno9RVSx+LT3nytSCHRFwDx8fvBP06Rx6liA==");

            migrationBuilder.CreateIndex(
                name: "IX_Races_RoundId",
                table: "Races",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceRefereeAssignments_RaceId",
                table: "RaceRefereeAssignments",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_RaceRefereeAssignments_RefereeId",
                table: "RaceRefereeAssignments",
                column: "RefereeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TournamentId",
                table: "Rounds",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Races_RaceId",
                table: "Bets",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prizes_Tournaments_TournamentId",
                table: "Prizes",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntries_Races_RaceId",
                table: "RaceEntries",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Races_Rounds_RoundId",
                table: "Races",
                column: "RoundId",
                principalTable: "Rounds",
                principalColumn: "RoundId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPrizePayouts_Tournaments_TournamentId",
                table: "TournamentPrizePayouts",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Violations_Races_RaceId",
                table: "Violations",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "RaceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Races_RaceId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_Prizes_Tournaments_TournamentId",
                table: "Prizes");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntries_Races_RaceId",
                table: "RaceEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Races_Rounds_RoundId",
                table: "Races");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPrizePayouts_Tournaments_TournamentId",
                table: "TournamentPrizePayouts");

            migrationBuilder.DropForeignKey(
                name: "FK_Violations_Races_RaceId",
                table: "Violations");

            migrationBuilder.DropTable(
                name: "RaceRefereeAssignments");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Races",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Races_RoundId",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "RaceId",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "RoundId",
                table: "Races");

            migrationBuilder.RenameColumn(
                name: "RaceDate",
                table: "Races",
                newName: "ScheduledTime");

            migrationBuilder.RenameColumn(
                name: "MaxLanes",
                table: "Races",
                newName: "TournamentId");

            migrationBuilder.RenameColumn(
                name: "DistanceMeter",
                table: "Races",
                newName: "Distance");

            migrationBuilder.AlterColumn<int>(
                name: "RaceId",
                table: "Violations",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Tournaments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Tournaments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Tournaments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "TournamentPrizePayouts",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Races",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Races",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "RaceId",
                table: "RaceResults",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "RaceId",
                table: "RaceEntries",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "Prizes",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "RaceId",
                table: "Predictions",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "RaceId",
                table: "Bets",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Races",
                table: "Races",
                column: "Id");

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

            migrationBuilder.CreateIndex(
                name: "IX_Races_TournamentId",
                table: "Races",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Races_RaceId",
                table: "Bets",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Prizes_Tournaments_TournamentId",
                table: "Prizes",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntries_Races_RaceId",
                table: "RaceEntries",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Races_Tournaments_TournamentId",
                table: "Races",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPrizePayouts_Tournaments_TournamentId",
                table: "TournamentPrizePayouts",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Violations_Races_RaceId",
                table: "Violations",
                column: "RaceId",
                principalTable: "Races",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
