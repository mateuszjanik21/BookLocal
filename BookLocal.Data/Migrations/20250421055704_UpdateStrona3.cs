using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStrona3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tresc",
                table: "StronaCms");

            migrationBuilder.DropColumn(
                name: "Tytul",
                table: "StronaCms");

            migrationBuilder.RenameColumn(
                name: "LinkTytul",
                table: "StronaCms",
                newName: "TytulNawigacji");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TytulNawigacji",
                table: "StronaCms",
                newName: "LinkTytul");

            migrationBuilder.AddColumn<string>(
                name: "Tresc",
                table: "StronaCms",
                type: "nvarchar(MAX)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tytul",
                table: "StronaCms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
