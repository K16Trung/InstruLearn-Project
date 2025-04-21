using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class LearningRegisFeedbackAnswer
    {
        [Key]
        public int AnswerId { get; set; }
        public int FeedbackId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
        public string Comment { get; set; }

        // Navigation properties
        public LearningRegisFeedback Feedback { get; set; }
        public LearningRegisFeedbackQuestion Question { get; set; }
        public LearningRegisFeedbackOption SelectedOption { get; set; }
    }
}
