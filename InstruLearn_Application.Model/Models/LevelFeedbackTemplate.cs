using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class LevelFeedbackTemplate
    {
        [Key]
        public int TemplateId { get; set; }
        public int LevelId { get; set; }
        public LevelAssigned Level { get; set; }
        public string TemplateName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<LevelFeedbackCriterion> Criteria { get; set; }
    }
}
