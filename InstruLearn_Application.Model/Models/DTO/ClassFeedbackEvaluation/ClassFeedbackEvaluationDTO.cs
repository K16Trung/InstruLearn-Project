using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.ClassFeedbackEvaluation
{
    public class ClassFeedbackEvaluationDTO
    {
        public int EvaluationId { get; set; }
        public int CriterionId { get; set; }
        public string GradeCategory { get; set; }
        public decimal Weight { get; set; }
        public decimal? AchievedPercentage { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public decimal WeightedScore { get; set; }
    }
}
