using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Class
{
    public class ClassDetailDTO
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int MajorId { get; set; }
        public string MajorName { get; set; }
        public int SyllabusId { get; set; }
        public string SyllabusName { get; set; }
        public TimeOnly ClassTime { get; set; }
        public int MaxStudents { get; set; }
        public int TotalDays { get; set; }
        public ClassStatus Status { get; set; }
        public decimal Price { get; set; }
        public DateOnly StartDate { get; set; }
        public ICollection<ClassDayDTO> ClassDays { get; set; }

        // Added fields for student information
        public int StudentCount { get; set; }
        public List<ClassStudentDTO> Students { get; set; }
    }
}
