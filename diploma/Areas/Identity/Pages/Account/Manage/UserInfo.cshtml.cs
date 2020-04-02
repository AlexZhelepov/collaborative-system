using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using diploma.Data;
using diploma.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace diploma.Areas.Identity.Pages.Account.Manage
{
    public class UserInfoModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<UserInfoModel> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserInfoModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEvironment,
            ILogger<UserInfoModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
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
        }
        
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Не удалось найти пользователя с ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }

            // Поиск подходящего описания пользователя по id-шнику.
            Input = new InputModel();

            using (ApplicationDbContext db = AppContextFactory.DB) 
            {
                var userInfo = db.UserInfos.FirstOrDefault(n => n.UserId == user.Id);
                   
                if (userInfo != null)
                {
                    Input.FIO = userInfo.Name;
                    Input.VacationStart = userInfo.VacationStart;
                    Input.CareerStart = userInfo.CareerStart;
                    Input.AvatarPath = userInfo.AvatarPath;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync() 
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                InputModel model = new InputModel();
                await TryUpdateModelAsync<InputModel>(model, "Input", n => n.FIO, n => n.CareerStart, n => n.VacationStart, n => n.Avatar, n => n.AvatarPath);

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound($"Не удалось найти пользователя с ID '{_userManager.GetUserId(User)}'.");
                }

                // Сохранение данных о пользователе и файла.
                using (ApplicationDbContext db = AppContextFactory.DB) 
                {
                    using var t = db.Database.BeginTransaction();
                    try
                    {
                        var userInfo = db.UserInfos.FirstOrDefault(n => n.UserId == user.Id);

                        // Удаление старого файла.
                        if (userInfo != null)
                        {
                            if (model.Avatar != null && !string.IsNullOrEmpty(userInfo.AvatarPath))
                            {
                                string deletePath = Path.Combine(_webHostEnvironment.WebRootPath, userInfo.AvatarPath);

                                if (System.IO.File.Exists(deletePath))
                                {
                                    System.IO.File.Delete(deletePath);
                                }
                            }
                        }

                        // Create.
                        if (userInfo == null)
                        {
                            userInfo = new UserInfo() { User = user, UserId = user.Id };
                        }

                        // Сохранение нового файла.
                        string filePath = string.Empty;
                        
                        if (model.Avatar != null)
                        {
                            string avatarPath = @"Files/UserInfo/";
                            string fileName = user.Id + "_" + model.Avatar.FileName;
                            filePath = Path.Combine(_webHostEnvironment.WebRootPath, avatarPath, fileName);

                            // Существует ли директория? Создаем, если вдруг нет.
                            string directoryPath = Path.Combine(_webHostEnvironment.WebRootPath, avatarPath);
                            if (!Directory.Exists(directoryPath)) 
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            using FileStream fs = new FileStream(filePath, FileMode.Create);
                            await model.Avatar.CopyToAsync(fs);
                            
                            userInfo.AvatarPath = Path.Combine(avatarPath, fileName);
                        }

                        // Update data.
                        userInfo.Name = model.FIO;
                        userInfo.VacationStart = model.VacationStart;
                        userInfo.CareerStart = model.CareerStart;

                        // Add new data.
                        if (userInfo.Id == 0) 
                        {
                            db.Entry(userInfo).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        }

                        await db.SaveChangesAsync();
                        await t.CommitAsync();
                    }
                    catch (Exception e)
                    {
                        await t.RollbackAsync();
                        ModelState.AddModelError("Error", "Не удалось сохранить данные пользователя: " + (e.InnerException.Message ?? e.Message));
                        return Page();
                    }
                }
                return RedirectToPage();
            }
            catch (Exception e) 
            {
                ModelState.AddModelError("Error", "Произошла ошибка: " + e.Message);
                return Page();
            }
        }
    }
}
