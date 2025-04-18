using InstruLearn_Application.Model.Models.DTO.LearningPathSession;
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
        public int? TeacherId { get; set; }
        public int LevelId { get; set; }
        public int? ResponseId { get; set; }
        //public decimal? Price { get; set; }
        //public string? LearningPath { get; set; }
        //public List<CreateLearningPathSessionDTO> LearningPathSessions { get; set; }
    }
}
