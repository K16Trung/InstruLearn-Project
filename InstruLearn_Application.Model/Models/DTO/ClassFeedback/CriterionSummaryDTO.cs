using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.ClassFeedback
{
    public class CriterionSummaryDTO
    {
        public int CriterionId { get; set; }
        public string GradeCategory { get; set; }
        public decimal Weight { get; set; }
        public decimal AverageScore { get; set; }
    }
}
