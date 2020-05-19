using diploma.Data;
using diploma.Data.Entities;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading.Tasks;

namespace diploma.Models
{
    public class GeneticViewModel
    {
        [Required]
        [Display(Name = "ID проекта")]
        public int ProjectId { get; set; }

        [Required]
        [Display(Name = "Число команд")]
        public int TeamCount { get; set; }

        [Required]
        [Display(Name = "Число замен (мутаций)")]
        public int SubstitutionsCount { get; set; }

        [Required]
        [Display(Name = "Число «элитных» команд")]
        public int EliteCount { get; set; }

        [Required]
        [Display(Name = "Число итераций алгоритма")]
        public int IterationsCount { get; set; }
    }

    [Serializable]
    public class GeneticSaveModel
    {
        public int ProjectId { get; set; }
        public int[] Employees { get; set; }
        public int[] Vacancies { get; set; }
    }

    public class GeneticChoiceViewModel
    {
        public int ProjectId { get; set; }
        public List<Team> Teams { get; set; }
    }

    /// <summary>
    /// Проектная команда.
    /// </summary>
    public class Team
    {
        public Team(Project p)
        {
            this.Project = p;
            this.Members = new List<TeamMember>();
        }

        public double Rating { get; set; }
        public Project Project { get; set; }
        public List<TeamMember> Members { get; set; }

        /// <summary>
        /// Инициализирует команду.
        /// </summary>
        public void InitializeTeam()
        {
            using var db = AppContextFactory.DB;

            // Забираем все вакансии проекта.
            var vacancies = db.Vacancies.Where(i => i.Project.Id == Project.Id).ToList();

            // Забираем всех мыслимых и немыслимых сотрудников (все только белыми, никакого черного бумера в карман или конверт).
            // Фильтруем рабов на предмет участия в 5 проектах, больше 5 не берем просто.
            var hasMoreThan5Projects = (
                from v in db.Vacancies
                group v by v.UserInfoId into grp
                select new { userId = grp.Key, count = grp.Count() }
            ).Where(i => i.count > 5)
                .Select(i => i.userId)
                .ToList();

            var employees = db.UserInfos.Where(i => !hasMoreThan5Projects.Contains(i.Id)).ToList();

            // Проверочка 1: рабов то не хватает! Нужно взять в плен еще =)
            if (vacancies.Count > employees.Count)
            {
                throw new Exception("Не хватает рабской силы! Нужно нанять еще!");
            }

            var employeeIds = employees.Select(i => i.Id).ToArray();
            var vacanciesIds = vacancies.Select(i => i.Id).ToArray();
            List<int> randomCandidates = new List<int>();

            Random r = new Random();

            while (randomCandidates.Count < vacancies.Count)
            {
                int rand = r.Next(0, employeeIds.Length); //случайное число от 0 до количества - 1 вакансий.
                int pickedId = employeeIds[rand];

                if (!randomCandidates.Contains(pickedId))
                {
                    randomCandidates.Add(pickedId);
                }
            }

            // Формируем команду на основе полной случайности.
            int n = 0;
            foreach (var id in randomCandidates)
            {
                var employee = employees.First(i => i.Id == id);
                var vacancy = vacancies[n];

                Members.Add(new TeamMember()
                {
                    Employee = employee,
                    Vacancy = vacancy
                });

                n++;
            }
        }

        public void CalcRating()
        {
            Rating = 0.0f;
            foreach (var tm in Members)
            {
                tm.CalcRating();
                Rating += tm.Rating;
            }

            Rating = Math.Round(Rating, 3);
        }

        /// <summary>
        /// Смена тех членов команды, которые в первую очередь не соответствуют занятым позициям.
        /// Работает как: ордерим всех членов команды по рейтингу, забираем последних n (зависит от настроек алгоритма) 
        /// и предлагаем их поменять, так как толку от них мало согласно нашему рейтингу.
        /// </summary>
        private List<int> DefineWhomToSubstitute(int substitutionsCount)
        {
            var orderedByRating = Members.OrderBy(i => i.Rating)
                                         .Take(substitutionsCount)
                                         .Select(i => i.Employee.Id)
                                         .ToList();

            return orderedByRating.ToList();
        }

        /// <summary>
        /// Находит "незанятых" для команды сотрудников.
        /// </summary>
        private List<int> FindAvailableEmployees(List<int> occupied)
        {
            using var db = AppContextFactory.DB;

            var availableIds = db.UserInfos.Select(i => i.Id).ToList().Except(occupied).ToList();

            return availableIds;
        }

        /// <summary>
        /// Реализация алгоритма мутации – другими словами замены.
        /// </summary>
        public void MakeSubstitutions(int substitutionCount)
        {
            using var db = AppContextFactory.DB;
            var users = db.UserInfos.ToList();

            // Определяем кого менять.
            var substitutes = DefineWhomToSubstitute(substitutionCount);

            // Занятые на проекте, кого не меняем (тех кого меняем, можем переставить и на другие позиции).
            var occupied = Members.Select(i => i.Employee.Id).Except(substitutes).ToList();

            // Ищем доступных сотрудников для замены.
            var available = FindAvailableEmployees(occupied);

            // Производим замену.
            foreach (var id in substitutes)
            {
                // Ищем замену чуваку. Подбираем случайным образом его.
                int? toChange = null;
                Random rnd = new Random();
                while (!toChange.HasValue)
                {
                    int pos = rnd.Next(0, available.Count);
                    int aid = available[pos];

                    // А может быть корова? Ну или может быть он состоит уже на данной вакансии?
                    if (id == aid)
                    {
                        continue;
                    }

                    // Ну вроде бы все круги ада проверок пройдены, пора менять гребца.
                    var newEmployee = users.First(i => i.Id == aid);
                    Members.First(i => i.Employee.Id == id).Employee = newEmployee;

                    // Удаляем кандидата из списка доступных, так как он только что уже взялся за весло.
                    available.RemoveAt(pos);

                    // Чтобы катапультироваться из цикла.
                    toChange = aid;
                }
            }
        }
    }

    /// <summary>
    /// Член команды проекта.
    /// </summary>
    public class TeamMember
    {
        public UserInfo Employee { get; set; }
        public Vacancy Vacancy { get; set; }
        public double Rating { get; set; }

        /// <summary>
        /// Получение оценки в зависимости от опыта работы.
        /// Она незначительная, чтобы другие оценки не замещать собой.
        /// </summary>
        private double GetExperienceEstimation()
        {
            double years = (DateTime.Now - Employee.CareerStart).Days / 365;

            if (years < 1)
            {
                return 0.05f;
            }
            else if (years >= 1 && years < 3)
            {
                return 0.10f;
            }
            else if (years >= 3 && years < 5)
            {
                return 0.15f;
            }
            else if (years >= 5)
            {
                return 0.20f;
            }

            return 0.0f;
        }

        /// <summary>
        /// Рассчитывает рейтинг совместимости исполнителя и вакансии.
        /// </summary>
        public void CalcRating()
        {
            // Сбросили рейтинг.
            Rating = 0.0f;

            using var db = AppContextFactory.DB;

            var userCompetences = (from uc in db.UserCompetences
                                   where uc.UserInfoId == Employee.Id
                                   join lvl in db.FacetItems on uc.LevelId equals lvl.Id
                                   select new { level = lvl, competence = uc.CompetenceId }).ToList();

            var vacancyCompetences = (from vc in db.VacancyCompetences
                                      where vc.VacancyId == Vacancy.Id
                                      join lvl in db.FacetItems on vc.LevelId equals lvl.Id
                                      select new { level = lvl, competence = vc.CompetenceId }).ToList();

            // И тут мы начинаем пляски с бубном (или бубнами)
            // , смотря кому как нравится и кому как главное удобно, 
            // а у кого-то просто нет выбора в таком случае.
            // Поехали!

            // ==================================================================================
            // ПУНКТ 1. Соответствие навыков. За каждый соответствующий навык будем давать оценку.
            // + 0,8 за соответсвующий навык.
            // бонусы: 
            // + 0,2 за требуемый или выше уровень квалификации.
            // - 0,05 за несоответствие уровня.
            // - 0.5 штраф за несоостветствие компетенции (уровень здесь не важен).
            // ==================================================================================

            double bonusSkillAccordance = 0.8f, bonusSkillLevel = 0.2f, drawbackSkillLevel = -0.05f, drawbackVariance = -0.5f;

            // Ищем совпадения.
            var merge = (from uc in userCompetences
                         join vc in vacancyCompetences on uc.competence equals vc.competence
                         select new { uc, vc }).ToList();

            // Есть какие-нибудь совпадения? Тогда считаем!
            if (merge.Any())
            {
                foreach (var accordance in merge)
                {
                    Rating += bonusSkillAccordance; // за соответствие.
                    if (accordance.uc.level.Value >= accordance.vc.level.Value)
                    {
                        Rating += bonusSkillLevel; // за соответствие уровня владения навыком.
                    }
                    else
                    {
                        Rating += drawbackSkillLevel; //+, потому что отрицательное число в рейтинге.
                    }
                }
            }

            // Считаем штраф за несовпавшие навыки.
            Rating += (vacancyCompetences.Count - merge.Count) * drawbackVariance;

            // ==================================================================================
            // ПУНКТ 2. Бонус человека в плане опыта работы. В функции выше расписано. 
            // ==================================================================================
            Rating += GetExperienceEstimation();

            // ==================================================================================
            // ПУНКТ 3. Однако! Если человек "перерос" вакансию это тоже плохо.
            // Средний уровень навыков человека несильно выше навыков вакансии. Бонус +0,5 (как литров огненной воды).
            // Средний уровень навыков человека сильно выше (али ниже) навыков вакансии. Бонус -1 (а тут в долгах немного остаемся). 
            // Сами-знаете-кто не будет в колл-центре работать, имхо считается умной дивой, поэтому такой сильный штраф за несоответствие навыков =)
            // ==================================================================================
            double bonusSkillLevelAccordance = 0.5f, drawbackSkillLevelInvariance = -1.0f;

            double vacancyLevel = vacancyCompetences.Sum(i => i.level.Value.Value);
            double userLevel = userCompetences.Sum(i => i.level.Value.Value);

            double difference = Math.Abs(userLevel - vacancyLevel);

            // Нехитрая логика.
            if (difference <= 2) // думаем что разница в 2 относительно невысока.
            {
                Rating += bonusSkillLevelAccordance;
            }
            else
            {
                Rating += drawbackSkillLevelInvariance;
            }

            // ==================================================================================
            // ПУНКТ 4. Отпуск.
            // Человек хочет уйти в отпуск во время важного проекта, давайте накажем его, лишив зп!
            // ==================================================================================
            double beforeProject = -0.2f, withinProject = -0.5f, beforeEndOfProject = -0.8f;

            var project = db.Projects.First(i => i.Id == Vacancy.ProjectId);

            // Если проект не имеет срока окончания, то пусть идет с миром в отпуск.
            // А иначе нагнем подлеца!
            if (project.DateEnd.HasValue) 
            {
                var vacationStart = Employee.VacationStart;
                var vacationEnd = Employee.VacationStart.AddDays(14); // в качестве отпуска берем 2 недельки до 2.

                // Если собирается в отпуск до начала проекта, но выходит, когда проект в самом разгаре.
                if (project.DateStart > vacationStart && project.DateStart < vacationEnd) 
                {
                    Rating += beforeProject;
                }

                // Если собирается в отпуск, когда идет проект, выходит когда тоже идет проект.
                if (project.DateStart < vacationStart && project.DateEnd > vacationEnd) 
                {
                    Rating += withinProject;
                }

                // Если уходит в отпуск, когда идет проект, но возвращается уже после проекта (прогулял защиту).
                if (project.DateStart < vacationStart && project.DateEnd < vacationEnd && project.DateEnd > vacationStart) 
                {
                    Rating += beforeEndOfProject;
                }
            }
            
            // Округляем, чтобы красиво смотрелось (до 1000ых).
            Rating = Math.Round(Rating, 3);
        }
    }
}
