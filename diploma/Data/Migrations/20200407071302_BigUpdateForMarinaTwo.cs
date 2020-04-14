using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace diploma.Data.Migrations
{
    public partial class BigUpdateForMarinaTwo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCompetence_FacetItems_CompetenceId",
                table: "UserCompetence");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCompetence_FacetItems_LevelId",
                table: "UserCompetence");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCompetence_UserInfos_UserInfoId",
                table: "UserCompetence");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCompetence",
                table: "UserCompetence");

            migrationBuilder.RenameTable(
                name: "UserCompetence",
                newName: "UserCompetences");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompetence_UserInfoId",
                table: "UserCompetences",
                newName: "IX_UserCompetences_UserInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompetence_LevelId",
                table: "UserCompetences",
                newName: "IX_UserCompetences_LevelId");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompetence_CompetenceId",
                table: "UserCompetences",
                newName: "IX_UserCompetences_CompetenceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCompetences",
                table: "UserCompetences",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    DateStart = table.Column<DateTime>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vacancies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: false),
                    UserInfoId = table.Column<int>(nullable: true),
                    ProjectId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacancies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vacancies_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vacancies_UserInfos_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "UserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VacancyCompetences",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompetenceId = table.Column<int>(nullable: false),
                    LevelId = table.Column<int>(nullable: false),
                    VacancyId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacancyCompetences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VacancyCompetences_FacetItems_CompetenceId",
                        column: x => x.CompetenceId,
                        principalTable: "FacetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VacancyCompetences_FacetItems_LevelId",
                        column: x => x.LevelId,
                        principalTable: "FacetItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VacancyCompetences_Vacancies_VacancyId",
                        column: x => x.VacancyId,
                        principalTable: "Vacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_ProjectId",
                table: "Vacancies",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_UserInfoId",
                table: "Vacancies",
                column: "UserInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyCompetences_CompetenceId",
                table: "VacancyCompetences",
                column: "CompetenceId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyCompetences_LevelId",
                table: "VacancyCompetences",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancyCompetences_VacancyId",
                table: "VacancyCompetences",
                column: "VacancyId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompetences_FacetItems_CompetenceId",
                table: "UserCompetences",
                column: "CompetenceId",
                principalTable: "FacetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompetences_FacetItems_LevelId",
                table: "UserCompetences",
                column: "LevelId",
                principalTable: "FacetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompetences_UserInfos_UserInfoId",
                table: "UserCompetences",
                column: "UserInfoId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCompetences_FacetItems_CompetenceId",
                table: "UserCompetences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCompetences_FacetItems_LevelId",
                table: "UserCompetences");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCompetences_UserInfos_UserInfoId",
                table: "UserCompetences");

            migrationBuilder.DropTable(
                name: "VacancyCompetences");

            migrationBuilder.DropTable(
                name: "Vacancies");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserCompetences",
                table: "UserCompetences");

            migrationBuilder.RenameTable(
                name: "UserCompetences",
                newName: "UserCompetence");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompetences_UserInfoId",
                table: "UserCompetence",
                newName: "IX_UserCompetence_UserInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompetences_LevelId",
                table: "UserCompetence",
                newName: "IX_UserCompetence_LevelId");

            migrationBuilder.RenameIndex(
                name: "IX_UserCompetences_CompetenceId",
                table: "UserCompetence",
                newName: "IX_UserCompetence_CompetenceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserCompetence",
                table: "UserCompetence",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompetence_FacetItems_CompetenceId",
                table: "UserCompetence",
                column: "CompetenceId",
                principalTable: "FacetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompetence_FacetItems_LevelId",
                table: "UserCompetence",
                column: "LevelId",
                principalTable: "FacetItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCompetence_UserInfos_UserInfoId",
                table: "UserCompetence",
                column: "UserInfoId",
                principalTable: "UserInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
