using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.Major;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Teacher
{
    public class TeacherDTO
    {
        public List<MajorDTO> Majors { get; set; } = new List<MajorDTO>();
        public int TeacherId { get; set; }
        public string AccountId { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public string? Heading { get; set; }
        public string? Details { get; set; }
        public string? Links { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public DateOnly DateOfEmployment { get; set; }
        public AccountStatus IsActive { get; set; }
    }
}
