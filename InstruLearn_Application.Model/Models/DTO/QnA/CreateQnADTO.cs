using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.QnA
{
    public class CreateQnADTO
    {
        public int CoursePackageId { get; set; }
        public string AccountId { get; set; }
        public string Title { get; set; }
        public string QuestionContent { get; set; }
    }
}
