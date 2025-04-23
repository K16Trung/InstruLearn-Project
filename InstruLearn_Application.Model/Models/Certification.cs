using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstruLearn_Application.Model.Enum;

namespace InstruLearn_Application.Model.Models
{
    public class Certification
    {
        [Key]
        public int CertificationId { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public string CertificationName { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public CertificationType CertificationType { get; set; }
        public int? LearningRegisId { get; set; }
        public Learning_Registration? LearningRegistration { get; set; }
        public ScheduleMode? LearningMode { get; set; }
        public string? TeacherName { get; set; }
        public string? Subject { get; set; }
    }
}