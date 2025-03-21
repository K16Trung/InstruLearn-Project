using InstruLearn_Application.Model.Enum;
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
        public int? MajorId { get; set; }
        public int? LearningRegisId { get; set; }
        public TestResultType ResultType { get; set; }
        public TestResultStatus Status { get; set; }
        

        // Navigation properties
        public Learner Learner { get; set; }
        public Teacher Teacher { get; set; }
        public Major? Major { get; set; }
        public Learning_Registration? LearningRegistration { get; set; }
    }
}
