using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class CourseType
    {
        [Key]
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public ICollection<Course> Courses { get; set; }
    }
}
