using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearnerCourse
{
    public class CourseContentDetailsDTO
    {
        public int ContentId { get; set; }
        public string Heading { get; set; }
        public int TotalContentItems { get; set; }
        public List<ContentItemProgressDTO> ContentItems { get; set; } = new List<ContentItemProgressDTO>();
    }
}
