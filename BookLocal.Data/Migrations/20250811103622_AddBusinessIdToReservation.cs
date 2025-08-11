using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessIdToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BusinessId",
                table: "Reservations",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Businesses_BusinessId",
                table: "Reservations",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "BusinessId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Businesses_BusinessId",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BusinessId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Reservations");
        }
    }
}
