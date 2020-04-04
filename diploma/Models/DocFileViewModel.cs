using diploma.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Models
{
    public class DocFileDetailsViewModel
    {
        public DocFile Document { get; set; }
        public List<DocFileDetailsItem> Words { get; set; }
    }

    public class DocFileDetailsItem
    {
        public Word Word { get; set; }
        public List<SelectListItem> Types { get; set; }
        public string Type { get; set; }
    }

    public class DocFileLoadViewModel 
    {
        public IFormFile Document { get; set; }
    }
}
