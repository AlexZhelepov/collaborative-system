using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using diploma.Data;
using diploma.Data.Entities;
using diploma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace diploma.Controllers
{
    [Authorize(Roles = "admin")]
    public class ProjectController : Controller
    {
        #region Project
        /// <summary>
        /// Список проектов.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            using var db = AppContextFactory.DB;

            // Достаем из базы список всех проектов.
            var list = db.Projects.ToList();
            
            return View(list);
        }

        /// <summary>
        /// Перечень вакансий проекта.
        /// </summary>
        public IActionResult Details(int id)
        {
            using var db = AppContextFactory.DB;

            // Достаем из базы список всех проектов.
            var list = (from v in db.Vacancies
                        join u in db.UserInfos on v.UserInfoId equals u.Id
                        into grp
                        from user in grp.DefaultIfEmpty()
                        select new VacancyViewModel()
                        {
                            Vacancy = v,
                            Applicant = new ApplicantViewModel() { Id = user.Id, Name = user.Name },
                            Competences = new List<VacancyCompetenceViewModel>()
                        }).ToList();

            list.ForEach(i =>
            {
                i.Competences = (from v in db.VacancyCompetences
                                 where v.VacancyId == i.Vacancy.Id
                                 join c in db.FacetItems on v.CompetenceId equals c.Id
                                 join lvl in db.FacetItems on v.LevelId equals lvl.Id
                                 join cat in db.Facets on c.FacetId equals cat.Id
                                 select new VacancyCompetenceViewModel()
                                 {
                                     Competence = v,
                                     Category = cat.Name,
                                     Level = lvl.Name,
                                     Name = c.Name
                                 }).ToList();
            });

            var model = new ProjectDetailsViewModel()
            {
                Vacancies = list,
                ProjectId = id
            };

            return View(model);
        }

        /// <summary>
        /// Страница создания нового проекта.
        /// </summary>
        /// <returns></returns>
        public IActionResult Edit(int? id)
        {
            using var db = AppContextFactory.DB;

            Project project = null;
            if (id.HasValue)
            {
                project = db.Projects.FirstOrDefault(i => i.Id == id.Value);
            }
            else
            {
                project = new Project() { DateStart = DateTime.Now.Date, DateEnd = DateTime.Now.AddDays(24).Date }; // не знаю почему 24, подумайте тоже =)
            }

            return View(project);
        }

        /// <summary>
        /// Сохранение нового проекта.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(int? id, string name)
        {
            Project model = new Project();
            await TryUpdateModelAsync<Project>(model, "", i => i.Name, i => i.Description, i => i.DateStart, i => i.DateEnd);

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
                var project = db.Projects.FirstOrDefault(i => i.Id == id) ?? new Project();

                // Обновляем данные пользователя.
                project.Name = model.Name;
                project.Description = model.Description;
                project.DateStart = model.DateStart;
                project.DateEnd = model.DateEnd;

                if (project.Id == 0)
                {
                    db.Entry(project).State = EntityState.Added;
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

        /// <summary>
        /// Удаление проекта.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using var db = AppContextFactory.DB;
            try
            {
                var project = db.Projects.FirstOrDefault(i => i.Id == id);
                if (project != null)
                {
                    db.Remove(project);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "В процессе удаления произошла ошибка! Ошибка: " + ex.Message);
            }

            return RedirectToAction("Index");
        }
        #endregion

        #region Вакансии проектов.

        /// <summary>
        /// Создание новой вакансии.
        /// </summary>
        public IActionResult VacancyEdit(int? id, int projectid)
        {
            using var db = AppContextFactory.DB;

            Vacancy v = new Vacancy() { ProjectId = projectid };
            ApplicantViewModel ui = new ApplicantViewModel();
            List<VacancyCompetenceViewModel> vclist = new List<VacancyCompetenceViewModel>();

            if (id.HasValue)
            {
                v = db.Vacancies.FirstOrDefault(i => i.Id == id.Value) ?? new Vacancy() { ProjectId = projectid };
            }

            // Готовимся редактировать.
            if (v.Id != 0)
            {
                // Ищем назначенного.
                if (v.UserInfoId.HasValue)
                {
                    var applicant = db.UserInfos.FirstOrDefault(i => i.Id == v.UserInfoId);
                    ui = new ApplicantViewModel() { Id = applicant.Id, Name = applicant.Name };
                }

                // Список компетенций и уровней к вакансии.
                vclist = (from vc in db.VacancyCompetences
                          where vc.VacancyId == v.Id
                          join c in db.FacetItems on vc.CompetenceId equals c.Id
                          join lvl in db.FacetItems on vc.LevelId equals lvl.Id
                          join cat in db.Facets on c.FacetId equals cat.Id
                          select new VacancyCompetenceViewModel()
                          {
                              Competence = vc,
                              Category = cat.Name,
                              Level = lvl.Name,
                              Name = c.Name
                          }).ToList();
            }
            else
            {
                v = new Vacancy() { ProjectId = projectid };
            }

            // Перечень всех пользователей для выбора под вакансией.
            List<SelectListItem> usersSelectList = new List<SelectListItem>() { new SelectListItem()
            {
                Text = "[ Выберите исполнителя ]",
                Value = "",
                Selected = true
            }};

            usersSelectList.AddRange(db.UserInfos.Select(i => new SelectListItem()
            {
                Text = i.Name,
                Value = i.Id.ToString(),
                Selected = v.UserInfoId.HasValue && i.Id == v.UserInfoId.Value ? true : false
            }).ToList());

            VacancyViewModel model = new VacancyViewModel()
            {
                UsersSelectList = usersSelectList,
                Applicant = ui,
                Competences = vclist,
                Vacancy = v
            };

            return View(model);
        }

        /// <summary>
        /// Создание вакансии.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VacancyEdit(int? id, string name)
        {
            VacancyViewModel model = new VacancyViewModel();
            int projectId = 0;

            // А работает ли так - хз ваще!
            await TryUpdateModelAsync(model, "", i => i.Vacancy, i => i.Applicant, i => i.UsersSelectList, i => i.Competences);

            projectId = model.Vacancy.ProjectId;

            if (projectId == 0)
            {
                ModelState.AddModelError("Error", "Ошибка с прикреплением вакансии к проекту");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var db = AppContextFactory.DB;
            using var t = db.Database.BeginTransaction();

            try
            {
                // Редактируем вакансию.
                var vacancy = db.Vacancies.FirstOrDefault(i => i.Id == id) ?? new Vacancy();

                vacancy.UserInfoId = model.Applicant.Id;
                vacancy.Name = model.Vacancy.Name;
                vacancy.ProjectId = model.Vacancy.ProjectId;

                if (vacancy.Id == 0)
                {
                    db.Entry(vacancy).State = EntityState.Added;
                }

                await db.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                ModelState.AddModelError("Error", "В процессе сохранения произошла ошибка! Ошибка: " + ex.Message);
            }

            return RedirectToAction("Details", new { id = projectId });
        }

        /// <summary>
        /// Удаление вакансии.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> VacancyDelete(int id, int projectid)
        {
            using var db = AppContextFactory.DB;
            try
            {
                var v = db.Vacancies.FirstOrDefault(i => i.Id == id);
                if (v != null)
                {
                    db.Remove(v);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "В процессе удаления вакансии произошла ошибка! Ошибка: " + ex.Message);
            }

            return RedirectToAction("Details", new { id = projectid });
        }

        /// <summary>
        /// Всегда только добавляем - не редактируемая штука.
        /// </summary>
        public IActionResult AddCompetence(int vacancyid, int projectid)
        {
            using var db = AppContextFactory.DB;

            VacancyCompetenceEditViewModel model = new VacancyCompetenceEditViewModel() { Competence = new VacancyCompetence() { VacancyId = vacancyid } };
            var skills = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Selected = true,
                    Text = "[ Выберите навык / область ]",
                    Value = null
                }
            };
            skills.AddRange(DataHelper.CreateSelectListItem(db, null, "skills", "subjects"));

            var levels = new List<SelectListItem>()
            {
                new SelectListItem()
                {
                    Selected = true,
                    Text = "[ Выберите уровень ]",
                    Value = null
                }
            };
            levels.AddRange(DataHelper.CreateSelectListItem(db, null, "levels"));

            model.Competences = skills;
            model.Levels = levels;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddCompetence(int projectid)
        {
            using var db = AppContextFactory.DB;
            VacancyCompetence model = new VacancyCompetence();

            await TryUpdateModelAsync<VacancyCompetence>(model, "Competence", i => i.CompetenceId, i => i.LevelId, i => i.VacancyId);

            // Проверка если такая компетенция уже есть у вакансии.
            if (db.VacancyCompetences.Any(i => i.VacancyId == model.VacancyId && i.CompetenceId == model.CompetenceId))
            {
                ModelState.AddModelError("Error", "К данной вакансии уже прикреплено данное умение. Жизнь за Нер'Зула!");
            }

            if (!ModelState.IsValid)
            {
                return View(new VacancyCompetenceEditViewModel()
                {
                    Competence = model,
                    Competences = DataHelper.CreateSelectListItem(db, model.CompetenceId, "skills", "subjects"),
                    Levels = DataHelper.CreateSelectListItem(db, model.LevelId, "levels")
                });
            }

            using var t = db.Database.BeginTransaction();
            try
            {
                if (model.Id == 0)
                {
                    db.Entry(model).State = EntityState.Added;
                }

                await db.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                ModelState.AddModelError("Error", "В процессе сохранения произошла ошибка! Ошибка: " + ex.Message);
            }

            return RedirectToAction("VacancyEdit", new { id = model.VacancyId, projectid });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompetence(int id, int vacancyid, int projectid)
        {
            using var db = AppContextFactory.DB;
            try
            {
                var vc = db.VacancyCompetences.FirstOrDefault(i => i.Id == id);
                if (vc != null)
                {
                    db.Remove(vc);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "В процессе удаления компетенции к вакансии произошла ошибка! Ошибка: " + ex.Message);
            }

            return RedirectToAction("VacancyEdit", new { id = vacancyid, projectid });
        }

        #endregion
    }
}