using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstruLearn_Application.Model.Models
{
    public class TeacherEvaluationOption
    {
        [Key]
        public int EvaluationOptionId { get; set; }
        public int EvaluationQuestionId { get; set; }
        public string OptionText { get; set; }
        public int RatingValue { get; set; }
        public TeacherEvaluationQuestion Question { get; set; }
    }
}