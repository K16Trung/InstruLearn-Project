using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackAnswer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegisFeedback
{
    public class CreateLearningRegisFeedbackDTO
    {
        public int LearningRegistrationId { get; set; }
        public int LearnerId { get; set; }
        public string AdditionalComments { get; set; }
        public List<CreateLearningRegisFeedbackAnswerDTO> Answers { get; set; }
        public bool ContinueStudying { get; set; }
        public bool ChangeTeacher { get; set; }
    }
}
