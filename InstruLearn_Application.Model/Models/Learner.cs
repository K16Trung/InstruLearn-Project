using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class Learner
    {
        [Key]
        public int LearnerId { get; set; }
        public string AccountId { get; set; }
        public Account Account { get; set; }

        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public Wallet Wallet { get; set; }
        public ICollection<OneOnOneRequest> OneOnOneRequests { get; set; }
        public ICollection<OneOnOneSchedules> OneOnOneSchedules { get; set; }
    }
}
