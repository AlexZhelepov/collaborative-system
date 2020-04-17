using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using diploma.Data;
using diploma.Data.Entities;
using diploma.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Spire.Pdf.Tables;

namespace diploma.Controllers
{
    public class UserController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UserController(IWebHostEnvironment env) : base()
        {
            _env = env;
        }

        #region UserInfo.

        /// <summary>
        /// Список пользователей.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            using var db = AppContextFactory.DB;

            // Достаем из базы список пользователей всех.
            var list = (from ui in db.UserInfos
                        join u in db.Users on ui.UserId equals u.Id
                        into grp
                        from user in grp.DefaultIfEmpty()
                        select new UserViewModel() { User = user, UserInfo = ui }).ToList();

            return View(list);
        }

        /// <summary>
        /// Редактирование пользователя.
        /// </summary>
        public IActionResult Edit(int? id)
        {
            using var db = AppContextFactory.DB;

            UserInfo user = null;
            if (id.HasValue)
            {
                user = db.UserInfos.FirstOrDefault(i => i.Id == id.Value);
            }

            UserEditViewModel model = new UserEditViewModel();

            // Когда left join писать лень!
            var attachedUsers = db.UserInfos.Where(i => !string.IsNullOrEmpty(i.UserId)).Select(i => i.UserId).ToArray();
            model.Bindings = (from u in db.Users
                              where !attachedUsers.Contains(u.Id)
                              select new SelectListItem() { Value = u.Id, Text = u.Email }).ToList()
                              ?? // застрахуй братуху, если вдруг все прикреплены уже.
                              new List<SelectListItem>();

            model.Bindings.Add(new SelectListItem() { Text = "[ Прикрепи пользователя ]", Value = string.Empty, Selected = true });

            if (user != null)
            {
                model.ApplicationUserId = user.UserId;
                model.VacationStart = user.VacationStart;
                model.FIO = user.Name;
                model.CareerStart = user.CareerStart;
                model.AvatarPath = user.AvatarPath;

                if (!string.IsNullOrEmpty(user.UserId))
                {
                    var appUser = db.Users.First(i => i.Id == user.UserId);
                    model.Bindings.Add(new SelectListItem() { Text = appUser.Email, Value = appUser.Id, Selected = true });
                }
            }

            return View(model);
        }

        /// <summary>
        /// Редактирование пользователя.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(int? id, string name)
        {
            UserEditViewModel model = new UserEditViewModel();
            await TryUpdateModelAsync(model, "", i => i.FIO, i => i.CareerStart, i => i.VacationStart, i => i.Avatar, i => i.AvatarPath, i => i.ApplicationUserId);

            // Валидация.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var db = AppContextFactory.DB;
            using var t = db.Database.BeginTransaction();

            try
            {
                // Редактируем пользователя.
                var user = db.UserInfos.FirstOrDefault(i => i.Id == id) ?? new UserInfo();

                // Аватара Фрейи (да был такой НИП в WOTLK, вернуться бы в 10 класс...).
                // Новый аватар.
                if (model.Avatar != null)
                {
                    // Удаление старого файла, если он у нас есть.
                    if (!string.IsNullOrEmpty(model.AvatarPath))
                    {
                        AvatarHelper.DeleteFile(_env, model.AvatarPath);
                    }

                    // Сохранение нового файла.
                    string filePath = await AvatarHelper.SaveFile(model.Avatar, _env);

                    // Прикрепляем / обновляем файл к профилю пользователя.
                    user.AvatarPath = filePath;
                }

                // Обновляем данные пользователя.
                user.CareerStart = model.CareerStart;
                user.Name = model.FIO;
                user.VacationStart = model.VacationStart;
                user.UserId = model.ApplicationUserId;

                if (user.Id == 0)
                {
                    db.Entry(user).State = EntityState.Added;
                }

                await db.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                ModelState.AddModelError("Error", "В процессе сохранения произошла ошибка! Ошибка: " + ex.Message);
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region UserCompetence

        /// <summary>
        /// Перечень компетенций и навыков участников.
        /// </summary>
        public IActionResult CompetenceList(int id)
        {
            using var db = AppContextFactory.DB;

            var list = (from uc in db.UserCompetences
                        join fi in db.FacetItems on uc.CompetenceId equals fi.Id
                        join f in db.Facets on fi.FacetId equals f.Id
                        join lvl in db.FacetItems on uc.LevelId equals lvl.Id
                        where uc.UserInfoId == id
                        select new UserCompetenceViewModel()
                        {
                            Id = uc.Id,
                            Category = f.Name,
                            Competence = fi.Name,
                            Level = lvl.Name
                        }).OrderBy(i => i.Category).ToList();

            var model = new UserCompetenceListViewModel()
            {
                UserId = id,
                List = list
            };

            return View(model);
        }

        public IActionResult CompetenceEdit(int? id, int? userid)
        {
            using var db = AppContextFactory.DB;

            UserCompetence uc = null;
            if (id.HasValue)
            {
                uc = db.UserCompetences.FirstOrDefault(i => i.Id == id.Value);
            }

            var competences = DataHelper.CreateSelectListItem(db, uc != null ? uc.CompetenceId : (int?)null, "subjects", "skills");
            var levels = DataHelper.CreateSelectListItem(db, uc != null ? uc.LevelId : (int?)null, "levels");

            var model = new UserCompetenceEditViewModel()
            {
                UserCompetence = uc ?? new UserCompetence() { UserInfoId = userid.Value },
                Competences = competences,
                Levels = levels
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CompetenceEdit(int? id, string name)
        {
            UserCompetence model = new UserCompetence();
            await TryUpdateModelAsync<UserCompetence>(model, "UserCompetence", i => i.UserInfoId, i => i.LevelId, i => i.CompetenceId);

            using var db = AppContextFactory.DB;

            if (db.UserCompetences.Any(i => i.CompetenceId == model.CompetenceId && i.Id != id))
            {
                ModelState.AddModelError("Error", "Такая компетенция уже добавлена!");
            }

            // Валидация.
            if (!ModelState.IsValid)
            {
                return View(
                    new UserCompetenceEditViewModel()
                    {
                        UserCompetence = model,
                        Competences = DataHelper.CreateSelectListItem(db, model.CompetenceId, "subjects", "skills"),
                        Levels = DataHelper.CreateSelectListItem(db, model.LevelId, "levels")
                    });
            }

            using var t = db.Database.BeginTransaction();
            try
            {
                // Редактируем навык или сохраняем новый.
                var uc = db.UserCompetences.FirstOrDefault(i => i.Id == id) ?? new UserCompetence();

                uc.LevelId = model.LevelId;
                uc.UserInfoId = model.UserInfoId;
                uc.CompetenceId = model.CompetenceId;

                if (uc.Id == 0)
                {
                    db.Entry(uc).State = EntityState.Added;
                }

                await db.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                ModelState.AddModelError("Error", "В процессе сохранения произошла ошибка! Ошибка: " + ex.Message);
            }

            return RedirectToAction("CompetenceList", new { id = model.UserInfoId });
        }

        [HttpPost]
        public async Task<IActionResult> CompetenceDelete(int id, int userid)
        {
            using var db = AppContextFactory.DB;
            try
            {
                var uc = db.UserCompetences.FirstOrDefault(i => i.Id == id);
                if (uc != null)
                {
                    db.Remove(uc);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "В процессе удаления произошла ошибка! Ошибка: " + ex.Message);
            }

            return RedirectToAction("CompetenceList", new { id = userid });
        }
        #endregion

        #region

        /// <summary>
        /// Делает импорт пользователеи и их компетенции в UserInfo таблицу.
        /// </summary>
        public async Task<IActionResult> ImportUserInfos()
        {
            using var db = AppContextFactory.DB;
            using var t = db.Database.BeginTransaction();

            int levelId = db.FacetItems.FirstOrDefault(i => i.Name == "низкий").Id; // надеемся и свято верим что он там есть. А чтобы узнать, спецом в try не сунул =)

            try
            {
                // Стадия 1. Обновление существующих пользователеи (+ их компетенции).
                // Правила игры: только добавляем новые компетенции. Ничего не обновляем и ничего в целом тут не делаем.
                var oldUsers = db.DocFiles.Where(i => i.UserInfoId.HasValue).ToList();

                foreach (var u in oldUsers)
                {
                    // Забираем скилы из фаила.
                    var skills = (from w in db.Words
                                  where w.DocFileId == u.Id && w.FacetItemId.HasValue
                                  join fi in db.FacetItems on w.FacetItemId equals fi.Id
                                  select fi.Id).ToList();

                    // Ищем компетенции, которые есть у пользователя на текущии момент.
                    var alreadyExisting = (from uc in db.UserCompetences
                                           where uc.UserInfoId == u.UserInfoId
                                           select uc.CompetenceId).ToList();

                    // То что добавить нужно будет.
                    var toAdd = skills.Except(alreadyExisting);

                    foreach (var c in toAdd)
                    {
                        UserCompetence uc = new UserCompetence()
                        {
                            LevelId = levelId,
                            CompetenceId = c,
                            UserInfoId = u.UserInfoId.Value
                        };

                        db.Entry(uc).State = EntityState.Added;
                        await db.SaveChangesAsync();
                    }
                }

                // Стадия 2. Добавление новых пользователеи (+ их компетенции).
                // Правила игры: уровень импорта скилов всегда ставив самыи низкии.
                var newUsers = db.DocFiles.Where(i => !i.UserInfoId.HasValue).ToList();

                foreach (var u in newUsers)
                {
                    // Забираем скилы из фаила.
                    var skills = (from w in db.Words
                                  where w.DocFileId == u.Id && w.FacetItemId.HasValue
                                  join fi in db.FacetItems on w.FacetItemId equals fi.Id
                                  join f in db.Facets on fi.FacetId equals f.Id
                                  select new { skillId = fi.Id }).ToList();

                    // Сохраняем пользователя со всеми потрохами.
                    UserInfo ui = new UserInfo()
                    {
                        Name = u.FIO,
                        CareerStart = DateTime.Now.Date //да просто рандомная дата. Как говорил один из персонажей в моей жизни бесполезной: "ГАВНОМ ЗАБИВАй (Звучит прям как "Родину Защищай".). Есть, Андрей Николаевич - чту, горжусь, помню и славно следую принципу борьбы с not null полями =)"
                    };

                    db.Entry(ui).State = EntityState.Added;
                    await db.SaveChangesAsync();

                    // Сохраняем связь между пользователем и документом.
                    u.UserInfoId = ui.Id;
                    db.Entry(u).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    // Сохраняем данные о компетенциях пользователя.
                    if (skills.Any())
                    {
                        foreach (var s in skills)
                        {
                            UserCompetence uc = new UserCompetence()
                            {
                                LevelId = levelId,
                                CompetenceId = s.skillId,
                                UserInfoId = ui.Id
                            };

                            db.Entry(uc).State = EntityState.Added;
                            await db.SaveChangesAsync();
                        }
                    }
                }

                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                ModelState.AddModelError("Error", "В процессе импорта произошла ошибка! Ошибка: " + ex.Message);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Объединение пользователеи (нужно после испорта, потому что в документах люди могут дублироваться).
        /// Объединять можно только двоих за раз. Не более.
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> JoinUserInfos([FromBody] int[] data)
        {
            using var db = AppContextFactory.DB;
            using var t = db.Database.BeginTransaction();

            try
            {
                var user_1 = db.UserInfos.First(i => i.Id == data[0]);
                var user_2 = db.UserInfos.First(i => i.Id == data[1]);

                // Есть ли на втором пользователе проекты? Привязан ли он ко второму пользователю?
                var vacancies_2 = db.Vacancies.Where(i => i.UserInfoId == user_2.Id).ToList();

                if (vacancies_2.Count > 0)
                {
                    foreach (var v in vacancies_2)
                    {
                        v.UserInfoId = user_1.Id;
                        await db.SaveChangesAsync();
                    }
                }

                // Есть ли привязка к пользователю в системе?
                if (!string.IsNullOrEmpty(user_2.UserId))
                {
                    // Если уже есть привязка к пользователю, то перевешиваем.
                    user_1.UserId ??= user_2.UserId;
                    await db.SaveChangesAsync();
                }

                // Если есть закрепленные навыки и предметные области.
                var competences_2 = db.UserCompetences.Where(i => i.UserInfoId == user_2.Id).ToList();
                var competences_1 = db.UserCompetences.Where(i => i.UserInfoId == user_1.Id).ToList();

                if (competences_2.Count > 0)
                {
                    foreach (var c in competences_2)
                    {
                        if (competences_1.Any(i => i.CompetenceId == c.Id))
                        {
                            continue;
                        }
                        else
                        {
                            c.UserInfoId = user_1.Id;
                            await db.SaveChangesAsync();
                        }
                    }
                }

                // Обновляем все DocFiles, чтобы повторно не плодить исполнителеи.
                var docFiles_2 = db.DocFiles.Where(i => i.UserInfoId == user_2.Id).ToList();

                foreach (var file in docFiles_2) 
                {
                    file.UserInfoId = user_1.Id;
                    await db.SaveChangesAsync();
                }

                // И, напоследок, удаляем или перемещаем аватарку пользователя.
                if (!string.IsNullOrEmpty(user_2.AvatarPath))
                {
                    if (string.IsNullOrEmpty(user_1.AvatarPath))
                    {
                        user_1.AvatarPath = user_2.AvatarPath;
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        AvatarHelper.DeleteFile(_env, user_2.AvatarPath);
                    }
                }

                db.UserInfos.Remove(user_2);
                await db.SaveChangesAsync();

                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                return Json(new { error = true, message = "Нельзя сотворить здесь!" });
            }

            return Json(new { error = false, message = "Исследование завершено!" });
        }

        #endregion
    }

    public static class AvatarHelper
    {
        public static void DeleteFile(IWebHostEnvironment env, string path)
        {
            string deletePath = Path.Combine(env.WebRootPath, path);

            if (File.Exists(deletePath))
            {
                File.Delete(deletePath);
            }
        }

        /// <summary>
        /// Сохранение файла на сервер.
        /// </summary>
        /// <returns>Возвращает строку-путь до файла.</returns>
        public static async Task<string> SaveFile(IFormFile file, IWebHostEnvironment env, string folder = @"Files/UserInfo/")
        {
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(env.WebRootPath, folder, uniqueFileName);

            string directoryPath = Path.Combine(env.WebRootPath, folder);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using var fs = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fs);

            return Path.Combine(folder, uniqueFileName);
        }
    }
}