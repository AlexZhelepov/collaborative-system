using diploma.Data.Entities;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace diploma.Models
{
    public class AnalysisViewModel
    {
        public List<AnalysisItemViewModel> DocsAnalysis { get; set; }
        public Dictionary<string, UserSummary> UsersAnalysis { get; set; }
    }

    public class AnalysisItemViewModel
    {
        public DocFile Document { get; set; }
        public List<SubjectStats> SubjectsAccessory { get; set; }
        public List<string> Skills { get; set; }
    }

    public class SubjectStats
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int TotalSum { get; set; } // везде повторятся будет, ну и ладно - быстрый костыль!
        public double Percent { get; set; }
    }

    public class UserSummaryData
    {
        public UserSummaryData() 
        {
            Skills = new List<string>();
            ManualClassification = new Dictionary<string, int>();
            AutoClassification = new Dictionary<string, int>();
        }
        public Dictionary<string, int> ManualClassification { get; set; }
        public Dictionary<string, int> AutoClassification { get; set; }
        public List<string> Skills { get; set; }
    }

    public class UserSummary 
    {
        public UserSummary() 
        {
            Skills = new List<string>();
            ManualClassification = new Dictionary<string, double>();
            AutoClassification = new Dictionary<string, double>();
        }

        public List<string> Skills { get; set; }
        public Dictionary<string, double> ManualClassification { get; set; }
        public Dictionary<string, double> AutoClassification { get; set; }
    }
}
