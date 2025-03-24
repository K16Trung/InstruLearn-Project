using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Schedule
{
    public class CreateScheduleDTO
    {
        public int? TeacherId { get; set; }
        public int? LearnerId { get; set; }
        public int LearningRegisId { get; set; }
        public TimeOnly TimeStart { get; set; }
        public TimeOnly TimeEnd { get; set; }
        public ScheduleMode Mode { get; set; }
    }
}
