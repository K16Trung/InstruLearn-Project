using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.QnAReplies
{
    public class QnARepliesDTO
    {
        public int ReplyId { get; set; }
        public int QuestionId { get; set; }
        public string AccountId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string QnAContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
