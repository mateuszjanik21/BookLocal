using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class LayaltyUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoyaltyProgramConfigs",
                columns: table => new
                {
                    ConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SpendAmountForOnePoint = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyProgramConfigs", x => x.ConfigId);
                    table.ForeignKey(
                        name: "FK_LoyaltyProgramConfigs_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "BusinessId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoyaltyPointId = table.Column<int>(type: "int", nullable: false),
                    PointsAmount = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_LoyaltyTransactions_LoyaltyPoints_LoyaltyPointId",
                        column: x => x.LoyaltyPointId,
                        principalTable: "LoyaltyPoints",
                        principalColumn: "LoyaltyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyProgramConfigs_BusinessId",
                table: "LoyaltyProgramConfigs",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_LoyaltyPointId",
                table: "LoyaltyTransactions",
                column: "LoyaltyPointId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoyaltyProgramConfigs");

            migrationBuilder.DropTable(
                name: "LoyaltyTransactions");
        }
    }
}
