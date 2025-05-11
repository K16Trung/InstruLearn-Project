using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }
        public string AccountId { get; set; }
        public Account Account { get; set; }
        public string Fullname { get; set; }
        public string? Heading { get; set; }
        public string? Details { get; set; }
        public string? Links { get; set; }

        //Navigation property
        public ICollection<TeacherMajor> TeacherMajors { get; set; }
        public ICollection<Class> Classes { get; set; }
        public ICollection<Learning_Registration> Learning_Registrations { get; set; }
        public ICollection<Schedules> Schedules { get; set; }
    }
}
