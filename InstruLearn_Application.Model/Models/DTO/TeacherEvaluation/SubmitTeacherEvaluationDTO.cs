using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.TeacherEvaluation
{
    public class SubmitTeacherEvaluationDTO
    {
        public int LearningRegistrationId { get; set; }
        public int LearnerId { get; set; }
        public bool GoalsAchieved { get; set; }
        public List<TeacherEvaluationAnswerSubmitDTO> Answers { get; set; }
    }
}