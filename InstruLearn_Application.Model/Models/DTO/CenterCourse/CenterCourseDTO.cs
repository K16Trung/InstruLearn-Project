using InstruLearn_Application.Model.Models.DTO.Class;
using InstruLearn_Application.Model.Models.DTO.Curriculum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.CenterCourse
{
    public class CenterCourseDTO
    {
        public int CenterCourseId { get; set; }
        public string CenterCourseName { get; set; }
        public ICollection<ClassDTO> Classes { get; set; }
        public ICollection<CurriculumDTO> Curriculums { get; set; }
    }
}
