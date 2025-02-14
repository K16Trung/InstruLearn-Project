using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Learner
{
    public class LearnerResponseDTO
    {
        public string Fullname { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public AccountStatus IsActive { get; set; }
        public AccountRoles Role { get; set; }
    }
}
