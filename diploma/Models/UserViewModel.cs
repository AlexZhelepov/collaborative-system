using diploma.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Models
{
    public class UserViewModel
    {
        public UserInfo UserInfo { get; set; }
        public ApplicationUser User { get; set; }
    }

    public class UserEditViewModel 
    {
        [Required]
        [Display(Name = "ФИО")]
        public string FIO { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Дата начала трудовой деятельности")]
        public DateTime CareerStart { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Дата начала отпуска")]
        public DateTime VacationStart { get; set; }

        [Display(Name = "Аватар")]
        public IFormFile Avatar { get; set; }

        [Display(Name = "Аватар")]
        public string AvatarPath { get; set; }

        [Display(Name = "Прикрепленный пользователь")]
        public string ApplicationUserId { get; set; }

        public List<SelectListItem> Bindings { get; set; }
    }
}
