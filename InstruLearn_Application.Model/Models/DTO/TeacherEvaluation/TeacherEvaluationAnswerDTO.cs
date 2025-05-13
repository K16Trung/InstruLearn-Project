using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.TeacherEvaluation
{
    public class TeacherEvaluationAnswerDTO
    {
        public int EvaluationAnswerId { get; set; }
        public int EvaluationFeedbackId { get; set; }
        public int EvaluationQuestionId { get; set; }
        public int SelectedOptionId { get; set; }
        public string QuestionText { get; set; }
        public string SelectedOptionText { get; set; }
        public int RatingValue { get; set; }
    }
}
