using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class ClassFeedback
    {
        [Key]
        public int FeedbackId { get; set; }
        public int ClassId { get; set; }
        public Class Class { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public int TemplateId { get; set; }
        public LevelFeedbackTemplate Template { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }
        public string? AdditionalComments { get; set; }
        public ICollection<ClassFeedbackEvaluation> Evaluations { get; set; }
    }
}
