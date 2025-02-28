using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class FeedBack
    {
        [Key]
        public int FeedbackId { get; set; }
        public string AccountId { get; set; }
        public Account Account { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string FeedbackContent { get; set; }
        public DateTime CreateAt { get; set; }
        public int Rating { get; set; }
        public ICollection<FeedbackReplies> FeedbackReplies { get; set; }
    }
}
