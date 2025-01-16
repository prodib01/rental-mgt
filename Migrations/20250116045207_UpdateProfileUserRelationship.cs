using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProfileUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Profiles_LandlordId",
                table: "Profiles");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_LandlordId",
                table: "Profiles",
                column: "LandlordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Profiles_LandlordId",
                table: "Profiles");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_LandlordId",
                table: "Profiles",
                column: "LandlordId");
        }
    }
}
