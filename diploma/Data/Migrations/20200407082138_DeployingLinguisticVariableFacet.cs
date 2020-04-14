using diploma.Data.Entities;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;
using System.Linq;

namespace diploma.Data.Migrations
{
    public partial class DeployingLinguisticVariableFacet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using ApplicationDbContext db = AppContextFactory.DB;
            List<Facet> facets = new List<Facet>()
                {
                    new Facet() { Name = "Уровни", Code = "levels" },
                };

            foreach (var item in facets)
            {
                db.Facets.Attach(item);
            }

            db.SaveChanges();

            var levelsFacet = facets.FirstOrDefault(i => i.Code == "levels");

            List<FacetItem> facetItems = new List<FacetItem>()
                    {
                        new FacetItem()
                        {
                            Facet = levelsFacet,
                            Name = "низкий",
                            Value = 0.1F
                        },
                        new FacetItem()
                        {
                            Facet = levelsFacet,
                            Name = "ниже среднего",
                            Value = 0.25F
                        },
                        new FacetItem()
                        {
                            Facet = levelsFacet,
                            Name = "средний",
                            Value = 0.5F
                        },
                        new FacetItem()
                        {
                            Facet = levelsFacet,
                            Name = "выше среднего",
                            Value = 0.75F
                        },
                        new FacetItem()
                        {
                            Facet = levelsFacet,
                            Name = "высокий",
                            Value = 1.0F
                        }
                    };
            db.FacetItems.AddRange(facetItems);
            db.SaveChanges();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // опять не написал обратку, ну и ... ладно =)
        }
    }
}
