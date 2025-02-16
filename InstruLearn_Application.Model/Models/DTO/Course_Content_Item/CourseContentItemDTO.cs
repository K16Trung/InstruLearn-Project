using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Course_Content_Item
{
    public class CourseContentItemDTO
    {
        public int ItemId { get; set; }
        public int ItemTypeId { get; set; }
        public int ContentId { get; set; }
        public string ItemDes { get; set; }
    }
}
