using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class OneOnOneRequestTests
    {
        [Key]
        public int TestSubmitionId { get; set; }
        public int RequestId { get; set; }
        public OneOnOneRequest OneOnOneRequest { get; set; }
        public int TestId { get; set; }
        public MajorTest MajorTest { get; set; }
        public string Video {  get; set; }
        public string? Score { get; set; }
        public string? Feedback { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
