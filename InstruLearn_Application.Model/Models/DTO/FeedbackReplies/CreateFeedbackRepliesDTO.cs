using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.FeedbackReplies
{
    public class CreateFeedbackRepliesDTO
    {
        public int FeedbackId { get; set; }
        public string AccountId { get; set; }
        public string RepliesContent { get; set; }
    }
}
