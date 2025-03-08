using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class OneOnOneSchedules
    {
        [Key]
        public int SchedulesId { get; set; }
        public int RequestId { get; set; }
        public OneOnOneRequest OneOnOneRequest { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public int TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public OneOnOneSchedulesStatus Status { get; set; }
    }
}
