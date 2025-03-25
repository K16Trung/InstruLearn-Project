using InstruLearn_Application.Model.Models.DTO.ScheduleDays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Schedules
{
    public class SchedulesByIdDTO
    {
        public int ScheduleId { get; set; }
        public int? TeacherId { get; set; }
        public int? LearnerId { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string Mode { get; set; }
        public string DayOfWeek { get; set; }
        public string StartDate { get; set; }
        public int LearningRegisId { get; set; }
        public DateOnly? RegistrationStartDay { get; set; }
        public int NumberOfSession { get; set; }
        public ICollection<ScheduleDaysDTO> ScheduleDays { get; set; }
    }
}
