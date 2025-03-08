using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class OneOnOneRequestDays
    {
        [Key]
        public int RequestDayId { get; set; }
        public int RequestId { get; set; }
        public OneOnOneRequest OneOnOneRequest { get; set; }
        public DayOfWeeks DayOfWeeks { get; set; }
    }
}
