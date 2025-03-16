using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Purchase
    {
        [Key]
        public int PurchaseId { get; set; }
        public int LearnerId { get; set; }
        public int CoursePackageId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public PurchaseStatus Status { get; set; }

        //Navigation property
        public Learner Learner { get; set; }
        public ICollection<Course_Package> Course_Packages { get; set; }
    }
}
