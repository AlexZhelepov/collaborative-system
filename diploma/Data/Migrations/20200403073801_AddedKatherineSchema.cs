using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace diploma.Data.Migrations
{
    public partial class AddedKatherineSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(nullable: false),
                    FileHash = table.Column<string>(nullable: false),
                    ApplicationUserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocFiles_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Facets",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    Code = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacetItems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    Code = table.Column<string>(nullable: false),
                    FacetId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacetItems_Facets_FacetId",
                        column: x => x.FacetId,
                        principalTable: "Facets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Artefacts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    TypeId = table.Column<int>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TextVersion = table.Column<string>(nullable: false),
                    MystemData = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    InitialForm = table.Column<string>(nullable: true),
                    DocFileId = table.Column<int>(nullable: true),
                    ArtefactId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Words_Artefacts_ArtefactId",
                        column: x => x.ArtefactId,
                        principalTable: "Artefacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Words_DocFiles_DocFileId",
                        column: x => x.DocFileId,
                        principalTable: "DocFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artefacts_TypeId",
                table: "Artefacts",
                column: "TypeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocFiles_ApplicationUserId",
                table: "DocFiles",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FacetItems_FacetId",
                table: "FacetItems",
                column: "FacetId");

            migrationBuilder.CreateIndex(
                name: "IX_Words_ArtefactId",
                table: "Words",
                column: "ArtefactId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Words_DocFileId",
                table: "Words",
                column: "DocFileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Words");

            migrationBuilder.DropTable(
                name: "Artefacts");

            migrationBuilder.DropTable(
                name: "DocFiles");

            migrationBuilder.DropTable(
                name: "FacetItems");

            migrationBuilder.DropTable(
                name: "Facets");
        }
    }
}
