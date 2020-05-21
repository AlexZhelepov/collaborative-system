using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using diploma.Data;
using diploma.Data.Entities;
using diploma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace diploma.Controllers
{
    [Authorize(Roles = "file_admin")]
    public class AnalysisController : Controller
    {
        public IActionResult Index() => View();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">docs или users, чтобы не заморачиваться с интерфейсом</param>
        /// <returns></returns>
        public IActionResult AnalysisList(string type)
        {
            using var db = AppContextFactory.DB;
            var docs = db.DocFiles.OrderBy(i => i.Id).ToList();

            // Классы.
            var subjects = (
                from f in db.Facets
                join fi in db.FacetItems on f.Id equals fi.Facet.Id
                where f.Code == "subjects"
                select fi
            ).ToList();

            // Компетенции.
            var skills = (
                from f in db.Facets
                join fi in db.FacetItems on f.Id equals fi.Facet.Id
                where f.Code == "skills"
                select fi
            ).ToList();

            var list = new List<AnalysisItemViewModel>();
            docs.ForEach(i =>
            {
                list.Add(AnalysisHelper.CalcDocStats(db, i, subjects, skills));
            });

            // Сводная по исполнителям (допилено, поэтому криво написано).
            var usersInfo = list.Select(i => new
            {
                fio = i.Document.FIO,
                skills = i.Skills,
                autoDefined = !string.IsNullOrEmpty(i.Document.JsonAutoClassificationResult) ? JsonConvert.DeserializeObject<ClassificationResult>(i.Document.JsonAutoClassificationResult) : null,
                manuallyDefined = i.SubjectsAccessory
            }).ToList();

            // Обрабатываем информацию о пользователях, преобразуя все к виду таблицы.
            var userSummary = new Dictionary<string, UserSummary>();
            var tmpSummary = new Dictionary<string, UserSummaryData>();

            // Подготовка данных.
            foreach (var item in usersInfo) 
            {
                if (!tmpSummary.ContainsKey(item.fio)) 
                {
                    tmpSummary.Add(item.fio, new UserSummaryData());
                }

                tmpSummary[item.fio].Skills.AddRange(item.skills);

                // Автоклассификация.
                if (item.autoDefined != null) 
                {
                    foreach (var adi in item.autoDefined.Values) 
                    {
                        if (!tmpSummary[item.fio].AutoClassification.ContainsKey(adi.Key)) 
                        {
                            tmpSummary[item.fio].AutoClassification.Add(adi.Key, 0);
                        }

                        tmpSummary[item.fio].AutoClassification[adi.Key] += (int)(adi.Value / 100 * item.autoDefined.Total); // немного интересно, но школьные знания о процентацах никто не отменял. Спасибо моему 5ти классному Учителю за это!
                    }
                }

                if (item.manuallyDefined != null)
                {
                    foreach (var md in item.manuallyDefined) 
                    {
                        if (!tmpSummary[item.fio].ManualClassification.ContainsKey(md.Name)) 
                        {
                            tmpSummary[item.fio].ManualClassification.Add(md.Name, 0);
                        }

                        tmpSummary[item.fio].ManualClassification[md.Name] += md.Count;
                    }
                }
            }

            // А теперь доводим все данные до продакшена.
            foreach (var user in tmpSummary) 
            {
                // Добавили пользователя.
                userSummary.Add(user.Key, new UserSummary());

                // Сделали distinct по его все умениям.
                if (user.Value.Skills != null && user.Value.Skills.Any())
                {
                    userSummary[user.Key].Skills = user.Value.Skills.Distinct().ToList();
                }

                // Сгруппировали и посчитали проценты по всем его предметным областям (автоматическая выборка).
                if (user.Value.AutoClassification != null && user.Value.AutoClassification.Any())
                {
                    int totalCount = user.Value.AutoClassification.Sum(i => i.Value);
                    userSummary[user.Key].AutoClassification = user.Value.AutoClassification.ToDictionary(i => i.Key, i => Math.Round((double)i.Value / totalCount * 100, 2));
                }

                // Сгруппировали и посчитали проценты по всем его предметным областям (ручная выборка).
                if (user.Value.ManualClassification != null && user.Value.ManualClassification.Any())
                {
                    int totalCount = user.Value.ManualClassification.Sum(i => i.Value);
                    userSummary[user.Key].ManualClassification = user.Value.ManualClassification.ToDictionary(i => i.Key, i => Math.Round((double)i.Value / totalCount * 100, 2));

                }
            }

            var model = new AnalysisViewModel()
            {
                DocsAnalysis = list,
                UsersAnalysis = userSummary
            };

            // Опять так лучше не делать!
            ViewBag.Type = type;

            return View(model);
        }

        /// <summary>
        /// Значимость терминов, прогоняется по всем загруженным текстам.
        /// </summary>
        public async Task<IActionResult> AnalyzeTerms()
        {
            using var db = AppContextFactory.DB;
            using var t = db.Database.BeginTransaction();

            try
            {
                // Если слово встречается во всех документах, то оно не является значимым.
                // Обновляем пометку у такого слова.

                // Документы.
                var docsCount = db.DocFiles.Count();

                // Группируем слова по документам: слово - количество документов в которых встречается.
                var groupedWords = (
                    from w in db.Words
                    group w by new { w.InitialForm, w.DocFileId } into grp
                    select new { word = grp.Key, count = grp.Count() }
                ).ToList();

                var groupedByDocs = (
                    from w in groupedWords
                    group w by w.word.InitialForm into grp
                    select new { word = grp.Key, filesIn = grp.Count() }
                ).ToList();

                // Теперь готовим слова по мере их "значимости" для текста.
                var notTermsAnyMore = groupedByDocs.Where(i => i.filesIn == docsCount).Select(i => i.word).ToArray();

                // Сбрасываем значимость у всех слов, переназначаем ее на новые слова.
                int portion = 0;

                foreach (var w in db.Words.ToList())
                {
                    if (w.HasMeaning)
                    {
                        if (notTermsAnyMore.Contains(w.InitialForm))
                        {
                            w.HasMeaning = false;
                        }
                    }
                    else
                    {
                        if (!notTermsAnyMore.Contains(w.InitialForm))
                        {
                            w.HasMeaning = true;
                        }
                    }

                    if (portion % 20 == 0)
                    {
                        db.SaveChanges();
                    }

                    portion++;
                }

                db.SaveChanges();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                ModelState.AddModelError("Error", "Возникла ошибка при обновлении значимости терминов в файлах! " + ex.Message);
                return RedirectToAction("Index", "DocFile");
            }

            return RedirectToAction("Index", "DocFile");
        }
    }

    public static class AnalysisHelper
    {
        public static AnalysisItemViewModel CalcDocStats(ApplicationDbContext db, DocFile document, List<FacetItem> subjects, List<FacetItem> skills)
        {
            // Слова текста имеющие какое-то значение.
            var words = db.Words.Where(i => i.FacetItemId.HasValue && i.DocFileId == document.Id).ToList();

            // Данные на выход.
            List<SubjectStats> subjectsAccessory = new List<SubjectStats>();
            List<string> personalSkills = new List<string>();

            var groupedTerms = (from w in words
                                join fi in subjects on w.FacetItemId equals fi.Id
                                group fi by fi.Name into grp
                                select new { name = grp.Key, count = grp.Count() }).ToList();

            var sum = groupedTerms.Sum(i => i.count);

            subjectsAccessory = groupedTerms.Select(i => new SubjectStats()
            {
                Name = i.name,
                Count = i.count,
                TotalSum = sum,
                Percent = Math.Round(((double)i.count / sum) * 100, 2)
            }).ToList();

            personalSkills = (
                from w in words
                join fi in skills on w.FacetItemId equals fi.Id
                select fi.Name
            ).Distinct().ToList();

            return new AnalysisItemViewModel() { SubjectsAccessory = subjectsAccessory, Document = document, Skills = personalSkills };
        }
    }
}