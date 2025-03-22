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
        public LearnerCertificationDTO learner  { get; set; }
        public CourseCertificationDTO course { get; set; }
        public string CertificationName { get; set; }
    }
}
