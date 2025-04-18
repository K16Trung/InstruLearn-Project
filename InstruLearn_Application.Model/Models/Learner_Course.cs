using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Learner_Course
    {
        [Key]
        public int LearnerCourseId { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public int CoursePackageId { get; set; }
        public Course_Package CoursePackage { get; set; }
        public double CompletionPercentage { get; set; } = 0;
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
        public DateTime? LastAccessDate { get; set; }
    }
}
