using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstruLearn_Application.Model.Models
{
    public class TeacherEvaluationQuestion
    {
        [Key]
        public int EvaluationQuestionId { get; set; }
        public string QuestionText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }

        // Navigation property
        public ICollection<TeacherEvaluationOption> Options { get; set; }
    }
}