using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    public class Word
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Версия слова в тексте.
        /// </summary>
        [Required]
        public string TextVersion { get; set; }
        
        /// <summary>
        /// Данные, которые получены из Mystem.
        /// </summary>
        [Required]
        public string MystemData { get; set; }

        /// <summary>
        /// Означает является ли слово значимым, не значимыми являются предлоги и т.д.
        /// </summary>
        public bool HasMeaning { get; set; }

        /// <summary>
        /// Порядок слова в тексте.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Частота встречаемости слова в тексте.
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// Начальная форма слова из анализа MyStem.
        /// </summary>
        public string InitialForm { get; set; }

        [ForeignKey("DocFile")]
        public int DocFileId { get; set; }

        // Ссылка на другие сущности.
        [Required]
        public DocFile DocFile { get; set; }

        [ForeignKey("FacetItem")]
        public int? FacetItemId { get; set; }
        public virtual FacetItem FacetItem { get; set; }
    }
}
