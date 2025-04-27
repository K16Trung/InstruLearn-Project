using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Schedules
{
    public class UpdateScheduleMakeupDTO
    {
        public DateOnly NewDate { get; set; }
        public TimeOnly NewTimeStart { get; set; }
        public int TimeLearning { get; set; }
        public string ChangeReason { get; set; }
    }
}
