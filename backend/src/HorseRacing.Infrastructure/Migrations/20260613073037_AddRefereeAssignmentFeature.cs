using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefereeAssignmentFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_Horse_Registration_RegistrationId",
            //     table: "Horse");
            // 
            // migrationBuilder.DropIndex(
            //     name: "IX_Horse_RegistrationId",
            //     table: "Horse");
            // 
            // migrationBuilder.DropColumn(
            //     name: "RegistrationId",
            //     table: "Horse");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
