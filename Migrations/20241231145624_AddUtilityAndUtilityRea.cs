using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUtilityAndUtilityRea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Utilities_Users_TenantId",
                table: "Utilities");

            migrationBuilder.DropForeignKey(
                name: "FK_UtilityReadings_Utilities_UtilityId",
                table: "UtilityReadings");

            migrationBuilder.DropIndex(
                name: "IX_Utilities_TenantId",
                table: "Utilities");

            migrationBuilder.DropColumn(
                name: "MeterNumber",
                table: "Utilities");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Utilities");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "UtilityReadings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Utilities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityReadings_TenantId",
                table: "UtilityReadings",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_UtilityReadings_Users_TenantId",
                table: "UtilityReadings",
                column: "TenantId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UtilityReadings_Utilities_UtilityId",
                table: "UtilityReadings",
                column: "UtilityId",
                principalTable: "Utilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UtilityReadings_Users_TenantId",
                table: "UtilityReadings");

            migrationBuilder.DropForeignKey(
                name: "FK_UtilityReadings_Utilities_UtilityId",
                table: "UtilityReadings");

            migrationBuilder.DropIndex(
                name: "IX_UtilityReadings_TenantId",
                table: "UtilityReadings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "UtilityReadings");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Utilities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "MeterNumber",
                table: "Utilities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Utilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Utilities_TenantId",
                table: "Utilities",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Utilities_Users_TenantId",
                table: "Utilities",
                column: "TenantId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UtilityReadings_Utilities_UtilityId",
                table: "UtilityReadings",
                column: "UtilityId",
                principalTable: "Utilities",
                principalColumn: "Id");
        }
    }
}
