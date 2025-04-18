using InstruLearn_Application.Model.Models.DTO.LearningPathSession;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningRegistration
{
    public class LearningPathSessionsCreateDTO
    {
        public int LearningRegisId { get; set; }
        public List<CreateLearningPathSessionDTO> LearningPathSessions { get; set; }
    }
}
