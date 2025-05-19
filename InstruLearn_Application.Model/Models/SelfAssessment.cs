using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class SelfAssessment
    {
        [Key]
        public int SelfAssessmentId { get; set; }
        public string Description { get; set; }
        public ICollection<Learning_Registration> LearningRegistrations { get; set; }
    }
}
