using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateZawartoscSekcja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StronaCms");

            migrationBuilder.AddColumn<int>(
                name: "SekcjaCmsId",
                table: "ZawartoscCms",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZawartoscCms_SekcjaCmsId",
                table: "ZawartoscCms",
                column: "SekcjaCmsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ZawartoscCms_SekcjaCms_SekcjaCmsId",
                table: "ZawartoscCms",
                column: "SekcjaCmsId",
                principalTable: "SekcjaCms",
                principalColumn: "IdSekcji");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ZawartoscCms_SekcjaCms_SekcjaCmsId",
                table: "ZawartoscCms");

            migrationBuilder.DropIndex(
                name: "IX_ZawartoscCms_SekcjaCmsId",
                table: "ZawartoscCms");

            migrationBuilder.DropColumn(
                name: "SekcjaCmsId",
                table: "ZawartoscCms");

            migrationBuilder.CreateTable(
                name: "StronaCms",
                columns: table => new
                {
                    IdStrony = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pozycja = table.Column<int>(type: "int", nullable: false),
                    TytulNawigacji = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StronaCms", x => x.IdStrony);
                });
        }
    }
}
