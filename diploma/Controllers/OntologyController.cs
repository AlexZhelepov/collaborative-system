using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using diploma.Data;
using diploma.Data.Entities;
using diploma.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace diploma.Controllers
{
    public class OntologyController : Controller
    {
        public IActionResult Index()
        {
            using var db = AppContextFactory.DB;

            // Достаем из базы всю онтологию.
            var list = (from fi in db.FacetItems
                        join f in db.Facets on fi.FacetId equals f.Id
                        where new string[] { "subjects", "skills" }.Contains(f.Code)
                        select new OntologyViewModel() { Class = f, Element = fi }
                        ).ToList();

            return View(list);
        }

        public IActionResult Edit(int? id)
        {
            using var db = AppContextFactory.DB;

            OntologyEditViewModel model = new OntologyEditViewModel() { Facets = new List<SelectListItem>() };

            model.Facets.Add(new SelectListItem() { Selected = true, Text = "[ Выберите класс ]" });
            model.Facets.AddRange(from f in db.Facets
                                  where new string[] { "skills", "subjects" }.Contains(f.Code)
                                  select new SelectListItem()
                                  {
                                      Text = f.Name,
                                      Value = f.Id.ToString()
                                  });


            FacetItem fi = new FacetItem();
            if (id.HasValue) 
            {
                fi = db.FacetItems.FirstOrDefault(i => i.Id == id) ?? new FacetItem();       
            }

            if (fi.Id != 0)
            {
                model.Facets.FirstOrDefault(i => i.Value == fi.Id.ToString()).Selected = true;
                model.FacetId = fi.FacetId;
                model.ElementName = fi.Name;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int? id, string name)
        {
            using var db = AppContextFactory.DB;

            OntologyEditViewModel model = new OntologyEditViewModel();
            await TryUpdateModelAsync<OntologyEditViewModel>(model, "", i => i.FacetId, i => i.ElementName);

            if (db.FacetItems.Any(i => i.FacetId == model.FacetId && i.Name == model.ElementName)) 
            {
                ModelState.AddModelError("Error", "Такой элемент уже есть! За Альянс!");
            }

            if (!ModelState.IsValid) 
            {
                return View(new OntologyEditViewModel() 
                {
                    ElementName = model.ElementName,
                    Facets = (from f in db.Facets
                             select new SelectListItem()
                             {
                                 Text = f.Name,
                                 Value = f.Id.ToString(),
                                 Selected = model.FacetId == f.Id
                             }).ToList()
                });
            }

            using var t = db.Database.BeginTransaction();

            try 
            {
                FacetItem fi = new FacetItem();

                if (id.HasValue) 
                {
                    fi = db.FacetItems.FirstOrDefault(i => i.Id == id.Value) ?? new FacetItem();
                }

                fi.FacetId = model.FacetId;
                fi.Name = model.ElementName;

                if (fi.Id == 0) 
                {
                    db.Entry(fi).State = Microsoft.EntityFrameworkCore.EntityState.Added;
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

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using var db = AppContextFactory.DB;

            // Использования элемента в словах. Если есть, то запрещаем удалять!
            if (db.Words.Any(i => i.FacetItemId == id)) 
            {
                ModelState.AddModelError("Ошибка", "Нельзя удалить элемент! За Орду!");
                return View("Index");
            }

            try
            {
                var fi = db.FacetItems.FirstOrDefault(i => i.Id == id);
                if (fi != null)
                {
                    db.Remove(fi);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "В процессе удаления элемента произошла ошибка! Ошибка: " + ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}