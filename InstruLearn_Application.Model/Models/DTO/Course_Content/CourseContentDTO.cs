using InstruLearn_Application.Model.Models.DTO.Course_Content_Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Course_Content
{
    public class CourseContentDTO
    {
        public int ContentId { get; set; }
        public int CoursePackageId { get; set; }
        public string Heading { get; set; }
        public ICollection<CourseContentItemDTO> CourseContentItems { get; set; }
    }
}
