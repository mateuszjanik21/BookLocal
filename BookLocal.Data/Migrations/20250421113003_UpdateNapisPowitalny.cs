using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLocal.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNapisPowitalny : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AltText",
                table: "NapisPowitalny");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "NapisPowitalny");

            migrationBuilder.DropColumn(
                name: "Kolejnosc",
                table: "NapisPowitalny");

            migrationBuilder.DropColumn(
                name: "LinkUrl",
                table: "NapisPowitalny");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "NapisPowitalny");

            migrationBuilder.RenameColumn(
                name: "IdLoga",
                table: "NapisPowitalny",
                newName: "IdNapisu");

            migrationBuilder.AddColumn<string>(
                name: "Naglowek",
                table: "NapisPowitalny",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tresc",
                table: "NapisPowitalny",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Naglowek",
                table: "NapisPowitalny");

            migrationBuilder.DropColumn(
                name: "Tresc",
                table: "NapisPowitalny");

            migrationBuilder.RenameColumn(
                name: "IdNapisu",
                table: "NapisPowitalny",
                newName: "IdLoga");

            migrationBuilder.AddColumn<string>(
                name: "AltText",
                table: "NapisPowitalny",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "NapisPowitalny",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Kolejnosc",
                table: "NapisPowitalny",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LinkUrl",
                table: "NapisPowitalny",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "NapisPowitalny",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
