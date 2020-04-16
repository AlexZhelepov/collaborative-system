using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    public class FacetItem
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        public double? Value { get; set; } 
 
        public int FacetId { get; set; }

        [Required]
        public Facet Facet { get; set; }
        //public virtual Word Word { get; set; }
    }
}
