using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeesIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_BusinessId",
                table: "Employees");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Business_IsArchived",
                table: "Employees",
                columns: new[] { "BusinessId", "IsArchived" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_Business_IsArchived",
                table: "Employees");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_BusinessId",
                table: "Employees",
                column: "BusinessId");
        }
    }
}
