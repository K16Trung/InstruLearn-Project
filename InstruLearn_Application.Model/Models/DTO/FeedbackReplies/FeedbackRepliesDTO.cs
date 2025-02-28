using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.FeedbackReplies
{
    public class FeedbackRepliesDTO
    {
        public int FeedbackRepliesId { get; set; }
        public int FeedbackId { get; set; }
        public string AccountId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string RepliesContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
