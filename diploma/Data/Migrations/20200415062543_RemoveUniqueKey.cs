using Microsoft.EntityFrameworkCore.Migrations;

namespace diploma.Data.Migrations
{
    public partial class RemoveUniqueKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Words_FacetItemId",
                table: "Words");

            migrationBuilder.CreateIndex(
                name: "IX_Words_FacetItemId",
                table: "Words",
                column: "FacetItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Words_FacetItemId",
                table: "Words");

            migrationBuilder.CreateIndex(
                name: "IX_Words_FacetItemId",
                table: "Words",
                column: "FacetItemId",
                unique: true);
        }
    }
}
