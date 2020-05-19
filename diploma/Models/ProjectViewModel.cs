using diploma.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Models
{
    public class ProjectDetailsViewModel
    {
        public int ProjectId { get; set; }
        public List<VacancyViewModel> Vacancies { get; set; }
    }

    public class VacancyViewModel
    {
        public Vacancy Vacancy { get; set; }
        public ApplicantViewModel Applicant { get; set; }
        public List<SelectListItem> UsersSelectList { get; set; }
        public List<VacancyCompetenceViewModel> Competences { get; set; }
    }

    public class ApplicantViewModel
    {
        [Display(Name = "Исполнитель")]
        public int? Id { get; set; }
        public string Name { get; set; }
    }

    public class VacancyCompetenceViewModel
    {
        public VacancyCompetence Competence { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
    }

    public class VacancyCompetenceEditViewModel 
    {
        public VacancyCompetence Competence { get; set; }
        public List<SelectListItem> Competences { get; set; }
        public List<SelectListItem> Levels { get; set; }
    }
}
