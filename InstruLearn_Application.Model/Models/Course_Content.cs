using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Course_Content
    {
        [Key]
        public int ContentId { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string Heading { get; set; }
        public ICollection<Course_Content_Item> CourseContentItems { get; set; }
    }
}
