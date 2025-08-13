using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedMainCategoryRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MainCategoryId",
                table: "ServiceCategories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MainCategories",
                columns: table => new
                {
                    MainCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MainCategories", x => x.MainCategoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_MainCategoryId",
                table: "ServiceCategories",
                column: "MainCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCategories_MainCategories_MainCategoryId",
                table: "ServiceCategories",
                column: "MainCategoryId",
                principalTable: "MainCategories",
                principalColumn: "MainCategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCategories_MainCategories_MainCategoryId",
                table: "ServiceCategories");

            migrationBuilder.DropTable(
                name: "MainCategories");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCategories_MainCategoryId",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "MainCategoryId",
                table: "ServiceCategories");
        }
    }
}
