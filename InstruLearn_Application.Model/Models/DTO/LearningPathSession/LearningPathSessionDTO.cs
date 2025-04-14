using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearningPathSession
{
    public class LearningPathSessionDTO
    {
        public int LearningPathSessionId { get; set; }
        public int LearningRegisId { get; set; }
        public int SessionNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
    }
}
