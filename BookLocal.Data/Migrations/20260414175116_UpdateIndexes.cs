using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkSchedules_EmployeeId",
                table: "WorkSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_BusinessId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BusinessId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CustomerId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_EmployeeId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_DailyFinancialReports_BusinessId",
                table: "DailyFinancialReports");

            migrationBuilder.AlterColumn<string>(
                name: "DayOfWeek",
                table: "WorkSchedules",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_Employee_Day",
                table: "WorkSchedules",
                columns: new[] { "EmployeeId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Business_Rating",
                table: "Reviews",
                columns: new[] { "BusinessId", "Rating" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Business_StartTime",
                table: "Reservations",
                columns: new[] { "BusinessId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Customer_Status",
                table: "Reservations",
                columns: new[] { "CustomerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Employee_StartTime",
                table: "Reservations",
                columns: new[] { "EmployeeId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_EndTime_Status",
                table: "Reservations",
                columns: new[] { "EndTime", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionDate",
                table: "Payments",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_DailyFinancialReports_Business_ReportDate",
                table: "DailyFinancialReports",
                columns: new[] { "BusinessId", "ReportDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_City",
                table: "Businesses",
                column: "City");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkSchedules_Employee_Day",
                table: "WorkSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_Business_Rating",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_Business_StartTime",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_Customer_Status",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_Employee_StartTime",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_EndTime_Status",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TransactionDate",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_DailyFinancialReports_Business_ReportDate",
                table: "DailyFinancialReports");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_City",
                table: "Businesses");

            migrationBuilder.AlterColumn<string>(
                name: "DayOfWeek",
                table: "WorkSchedules",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_EmployeeId",
                table: "WorkSchedules",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BusinessId",
                table: "Reviews",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BusinessId",
                table: "Reservations",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CustomerId",
                table: "Reservations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_EmployeeId",
                table: "Reservations",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyFinancialReports_BusinessId",
                table: "DailyFinancialReports",
                column: "BusinessId");
        }
    }
}
