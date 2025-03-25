using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.ScheduleDays
{
    public class ScheduleDaysDTO
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DayOfWeeks DayOfWeeks { get; set; }
    }
}
