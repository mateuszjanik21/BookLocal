using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSekcjaCms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ikona",
                table: "SekcjaCms");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SekcjaCms");

            migrationBuilder.DropColumn(
                name: "LinkText",
                table: "SekcjaCms");

            migrationBuilder.DropColumn(
                name: "LinkUrl",
                table: "SekcjaCms");

            migrationBuilder.DropColumn(
                name: "Opis",
                table: "SekcjaCms");

            migrationBuilder.DropColumn(
                name: "Tytul",
                table: "SekcjaCms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ikona",
                table: "SekcjaCms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SekcjaCms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LinkText",
                table: "SekcjaCms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkUrl",
                table: "SekcjaCms",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Opis",
                table: "SekcjaCms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tytul",
                table: "SekcjaCms",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
