using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.StaffNotification
{
    public class SessionDetailDTO
    {
        public int SessionNumber { get; set; }
        public DateOnly Date { get; set; }
        public string DayOfWeek { get; set; }
        public string Time { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public bool IsChangeRequested { get; set; }
        public bool IsCompleted { get; set; }
        public bool NeedsFeedback { get; set; }
    }
}
