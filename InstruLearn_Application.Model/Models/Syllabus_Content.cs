using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Syllabus_Content
    {
        [Key]
        public int SyllabusContentId { get; set; }
        public int SyllabusId { get; set; }
        public Syllabus Syllabus { get; set; }
        public string ContentName { get; set; }
    }
}
