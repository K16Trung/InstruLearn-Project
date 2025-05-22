using InstruLearn_Application.Model.Models.DTO.ClassFeedbackEvaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.ClassFeedback
{
    public class ClassFeedbackDTO
    {
        public int FeedbackId { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int LearnerId { get; set; }
        public string LearnerName { get; set; }
        public int TemplateId { get; set; }
        public string TemplateName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string AdditionalComments { get; set; }
        public List<ClassFeedbackEvaluationDTO> Evaluations { get; set; }
        public decimal AverageScore { get; set; }
        public decimal TotalWeight { get; set; }
    }
}
