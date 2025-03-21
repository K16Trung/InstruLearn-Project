using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class MajorTest
    {
        [Key]
        public int MajorTestId { get; set; }
        public int MajorId { get; set; }
        public Major Major { get; set; }
        public string MajorTestName { get; set; }

        // Navigation properties
        
    }
}
