using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Center_Course
    {
        [Key]
        public int CenterCourseId { get; set; }
        public string CenterCourseName { get; set; }
        // Add these properties
        public ICollection<Class> Classes { get; set; }
        public ICollection<Curriculum> Curriculums { get; set; }
    }
}
