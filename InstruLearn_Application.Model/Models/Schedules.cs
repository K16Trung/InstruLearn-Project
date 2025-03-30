using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Schedules
    {
        [Key]
        public int ScheduleId { get; set; }
        public int? TeacherId { get; set; }
        public Teacher Teacher { get; set; }
        public int? LearnerId { get; set; }
        public Learner Learner { get; set; }
        public int? LearningRegisId { get; set; }
        public Learning_Registration? Registration { get; set; }
        public int? ClassId { get; set; }
        public Class? Class { get; set; }
        public TimeOnly TimeStart { get; set; }
        public TimeOnly TimeEnd { get; set; }
        public ScheduleMode Mode { get; set; }
        public ICollection<ScheduleDays> ScheduleDays { get; set; }
    }
}
