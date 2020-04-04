using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace diploma.Data.Migrations
{
    public partial class GetRidsOfArtefact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Words_Artefacts_ArtefactId",
                table: "Words");

            migrationBuilder.DropTable(
                name: "Artefacts");

            migrationBuilder.DropIndex(
                name: "IX_Words_ArtefactId",
                table: "Words");

            migrationBuilder.DropColumn(
                name: "ArtefactId",
                table: "Words");

            migrationBuilder.AddColumn<int>(
                name: "FacetItemId",
                table: "Words",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Words_FacetItemId",
                table: "Words",
                column: "FacetItemId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Words_FacetItems_FacetItemId",
                table: "Words",
                column: "FacetItemId",
                principalTable: "FacetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Words_FacetItems_FacetItemId",
                table: "Words");

            migrationBuilder.DropIndex(
                name: "IX_Words_FacetItemId",
                table: "Words");

            migrationBuilder.DropColumn(
                name: "FacetItemId",
                table: "Words");

            migrationBuilder.AddColumn<int>(
                name: "ArtefactId",
                table: "Words",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Artefacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artefacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artefacts_FacetItems_TypeId",
                        column: x => x.TypeId,
                        principalTable: "FacetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Words_ArtefactId",
                table: "Words",
                column: "ArtefactId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artefacts_TypeId",
                table: "Artefacts",
                column: "TypeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Words_Artefacts_ArtefactId",
                table: "Words",
                column: "ArtefactId",
                principalTable: "Artefacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
