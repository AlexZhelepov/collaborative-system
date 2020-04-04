using diploma.Data.Entities;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Collections.Generic;
using System.Linq;

namespace diploma.Data.Migrations
{
    public partial class SlightFix2ForKatherineSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using (ApplicationDbContext db = AppContextFactory.DB)
            {
                List<Facet> facets = new List<Facet>()
                {
                    new Facet() { Name = "Компетенции", Code = "skills" },
                    new Facet() { Name = "Предметные области", Code = "subject" }
                };

                foreach (var item in facets)
                {
                    db.Facets.Attach(item);
                }

                db.SaveChanges();

                var skillsFacet = facets.FirstOrDefault(i => i.Code == "skills");
                var subjectsFacet = facets.FirstOrDefault(i => i.Code == "subject");

                List<FacetItem> facetItems = new List<FacetItem>()
                    {
                        // компетенции (skills).
                        new FacetItem()
                        {
                            Facet = skillsFacet,
                            Name = "составление плана-графика"
                        },
                        new FacetItem()
                        {
                            Facet = skillsFacet,
                            Name = "составление отчетности"
                        },
                        new FacetItem()
                        {
                            Facet = skillsFacet,
                            Name = "создание плана мероприятия"
                        },
                        new FacetItem()
                        {
                            Facet = skillsFacet,
                            Name = "финансирование"
                        },
                        new FacetItem()
                        {
                            Facet = skillsFacet,
                            Name = "заключение соглашения"
                        },
                        new FacetItem()
                        {
                            Facet = skillsFacet,
                            Name = "составление рекомендаций"
                        },
                        new FacetItem()
                        {
                            Facet = skillsFacet,
                            Name = "создание"
                        },
                        new FacetItem()
                        {
                            Facet = skillsFacet,
                            Name = "утверждение"
                        },
                        new FacetItem()
                        {
                            Facet = skillsFacet,
                            Name = "проведение мероприятия"
                        },
                        // предметные области (subjects).
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "здравоохраниение",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "демография",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "инновации",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "городская среда ",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "ит",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "it",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "агропромышленность",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "экспорт",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "семья",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "образование",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "кадры",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "бюджетирование",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "экология",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "строительство",
                        },
                        new FacetItem()
                        {
                            Facet = subjectsFacet,
                            Name = "культура",
                        }
                    };
                db.FacetItems.AddRange(facetItems);
                db.SaveChanges();
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
