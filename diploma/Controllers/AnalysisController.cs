using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using diploma.Data;
using diploma.Data.Entities;
using diploma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace diploma.Controllers
{
    [Authorize(Roles = "file_admin")]
    public class AnalysisController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult AnalysisList()
        {
            using var db = AppContextFactory.DB;
            var docs = db.DocFiles.ToList();

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

            var list = new List<AnalysisViewModel>();
            docs.ForEach(i => { list.Add(AnalysisHelper.CalcDocStats(db, i, subjects, skills)); });

            return View(list);
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
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
    }

    public static class AnalysisHelper
    {
        public static AnalysisViewModel CalcDocStats(ApplicationDbContext db, DocFile document, List<FacetItem> subjects, List<FacetItem> skills)
        {
            var words = db.Words.Where(i => i.DocFileId == document.Id);

            // Данные на выход.
            Dictionary<string, double> subjectsAccessory = new Dictionary<string, double>();
            List<string> personalSkills = new List<string>();

            // TODO.
            var groupedTerms = (from w in db.Words
                                where w.FacetItemId.HasValue && w.DocFileId == document.Id
                                join fi in subjects on w.FacetItemId.Value equals fi.Id
                                group fi by fi.Name into grp
                                select new { name = grp.Key, count = grp.Count() }).ToList();

            //var items = Context.Assets.AsEnumerable().GroupBy(p => p.CategoryName).Select(p => new AssetCategorySummary
            //{
            //    CategoryId = p.Select(r => r.CategoryId).FirstOrDefault(),
            //    CategoryName = p.Select(r => r.CategoryName).FirstOrDefault(),
            //    TotalAsset = p.Count()
            //}).ToList();

            subjectsAccessory = groupedTerms.ToDictionary(i => i.name, i => (double)i.count / groupedTerms.Count);
            personalSkills = (
                from w in words
                join fi in skills on w.FacetItemId equals fi.Id
                select fi.Name
            ).Distinct().ToList();

            return new AnalysisViewModel() { SubjectsAccessory = subjectsAccessory, Document = document, Skills = personalSkills };
        }
    }
}