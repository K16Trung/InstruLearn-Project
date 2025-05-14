using InstruLearn_Application.Model.Models.DTO.ClassFeedbackEvaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.ClassFeedback
{
    public class CreateClassFeedbackDTO
    {
        public int ClassId { get; set; }
        public int LearnerId { get; set; }
        public string AdditionalComments { get; set; }
        public List<CreateClassFeedbackEvaluationDTO> Evaluations { get; set; }
    }
}
