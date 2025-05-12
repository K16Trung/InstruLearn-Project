using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.TeacherEvaluation
{
    public class TeacherEvaluationAnswerSubmitDTO
    {
        public int EvaluationQuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }
}
