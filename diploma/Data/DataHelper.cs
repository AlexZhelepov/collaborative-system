using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data
{
    public static class DataHelper
    {
        public static List<SelectListItem> CreateSelectListItem(ApplicationDbContext db, int? selected, params string[] codes) 
        {
            var list = (from f in db.Facets
                        join fi in db.FacetItems on f.Id equals fi.FacetId
                        where codes.Contains(f.Code)
                        orderby fi.FacetId, fi.Value
                        select new SelectListItem() { Text = fi.Name, Value = fi.Id.ToString() }).ToList();

            if (selected.HasValue)
            {
                string val = selected.Value.ToString();
                list.FirstOrDefault(i => i.Value == val).Selected = true;
            }

            return list;
        }
    }
}
