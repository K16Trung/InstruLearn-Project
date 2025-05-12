// InstruLearn_Application.Model/Models/TeacherEvaluationFeedback.cs
using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstruLearn_Application.Model.Models
{
    public class TeacherEvaluationFeedback
    {
        [Key]
        public int EvaluationFeedbackId { get; set; }
        public int LearningRegistrationId { get; set; }
        public Learning_Registration LearningRegistration { get; set; }
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TeacherEvaluationStatus Status { get; set; }
        public string? GoalsAssessment { get; set; }
        public int ProgressRating { get; set; }
        public bool GoalsAchieved { get; set; }

        // Navigation property for the answers
        public ICollection<TeacherEvaluationAnswer> Answers { get; set; }
    }
}