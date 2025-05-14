using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.ClassFeedbackEvaluation
{
    public class UpdateClassFeedbackEvaluationDTO
    {
        public int EvaluationId { get; set; }
        public int CriterionId { get; set; }
        public decimal AchievedPercentage { get; set; }
        public string Comment { get; set; }
    }
}
