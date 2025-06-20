﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Learner
    {
        [Key]
        public int LearnerId { get; set; }
        public string AccountId { get; set; }
        public Account Account { get; set; }

        public string FullName { get; set; }
        public Wallet Wallet { get; set; }

        // Navigation properties
        public ICollection<Certification> Certifications { get; set; }
        public ICollection<Learning_Registration> Learning_Registrations { get; set; }
        public ICollection<Purchase> Purchases { get; set; }
        public ICollection<Schedules> Schedules { get; set; }
        public ICollection<Learner_class> Learner_Classes { get; set; }
        public ICollection<Learner_Course> LearnerCourses { get; set; }
        public ICollection<Learner_Content_Progress> ContentProgresses { get; set; }
    }
}
