using diploma.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Models
{
    public class UserCompetenceEditViewModel
    {
        public UserCompetence UserCompetence { get; set; }
        public List<SelectListItem> Competences { get; set; }
        public List<SelectListItem> Levels { get; set; }
    }

    public class UserCompetenceListViewModel 
    {
        public int UserId { get; set; }
        public IEnumerable<UserCompetenceViewModel> List { get; set; }
    }

    public class UserCompetenceViewModel 
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string Competence { get; set; }
        public string Level { get; set; }
    }
}
