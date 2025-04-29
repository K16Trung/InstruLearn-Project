using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Certification
{
    public class CreateCertificationDTO
    {
        public int LearnerId { get; set; }
        public int? ClassId { get; set; }
        public string CertificationName { get; set; }
        public CertificationType CertificationType { get; set; }
        public ScheduleMode? LearningMode { get; set; }
        public string? TeacherName { get; set; }
        public string? Subject { get; set; }
    }
}
