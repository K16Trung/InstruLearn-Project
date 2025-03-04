using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Class
{
    public class CreateClassDTO
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int TeacherId { get; set; }
        public int CenterCourseId { get; set; }
        public int CuriculumId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeOnly ClassTime { get; set; }
    }
}
