using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackAnswer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegisFeedback
{
    public class UpdateLearningRegisFeedbackDTO
    {
        public string AdditionalComments { get; set; }
        public List<CreateLearningRegisFeedbackAnswerDTO> Answers { get; set; }
    }
}
