using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.QnAReplies
{
    public class CreateQnARepliesDTO
    {
        public int QuestionId { get; set; }
        public string AccountId { get; set; }
        public string QnAContent { get; set; }
    }
}
