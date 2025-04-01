using InstruLearn_Application.Model.Models.DTO.Major;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Teacher
{
    public class ValidTeacherDTO
    {
        public int TeacherId { get; set; }
        public string Fullname { get; set; }
        public List<MajorDTO> Majors { get; set; } = new List<MajorDTO>();
    }

}
