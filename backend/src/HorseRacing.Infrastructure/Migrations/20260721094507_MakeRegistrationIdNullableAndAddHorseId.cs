using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorseRacing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeRegistrationIdNullableAndAddHorseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RegistrationId",
                table: "MedicalCheckRecord",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "HorseId",
                table: "MedicalCheckRecord",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalCheckRecord_HorseId",
                table: "MedicalCheckRecord",
                column: "HorseId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalCheckRecord_Horse_HorseId",
                table: "MedicalCheckRecord",
                column: "HorseId",
                principalTable: "Horse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalCheckRecord_Horse_HorseId",
                table: "MedicalCheckRecord");

            migrationBuilder.DropIndex(
                name: "IX_MedicalCheckRecord_HorseId",
                table: "MedicalCheckRecord");

            migrationBuilder.DropColumn(
                name: "HorseId",
                table: "MedicalCheckRecord");

            migrationBuilder.AlterColumn<int>(
                name: "RegistrationId",
                table: "MedicalCheckRecord",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
