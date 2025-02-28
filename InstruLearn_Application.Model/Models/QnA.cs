using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class QnA
    {
        [Key]
        public int QuestionId { get; set; }
        public string AccountId { get; set; }
        public Account Account { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public string Title { get; set; }
        public string QuestionContent { get; set; }
        public DateTime CreateAt { get; set; }
        public ICollection<QnAReplies> QnAReplies { get; set; }
    }
}
