using diploma.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Models
{
    public class AnalysisViewModel
    {
        public DocFile Document { get; set; }
        public Dictionary<string, double> SubjectsAccessory { get; set; }
        public List<string> Skills { get; set; }
    }
}
