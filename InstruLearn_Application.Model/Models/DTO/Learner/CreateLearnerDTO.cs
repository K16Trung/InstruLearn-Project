﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Learner
{
    public class CreateLearnerDTO
    {
        public string Fullname { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
