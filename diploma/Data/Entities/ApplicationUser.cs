using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    // avatar: https://stackoverflow.com/questions/32296866/asp-net-mvc-5-add-avatar-to-aspnetuser .
    public class ApplicationUser : IdentityUser
    {
        public virtual UserInfo UserInfo { get; set; }
    }
}
