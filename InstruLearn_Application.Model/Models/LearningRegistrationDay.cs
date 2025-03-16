using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class LearningRegistrationDay
    {
        [Key]
        public int LearnRegisDayId { get; set; }
        public int LearningRegisId { get; set; }
        public Learning_Registration Learning_Registration { get; set; }
        public DayOfWeeks DayOfWeek { get; set; }
    }
}
