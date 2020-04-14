using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    /// <summary>
    /// Класс, описывающий компетенцию пользователя.
    /// </summary>
    public class UserCompetence
    {    
        public int Id { get; set; }

        /// <summary>
        /// Ссылка на пользователя.
        /// </summary>
        [ForeignKey("UserInfo")]
        public int UserInfoId { get; set; }

        /// <summary>
        /// Компетенция (навык или предметная область).
        /// </summary>
        [ForeignKey("FacetItem")]
        [Display(Name="Навык")]
        public int CompetenceId { get; set; }

        /// <summary>
        /// Уровень владения навыком (лингвистическая переменная).
        /// </summary>
        [ForeignKey("FacetItem")]
        [Display(Name = "Уровень подготовки")]
        public int LevelId { get; set; }

        // Ключи.
        public virtual UserInfo UserInfo { get; set; }
        public virtual FacetItem Competence { get; set; }
        public virtual FacetItem Level { get; set; }
    }
}
