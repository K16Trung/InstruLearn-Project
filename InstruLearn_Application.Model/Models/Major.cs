using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Major
    {
        [Key]
        public int MajorId { get; set; }
        public string MajorName { get; set; }

        //Navigation property
        public ICollection<Teacher> Teachers { get; set; }
        public ICollection<MajorTest> MajorTests { get; set; }
        public ICollection<Learning_Registration> learning_Registrations { get; set; }
    }
}
