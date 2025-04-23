using InstruLearn_Application.Model.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Course_Package
    {
        [Key]
        public int CoursePackageId { get; set; }
        public int CourseTypeId { get; set; }
        public CourseType Type { get; set; }
        public string CourseName { get; set; }
        public string? CourseDescription { get; set; }
        public string? Headline { get; set; }
        public int? Rating { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }
        public int? Discount { get; set; }
        public string? ImageUrl { get; set; }
        public CoursePackageStatus Status { get; set; }

        //Navigation property
        public ICollection<Course_Content> CourseContents { get; set; }
        public ICollection<FeedBack> FeedBacks { get; set; }
        public ICollection<QnA> QnAs { get; set; }
        public ICollection<Purchase_Items> PurchaseItems { get; set; }
        public ICollection<Learner_Course> LearnerCourses { get; set; }

    }
}
