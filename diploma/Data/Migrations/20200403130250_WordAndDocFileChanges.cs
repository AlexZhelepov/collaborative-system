using Microsoft.EntityFrameworkCore.Migrations;

namespace diploma.Data.Migrations
{
    public partial class WordAndDocFileChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasMeaning",
                table: "Words",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FIO",
                table: "DocFiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "DocFiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPlace",
                table: "DocFiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasMeaning",
                table: "Words");

            migrationBuilder.DropColumn(
                name: "FIO",
                table: "DocFiles");

            migrationBuilder.DropColumn(
                name: "Skills",
                table: "DocFiles");

            migrationBuilder.DropColumn(
                name: "WorkPlace",
                table: "DocFiles");
        }
    }
}
