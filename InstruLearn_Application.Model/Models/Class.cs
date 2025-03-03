using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int TeacherId { get; set; }
        public int CenterCourseId { get; set; }
        public int CuriculumId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeOnly ClassTime { get; set; }
        public int MaxStudents { get; set; }
        public int totalDays { get; set; }
        public ClassStatus Status { get; set; }
        public decimal Price { get; set; }
        // Add these properties
        public Center_Course CenterCourse { get; set; }
        public Curriculum Curriculum { get; set; }
        public Teacher Teacher { get; set; }
        public virtual ICollection<ClassDay> ClassDays { get; set; }
    }
}
