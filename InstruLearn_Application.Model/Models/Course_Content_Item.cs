﻿using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Course_Content_Item
    {
        [Key]
        public int ItemId { get; set; }
        public int ContentId { get; set; }
        public Course_Content CourseContent { get; set; }
        public int ItemTypeId { get; set; }
        public ItemTypes ItemType { get; set; }
        public string ItemDes { get; set; }
        public CourseContentItemStatus Status { get; set; }
        public double? DurationInSeconds { get; set; }
        public ICollection<Learner_Content_Progress> LearnerProgresses { get; set; }
    }
}