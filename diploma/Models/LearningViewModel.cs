using diploma.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace diploma.Models
{
    /// <summary>
    /// Для списка.
    /// </summary>
    public class LearningViewModel
    {
        public DocFile Doc { get; set; }
        public FacetItem Class { get; set; }
    }

    /// <summary>
    /// Для редактирования и обновления терминов.
    /// </summary>
    public class LearningEditViewModel
    {
        [Display(Name = "Документ")]
        public int DocId { get; set; }

        [Display(Name = "Класс")]
        public int? ClassId { get; set; }

        public List<SelectListItem> Classes { get; set; }
    }

    public class LearningResultViewModel
    {
        [Display(Name = "Подобранные категории")]
        public int? ClassId { get; set; }
        public Dictionary<FacetItem, int> Result { get; set; }
        public List<SelectListItem> Choice { get; set; }
        public string JsonClassification { get; set; }
    }

    /// <summary>
    /// Результат классификации.
    /// </summary>
    [Serializable]
    public class ClassificationResult 
    {
        public int Total { get; set; }
        public Dictionary<string, double> Values { get; set; }
    }
}
