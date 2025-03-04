using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.ClassDay
{
    public class UpdateClassDayDTO
    {
        public int ClassDayId { get; set; }
        public int ClassId { get; set; }
        public DayOfWeek Day { get; set; }
    }
}
