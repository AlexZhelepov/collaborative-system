using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace diploma.Data.Migrations
{
    public partial class BigUpdateForMarina : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Value",
                table: "FacetItems",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserCompetence",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserInfoId = table.Column<int>(nullable: false),
                    CompetenceId = table.Column<int>(nullable: false),
                    LevelId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompetence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCompetence_FacetItems_CompetenceId",
                        column: x => x.CompetenceId,
                        principalTable: "FacetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCompetence_FacetItems_LevelId",
                        column: x => x.LevelId,
                        principalTable: "FacetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCompetence_UserInfos_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "UserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCompetence_CompetenceId",
                table: "UserCompetence",
                column: "CompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompetence_LevelId",
                table: "UserCompetence",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompetence_UserInfoId",
                table: "UserCompetence",
                column: "UserInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCompetence");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "FacetItems");
        }
    }
}
