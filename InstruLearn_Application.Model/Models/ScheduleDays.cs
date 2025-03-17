using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class ScheduleDays
    {
        [Key]
        public int ScheduleDayId { get; set; }
        public int ScheduleId { get; set; }
        public Schedules Schedules { get; set; }
        public DayOfWeeks DayOfWeeks {  get; set; } 
    }
}
