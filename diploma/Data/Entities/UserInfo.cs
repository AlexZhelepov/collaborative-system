using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    public class UserInfo
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        public string AvatarPath { get; set; }
        
        [Required]
        public DateTime CareerStart { get; set; }
        public DateTime VacationStart { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
