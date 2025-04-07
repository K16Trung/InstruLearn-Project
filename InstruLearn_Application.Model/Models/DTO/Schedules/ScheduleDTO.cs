using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.ClassDay;
using InstruLearn_Application.Model.Models.DTO.ScheduleDays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Schedules
{
    public class ScheduleDTO
    {
        public int ScheduleId { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int? LearnerId { get; set; }
        public string LearnerName { get; set; }
        public string LearnerAddress { get; set; }
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string DayOfWeek { get; set; }
        public DateOnly StartDay { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScheduleMode Mode { get; set; }

        // From Learning_Registration
        public DateOnly? RegistrationStartDay { get; set; }
        //public TimeOnly RegistrationTimeStart { get; set; }
        public int LearningRegisId { get; set; }
        public List<ScheduleDaysDTO> ScheduleDays { get; set; }
        public List<ClassDayDTO> classDayDTOs { get; set; }
    }
}
