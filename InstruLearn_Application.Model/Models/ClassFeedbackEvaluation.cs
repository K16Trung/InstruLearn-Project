using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class ClassFeedbackEvaluation
    {
        [Key]
        public int EvaluationId { get; set; }
        public int FeedbackId { get; set; }
        public ClassFeedback Feedback { get; set; }
        public int CriterionId { get; set; }
        public LevelFeedbackCriterion Criterion { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal Value { get; set; }
        public string Comment { get; set; }
    }
}
