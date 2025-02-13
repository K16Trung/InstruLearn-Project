using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Learner
{
    public class LearnerDTO
    {
        public int LearnerId { get; set; }
        public string AccountId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
