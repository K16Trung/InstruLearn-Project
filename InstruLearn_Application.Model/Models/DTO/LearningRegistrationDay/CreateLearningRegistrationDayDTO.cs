﻿using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegistrationDay
{
    public class CreateLearningRegistrationDayDTO
    {
        public int LearningRegisId { get; set; }
        public DayOfWeeks DayOfWeek { get; set; }
    }
}
