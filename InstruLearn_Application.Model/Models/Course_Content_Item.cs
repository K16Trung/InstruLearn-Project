using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Course_Content_Item
    {
        [Key]
        public int ItemId { get; set; }
        public int ContentId { get; set; }
        public Course_Content CourseContent { get; set; }
        public string ItemDes { get; set; }
    }
}
