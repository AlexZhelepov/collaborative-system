using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    public class Facet
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Code { get; set; }

        public ICollection<FacetItem> FacetItems { get; set; }
    }
}
