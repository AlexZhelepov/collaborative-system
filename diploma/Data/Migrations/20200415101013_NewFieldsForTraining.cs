using Microsoft.EntityFrameworkCore.Migrations;

namespace diploma.Data.Migrations
{
    public partial class NewFieldsForTraining : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasMeaningClass",
                table: "Words",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "DocFiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasMeaningClass",
                table: "Words");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "DocFiles");
        }
    }
}
