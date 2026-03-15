using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedOwnerCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoyaltyPointsUsed",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoyaltyPointsUsed",
                table: "Reservations");
        }
    }
}
