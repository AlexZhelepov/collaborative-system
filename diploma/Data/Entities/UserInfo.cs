using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    public class UserInfo : IdentityUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CareerStart { get; set; }
        public DateTime? VacationStart { get; set; }
    }
}
