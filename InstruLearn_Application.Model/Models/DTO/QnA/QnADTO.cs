using InstruLearn_Application.Model.Models.DTO.QnAReplies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.QnA
{
    public class QnADTO
    {
        public int QuestionId { get; set; }
        public int CoursePackageId { get; set; }
        public string AccountId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Title { get; set; }
        public string QuestionContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<QnARepliesDTO> Replies { get; set; }
    }
}
