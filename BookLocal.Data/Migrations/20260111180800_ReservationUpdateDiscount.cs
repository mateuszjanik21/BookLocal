using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReservationUpdateDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Reservations",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DiscountId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_DiscountId",
                table: "Reservations",
                column: "DiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Discounts_DiscountId",
                table: "Reservations",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "DiscountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Discounts_DiscountId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_DiscountId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DiscountId",
                table: "Reservations");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Discounts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
