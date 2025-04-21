using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearnerCourse
{
    public class CoursePackageDetailsDTO
    {
        public int CoursePackageId { get; set; }
        public string CourseName { get; set; }
        public int TotalContents { get; set; }
        public int TotalContentItems { get; set; }
        public double OverallProgressPercentage { get; set; }
        public List<CourseContentDetailsDTO> Contents { get; set; } = new List<CourseContentDetailsDTO>();
    }
}
