using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LearnerCourse
{
    public class UpdateVideoWatchTimeDTO
    {
        public int LearnerId { get; set; }
        public int ItemId { get; set; }
        public double WatchTimeInSeconds { get; set; }
    }
}
