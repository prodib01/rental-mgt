using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialReports_Houses_HouseId",
                table: "FinancialReports");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaseReports_Houses_HouseId",
                table: "LeaseReports");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceReports_Houses_HouseId",
                table: "MaintenanceReports");

            migrationBuilder.DropForeignKey(
                name: "FK_OccupancyReports_Houses_HouseId",
                table: "OccupancyReports");

            migrationBuilder.RenameColumn(
                name: "HouseId",
                table: "OccupancyReports",
                newName: "PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_OccupancyReports_HouseId",
                table: "OccupancyReports",
                newName: "IX_OccupancyReports_PropertyId");

            migrationBuilder.RenameColumn(
                name: "HouseId",
                table: "MaintenanceReports",
                newName: "PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceReports_HouseId",
                table: "MaintenanceReports",
                newName: "IX_MaintenanceReports_PropertyId");

            migrationBuilder.RenameColumn(
                name: "HouseId",
                table: "LeaseReports",
                newName: "PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaseReports_HouseId",
                table: "LeaseReports",
                newName: "IX_LeaseReports_PropertyId");

            migrationBuilder.RenameColumn(
                name: "HouseId",
                table: "FinancialReports",
                newName: "PropertyId");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialReports_HouseId",
                table: "FinancialReports",
                newName: "IX_FinancialReports_PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialReports_Properties_PropertyId",
                table: "FinancialReports",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseReports_Properties_PropertyId",
                table: "LeaseReports",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceReports_Properties_PropertyId",
                table: "MaintenanceReports",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OccupancyReports_Properties_PropertyId",
                table: "OccupancyReports",
                column: "PropertyId",
                principalTable: "Properties",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialReports_Properties_PropertyId",
                table: "FinancialReports");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaseReports_Properties_PropertyId",
                table: "LeaseReports");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceReports_Properties_PropertyId",
                table: "MaintenanceReports");

            migrationBuilder.DropForeignKey(
                name: "FK_OccupancyReports_Properties_PropertyId",
                table: "OccupancyReports");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "OccupancyReports",
                newName: "HouseId");

            migrationBuilder.RenameIndex(
                name: "IX_OccupancyReports_PropertyId",
                table: "OccupancyReports",
                newName: "IX_OccupancyReports_HouseId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "MaintenanceReports",
                newName: "HouseId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceReports_PropertyId",
                table: "MaintenanceReports",
                newName: "IX_MaintenanceReports_HouseId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "LeaseReports",
                newName: "HouseId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaseReports_PropertyId",
                table: "LeaseReports",
                newName: "IX_LeaseReports_HouseId");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "FinancialReports",
                newName: "HouseId");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialReports_PropertyId",
                table: "FinancialReports",
                newName: "IX_FinancialReports_HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialReports_Houses_HouseId",
                table: "FinancialReports",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseReports_Houses_HouseId",
                table: "LeaseReports",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceReports_Houses_HouseId",
                table: "MaintenanceReports",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OccupancyReports_Houses_HouseId",
                table: "OccupancyReports",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "Id");
        }
    }
}
