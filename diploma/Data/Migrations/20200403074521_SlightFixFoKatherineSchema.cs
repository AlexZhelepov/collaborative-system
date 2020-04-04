using Microsoft.EntityFrameworkCore.Migrations;

namespace diploma.Data.Migrations
{
    public partial class SlightFixFoKatherineSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacetItems_Facets_FacetId",
                table: "FacetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Words_Artefacts_ArtefactId",
                table: "Words");

            migrationBuilder.DropForeignKey(
                name: "FK_Words_DocFiles_DocFileId",
                table: "Words");

            migrationBuilder.AlterColumn<int>(
                name: "DocFileId",
                table: "Words",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ArtefactId",
                table: "Words",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "FacetId",
                table: "FacetItems",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FacetItems_Facets_FacetId",
                table: "FacetItems",
                column: "FacetId",
                principalTable: "Facets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Words_Artefacts_ArtefactId",
                table: "Words",
                column: "ArtefactId",
                principalTable: "Artefacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Words_DocFiles_DocFileId",
                table: "Words",
                column: "DocFileId",
                principalTable: "DocFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacetItems_Facets_FacetId",
                table: "FacetItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Words_Artefacts_ArtefactId",
                table: "Words");

            migrationBuilder.DropForeignKey(
                name: "FK_Words_DocFiles_DocFileId",
                table: "Words");

            migrationBuilder.AlterColumn<int>(
                name: "DocFileId",
                table: "Words",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "ArtefactId",
                table: "Words",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FacetId",
                table: "FacetItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_FacetItems_Facets_FacetId",
                table: "FacetItems",
                column: "FacetId",
                principalTable: "Facets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Words_Artefacts_ArtefactId",
                table: "Words",
                column: "ArtefactId",
                principalTable: "Artefacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Words_DocFiles_DocFileId",
                table: "Words",
                column: "DocFileId",
                principalTable: "DocFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
