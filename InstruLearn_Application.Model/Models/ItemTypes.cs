using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class ItemTypes
    {
        [Key]
        public int ItemTypeId { get; set; }
        public string ItemTypeName { get; set; }
        public ICollection<Course_Content_Item> CourseContentItems { get; set; }
    }
}
