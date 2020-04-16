using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Data.Entities
{
    public class DocFile
    {
        public int Id { get; set; }

        /// <summary>
        /// Название файла.
        /// </summary>
        [Required]
        public string FileName { get; set; }

        /// <summary>
        /// Хеш файла.
        /// </summary>
        [Required]
        public string FileHash { get; set; }

        /// <summary>
        /// ФИО исполнителя. Просто строчкой, но можно использовать в выгрузках далее.
        /// </summary>
        public string FIO { get; set; }

        /// <summary>
        /// Компетенции через разделитель. Пусть будет "|".
        /// </summary>
        public string Skills { get; set; }

        /// <summary>
        /// Место работы.
        /// </summary>
        public string WorkPlace { get; set; }

        /// <summary>
        /// Отношение к предметной области, к классу.
        /// </summary>
        [ForeignKey("FacetItem")]
        public int? ClassId { get; set; }


        [ForeignKey("UserInfo")]
        public int? UserInfoId { get; set; }


        [ForeignKey("ApplicationUser")]
        public string ApplicationUserId { get; set; }

        /// <summary>
        /// Пользователь, который загрузил файл.
        /// </summary>
        [Required]
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<Word> Words { get; set; }
    }
}
