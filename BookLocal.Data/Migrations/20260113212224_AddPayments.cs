using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentMethodId",
                table: "Payments",
                newName: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BusinessId",
                table: "Payments",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Businesses_BusinessId",
                table: "Payments",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "BusinessId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Businesses_BusinessId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BusinessId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "Payments",
                newName: "PaymentMethodId");
        }
    }
}
