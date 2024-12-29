using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class Addys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leases_Houses_HouseId",
                table: "Leases");

            migrationBuilder.DropIndex(
                name: "IX_Leases_HouseId",
                table: "Leases");

            migrationBuilder.DropColumn(
                name: "HouseId",
                table: "Leases");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HouseId",
                table: "Leases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Leases_HouseId",
                table: "Leases",
                column: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leases_Houses_HouseId",
                table: "Leases",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
