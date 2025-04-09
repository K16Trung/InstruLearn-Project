using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Syllabus
    {
        [Key]
        public int SyllabusId { get; set; }
        public string SyllabusName { get; set; }
        public string SyllabusDescription { get; set; }

        // Navigation properties
        public ICollection<Class> Classes { get; set; }
        public ICollection<Syllabus_Content> SyllabusContents { get; set; }
    }
}
