using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstruLearn_Application.Model.Models
{
    public class TeacherEvaluationAnswer
    {
        [Key]
        public int EvaluationAnswerId { get; set; }
        public int EvaluationFeedbackId { get; set; }
        public TeacherEvaluationFeedback Feedback { get; set; }
        public int EvaluationQuestionId { get; set; }
        public TeacherEvaluationQuestion Question { get; set; }
        public int SelectedOptionId { get; set; }
        public TeacherEvaluationOption SelectedOption { get; set; }
    }
}