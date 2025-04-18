using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearnerCourse
{
    public class LearnerCourseDTO
    {
        public int LearnerCourseId { get; set; }
        public int LearnerId { get; set; }
        public string LearnerName { get; set; }
        public int CoursePackageId { get; set; }
        public string CourseName { get; set; }
        public double CompletionPercentage { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime? LastAccessDate { get; set; }
        public int TotalContentItems { get; set; }
    }
}
