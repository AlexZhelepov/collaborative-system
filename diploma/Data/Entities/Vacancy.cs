using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    public class Vacancy
    {
        public int Id { get; set; }

        /// <summary>
        /// Наименование вакансии.
        /// </summary>
        [Required]
        [Display(Name="Наименование вакансии")]
        public string Name { get; set; }

        /// <summary>
        /// Исполнитель.
        /// </summary>
        [ForeignKey("UserInfo")]
        public int? UserInfoId { get; set; }

        /// <summary>
        /// Id-шник проекта.
        /// </summary>
        public int ProjectId { get; set; }

        // Связи.
        public Project Project { get; set; }
        public virtual UserInfo UserInfo { get; set; }
        public ICollection<VacancyCompetence> VacancyCompetences { get; set; }
    }
}
