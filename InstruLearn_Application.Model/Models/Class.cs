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
        public int TeacherId { get; set; }
        public int CoursePackageId { get; set; }
        public string ClassName { get; set; }
        public string TeacherName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeOnly ClassTime { get; set; }
        public int MaxStudents { get; set; }
        public ClassStatus Status { get; set; }
        public decimal Price { get; set; }

        //Navigation property
        public Teacher Teacher { get; set; }
        public Learning_Registration Learning_Registration { get; set; }
        public Course_Package CoursePackage { get; set; }
        public ICollection<ClassDay> ClassDays { get; set; }
    }
}
