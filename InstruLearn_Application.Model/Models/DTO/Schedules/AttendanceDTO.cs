using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Schedules
{
    public class AttendanceDTO
    {
        public int ScheduleId { get; set; }
        public DateOnly StartDay { get; set; }
        public TimeOnly TimeStart { get; set; }
        public TimeOnly TimeEnd { get; set; }
        public int LearnerId { get; set; }
        public string LearnerName { get; set; }
        public AttendanceStatus AttendanceStatus { get; set; }
    }
}
