using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegistration
{
    public class CreateLearningRegisDTO
    {
        public int LearnerId { get; set; }
        public int? TeacherId { get; set; }
        public int RegisTypeId { get; set; }
        public int MajorId { get; set; }
        public string VideoUrl { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime RequestDate { get; set; }
        public int NumberOfSession { get; set; }
        public List<DayOfWeeks> LearningDays { get; set; }
    }
}
