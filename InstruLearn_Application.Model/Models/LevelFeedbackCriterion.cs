using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class LevelFeedbackCriterion
    {
        [Key]
        public int CriterionId { get; set; }
        public int TemplateId { get; set; }
        public LevelFeedbackTemplate Template { get; set; }
        public string GradeCategory { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Weight { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public ICollection<ClassFeedbackEvaluation> Evaluations { get; set; }
    }
}
