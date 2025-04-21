using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class LearningRegisFeedbackOption
    {
        [Key]
        public int OptionId { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; }
        public int Value { get; set; } // 1-5 for rating scale
        public int DisplayOrder { get; set; }

        // Navigation property
        public LearningRegisFeedbackQuestion Question { get; set; }
    }
}
