using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using diploma.Data;
using diploma.Data.Entities;
using diploma.Models;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Controllers
{
    public class GeneticController : Controller
    {
        /// <summary>
        /// Формирует вьюшку подбора команды на проект. 
        /// Задаются основные параметры алгоритма.
        /// </summary>
        public IActionResult Index(int projectid)
        {
            // Задаем некоторые значения по умолчанию.
            GeneticViewModel model = new GeneticViewModel()
            {
                ProjectId = projectid,
                EliteCount = 3,
                IterationsCount = 30,
                SubstitutionsCount = 2,
                TeamCount = 30
            };

            // Делаем несколько проверок, чтобы предупредит пользователя о возможных ошибках.
            // Ушел играть в Rome Total War 2 - потом допишу как-нить =)
            /* using var db = AppContextFactory.DB;
            var project = db.Projects.First(i => i.Id == projectid);*/ 

            return View(model);
        }

        /// <summary>
        ///  Берет всех пользователей и портит их с точки зрения рандома их скиллов.
        ///  Каждому добавляется по 5-7 навыков разного уровня.
        ///  Каждому добавляется по 3-5 предметные области разного уровня.
        /// </summary>
        public async Task<IActionResult> RandomizeSkills() 
        {
            using var db = AppContextFactory.DB;
            using var t = db.Database.BeginTransaction();

            try
            {
                var employees = db.UserInfos.ToList();
                var facetItems = (from fi in db.FacetItems
                                  join f in db.Facets on fi.FacetId equals f.Id
                                  select new { fi.Id, f.Code }).ToList();

                int[] skills = facetItems.Where(i => i.Code == "skills").Select(i => i.Id).ToArray();
                int[] subjects = facetItems.Where(i => i.Code == "subjects").Select(i => i.Id).ToArray();
                int[] lvls = facetItems.Where(i => i.Code == "levels").Select(i => i.Id).ToArray();

                Random rnd = new Random();

                foreach (var e in employees)
                {
                    // Удаляем старые навыки.
                    var oldUcs = db.UserCompetences.Where(i => i.UserInfoId == e.Id).ToList();
                    db.RemoveRange(oldUcs);
                    await db.SaveChangesAsync();

                    // Добавляем новые навыки.
                    int count = rnd.Next(5, 7); // Использую антипаттерн magic numbers, потому что хочу =)
                    int n = 0;
                    List<int> stopList = new List<int>();

                    // Добавляем, пока не закончится цикл.
                    while (n < count)
                    {
                        int l = rnd.Next(0, lvls.Length);
                        int levelId = lvls[l];

                        int s = rnd.Next(0, skills.Length);
                        int skillId = skills[s];

                        if (stopList.Contains(skillId))
                        {
                            continue;
                        }

                        var us = new UserCompetence()
                        {
                            LevelId = levelId,
                            CompetenceId = skillId,
                            UserInfoId = e.Id
                        };

                        stopList.Add(skillId);

                        db.Entry(us).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        await db.SaveChangesAsync();

                        n++;
                    }

                    // Добавляем новые предметные области.
                    count = rnd.Next(3, 5); // Использую антипаттерн magic numbers, потому что хочу =)
                    n = 0;
                    stopList.Clear();

                    while (n < count)
                    {
                        int l = rnd.Next(0, lvls.Length);
                        int levelId = lvls[l];

                        int s = rnd.Next(0, subjects.Length);
                        int subjectId = subjects[s];

                        if (stopList.Contains(subjectId))
                        {
                            continue;
                        }

                        var us = new UserCompetence()
                        {
                            LevelId = levelId,
                            CompetenceId = subjectId,
                            UserInfoId = e.Id
                        };

                        stopList.Add(subjectId);

                        db.Entry(us).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        await db.SaveChangesAsync();

                        n++;
                    }
                }

                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                ModelState.AddModelError("Error", "Нельзя сотворить здесь!");
            }

            return RedirectToAction("Index", "User");
        }

        /// <summary>
        ///  Подбор команды проекта.
        /// </summary>
        public async Task<IActionResult> FindTeam(int projectId) 
        {
            GeneticViewModel model = new GeneticViewModel();
            await TryUpdateModelAsync(model, "", i => i.TeamCount, i => i.SubstitutionsCount, i => i.EliteCount, i => i.IterationsCount, i => i.ProjectId);

            if (!ModelState.IsValid) 
            {
                return View(model);
            }

            // Начинаем представленье, начинаем песни петь — разрешите для начала алгоритм мне запустить =)
            int epoch = 0;
            List<Team> teams = new List<Team>();

            while (epoch <= model.IterationsCount) 
            {
                // Производим инициализацию команд в первой эпохе.
                if (epoch == 0) 
                {
                    teams = InitializeTeams(model.ProjectId, model.TeamCount);
                }

                // Рассчитываем рейтинги команд.
                teams.ForEach(i => { i.CalcRating(); });

                // Собираем рейтинг. Привет Леше Р.!
                var orderedTeams = teams.OrderByDescending(i => i.Rating).ToList();

                // На последней итерации перестановок не делаем - толку нет, только время компьютерное убиваем нещадно, лучше свое время убедить, его не жалко.
                if (epoch == model.IterationsCount)
                {
                    teams = orderedTeams;
                    break;
                }

                // Оставляем "элитные" команды без изменений. В остальных меняем гребцов - они устали!
                var nextTeams = orderedTeams.Take(model.EliteCount).ToList();

                // Остальные команды, в которых предусмотрены перестановки.
                var restTeams = orderedTeams.Skip(model.EliteCount);

                // Делаем перестановки.
                foreach (var t in restTeams)
                {
                    t.MakeSubstitutions(model.SubstitutionsCount);
                    nextTeams.Add(t);
                }

                // Переходим в следующую эпоху. Пищи и золота вроде хватает.
                teams = nextTeams;
                epoch++;
            }

            // А тут формируем данные для странички с определением команд. Строим нехитрый рейтинг (не) Теглайна.
            GeneticChoiceViewModel viewModel = new GeneticChoiceViewModel
            {
                ProjectId = projectId,
                Teams = teams
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<JsonResult> FindTeam([FromBody] GeneticSaveModel data) 
        {
            using var db = AppContextFactory.DB;
            using var t = db.Database.BeginTransaction();

            try
            {
                int n = 0;
                foreach (var v in data.Vacancies) 
                {
                    var vacancy = db.Vacancies.First(i => i.Id == v);
                    vacancy.UserInfoId = data.Employees[n];
                    n++;
                }

                await db.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex) 
            {
                await t.RollbackAsync();
                ModelState.AddModelError("Error", "Произошла ошибка при сохранении команды!");
            }

            // Возвращаемся к проекту.
            return new JsonResult(new { error = false, message = "Исследование завершено!" });
        }

        #region Некоторые методы для реализация самого алгоритма.

        private List<Team> InitializeTeams(int projectId, int teamCount)
        {
            using var db = AppContextFactory.DB;

            var project = db.Projects.First(i => i.Id == projectId);
            var teams = new List<Team>();

            int n = 0;
            while (n < teamCount) 
            {
                Team t = new Team(project);
                t.InitializeTeam();
                teams.Add(t);
                n++;
            }

            return teams;
        }
        
        #endregion
    }
}