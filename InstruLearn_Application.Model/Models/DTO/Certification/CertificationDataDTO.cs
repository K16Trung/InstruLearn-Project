using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Certification
{
    public class CertificationDataDTO
    {
        public int CertificationId { get; set; }
        public string LearnerName { get; set; }
        public string LearnerEmail { get; set; }
        public string CertificationType { get; set; }
        public string CertificationName { get; set; }
        public DateTime IssueDate { get; set; }
        public string TeacherName { get; set; }
        public string Subject { get; set; }
        public string FileStatus { get; set; }
        public string FileLink { get; set; }
    }
}
