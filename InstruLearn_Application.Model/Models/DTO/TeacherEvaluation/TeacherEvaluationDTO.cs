using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.TeacherEvaluation
{
    public class TeacherEvaluationDTO
    {
        public int EvaluationFeedbackId { get; set; }
        public int LearningRegistrationId { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int LearnerId { get; set; }
        public string LearnerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TeacherEvaluationStatus Status { get; set; }
        public bool GoalsAchieved { get; set; }
        public int CompletedSessions { get; set; }
        public int TotalSessions { get; set; }
        public List<TeacherEvaluationAnswerDTO> Answers { get; set; }
    }
}
