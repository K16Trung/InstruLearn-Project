using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.TeacherEvaluation
{
    public class TeacherEvaluationQuestionDTO
    {
        public int EvaluationQuestionId { get; set; }
        public string QuestionText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public List<TeacherEvaluationOptionDTO> Options { get; set; }
    }
}
