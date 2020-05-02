using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using diploma.Data;
using diploma.Data.Entities;
using diploma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace diploma.Controllers
{
    public class LearningController : Controller
    {
        /// <summary>
        /// Перечень документов с распределением по классам.
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            using var db = AppContextFactory.DB;
            var list = (from d in db.DocFiles
                        join fi in db.FacetItems on d.ClassId equals fi.Id
                        into grp
                        from g in grp.DefaultIfEmpty()
                        orderby d.Id
                        select new LearningViewModel() { Class = g, Doc = d }).ToList();

            return View(list);
        }

        /// <summary>
        /// Установка класса документу.
        /// </summary>
        public IActionResult Edit(int id)
        {
            using var db = AppContextFactory.DB;

            var doc = db.DocFiles.FirstOrDefault(i => i.Id == id) ?? new DocFile();

            if (doc.Id == 0)
            {
                throw new Exception("Не выпадет никогда (хотя может, если постараться =) ), но все же оставлю это здесь!");
            }

            LearningEditViewModel model = new LearningEditViewModel
            {
                DocId = doc.Id,
                ClassId = doc.ClassId,
                Classes = new List<SelectListItem>() { new SelectListItem() { Text = "[ Нет класса ]", Selected = true, Value = "" } }
            };

            model.Classes.AddRange(DataHelper.CreateSelectListItem(db, doc.ClassId, "subjects"));

            return View(model);
        }

        /// <summary>
        /// Сохранение и тут же перерасчет терминов внутри класса. Долгая операция имхо, если документов много будет, которые классу принадлежат.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name)
        {
            using var db = AppContextFactory.DB;

            var model = new LearningEditViewModel();
            await TryUpdateModelAsync<LearningEditViewModel>(model, "", i => i.DocId, i => i.ClassId);

            if (!ModelState.IsValid)
            {
                var classes = new List<SelectListItem>() { new SelectListItem() { Text = "[ Выбрать класс ]", Selected = true, Value = "" } };
                classes.AddRange(DataHelper.CreateSelectListItem(db, model.ClassId, "subjects"));

                return View(new LearningEditViewModel()
                {
                    DocId = model.DocId,
                    ClassId = model.ClassId,
                    Classes = classes
                });
            }

            using var t = db.Database.BeginTransaction();

            try
            {
                var doc = db.DocFiles.First(i => i.Id == id);

                doc.ClassId = model.ClassId ?? (int?)null;

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
        /// Обновляет все документы в категориях и обновляет перечни значимых и незначимых внутри них терминов.
        /// Может занимать долго времени, поэтому можно покурить бамбук и еще успеть смело провернуть пару дел.
        /// Код не является достоянием best practices, так как написан на голой коленке (ноутбук на ней и стоял) =)
        /// </summary>
        public async Task<IActionResult> UpdateMeaningInGroups()
        {
            using var db = AppContextFactory.DB;

            // Забираем все слова, которые есть в документах, которым присвоены классы + берем категории документов.
            var words = (from w in db.Words
                         join d in db.DocFiles on w.DocFileId equals d.Id
                         where d.ClassId.HasValue
                         select new { word = w, classId = d.ClassId }).ToList();

            var categories = words.Select(i => i.classId).Distinct().ToList();

            // Бежим в отрыжку, то есть врипрыжку по категориям (или классам).
            foreach (int cat in categories)
            {
                using var t = db.Database.BeginTransaction();

                try
                {
                    // Считаем количество документов категории. Я не виноват(а), что так криво - он(а) сам(а) пришел(ла) =)
                    var docsCount = (from w in words
                                     where w.classId == cat
                                     select w.word.DocFileId).Distinct().Count();

                    // Группируем слова по документам, а затем по количеству в документах.
                    var groupedWords = (
                           from w in words
                           where w.classId == cat
                           group w by new { w.word.InitialForm, w.word.DocFileId } into grp
                           select new { word = grp.Key, count = grp.Count() }
                       ).ToList();

                    var groupedByDocs = (
                        from w in groupedWords
                        group w by w.word.InitialForm into grp
                        select new { word = grp.Key, filesIn = grp.Count() }
                    ).ToList();

                    // Больше не термины.
                    var notTermsAnyMore = groupedByDocs.Where(i => i.filesIn == docsCount).Select(i => i.word).ToArray();

                    // Сбрасываем значимость у всех слов, переназначаем ее на новые слова.
                    int portion = 0;

                    foreach (var w in words.Where(i => i.classId == cat))
                    {
                        if (w.word.HasMeaningClass)
                        {
                            if (notTermsAnyMore.Contains(w.word.InitialForm) && w.word.HasMeaningClass != false)
                            {
                                w.word.HasMeaningClass = false;
                                portion++;
                            }
                        }
                        else
                        {
                            if (!notTermsAnyMore.Contains(w.word.InitialForm) && w.word.HasMeaningClass != true)
                            {
                                w.word.HasMeaningClass = true;
                                portion++;
                            }
                        }

                        if (portion % 20 == 0)
                        {
                            db.SaveChanges();
                        }
                    }

                    db.SaveChanges();
                    await t.CommitAsync();
                }
                catch (Exception ex)
                {
                    await t.RollbackAsync();
                    ModelState.AddModelError("Error", "Возникла ошибка при обновлении значимости терминов в группах файлов! " + ex.Message);
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Автоматическое определение класса на основе обучающеи выборки.
        /// </summary>
        public IActionResult DefineClass(int id)
        {
            using var db = AppContextFactory.DB;

            // Документ, котороыи и будем определять.
            var doc = db.DocFiles.FirstOrDefault(i => i.Id == id) ?? new DocFile();

            // Небольшая проверочка, что все хорошо и такои документ существует.
            if (doc.Id == 0)
            {
                return RedirectToAction("Index");
            }

            // Сравниваем слова со всеми группами терминов категорий.
            // Нет, не кошечки, а всего лишь категории или классы =)
            var cats = db.DocFiles.Where(i => i.Id != id && i.ClassId.HasValue)
                                  .Select(i => i.ClassId)
                                  .Distinct()
                                  .ToList();

            // Для выстраивания дальнеишего реитинга.
            Dictionary<int, int> rating = new Dictionary<int, int>();

            // Гладим кошечек.
            foreach (int cat in cats) 
            {
                var catWords = from d in db.DocFiles
                               join w in db.Words on d.Id equals w.DocFileId
                               where d.ClassId == cat && w.HasMeaningClass
                               select w;

                // Находим число пересечения слов документа.
                var intersections = (from w in db.Words
                                     join cw in catWords on w.InitialForm equals cw.InitialForm
                                     where w.DocFileId == id
                                     select w).Count();

                rating.Add(cat, intersections);
            }

            // Формируем реитинг.
            var result = rating.OrderByDescending(i => i.Value);

            // Готовим страничку со статискои нашеи авантюры.
            LearningResultViewModel model = new LearningResultViewModel();

            // Список выборов к какому классу относится.
            List<SelectListItem> choice = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "[ Не относить ]", Selected = true },
            };

            var choiceSelectList = (from r in result
                             join fi in db.FacetItems on r.Key equals fi.Id
                             select new SelectListItem()
                             {
                                 Text = $"{fi.Name} – {r.Value} (пересечение терминов)",
                                 Value = fi.Id.ToString()
                             }).ToList();
            
            choice.AddRange(choiceSelectList);

            model.Choice = choiceSelectList;
            model.Result = (from r in result
                            join fi in db.FacetItems on r.Key equals fi.Id
                            select new { fi, r.Value }).ToDictionary(i => i.fi, i => i.Value);

            return View("Result", model);
        }

        [HttpPost]
        public async Task<IActionResult> DefineClass(int id, string name) 
        {
            using var db = AppContextFactory.DB;

            var doc = db.DocFiles.FirstOrDefault(i => i.Id == id) ?? new DocFile();
            if (doc.Id == 0) 
            {
                return RedirectToAction("Index");
            }

            LearningResultViewModel model = new LearningResultViewModel();
            await TryUpdateModelAsync(model, "", i => i.ClassId);

            try
            {
                if (model.ClassId.HasValue) 
                {
                    doc.ClassId = model.ClassId;
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex) 
            {
                ModelState.AddModelError("Error", "Возникла ошибка при обновлении категории документа! " + ex.Message);
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }
    }
}