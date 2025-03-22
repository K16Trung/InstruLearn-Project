using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegistration
{
    public class UpdateLearningRegisDTO
    {
        public int LearningRegisId { get; set; }
        public int? Score { get; set; }
        public string? LevelAssigned { get; set; }
        public decimal? Price { get; set; }
        public string? Feedback { get; set; }
    }
}
