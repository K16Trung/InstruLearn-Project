using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class QnAReplies
    {
        [Key]
        public int QnARepliesId {  get; set; }
        public string AccountId { get; set; }
        public Account Account { get; set; }
        public int QuestionId { get; set; }
        public QnA QnA { get; set; }
        public string QnAContent { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
