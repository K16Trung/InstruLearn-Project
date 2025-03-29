using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Test_Result
{
    public class CreateTestResultDTO
    {
        public int LearnerId { get; set; }
        public int TeacherId { get; set; }
        public int? MajorId { get; set; }
        public int? LearningRegisId { get; set; }
    }
}
