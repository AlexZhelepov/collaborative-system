using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    /// <summary>
    /// Сущность, описывающая проект.
    /// </summary>
    public class Project
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name="Наименование")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Дата начала")]
        public DateTime DateStart { get; set; }

        [Display(Name = "Дата окончания")]
        public DateTime? DateEnd { get; set; }


        // Связи, связи и еще раз связи - без них сегодня никуда.
        public ICollection<Vacancy> Vacancies { get; set; }
    }
}
