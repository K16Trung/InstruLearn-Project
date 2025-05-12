using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.TeacherEvaluation
{
    public class TeacherEvaluationOptionDTO
    {
        public int EvaluationOptionId { get; set; }
        public int EvaluationQuestionId { get; set; }
        public string OptionText { get; set; }
    }
}
