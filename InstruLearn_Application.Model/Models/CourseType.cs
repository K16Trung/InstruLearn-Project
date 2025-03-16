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
        public int CourseTypeId { get; set; }
        public string CourseTypeName { get; set; }
        public ICollection<Course_Package> CoursePackages { get; set; }
    }
}
