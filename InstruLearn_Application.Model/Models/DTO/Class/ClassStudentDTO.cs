using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Class
{
    public class ClassStudentDTO
    {
        public int LearnerId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public bool? IsEligible { get; set; }
    }
}
