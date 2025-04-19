using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearnerVideoProgress
{
    public class VideoProgressDTO
    {
        public int LearnerId { get; set; }
        public int ContentItemId { get; set; }
        public double WatchTimeInSeconds { get; set; }
        public bool IsCompleted { get; set; }
        public double? TotalDuration { get; set; }
        public double CompletionPercentage { get; set; }
    }
}
