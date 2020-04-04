using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using diploma.Data;
using diploma.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace diploma.Controllers
{
    [Authorize(Roles = "file_admin")]
    public class AnalysisController : Controller
    {
        public IActionResult Index()
        {
            using var db = AppContextFactory.DB;
            var docs = db.DocFiles.ToList();

            // Подготовка данных и рассчет метрик для каждого документа.

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
                
            // Читаем процентное содержание классов.
            var words = db.Words.Where(i => i.)

            // Выделяем компетенции.

            return View();
        }

        private void CalcDocStats(ApplicationDbContext db, DocFile document, List<FacetItem> subjects, List<FacetItem> skills) 
        {
            var words = db.Words.Where(i => i.DocFileId == document.Id);

            Dictionary<string, int> accessory = new Dictionary<string, int>();

            
        }

        /// <summary>
        /// Значимость терминов, прогоняется по всем текстам.
        /// </summary>
        public async Task<IActionResult> AnalyzeTerms()
        {
            using var db = AppContextFactory.DB;

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

            using var t = db.Database.BeginTransaction();

            try
            {
                foreach (var w in db.Words)
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
                        await db.SaveChangesAsync();
                    }

                    portion++;
                }

                await db.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                ModelState.AddModelError("Error", "Возникла ошибка при обновлении значимости терминов в файлах");
                return View();
            }

            return View();
        }
    }
}