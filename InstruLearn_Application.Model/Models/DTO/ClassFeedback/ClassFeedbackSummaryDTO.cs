using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.ClassFeedback
{
    public class ClassFeedbackSummaryDTO
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string MajorName { get; set; }
        public string LevelName { get; set; }
        public int TotalFeedbacks { get; set; }
        public decimal OverallAverageScore { get; set; }
        public List<CriterionSummaryDTO> CriterionSummaries { get; set; }
    }
}
