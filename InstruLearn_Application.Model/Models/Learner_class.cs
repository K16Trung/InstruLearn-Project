using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Learner_class
    {
        [Key]
        public int LearnerClassId { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public int ClassId { get; set; }
        public Class Classes { get; set; }
    }
}
