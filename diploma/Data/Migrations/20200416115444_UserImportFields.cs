using Microsoft.EntityFrameworkCore.Migrations;

namespace diploma.Data.Migrations
{
    public partial class UserImportFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserInfoId",
                table: "DocFiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserInfoId",
                table: "DocFiles");
        }
    }
}
