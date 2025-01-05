using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReportsModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaintenanceReportId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinancialReportId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeaseReportId",
                table: "Leases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OccupancyReportId",
                table: "Houses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FinancialReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialReports_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeaseReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveLeases = table.Column<int>(type: "int", nullable: false),
                    ExpiringLeases = table.Column<int>(type: "int", nullable: false),
                    RenewedLeases = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseReports_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalRequests = table.Column<int>(type: "int", nullable: false),
                    ResolvedRequests = table.Column<int>(type: "int", nullable: false),
                    PendingRequests = table.Column<int>(type: "int", nullable: false),
                    TotalMaintenanceCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceReports_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OccupancyReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalUnits = table.Column<int>(type: "int", nullable: false),
                    OccupiedUnits = table.Column<int>(type: "int", nullable: false),
                    VacantUnits = table.Column<int>(type: "int", nullable: false),
                    OccupancyRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccupancyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OccupancyReports_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_MaintenanceReportId",
                table: "Requests",
                column: "MaintenanceReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FinancialReportId",
                table: "Payments",
                column: "FinancialReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Leases_LeaseReportId",
                table: "Leases",
                column: "LeaseReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Houses_OccupancyReportId",
                table: "Houses",
                column: "OccupancyReportId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReports_PropertyId",
                table: "FinancialReports",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseReports_PropertyId",
                table: "LeaseReports",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceReports_PropertyId",
                table: "MaintenanceReports",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_OccupancyReports_PropertyId",
                table: "OccupancyReports",
                column: "PropertyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Houses_OccupancyReports_OccupancyReportId",
                table: "Houses",
                column: "OccupancyReportId",
                principalTable: "OccupancyReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leases_LeaseReports_LeaseReportId",
                table: "Leases",
                column: "LeaseReportId",
                principalTable: "LeaseReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_FinancialReports_FinancialReportId",
                table: "Payments",
                column: "FinancialReportId",
                principalTable: "FinancialReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_MaintenanceReports_MaintenanceReportId",
                table: "Requests",
                column: "MaintenanceReportId",
                principalTable: "MaintenanceReports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Houses_OccupancyReports_OccupancyReportId",
                table: "Houses");

            migrationBuilder.DropForeignKey(
                name: "FK_Leases_LeaseReports_LeaseReportId",
                table: "Leases");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_FinancialReports_FinancialReportId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_MaintenanceReports_MaintenanceReportId",
                table: "Requests");

            migrationBuilder.DropTable(
                name: "FinancialReports");

            migrationBuilder.DropTable(
                name: "LeaseReports");

            migrationBuilder.DropTable(
                name: "MaintenanceReports");

            migrationBuilder.DropTable(
                name: "OccupancyReports");

            migrationBuilder.DropIndex(
                name: "IX_Requests_MaintenanceReportId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Payments_FinancialReportId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Leases_LeaseReportId",
                table: "Leases");

            migrationBuilder.DropIndex(
                name: "IX_Houses_OccupancyReportId",
                table: "Houses");

            migrationBuilder.DropColumn(
                name: "MaintenanceReportId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "FinancialReportId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "LeaseReportId",
                table: "Leases");

            migrationBuilder.DropColumn(
                name: "OccupancyReportId",
                table: "Houses");
        }
    }
}
