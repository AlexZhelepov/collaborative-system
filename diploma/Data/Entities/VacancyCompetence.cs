using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    /// <summary>
    /// Предметная область или навык, важный для вакансии.
    /// </summary>
    public class VacancyCompetence
    {
        public int Id { get; set; }

        /// <summary>
        /// Требуемая компетенция (навык или предметная область).
        /// </summary>
        [ForeignKey("FacetItem")]
        [Display(Name="Требуемый навык / предметная область")]
        public int CompetenceId { get; set; }

        /// <summary>
        /// Требуемый уровень вакансии.
        /// </summary>
        [ForeignKey("FacetItem")]
        [Display(Name="Требуемый уровень подготовки")]
        public int LevelId { get; set; }
        
        /// <summary>
        /// Id-шник вакансии самой.
        /// </summary>
        public int VacancyId { get; set; }

        public Vacancy Vacancy { get; set; }

        public virtual FacetItem Level { get; set; }

        public virtual FacetItem Competence { get; set; }
    }
}
