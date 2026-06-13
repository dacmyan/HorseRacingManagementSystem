using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefereeAssignmentFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Horse_Registration_RegistrationId",
                table: "Horse");

            migrationBuilder.DropIndex(
                name: "IX_Horse_RegistrationId",
                table: "Horse");

            migrationBuilder.DropColumn(
                name: "RegistrationId",
                table: "Horse");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceRefereeAssignment_RefereeProfile_RefereeId",
                table: "RaceRefereeAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_JockeyProfile_JockeyId",
                table: "RaceEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefereeProfile",
                table: "RefereeProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JockeyProfile",
                table: "JockeyProfile");

            migrationBuilder.AlterColumn<long>(
                name: "RefereeId",
                table: "RefereeProfile",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "RefereeId",
                table: "RaceRefereeAssignment",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "WinningProbability",
                table: "RaceEntry",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "JockeyId",
                table: "RaceEntry",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentOdds",
                table: "RaceEntry",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "JockeyId",
                table: "JockeyProfile",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefereeProfile",
                table: "RefereeProfile",
                column: "RefereeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JockeyProfile",
                table: "JockeyProfile",
                column: "JockeyId");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceRefereeAssignment_RefereeProfile_RefereeId",
                table: "RaceRefereeAssignment",
                column: "RefereeId",
                principalTable: "RefereeProfile",
                principalColumn: "RefereeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntry_JockeyProfile_JockeyId",
                table: "RaceEntry",
                column: "JockeyId",
                principalTable: "JockeyProfile",
                principalColumn: "JockeyId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RaceRefereeAssignment_RefereeProfile_RefereeId",
                table: "RaceRefereeAssignment");

            migrationBuilder.DropForeignKey(
                name: "FK_RaceEntry_JockeyProfile_JockeyId",
                table: "RaceEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefereeProfile",
                table: "RefereeProfile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JockeyProfile",
                table: "JockeyProfile");

            migrationBuilder.AlterColumn<int>(
                name: "RefereeId",
                table: "RefereeProfile",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "RefereeId",
                table: "RaceRefereeAssignment",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "WinningProbability",
                table: "RaceEntry",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "JockeyId",
                table: "RaceEntry",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentOdds",
                table: "RaceEntry",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "JockeyId",
                table: "JockeyProfile",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefereeProfile",
                table: "RefereeProfile",
                column: "RefereeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JockeyProfile",
                table: "JockeyProfile",
                column: "JockeyId");

            migrationBuilder.AddForeignKey(
                name: "FK_RaceRefereeAssignment_RefereeProfile_RefereeId",
                table: "RaceRefereeAssignment",
                column: "RefereeId",
                principalTable: "RefereeProfile",
                principalColumn: "RefereeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RaceEntry_JockeyProfile_JockeyId",
                table: "RaceEntry",
                column: "JockeyId",
                principalTable: "JockeyProfile",
                principalColumn: "JockeyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "RegistrationId",
                table: "Horse",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Horse_RegistrationId",
                table: "Horse",
                column: "RegistrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Horse_Registration_RegistrationId",
                table: "Horse",
                column: "RegistrationId",
                principalTable: "Registration",
                principalColumn: "Id");
        }
    }
}
