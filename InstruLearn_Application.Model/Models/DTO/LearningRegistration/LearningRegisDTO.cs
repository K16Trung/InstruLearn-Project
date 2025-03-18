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
        public int LearnerId { get; set; }
        public int? ClassId { get; set; }
        public int RegisTypeId { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime RequestDate { get; set; }
        public LearningRegis Status { get; set; }
        public int NumberOfSession { get; set; }
    }
}
