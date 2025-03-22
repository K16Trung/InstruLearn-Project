using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegistration
{
    public class LearningRegisDTO
    {
        public int LearningRegisId { get; set; }
        public string VideoUrl { get; set; }
        public int? Score { get; set; }
        public string? Feedback { get; set; }
    }
}
