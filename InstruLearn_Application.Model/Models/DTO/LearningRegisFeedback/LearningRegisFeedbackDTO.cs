using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackAnswer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegisFeedback
{
    public class LearningRegisFeedbackDTO
    {
        public int FeedbackId { get; set; }
        public int LearningRegistrationId { get; set; }
        public string LearnerId { get; set; }
        public string LearnerName { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AdditionalComments { get; set; }
        public FeedbackStatus Status { get; set; }
        public double AverageRating { get; set; }
        public List<LearningRegisFeedbackAnswerDTO> Answers { get; set; }
    }
}
