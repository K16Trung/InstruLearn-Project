﻿using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Course
{
    public class UpdateCourseDTO
    {
        public int CourseTypeId { get; set; }
        public string CourseName { get; set; }
        public string? CourseDescription { get; set; }
        public string? Headline { get; set; }
        public int? Rating { get; set; }
        public decimal? Price { get; set; }
        public int? Discount { get; set; }
        public string? ImageUrl { get; set; }
        public CoursePackageStatus Status { get; set; }
    }
}
