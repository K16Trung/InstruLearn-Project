using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Certification
    {
        [Key]
        public int CertificationId { get; set; }
        public int LearnerId { get; set; }
        public Learner Learner { get; set; }
        public int CoursePackageId { get; set; }
        public string CertificationName { get; set; }

        //Navigation properties
        public virtual Course_Package CoursePackages { get; set; }
    }
}
