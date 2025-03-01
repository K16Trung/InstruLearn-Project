using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Feedback
{
    public class CreateFeedbackDTO
    {
        public int CourseId { get; set; }
        public string AccountId { get; set; }
        public string FeedbackContent { get; set; }
        public int Rating { get; set; }
    }
}
