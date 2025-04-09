using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Schedules
{
    public class ScheduleParticipantDTO
    {
        public int LearnerId { get; set; }
        public string LearnerName { get; set; }
        public int LearningRegisId { get; set; }
        public int ScheduleId { get; set; }
        public AttendanceStatus AttendanceStatus { get; set; }
    }
}
