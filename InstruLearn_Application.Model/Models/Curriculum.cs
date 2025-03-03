using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Curriculum
    {
        [Key]
        public int CurriculumId { get; set; }
        public int CenterCourseId { get; set; }
        public string CurriculumName { get; set; }
        public string Description { get; set; }
        // Add these properties
        public Center_Course CenterCourse { get; set; }
        public ICollection<Class> Classes { get; set; }
    }
}
