using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Test_Result
{
    public class CreateTestResultDTO
    {
        public int TestResultId { get; set; }
        public int LearnerId { get; set; }
        public int TeacherId { get; set; }
        public int MajorTestId { get; set; }
        public int LearningRegisId { get; set; }
        public int Score { get; set; }
        public string LevelAssigned { get; set; }
        public string Feedback { get; set; }
    }
}
