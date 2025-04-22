using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackAnswer
{
    public class LearningRegisFeedbackAnswerDTO
    {
        public int AnswerId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int SelectedOptionId { get; set; }
        public string SelectedOptionText { get; set; }
    }
}
