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
        public ICollection<Class> Classes { get; set; }
    }
}
