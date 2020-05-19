using Microsoft.EntityFrameworkCore.Migrations;

namespace diploma.Data.Migrations
{
    public partial class JsonField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JsonAutoClassificationResult",
                table: "DocFiles",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JsonAutoClassificationResult",
                table: "DocFiles");
        }
    }
}
