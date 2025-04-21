using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class LearningRegisFeedback
    {
        [Key]
        public int FeedbackId { get; set; }
        public int LearningRegistrationId { get; set; }
        public int LearnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AdditionalComments { get; set; }
        public FeedbackStatus Status { get; set; }

        // Navigation properties
        public Learning_Registration LearningRegistration { get; set; }
        public Learner Learner { get; set; }
        public ICollection<LearningRegisFeedbackAnswer> Answers { get; set; }
    }
}
