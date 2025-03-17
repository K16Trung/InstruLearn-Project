using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Test_Result
    {
        [Key]
        public int TestResultId { get; set; }
        public int LearnerId { get; set; }
        public int TeacherId { get; set; }
        public int MajorTestId { get; set; }
        public int LearningRegisId { get; set; }
        public string VideoUrl { get; set; }
        public int? Score { get; set; }
        public string LevelAssigned { get; set; }
        public string? Feedback { get; set; }

        // Navigation properties
        public Learner Learner { get; set; }
        public Teacher Teacher { get; set; }
        public MajorTest MajorTest { get; set; }
        public Learning_Registration LearningRegistration { get; set; }
    }
}
