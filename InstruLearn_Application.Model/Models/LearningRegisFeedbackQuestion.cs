using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class LearningRegisFeedbackQuestion
    {
        [Key]
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string Category { get; set; } // Teaching, Communication, Content, etc.
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }

        // Navigation property
        public ICollection<LearningRegisFeedbackOption> Options { get; set; }
    }
}
