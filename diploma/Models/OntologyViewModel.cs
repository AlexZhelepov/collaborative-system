using diploma.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Models
{
    public class OntologyViewModel
    {
        public Facet Class { get; set; }
        public FacetItem Element { get; set; }
    }

    public class OntologyEditViewModel 
    {
        [Required]
        [Display(Name = "Тип класса")]
        public int FacetId { get; set; }
        
        [Required]
        [Display(Name = "Класс")]
        public string ElementName { get; set; }

        public List<SelectListItem> Facets { get; set; }
    }
}
