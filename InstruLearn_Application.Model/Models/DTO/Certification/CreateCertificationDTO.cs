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
        public int CoursePackageId { get; set; }
        public string CertificationName { get; set; }
    }
}
