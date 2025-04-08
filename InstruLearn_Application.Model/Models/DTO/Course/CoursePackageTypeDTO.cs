using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Course
{
    public class CoursePackageTypeDTO
    {
        public int CoursePackageId { get; set; }
        public string TypeName { get; set; }
        public string CourseName { get; set; }
        public string? CourseDescription { get; set; }
        public string? Headline { get; set; }
        public string? ImageUrl { get; set; }
        public CoursePackageStatus CoursePackageType { get; set; }
    }
}
