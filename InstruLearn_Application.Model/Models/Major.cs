﻿using InstruLearn_Application.Model.Enum;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Major
    {
        [Key]
        public int MajorId { get; set; }
        public string MajorName { get; set; }
        public MajorStatus Status { get; set; }

        //Navigation property
        public ICollection<MajorTest> MajorTests { get; set; }
        public ICollection<Learning_Registration> learning_Registrations { get; set; }
        public ICollection<TeacherMajor> TeacherMajors { get; set; }
        public ICollection<LevelAssigned> LevelAssigneds { get; set; }
        public ICollection<Class> Classes { get; set; }
    }
}
