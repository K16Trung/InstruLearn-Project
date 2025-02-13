using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }
        public string AccountId { get; set; }
        public Account Account { get; set; }

        public string Fullname { get; set; }
    }
}
