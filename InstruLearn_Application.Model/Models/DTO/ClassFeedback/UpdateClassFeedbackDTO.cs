using InstruLearn_Application.Model.Models.DTO.ClassFeedbackEvaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.ClassFeedback
{
    public class UpdateClassFeedbackDTO
    {
        public string AdditionalComments { get; set; }
        public List<UpdateClassFeedbackEvaluationDTO> Evaluations { get; set; }
    }
}
