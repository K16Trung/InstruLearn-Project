using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearnerClass
{
    public class LearnerClassPaymentDTO
    {
        public int LearnerId { get; set; }
        public int ClassId { get; set; }
        public int? LevelId { get; set; }
    }
}
