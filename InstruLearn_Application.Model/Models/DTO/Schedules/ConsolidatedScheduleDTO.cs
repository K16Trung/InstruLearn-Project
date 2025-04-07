using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.ScheduleDays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Schedules
{
    public class ConsolidatedScheduleDTO
    {
        public int ScheduleId { get; set; }
        public int? TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int? ClassId { get; set; }
        public string ClassName { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string DayOfWeek { get; set; }
        public DateOnly StartDay { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScheduleMode Mode { get; set; }
        public DateOnly? RegistrationStartDay { get; set; }
        public List<ScheduleParticipantDTO> Participants { get; set; } = new List<ScheduleParticipantDTO>();
        public List<ScheduleDaysDTO> ScheduleDays { get; set; } = new List<ScheduleDaysDTO>();
    }
}
