using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Schedules
{
    public class UpdateAttendanceDTO
    {
        public AttendanceStatus Status { get; set; }
        public PreferenceStatus PreferenceStatus { get; set; } = PreferenceStatus.None;
    }
}
