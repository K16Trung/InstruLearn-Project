using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Class
{
    public class CreateClassDTO
    {
        public int TeacherId { get; set; }
        public int MajorId { get; set; }
        public int SyllabusId { get; set; }
        public int LevelId { get; set; }
        public string ClassName { get; set; }
        public DateOnly StartDate { get; set; }
        public TimeOnly ClassTime { get; set; }
        public int MaxStudents { get; set; }
        public int totalDays { get; set; }
        public decimal Price { get; set; }
        public ClassStatus? Status { get; set; }
        public ICollection<DayOfWeeks> ClassDays { get; set; }
    }
}
