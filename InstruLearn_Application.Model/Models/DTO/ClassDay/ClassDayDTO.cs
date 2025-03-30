using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.ClassDay
{
    public class ClassDayDTO
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DayOfWeeks Day { get; set; }
    }
}
