﻿using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Learning_Registration
    {
        [Key]
        public int LearningRegisId { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public int? ClassId { get; set; }
        public Class? Classes { get; set; }
        public int RegisTypeId { get; set; }
        public Learning_Registration_Type Learning_Registration_Type { get; set; }
        public int MajorId { get; set; }
        public Major Major { get; set; }
        public int? ResponseId { get; set; }
        public Response? Response { get; set; }
        public int? LevelId { get; set; }
        public LevelAssigned? LevelAssigned { get; set; }
        public DateOnly? StartDay { get; set; }
        public TimeOnly TimeStart { get; set; }
        public int TimeLearning { get; set; }
        public string LearningRequest { get; set; }
        public DateTime RequestDate { get; set; }
        public LearningRegis Status { get; set; }
        public int NumberOfSession { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        public string VideoUrl { get; set; }
        public string? LearningPath { get; set; }
        public int? SelfAssessmentId { get; set; }
        public SelfAssessment? SelfAssessment { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? RemainingAmount { get; set; }

        public DateTime? AcceptedDate { get; set; }
        public DateTime? PaymentDeadline { get; set; }
        public bool HasPendingLearningPath { get; set; } = false;
        // Add these properties to Learning_Registration class
        public DateTime? LastReminderSent { get; set; }
        public int ReminderCount { get; set; } = 0;
        public bool ChangeTeacherRequested { get; set; } = false;
        public bool TeacherChangeProcessed { get; set; } = false;
        public bool SentTeacherChangeReminder { get; set; } = false;
        //

        // Navigation properties
        public ICollection<LearningRegistrationDay> LearningRegistrationDay { get; set; }
        public ICollection<Schedules> Schedules { get; set; }
        public ICollection<LearningPathSession> LearningPathSessions { get; set; }

    }
}
