using InstruLearn_Application.Model.Models.DTO.FeedbackReplies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Feedback
{
    public class FeedbackDTO
    {
        public int FeedbackId { get; set; }
        public int CoursePackageId { get; set; }
        public string AccountId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FeedbackContent { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<FeedbackRepliesDTO> Replies { get; set; }
    }
}
