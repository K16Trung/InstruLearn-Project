﻿using InstruLearn_Application.Model.Models.DTO.Course_Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Course
{
    public class CourseDetailPurchaseDTO
    {
        public string CourseTypeName { get; set; }
        public string CourseName { get; set; }
        public string CourseDescription { get; set; }
        public string Headline { get; set; }
        public string ImageUrl { get; set; }
        public List<CourseContentDTO> CourseContents { get; set; } = new List<CourseContentDTO>();
    }
}
