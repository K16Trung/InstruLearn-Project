using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class FeedbackReplies
    {
        [Key]
        public int FeedbackRepliesId { get; set; }
        public int FeedbackId { get; set; }
        public FeedBack FeedBack { get; set; }
        public string AccountId { get; set; }
        public Account Account { get; set; }
        public DateTime CreateAt { get; set; }
        public string RepliesContent { get; set; }
    }
}
