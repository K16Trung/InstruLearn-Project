using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Teacher
{
    public class TeacherDTO
    {
        public int TeacherId { get; set; }
        public string AccountId { get; set; }
        public string Fullname { get; set; }
        public string? Heading { get; set; }
        public string? Details { get; set; }
        public string? Links { get; set; }
    }
}
