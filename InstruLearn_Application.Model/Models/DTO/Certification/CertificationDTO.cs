using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO.Course;
using InstruLearn_Application.Model.Models.DTO.Learner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Certification
{
    public class CertificationDTO
    {
        public int CertificationId { get; set; }
        public LearnerCertificationDTO Learner { get; set; }
        public string CertificationName { get; set; }
        public DateTime IssueDate { get; set; }
        public CertificationType CertificationType { get; set; }
        public int? LearningRegisId { get; set; }
        public ScheduleMode? LearningMode { get; set; }
        public string? TeacherName { get; set; }
        public string? Subject { get; set; }
    }
}
