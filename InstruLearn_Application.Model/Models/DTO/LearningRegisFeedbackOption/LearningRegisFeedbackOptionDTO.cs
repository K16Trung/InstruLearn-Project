﻿using InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackQuestion;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegisFeedbackOption
{
    public class LearningRegisFeedbackOptionDTO
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; }
        //public int QuestionId { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public LearningRegisFeedbackQuestionDTO Question { get; set; }
    }
}
