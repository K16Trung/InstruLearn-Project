using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackOption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackQuestion
{
    public class LearningRegisFeedbackQuestionDTO
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string Category { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public List<LearningRegisFeedbackOptionDTO> Options { get; set; }
    }
}
