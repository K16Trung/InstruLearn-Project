using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Class
{
    public class ClassDTO
    {
        public int ClassId { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int MajorId { get; set; }
        public string MajorName { get; set; }
        public int LevelId { get; set; }
        public string LevelName { get; set; }
        public string ClassName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public TimeOnly ClassTime { get; set; }
        public TimeOnly ClassEndTime { get; set; }
        public int MaxStudents { get; set; }
        public int StudentCount { get; set; }
        public int totalDays { get; set; }
        public ClassStatus Status { get; set; }
        public decimal Price { get; set; }
        public List<DateOnly> SessionDates { get; set; } = new List<DateOnly>();
        public ICollection<ClassDayDTO> ClassDays { get; set; }
    }
}
