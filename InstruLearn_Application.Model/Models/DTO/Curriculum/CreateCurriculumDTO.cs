using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Curriculum
{
    public class CreateCurriculumDTO
    {
        public int CurriculumId { get; set; }
        public int CenterCourseId { get; set; }
        public string CurriculumName { get; set; }
        public string Description { get; set; }
    }
}
