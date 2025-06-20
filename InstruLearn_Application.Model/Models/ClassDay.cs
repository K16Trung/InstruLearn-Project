﻿using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class ClassDay
    {
        [Key]
        public int ClassDayId { get; set; }
        public int ClassId { get; set; }
        public DayOfWeeks Day { get; set; }

        // Navigation property
        public Class Class { get; set; }
    }
}
